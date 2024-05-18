
using CurrencyConverter.WebAPI.Models;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyConverter.WebAPI.Interfaces
{
    public interface IExchangeService
    {
        Task<string> GetLatest(string baseCurrency);
        Task<string> Convert(decimal amount, string from, string to);
        Task<CurrencyRates?> History(string from, string to, string currency);
    }

    
}
