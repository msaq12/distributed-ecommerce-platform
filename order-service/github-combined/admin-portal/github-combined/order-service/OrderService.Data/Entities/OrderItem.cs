namespace OrderService.Data.Entities;

/// <summary>
/// Represents a line item in an order
/// Maps to OrderItems table
/// </summary>
public class OrderItem
{
    // ===========================
    // Primary Key
    // ===========================

    /// <summary>
    /// Primary key - auto-generated
    /// </summary>
    public int OrderItemId { get; set; }

    // ===========================
    // Foreign Key
    // ===========================

    /// <summary>
    /// Foreign key to parent order
    /// </summary>
    public int OrderId { get; set; }

    // ===========================
    // Product Information
    // ===========================

    /// <summary>
    /// Product SKU (Stock Keeping Unit)
    /// Links to product in Cosmos DB
    /// </summary>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// Product name at time of order
    /// Denormalized - product name might change later
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Product description at time of order
    /// </summary>
    public string? ProductDescription { get; set; }

    // ===========================
    // Variant Information
    // ===========================

    /// <summary>
    /// Variant SKU if product has variants (e.g., size, color)
    /// </summary>
    public string? VariantSku { get; set; }

    /// <summary>
    /// Variant name (e.g., "Blue, Large")
    /// </summary>
    public string? VariantName { get; set; }

    // ===========================
    // Pricing
    // ===========================

    /// <summary>
    /// Quantity ordered
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Price per unit at time of order
    /// Important: Store historical price!
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Line total before discounts (Quantity * UnitPrice)
    /// </summary>
    public decimal LineTotal { get; set; }

    /// <summary>
    /// Discount applied to this line item
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Final line total after discount
    /// </summary>
    public decimal FinalLineTotal { get; set; }

    // ===========================
    // Fulfillment
    // ===========================

    /// <summary>
    /// Fulfillment status for this item
    /// ("Pending", "Fulfilled", "Backordered", "Cancelled")
    /// </summary>
    public string FulfillmentStatus { get; set; } = "Pending";

    /// <summary>
    /// How many have been fulfilled (for partial fulfillment)
    /// </summary>
    public int FulfilledQuantity { get; set; }

    // ===========================
    // Audit Fields
    // ===========================

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }

    // ===========================
    // Navigation Properties
    // ===========================

    /// <summary>
    /// Parent order (lazy-loaded)
    /// </summary>
    public Order? Order { get; set; }

    // ===========================
    // Calculated Properties
    // ===========================

    /// <summary>
    /// Whether this item is fully fulfilled
    /// </summary>
    public bool IsFullyFulfilled => FulfilledQuantity >= Quantity;

    /// <summary>
    /// Remaining quantity to fulfill
    /// </summary>
    public int RemainingQuantity => Math.Max(0, Quantity - FulfilledQuantity);

    /// <summary>
    /// Discount percentage
    /// </summary>
    public decimal DiscountPercentage =>
        LineTotal > 0 ? (DiscountAmount / LineTotal) * 100 : 0;
}