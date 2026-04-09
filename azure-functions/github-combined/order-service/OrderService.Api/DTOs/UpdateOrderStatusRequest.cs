namespace OrderService.Api.DTOs;

/// <summary>
/// Request to update order status
/// </summary>
public class UpdateOrderStatusRequest
{
    public int OrderStatusId { get; set; }
    public string? InternalNotes { get; set; }
}