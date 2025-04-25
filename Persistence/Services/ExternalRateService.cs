using Application.DTOs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Services
{
    public class ExternalRateService : IExternalRateService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<ExternalRateService> _logger;
        private readonly IExchangeRateRepository _repository;

        public ExternalRateService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ExternalRateService> logger,
            IExchangeRateRepository repository)
        {
            _httpClient = httpClient;
            _apiKey = configuration["ExchangeRatesApi:ApiKey"];
            _httpClient.BaseAddress = new Uri("https://v6.exchangerate-api.com/v6/");
            _logger = logger;
            _repository = repository;
        }

        public async Task<ExchangeRate> GetCurrentRatesAsync(string baseCurrencyCode, string targetCurrencyCode)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(baseCurrencyCode) || baseCurrencyCode.Length != 3)
                throw new ArgumentException("Invalid base currency code");

            if (string.IsNullOrWhiteSpace(targetCurrencyCode) || targetCurrencyCode.Length != 3)
                throw new ArgumentException("Invalid target currency code");

            // Check cache first
            var cachedRate = await _repository.GetRealTimeRateAsync(baseCurrencyCode, targetCurrencyCode);
            if (cachedRate?.Date.Date == DateTime.UtcNow.Date)
            {
                _logger.LogInformation("Returning cached rate for {Base}-{Target}",
                    baseCurrencyCode, targetCurrencyCode);
                return cachedRate;
            }

            // Call external API with retry policy
            var response = await Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, delay, retryAttempt, context) =>
                    {
                        _logger.LogWarning("Retry {Attempt} for {Base}-{Target} due to {Error}",
                            retryAttempt, baseCurrencyCode, targetCurrencyCode,
                            outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                    })
                .ExecuteAsync(() => _httpClient.GetAsync($"v6/{_apiKey}/pair/{baseCurrencyCode}/{targetCurrencyCode}"));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API request failed with status {StatusCode}", response.StatusCode);
                throw new Exception($"API returned status code: {response.StatusCode}");
            }

            var content = await response.Content.ReadFromJsonAsync<ExchangeRateApiResponse>();
            if (content?.Result != "success")
            {
                _logger.LogError("API returned error: {Error}", content?.ErrorType);
                throw new Exception($"API error: {content?.ErrorType ?? "Unknown error"}");
            }

            var rate = new ExchangeRate
            {
                BasecurrencyCode = content.Base_code,
                TargetcurrencyCode = content.Target_code,
                Rate = content.Conversion_rate,
                Date = DateTimeOffset.FromUnixTimeSeconds(content.Time_last_update_unix).DateTime
            };

            // Cache the new rate
            await _repository.AddRealTimeRateAsync(rate);
            _logger.LogInformation("Successfully retrieved and cached rate for {Base}-{Target}",
                baseCurrencyCode, targetCurrencyCode);

            return rate;
        }

        public async Task<IEnumerable<ExchangeRate>> GetHistoricalRatesAsync(
            string baseCurrencyCode,
            string targetCurrencyCode,
            DateTime startDate,
            DateTime endDate)
        {
            // Validate input
            if (startDate > endDate)
                throw new ArgumentException("Start date must be before end date");

            if ((endDate - startDate).TotalDays > 365)
                throw new ArgumentException("Date range cannot exceed 1 year");

            var rates = new List<ExchangeRate>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                try
                {
                    var response = await _httpClient.GetAsync(
                        $"v6/{_apiKey}/history/{baseCurrencyCode}/{currentDate.Year}/{currentDate.Month}/{currentDate.Day}");
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Failed to get rates for {Date}: {StatusCode}",
                            currentDate, response.StatusCode);
                        continue;
                    }
                    var content = await response.Content.ReadFromJsonAsync<HistoryRateResponse>();
                    if (content?.Result == "success" && content.ConversionRates.TryGetValue(targetCurrencyCode, out var rate))
                    {
                        var exchangeRate = new ExchangeRate
                        {
                            BasecurrencyCode = baseCurrencyCode,
                            TargetcurrencyCode = targetCurrencyCode,
                            Rate = rate,
                            Date = currentDate,
                            IsHistorical = true
                        };

                        rates.Add(exchangeRate);
                        await _repository.AddHistoricalRateAsync(exchangeRate);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving historical rates for {Date}", currentDate);
                }

                currentDate = currentDate.AddDays(1);
            }

            if (!rates.Any())
            {
                throw new Exception("No historical rates found for the specified period");
            }

            return rates;
        }
    }
}
