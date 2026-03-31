namespace OrderService.Data.Entities;

/// <summary>
/// Represents a customer order
/// Maps to Orders table
/// </summary>
public class Order
{
    // ===========================
    // Primary Key
    // ===========================

    /// <summary>
    /// Primary key - auto-generated
    /// </summary>
    public int OrderId { get; set; }

    // ===========================
    // Order Information
    // ===========================

    /// <summary>
    /// Human-readable order number (e.g., "ORD-2024-001234")
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// When the order was placed (UTC)
    /// </summary>
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    // ===========================
    // Customer Information
    // ===========================

    /// <summary>
    /// Customer's email address
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Customer's full name
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    // ===========================
    // Shipping Address
    // ===========================

    public string ShippingAddressLine1 { get; set; } = string.Empty;
    public string? ShippingAddressLine2 { get; set; }
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingState { get; set; } = string.Empty;
    public string ShippingZipCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = "USA";

    // ===========================
    // Billing Address
    // ===========================

    public string BillingAddressLine1 { get; set; } = string.Empty;
    public string? BillingAddressLine2 { get; set; }
    public string BillingCity { get; set; } = string.Empty;
    public string BillingState { get; set; } = string.Empty;
    public string BillingZipCode { get; set; } = string.Empty;
    public string BillingCountry { get; set; } = "USA";

    // ===========================
    // Financial Information
    // ===========================

    /// <summary>
    /// Subtotal before tax and shipping
    /// </summary>
    public decimal SubtotalAmount { get; set; }

    /// <summary>
    /// Tax amount
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Shipping cost
    /// </summary>
    public decimal ShippingAmount { get; set; }

    /// <summary>
    /// Discount applied to order
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Final total amount (Subtotal + Tax + Shipping - Discount)
    /// </summary>
    public decimal TotalAmount { get; set; }

    // ===========================
    // Status Information
    // ===========================

    /// <summary>
    /// Foreign key to OrderStatuses
    /// </summary>
    public int OrderStatusId { get; set; }

    // ===========================
    // Payment Information
    // ===========================

    /// <summary>
    /// Payment method used (e.g., "CreditCard", "PayPal")
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Payment status ("Pending", "Paid", "Failed")
    /// </summary>
    public string PaymentStatus { get; set; } = "Pending";

    /// <summary>
    /// Transaction ID from payment gateway
    /// </summary>
    public string? PaymentTransactionId { get; set; }

    // ===========================
    // Fulfillment Information
    // ===========================

    /// <summary>
    /// Fulfillment status ("Pending", "Processing", "Shipped", "Delivered")
    /// </summary>
    public string FulfillmentStatus { get; set; } = "Pending";

    /// <summary>
    /// Shipping carrier (e.g., "FedEx", "UPS")
    /// </summary>
    public string? ShippingCarrier { get; set; }

    /// <summary>
    /// Tracking number for shipment
    /// </summary>
    public string? TrackingNumber { get; set; }

    /// <summary>
    /// When order was shipped (nullable - not shipped yet)
    /// </summary>
    public DateTime? ShippedDate { get; set; }

    /// <summary>
    /// When order was delivered (nullable - not delivered yet)
    /// </summary>
    public DateTime? DeliveredDate { get; set; }

    // ===========================
    // Notes
    // ===========================

    /// <summary>
    /// Notes from customer (e.g., special delivery instructions)
    /// </summary>
    public string? CustomerNotes { get; set; }

    /// <summary>
    /// Internal notes (not visible to customer)
    /// </summary>
    public string? InternalNotes { get; set; }

    // ===========================
    // Audit Fields
    // ===========================

    /// <summary>
    /// When this record was created (UTC)
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Who created this record
    /// </summary>
    public string CreatedBy { get; set; } = "System";

    /// <summary>
    /// When this record was last modified (UTC)
    /// </summary>
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Who last modified this record
    /// </summary>
    public string ModifiedBy { get; set; } = "System";

    /// <summary>
    /// Soft delete flag (true = deleted, but still in database)
    /// </summary>
    public bool IsDeleted { get; set; }

    // ===========================
    // Navigation Properties
    // ===========================

    /// <summary>
    /// The status of this order (lazy-loaded)
    /// This is like a foreign key lookup
    /// </summary>
    public OrderStatus? OrderStatus { get; set; }

    /// <summary>
    /// Line items in this order
    /// This is like a List<OrderItem> in memory
    /// </summary>
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    // ===========================
    // Calculated Properties (not mapped to database)
    // ===========================

    /// <summary>
    /// Full shipping address as single string
    /// </summary>
    public string ShippingAddressFull
    {
        get
        {
            var line2 = string.IsNullOrWhiteSpace(ShippingAddressLine2)
                ? ""
                : $", {ShippingAddressLine2}";

            return $"{ShippingAddressLine1}{line2}, {ShippingCity}, {ShippingState} {ShippingZipCode}";
        }
    }

    /// <summary>
    /// Number of items in this order
    /// </summary>
    public int ItemCount => OrderItems?.Count ?? 0;

    /// <summary>
    /// Total quantity of all items
    /// </summary>
    public int TotalQuantity => OrderItems?.Sum(i => i.Quantity) ?? 0;
}