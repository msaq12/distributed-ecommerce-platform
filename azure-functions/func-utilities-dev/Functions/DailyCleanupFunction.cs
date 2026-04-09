using Azure.Storage.Blobs;
using func_utilities_dev.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace func_utilities_dev.Functions;

public class DailyCleanupFunction
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly OrderApiClient _orderApiClient;
    private readonly ILogger<DailyCleanupFunction> _logger;

    public DailyCleanupFunction(BlobServiceClient blobServiceClient, OrderApiClient orderApiClient, ILogger<DailyCleanupFunction> logger)
    {
        _blobServiceClient = blobServiceClient;
        _orderApiClient = orderApiClient;
        _logger = logger;
    }

    [Function(nameof(DailyCleanupFunction))]
    public async Task Run([TimerTrigger("0 0 2 * * *")] TimerInfo timerInfo)
    {
        _logger.LogInformation("DailyCleanupFunction triggered at {Time} UTC", DateTime.UtcNow);

        var exportsContainer = _blobServiceClient.GetBlobContainerClient("exports");
        var cutoff = DateTimeOffset.UtcNow.AddDays(-7);
        int deletedCount = 0;

        await foreach (var blob in exportsContainer.GetBlobsAsync())
        {
            if (blob.Properties.LastModified < cutoff)
            {
                await exportsContainer.DeleteBlobIfExistsAsync(blob.Name);
                deletedCount++;
                _logger.LogInformation("Deleted export blob: {BlobName}", blob.Name);
            }
        }

        _logger.LogInformation("Cleanup complete. Deleted {Count} export blobs older than 7 days", deletedCount);

        var statistics = await _orderApiClient.GetOrderStatisticsAsync();
        if (statistics is not null)
        {
            _logger.LogInformation(
                "Order statistics — Total: {Total}, Revenue: {Revenue:C}, Avg order value: {Avg:C}, Last 24h: {Last24h}",
                statistics.TotalOrders,
                statistics.TotalRevenue,
                statistics.AverageOrderValue,
                statistics.OrdersLast24Hours);
        }
    }
}
