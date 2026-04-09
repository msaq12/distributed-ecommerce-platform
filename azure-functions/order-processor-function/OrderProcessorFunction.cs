using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.ApplicationInsights;

namespace OrderProcessorFunction;

public class OrderProcessorFunction
{
    private readonly ILogger<OrderProcessorFunction> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _orderServiceUrl;
    private readonly TelemetryClient _telemetryClient;
    public OrderProcessorFunction(
        ILogger<OrderProcessorFunction> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        TelemetryClient telemetryClient)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _telemetryClient = telemetryClient;
        _orderServiceUrl = configuration["OrderServiceUrl"]
            ?? throw new InvalidOperationException("OrderServiceUrl not configured");
    }

    [Function(nameof(OrderProcessorFunction))]
    public async Task Run(
        [ServiceBusTrigger("order-processing", Connection = "ServiceBusConnectionString")]
        string messageBody)
    {
        _logger.LogInformation("Processing order message: {MessageBody}", messageBody);

        try
        {
            var orderMessage = JsonSerializer.Deserialize<OrderCreatedMessage>(messageBody);

            if (orderMessage == null)
            {
                _logger.LogError("Failed to deserialize message");
                return;
            }

            _logger.LogInformation("Processing Order {OrderId} ({OrderNumber})",
                orderMessage.OrderId, orderMessage.OrderNumber);
            _telemetryClient.TrackEvent("OrderProcessor.Started", new Dictionary<string, string>
            {
                { "OrderId", orderMessage.OrderId.ToString() },
                { "OrderNumber", orderMessage.OrderNumber },
                { "EventType", orderMessage.EventType }
            });
            // Simulate processing delay
            await Task.Delay(2000);

            // Update to Processing (StatusId = 3)
            _logger.LogInformation("🔄 Updating order to Processing...");
            await UpdateOrderStatus(orderMessage.OrderId, 3, "Order processing started");

            _telemetryClient.TrackEvent("OrderProcessor.StatusChanged", new Dictionary<string, string>
            {
                { "OrderId", orderMessage.OrderId.ToString() },
                { "NewStatus", "Processing" },
                { "StatusId", "3" }
            });

            // Simulate fulfillment
            await Task.Delay(3000);

            // Update to Shipped (StatusId = 4)
            _logger.LogInformation("📦 Updating order to Shipped...");
            await UpdateOrderStatus(orderMessage.OrderId, 4, "Order shipped");

            _telemetryClient.TrackEvent("OrderProcessor.StatusChanged", new Dictionary<string, string>
            {
                { "OrderId", orderMessage.OrderId.ToString() },
                { "NewStatus", "Shipped" },
                { "StatusId", "4" }
            });

            _logger.LogInformation("✅ Order {OrderNumber} processed successfully!",
                orderMessage.OrderNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing order message");
            _telemetryClient.TrackException(ex, new Dictionary<string, string>
            {
                { "Function", "OrderProcessorFunction" },
                { "MessageBody", messageBody }
            });
            throw;
        }
    }

    private async Task UpdateOrderStatus(int orderId, int statusId, string notes)
    {
        var request = new
        {
            orderStatusId = statusId,
            internalNotes = notes
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync(
            $"{_orderServiceUrl}/api/orders/{orderId}/status",
            content);

        response.EnsureSuccessStatusCode();
    }
}

public class OrderCreatedMessage
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
