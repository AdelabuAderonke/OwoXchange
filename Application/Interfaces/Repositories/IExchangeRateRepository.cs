using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IExchangeRateRepository
    {
        Task<ExchangeRate> GetRealTimeRateAsync(string baseCurrencyCode, string targetCurrencyCode);
        Task<IEnumerable<ExchangeRate>> GetAllRealTimeRate(string baseCurrencyCode);
        Task AddRealTimeRateAsync(ExchangeRate exchangeRate);
        Task AddHistoricalRateAsync(ExchangeRate exchangeRate);

        Task<DateTime> GetcurrentRateTimeAsync(string baseCurrencyCode);
        
    }
}

