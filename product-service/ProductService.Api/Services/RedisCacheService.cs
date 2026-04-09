using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights;

namespace ProductService.Api.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly TelemetryClient _telemetryClient;

    public RedisCacheService(IConnectionMultiplexer redis,
    ILogger<RedisCacheService> logger,
    TelemetryClient telemetryClient)
    {
        _redis = redis.GetDatabase();
        _logger = logger;
        _telemetryClient = telemetryClient;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var value = await _redis.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                _logger.LogDebug("Cache MISS: {Key}", key);
                _telemetryClient.TrackMetric("Cache.Miss", 1);
                return null;
            }

            _logger.LogDebug("Cache HIT: {Key}", key);
            _telemetryClient.TrackMetric("Cache.Hit", 1);
            return JsonSerializer.Deserialize<T>(value.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis GET error for key: {Key}", key);
            _telemetryClient.TrackMetric("Cache.Error", 1);
            return null; // Fail gracefully
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _redis.StringSetAsync(key, json, expiration);
            _logger.LogDebug("Cache SET: {Key} (TTL: {TTL}s)", key, expiration.TotalSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis SET error for key: {Key}", key);
            // Don't throw - cache failures shouldn't break the app
        }
    }

    public async Task DeleteAsync(string key)
    {
        try
        {
            await _redis.KeyDeleteAsync(key);
            _logger.LogDebug("Cache DELETE: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis DELETE error for key: {Key}", key);
        }
    }

    public async Task<bool> LockAsync(string key, TimeSpan expiration)
    {
        try
        {
            var lockAcquired = await _redis.StringSetAsync(key, "locked", expiration, When.NotExists);
            if (lockAcquired)
            {
                _logger.LogDebug("Lock ACQUIRED: {Key}", key);
            }
            else
            {
                _logger.LogDebug("Lock FAILED: {Key} (already locked)", key);
            }
            return lockAcquired;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis LOCK error for key: {Key}", key);
            return false;
        }
    }

    public async Task UnlockAsync(string key)
    {
        try
        {
            await _redis.KeyDeleteAsync(key);
            _logger.LogDebug("Lock RELEASED: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis UNLOCK error for key: {Key}", key);
        }
    }
}
