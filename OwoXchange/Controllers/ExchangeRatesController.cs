using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace OwoXchange.Controllers
{

    [ApiController]
    [Route("api/v{version:apiVersion}/exchange-rates")]
    [ApiVersion("1.0")]
    public class ExchangeRatesController : ControllerBase
    {
        private readonly IExternalRateService _rateService;
        private readonly ILogger<ExchangeRatesController> _logger;

        public ExchangeRatesController(
            IExternalRateService rateService,
            ILogger<ExchangeRatesController> logger)
        {
            _rateService = rateService;
            _logger = logger;
        }

        [HttpGet("convert")]
        [ProducesResponseType(typeof(ConversionResultDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<ConversionResultDTO>> ConvertCurrency(
            [FromQuery, Required, StringLength(3, MinimumLength = 3)] string from,
            [FromQuery, Required, StringLength(3, MinimumLength = 3)] string to,
            [FromQuery, Range(0.01, double.MaxValue)] decimal amount)
        {
            try
            {
                var rate = await _rateService.GetCurrentRatesAsync(from.ToUpper(), to.ToUpper());
                var result = new ConversionResultDTO(
                    rate.BasecurrencyCode,
                    rate.TargetcurrencyCode,
                    rate.Rate,
                    amount * rate.Rate,
                    rate.Date);

                _logger.LogInformation("Converted {Amount} {From} to {To}", amount, from, to);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Conversion failed for {From} to {To}", from, to);
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("convert/historical")]
        [ProducesResponseType(typeof(ConversionResultDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult<ConversionResultDTO>> ConvertUsingHistoricalRate(
        [FromQuery, Required] ConversionRequestDTO request)
        {
            try
            {
                if (request.Date > DateTime.UtcNow.Date)
                    return BadRequest("Historical date cannot be in the future");

                var rates = await _rateService.GetHistoricalRatesAsync(
                    request.FromCurrency.ToUpper(),
                    request.ToCurrency.ToUpper(),
                    request.Date.Date,
                    request.Date.Date);

                var rate = rates.FirstOrDefault();
                if (rate == null)
                    return NotFound("No rate found for specified date");

                return Ok(new ConversionResultDTO(
                    rate.BasecurrencyCode,
                    rate.TargetcurrencyCode,
                    rate.Rate,
                    request.Amount * rate.Rate,
                    rate.Date));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Historical conversion failed");
                return StatusCode(500, "Conversion error");
            }
        }

        [HttpGet("historical")]
        [ProducesResponseType(typeof(IEnumerable<HistoricalResponseDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<HistoricalResponseDTO>>> GetHistoricalRates(
            [FromQuery, Required, StringLength(3, MinimumLength = 3)] string from,
            [FromQuery, Required, StringLength(3, MinimumLength = 3)] string to,
            [FromQuery, Required] DateTime startDate,
            [FromQuery, Required] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                    return BadRequest("Start date must be before end date");

                if ((endDate - startDate).TotalDays > 365)
                    return BadRequest("Date range cannot exceed 1 year");

                var rates = await _rateService.GetHistoricalRatesAsync(
                    from.ToUpper(),
                    to.ToUpper(),
                    startDate.Date,
                    endDate.Date);

                var result = rates.Select(r => new HistoricalResponseDTO(
                    r.BasecurrencyCode,
                    r.TargetcurrencyCode,
                    r.Rate,
                    r.Date));

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get historical rates");
                return StatusCode(500, "Error retrieving historical rates");
            }
        }
    }
}
