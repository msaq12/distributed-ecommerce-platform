using System.Text.Json.Serialization;

namespace ProductService.Api.Models;

/// <summary>
/// Represents a furniture product in the catalog
/// Supports both simple products and products with variants
/// Maps to Cosmos DB Products container
/// </summary>
public class Product
{
    // ===========================
    // Primary Identifier
    // ===========================

    /// <summary>
    /// Cosmos DB document ID (unique identifier)
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    // ===========================
    // Basic Information
    // ===========================

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    // ===========================
    // Categorization (Partition Key)
    // ===========================

    /// <summary>
    /// Category ID - used as partition key in Cosmos DB
    /// </summary>
    [JsonPropertyName("categoryId")]
    public string CategoryId { get; set; } = string.Empty;

    [JsonPropertyName("categoryName")]
    public string CategoryName { get; set; } = string.Empty;

    // ===========================
    // Product Identifiers
    // ===========================

    [JsonPropertyName("sku")]
    public string Sku { get; set; } = string.Empty;

    [JsonPropertyName("barcode")]
    public string? Barcode { get; set; }

    [JsonPropertyName("vendor")]
    public string? Vendor { get; set; }

    [JsonPropertyName("supplier")]
    public string? Supplier { get; set; }

    // ===========================
    // Variant Support
    // ===========================

    /// <summary>
    /// Indicates if this product has variants (e.g., different colors/sizes)
    /// </summary>
    [JsonPropertyName("hasVariants")]
    public bool HasVariants { get; set; } = false;

    // ===========================
    // Pricing (for SIMPLE products)
    // ===========================

    /// <summary>
    /// Price for simple products (null if HasVariants = true)
    /// </summary>
    [JsonPropertyName("price")]
    public decimal? Price { get; set; }

    [JsonPropertyName("compareAtPrice")]
    public decimal? CompareAtPrice { get; set; }

    [JsonPropertyName("costPrice")]
    public decimal? CostPrice { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "USD";

    // ===========================
    // Pricing (for VARIANT products)
    // ===========================

    /// <summary>
    /// Price range for variant products (null if HasVariants = false)
    /// </summary>
    [JsonPropertyName("priceRange")]
    public PriceRange? PriceRange { get; set; }

    // ===========================
    // Variants (for VARIANT products)
    // ===========================

    /// <summary>
    /// List of product variants (null if HasVariants = false)
    /// </summary>
    [JsonPropertyName("variants")]
    public List<ProductVariant>? Variants { get; set; }

    // ===========================
    // Media
    // ===========================

    [JsonPropertyName("images")]
    public List<ProductImage> Images { get; set; } = new();

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    // ===========================
    // Inventory (for SIMPLE products)
    // ===========================

    /// <summary>
    /// Inventory for simple products (null if HasVariants = true)
    /// For variant products, inventory is tracked per variant
    /// </summary>
    [JsonPropertyName("inventory")]
    public ProductInventory? Inventory { get; set; }

    // ===========================
    // Physical Properties
    // ===========================

    [JsonPropertyName("dimensions")]
    public ProductDimensions? Dimensions { get; set; }

    [JsonPropertyName("weight")]
    public ProductWeight? Weight { get; set; }

    // ===========================
    // SEO
    // ===========================

    [JsonPropertyName("seo")]
    public ProductSeo? Seo { get; set; }

    // ===========================
    // Status Flags
    // ===========================

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; set; } = false;

    [JsonPropertyName("isFeatured")]
    public bool IsFeatured { get; set; } = false;

    [JsonPropertyName("requiresShipping")]
    public bool RequiresShipping { get; set; } = true;

    [JsonPropertyName("taxable")]
    public bool Taxable { get; set; } = true;

    // ===========================
    // Audit Fields
    // ===========================

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("createdBy")]
    public string CreatedBy { get; set; } = "system";

    [JsonPropertyName("updatedBy")]
    public string UpdatedBy { get; set; } = "system";
}

// ===========================
// Nested Classes
// ===========================

/// <summary>
/// Represents a product variant (e.g., Blue/Large, Black/Small)
/// </summary>
public class ProductVariant
{
    [JsonPropertyName("variantId")]
    public string VariantId { get; set; } = string.Empty;

    [JsonPropertyName("sku")]
    public string Sku { get; set; } = string.Empty;

    [JsonPropertyName("barcode")]
    public string? Barcode { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Variant attributes (e.g., Color: Blue, Size: Large)
    /// </summary>
    [JsonPropertyName("attributes")]
    public List<VariantAttribute> Attributes { get; set; } = new();

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("compareAtPrice")]
    public decimal? CompareAtPrice { get; set; }

    [JsonPropertyName("costPrice")]
    public decimal? CostPrice { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("inventory")]
    public ProductInventory Inventory { get; set; } = new();

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Represents a variant attribute (e.g., Color, Size)
/// </summary>
public class VariantAttribute
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Price range for products with variants
/// </summary>
public class PriceRange
{
    [JsonPropertyName("min")]
    public decimal Min { get; set; }

    [JsonPropertyName("max")]
    public decimal Max { get; set; }
}

public class ProductImage
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("altText")]
    public string? AltText { get; set; }

    [JsonPropertyName("isPrimary")]
    public bool IsPrimary { get; set; }

    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; set; }
}

public class ProductInventory
{
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("tracked")]
    public bool Tracked { get; set; } = true;

    [JsonPropertyName("allowBackorder")]
    public bool AllowBackorder { get; set; } = false;

    [JsonPropertyName("lowStockThreshold")]
    public int LowStockThreshold { get; set; } = 5;

    [JsonPropertyName("warehouse")]
    public string? Warehouse { get; set; }
}

public class ProductDimensions
{
    [JsonPropertyName("length")]
    public decimal Length { get; set; }

    [JsonPropertyName("width")]
    public decimal Width { get; set; }

    [JsonPropertyName("height")]
    public decimal Height { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = "inches";
}

public class ProductWeight
{
    [JsonPropertyName("value")]
    public decimal Value { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = "lbs";
}

public class ProductSeo
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("handle")]
    public string? Handle { get; set; }
}