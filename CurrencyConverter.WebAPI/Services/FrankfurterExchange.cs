using CurrencyConverter.WebAPI.Interfaces;
using CurrencyConverter.WebAPI.Models;
using System;
using System.Text.Json.Serialization;

namespace CurrencyConverter.WebAPI.Services
{
    public class FrankfurterExchange : IExchangeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public FrankfurterExchange(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }
        public async Task<string> GetLatest(string baseCurrency)
        {
            var client = _httpClientFactory.CreateClient("FrankfurterApiClient");

            var response = await client.GetAsync($"/latest?from={baseCurrency}");
            if (response.IsSuccessStatusCode)
            {
                return (await response.Content.ReadAsStringAsync());
            }

            return string.Empty;
        }

        public async Task<string> Convert(decimal amount, string from, string to)
        {
            var client = _httpClientFactory.CreateClient("FrankfurterApiClient");

            var response = await client.GetAsync($"/latest?amount={amount}&from={from}&to={to}");
            if (response.IsSuccessStatusCode)
            {
                return (await response.Content.ReadAsStringAsync());
            }

            return string.Empty;

        }

        public async Task<CurrencyRates?> History(string from, string to, string currency)
        {
            var client = _httpClientFactory.CreateClient("FrankfurterApiClient");

            var response = await client.GetAsync($"/{from}..{to}?to={currency}");
            if (response.IsSuccessStatusCode)
            {
                return (await response.Content.ReadFromJsonAsync<CurrencyRates>());
            }

            return null;
        }
    }

    

}
