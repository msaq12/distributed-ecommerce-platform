namespace OrderService.Api.Services;

public interface IMessagePublisher
{
    Task PublishOrderCreatedAsync(int orderId, string orderNumber, CancellationToken cancellationToken = default);
}