using ProductService.Api.Models;

namespace ProductService.Api.Services;

/// <summary>
/// Interface for Product operations
/// Defines contract for product management
/// </summary>
public interface IProductService
{
    // Read operations
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(string categoryId);
    Task<Product?> GetProductByIdAsync(string id, string categoryId);
    
    // Write operations
    Task<Product> CreateProductAsync(Product product);
    Task<Product?> UpdateProductAsync(string id, string categoryId, Product product);
    Task<bool> DeleteProductAsync(string id, string categoryId);
    
    // Search
    Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
}