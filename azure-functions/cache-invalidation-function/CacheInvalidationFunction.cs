using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventGrid;
using System.Text.Json;
using StackExchange.Redis;
using Microsoft.ApplicationInsights;

namespace CacheInvalidationFunction;

public class CacheInvalidationFunction
{
    private readonly ILogger<CacheInvalidationFunction> _logger;
    private readonly IConnectionMultiplexer _redis;
    private readonly TelemetryClient _telemetryClient;

    public CacheInvalidationFunction(
        ILogger<CacheInvalidationFunction> logger,
        IConnectionMultiplexer redis,
        TelemetryClient telemetryClient)
    {
        _logger = logger;
        _redis = redis;
        _telemetryClient = telemetryClient;
    }

    [Function(nameof(CacheInvalidationFunction))]
    public async Task Run(
        [EventGridTrigger] EventGridEvent eventGridEvent)
    {
        _logger.LogInformation("🔔 Event received: {EventType}", eventGridEvent.EventType);
        _logger.LogInformation("Subject: {Subject}", eventGridEvent.Subject);

        try
        {
            // Parse event data
            var eventData = JsonSerializer.Deserialize<ProductEventData>(
                eventGridEvent.Data.ToString() ?? "{}"
            );

            if (eventData == null || string.IsNullOrEmpty(eventData.ProductId))
            {
                _logger.LogWarning("Invalid event data received");
                return;
            }

            _logger.LogInformation("Processing event for Product: {ProductId}, Category: {CategoryId}",
                eventData.ProductId, eventData.CategoryId);
            _telemetryClient.TrackEvent("CacheInvalidation.Executing", new Dictionary<string, string>
            {
                { "Product Id", eventData.ProductId},
                { "Product Name", eventData.Name }
            });
            // Invalidate cache
            var db = _redis.GetDatabase();
            var cacheKey = $"product:{eventData.CategoryId}:{eventData.ProductId}";

            var deleted = await db.KeyDeleteAsync(cacheKey);

            if (deleted)
            {
                _logger.LogInformation("✅ Cache invalidated: {CacheKey}", cacheKey);
            }
            else
            {
                _logger.LogInformation("ℹ️ Cache key not found (may not have been cached): {CacheKey}", cacheKey);
            }

            // Also invalidate category cache (if exists)
            var categoryCacheKey = $"products:category:{eventData.CategoryId}";
            await db.KeyDeleteAsync(categoryCacheKey);
            _logger.LogInformation("✅ Category cache invalidated: {CacheKey}", categoryCacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing event");
            throw; // Let Event Grid retry
        }
    }
}

public class ProductEventData
{
    public string ProductId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Sku { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
