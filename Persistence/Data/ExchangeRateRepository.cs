using Application.Interfaces.Repositories;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data
{
    public class ExchangeRateRepository:IExchangeRateRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ExchangeRateRepository> _logger;

        public ExchangeRateRepository(AppDbContext context, ILogger<ExchangeRateRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task AddHistoricalRateAsync(ExchangeRate exchangeRate)
        {
            throw new NotImplementedException();
        }

        public async Task AddRealTimeRateAsync(ExchangeRate exchangeRate)
        {
            try
            {
                if(await RateExists(exchangeRate))
    {
                    _logger.LogWarning("Rate already exists for {Base}-{Target} on {Date}",
                        exchangeRate.BasecurrencyCode, exchangeRate.TargetcurrencyCode, exchangeRate.Date);
                    return; 
                }

                exchangeRate.IsHistorical = false;
                await _context.ExchangeRates.AddAsync(exchangeRate);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding real-time rate for {Base}-{Target}",
                    exchangeRate.BasecurrencyCode, exchangeRate.TargetcurrencyCode);
                throw;
            }
        }

        public async Task<IEnumerable<ExchangeRate>> GetAllRealTimeRate(string baseCurrencyCode)
        {
            try
            {
                return await _context.ExchangeRates
                    .Where(rate => rate.BasecurrencyCode == baseCurrencyCode && !rate.IsHistorical)
                    .OrderByDescending(rate => rate.Date)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all real-time rates for {Base}", baseCurrencyCode);
                throw;
            }
        }

        public async Task<DateTime> GetcurrentRateTimeAsync(string baseCurrencyCode)
        {
            try
            {
                return await _context.ExchangeRates
                    .Where(rate => rate.BasecurrencyCode == baseCurrencyCode && !rate.IsHistorical)
                    .MaxAsync(rate => rate.Date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest rate time for {Base}", baseCurrencyCode);
                throw;
            }
        }

        public async Task<ExchangeRate> GetRealTimeRateAsync(string baseCurrencyCode, string targetCurrencyCode)
        {
            try
            {
                return await _context.ExchangeRates
                    .Where(rate => rate.BasecurrencyCode == baseCurrencyCode
                                && rate.TargetcurrencyCode == targetCurrencyCode
                               && !rate.IsHistorical)
                    .OrderByDescending(rate => rate.Date)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving real-time rate for {Base}-{Target}",
                    baseCurrencyCode, targetCurrencyCode);
                throw;

            }
        }
        private async Task<bool> RateExists(ExchangeRate rate)
        {
            return await _context.ExchangeRates.AnyAsync(r =>
                r.BasecurrencyCode == rate.BasecurrencyCode &&
                r.TargetcurrencyCode == rate.TargetcurrencyCode &&
                r.Date == rate.Date);
        }
    }
}
