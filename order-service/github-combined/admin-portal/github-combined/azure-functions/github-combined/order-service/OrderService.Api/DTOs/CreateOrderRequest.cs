namespace OrderService.Api.DTOs;

/// <summary>
/// Request model for creating a new order
/// </summary>
public class CreateOrderRequest
{
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    
    // Shipping Address
    public string ShippingAddressLine1 { get; set; } = string.Empty;
    public string? ShippingAddressLine2 { get; set; }
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingState { get; set; } = string.Empty;
    public string ShippingZipCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = "USA";
    
    // Billing Address
    public string BillingAddressLine1 { get; set; } = string.Empty;
    public string? BillingAddressLine2 { get; set; }
    public string BillingCity { get; set; } = string.Empty;
    public string BillingState { get; set; } = string.Empty;
    public string BillingZipCode { get; set; } = string.Empty;
    public string BillingCountry { get; set; } = "USA";
    
    // Order Items
    public List<CreateOrderItemRequest> Items { get; set; } = new();
    
    // Optional fields
    public string? CustomerNotes { get; set; }
}

public class CreateOrderItemRequest
{
    public string ProductSku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? ProductDescription { get; set; }
    public string? VariantSku { get; set; }
    public string? VariantName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}