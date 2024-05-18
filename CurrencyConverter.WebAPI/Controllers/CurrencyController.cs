using CurrencyConverter.WebAPI.Interfaces;
using CurrencyConverter.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CurrencyConverter.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
 
    public class CurrencyController: ControllerBase
    {
        private IExchangeService _exchangeService { get; set; }
        private ILogger<CurrencyController> _logger { get; set; }
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cache;
        private readonly int cacheDurationInSeconds;
        public CurrencyController(IExchangeService exchangeService, ILogger<CurrencyController> logger,
            IConfiguration configuration, ICacheService cache)
        {
            _exchangeService = exchangeService;
            _logger = logger;
            _configuration = configuration;
            _cache = cache;
            cacheDurationInSeconds = int.TryParse( _configuration["CachDurationInSeconds"] , out int result) ? result : 60;
        }
        [HttpGet]
        [Route("latest")]
        public async Task<IActionResult> GetLatestExchangeRates(string baseCurrency = "EUR")
        {
            try
            {
                _logger.LogInformation($"GetLatestExchangeRates called => {baseCurrency}");

                if (baseCurrency.Length != 3)
                {
                    return BadRequest("The currency code should be in 3 characters");
                }

                string cacheKey = $"GetLatestExchangeRates_{baseCurrency}";

                string? result = await _cache.GetOrSetCacheAsync<string?>(cacheKey, 
                    async () => await _exchangeService.GetLatest(baseCurrency),
                    TimeSpan.FromSeconds(cacheDurationInSeconds));


                if (String.IsNullOrEmpty(result))
                {
                    _logger.LogInformation($"GetLatestExchangeRates Not Found => {baseCurrency}");
                    return NotFound();
                }

                _logger.LogInformation($"Successfully retrieved GetLatestExchangeRates => {baseCurrency}");
                return Ok(result);

            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error occurred while retrieving GetLatestExchangeRates: {baseCurrency}", baseCurrency);
                return StatusCode(500, "Internal server error");
            }
            

                     
        }

        [HttpGet]
        [Route("convert")]
        public async Task<IActionResult> ConvertAmount(decimal amount = 1, string fromCurrency= "EUR", string toCurrency = "USD")
        {

            try
            {
                _logger.LogInformation($"ConvertAmount called => {amount}, {fromCurrency}, {toCurrency}");

                if (amount <= 0)
                {
                    return BadRequest("The amount should be greater than Zero(0)");
                }
                if (fromCurrency.Length != 3)
                {
                    return BadRequest($"The {nameof(fromCurrency)} should be in 3 characters");
                }

                if (toCurrency.Length != 3)
                {
                    return BadRequest($"The {nameof(toCurrency)} should be in 3 characters");
                }

                var exclusionList = _configuration["CONVERT_EXCLUSION_LIST"];
                if (exclusionList != null)
                {
                    var exlusion = JsonSerializer.Deserialize<List<string>>(exclusionList);
                    if (exclusionList.Contains(fromCurrency.ToUpper()))
                    {
                        return BadRequest($"The currency {fromCurrency} is not allowed for conversion");
                    }else if (exclusionList.Contains(toCurrency.ToUpper()))
                    {
                        return BadRequest($"The currency {toCurrency} is not allowed for conversion");
                    }
                }

                string cacheKey = $"ConvertAmount_{amount}_{fromCurrency}_{toCurrency}";

                string? result = await _cache.GetOrSetCacheAsync<string?>(cacheKey,
                    async () => await _exchangeService.Convert(amount, fromCurrency, toCurrency),
                    TimeSpan.FromSeconds(cacheDurationInSeconds));

                if (String.IsNullOrEmpty(result))
                {
                    _logger.LogInformation($"ConvertAmount Not Found => {amount}, {fromCurrency}, {toCurrency}");
                    return NotFound();
                }

                _logger.LogInformation($"Successfully retrieved ConvertAmount => {amount}, {fromCurrency}, {toCurrency}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving ConvertAmount: {amount} ,{fromCurrency}, {toCurrency}", amount, fromCurrency, toCurrency);
                return StatusCode(500, "Internal server error");
            }

            
        }

        


        [HttpGet]
        [Route("history")]
        public async Task<IActionResult> History(string fromDate = "2024-05-01" , string toDate = "2024-05-17", string currency = "AUD",  int page=1, int pageSize=10)
        {
            try
            {
                _logger.LogInformation($"History called => {fromDate} ,{toDate}, {currency}, {page}, {pageSize}");

                if (!IsValidDate(fromDate, out var fromDateValue))
                    return BadRequest($"The {fromDate} is not valid");

                if (!IsValidDate(toDate, out var toDateValue))
                    return BadRequest($"The {toDate} is not valid");

                if (currency.Length != 3)
                {
                    return BadRequest("The currency code should be in 3 characters");
                }

                if (fromDateValue > toDateValue)
                {
                    return BadRequest("'to' date should be greater than 'from' date");
                }


                string cacheKey = $"History_{fromDate}_{toDate}_{currency}";

                CurrencyRates? result = await _cache.GetOrSetCacheAsync<CurrencyRates?>(cacheKey,
                    async () => await _exchangeService.History(fromDate, toDate, currency),
                    TimeSpan.FromSeconds(cacheDurationInSeconds));

                if (result == null)
                {
                    _logger.LogInformation($"History Not Found => {fromDate} ,{toDate}, {currency}, {page}, {pageSize}");
                    return NotFound();
                }

                _logger.LogInformation($"Successfully retrieved History => {fromDate} ,{toDate}, {currency}, {page}, {pageSize}");
                return Ok(result.Rates.Skip((page - 1) * pageSize).Take(pageSize));
            }
            catch  (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving History: {fromDate} ,{toDate}, {currency}, {page}, {pageSize}", fromDate, toDate, currency, page, pageSize);
                return StatusCode(500, "Internal server error");
            }
            
        }

        private bool IsValidDate(string dateString, out DateTime dateValue)
        {
            //DateTime dateValue;
            string format = "yyyy-MM-dd";
            bool isValid = DateTime.TryParseExact(
                dateString,
                format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dateValue
            );

            return isValid;
        }

    }
}
