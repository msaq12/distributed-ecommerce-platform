using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace OrderService.Api.Services;

public class ServiceBusMessagePublisher : IMessagePublisher
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ILogger<ServiceBusMessagePublisher> _logger;
    private readonly string _queueName = "order-processing";

    public ServiceBusMessagePublisher(
        ServiceBusClient serviceBusClient,
        ILogger<ServiceBusMessagePublisher> logger)
    {
        _serviceBusClient = serviceBusClient;
        _logger = logger;
    }

    public async Task PublishOrderCreatedAsync(
        int orderId,
        string orderNumber,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Publishing order created message for Order {OrderId} ({OrderNumber})",
                orderId,
                orderNumber);

            // Create message payload
            var messagePayload = new
            {
                OrderId = orderId,
                OrderNumber = orderNumber,
                EventType = "OrderCreated",
                Timestamp = DateTime.UtcNow
            };

            var messageBody = JsonSerializer.Serialize(messagePayload);

            // Create message
            var message = new ServiceBusMessage(messageBody)
            {
                ContentType = "application/json",
                MessageId = Guid.NewGuid().ToString(),
                CorrelationId = orderNumber
            };

            // Add custom properties
            message.ApplicationProperties.Add("OrderId", orderId);
            message.ApplicationProperties.Add("OrderNumber", orderNumber);
            message.ApplicationProperties.Add("EventType", "OrderCreated");

            // Send message
            ServiceBusSender sender = _serviceBusClient.CreateSender(_queueName);
            await sender.SendMessageAsync(message, cancellationToken);

            _logger.LogInformation(
                "✅ Order created message published successfully for Order {OrderId}",
                orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "❌ Failed to publish order created message for Order {OrderId}",
                orderId);
            // Don't throw - order was created successfully, message failure shouldn't fail the request
        }
    }
}