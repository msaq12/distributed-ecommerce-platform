namespace ProductService.Api.Services;

public class NullCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key) where T : class => Task.FromResult<T?>(null);
    public Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class => Task.CompletedTask;
    public Task DeleteAsync(string key) => Task.CompletedTask;
    public Task<bool> LockAsync(string key, TimeSpan expiration) => Task.FromResult(true);
    public Task UnlockAsync(string key) => Task.CompletedTask;
}
