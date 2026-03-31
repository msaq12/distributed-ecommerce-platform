using OrderService.Api.DTOs;

namespace OrderService.Api.Services;

/// <summary>
/// Order service interface defining business operations
/// </summary>
public interface IOrderService
{
    // Retrieve operations
    Task<IEnumerable<OrderResponse>> GetAllOrdersAsync();
    Task<OrderResponse?> GetOrderByIdAsync(int orderId);
    Task<OrderResponse?> GetOrderByNumberAsync(string orderNumber);
    Task<IEnumerable<OrderResponse>> GetOrdersByCustomerEmailAsync(string email);
    Task<IEnumerable<OrderResponse>> GetOrdersByStatusAsync(int statusId);

    // Create operations
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);

    // Update operations
    Task<OrderResponse?> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request);

    // Statistics
    Task<Dictionary<string, object>> GetOrderStatisticsAsync();
}