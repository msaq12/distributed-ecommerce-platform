namespace ProductService.Api.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class;
    Task DeleteAsync(string key);
    Task<bool> LockAsync(string key, TimeSpan expiration);
    Task UnlockAsync(string key);
}
