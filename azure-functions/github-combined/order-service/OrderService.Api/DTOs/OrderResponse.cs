namespace OrderService.Api.DTOs;

/// <summary>
/// Response model for order data
/// </summary>
public class OrderResponse
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    
    public string ShippingAddress { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string FulfillmentStatus { get; set; } = string.Empty;
    
    public List<OrderItemResponse> Items { get; set; } = new();
    
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

public class OrderItemResponse
{
    public int OrderItemId { get; set; }
    public string ProductSku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalLineTotal { get; set; }
    public string FulfillmentStatus { get; set; } = string.Empty;
}