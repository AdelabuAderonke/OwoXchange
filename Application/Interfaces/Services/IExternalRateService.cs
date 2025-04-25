using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IExternalRateService
    {
        Task<ExchangeRate> GetCurrentRatesAsync(string baseCurrencyCode, string targetCurrencyCode);
        Task<IEnumerable<ExchangeRate>> GetHistoricalRatesAsync(string baseCurrencyCode, string targetCurrencyCode, DateTime startDate, DateTime endDate);
        
    }
}
