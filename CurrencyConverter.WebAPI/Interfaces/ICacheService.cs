namespace CurrencyConverter.WebAPI.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetOrSetCacheAsync<T>(string cacheKey, Func<Task<T>> fetchData, TimeSpan cacheDuration);
    }
}
