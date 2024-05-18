using CurrencyConverter.WebAPI.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyConverter.WebAPI.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<T?> GetOrSetCacheAsync<T>(string cacheKey, Func<Task<T>> fetchData, TimeSpan cacheDuration)
        {
            if (_cache.TryGetValue(cacheKey, out T? cachedData))
            {
                return cachedData;
            }

            var data = await fetchData();

            _cache.Set(cacheKey, data, cacheDuration);

            return data;
        }
    }

    

}
