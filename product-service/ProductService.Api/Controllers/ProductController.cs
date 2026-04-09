using Microsoft.AspNetCore.Mvc;
using ProductService.Api.Models;
using ProductService.Api.Services;

namespace ProductService.Api.Controllers;

/// <summary>
/// Products API Controller
/// Handles all product catalog operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    // ===========================
    // GET Endpoints
    // ===========================

    /// <summary>
    /// Get all products
    /// </summary>
    /// <returns>List of all products</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all products");
            return StatusCode(500, new { error = "An error occurred while retrieving products" });
        }
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    /// <param name="categoryId">Category identifier</param>
    /// <returns>List of products in the category</returns>
    [HttpGet("category/{categoryId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Product>>> GetProductsByCategory(string categoryId)
    {
        try
        {
            var products = await _productService.GetProductsByCategoryAsync(categoryId);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products for category {CategoryId}", categoryId);
            return StatusCode(500, new { error = "An error occurred while retrieving products" });
        }
    }

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="categoryId">Category ID (partition key)</param>
    /// <returns>Product details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Product>> GetProductById(
        string id, 
        [FromQuery] string categoryId)
    {
        if (string.IsNullOrWhiteSpace(categoryId))
        {
            return BadRequest(new { error = "categoryId query parameter is required" });
        }

        try
        {
            var product = await _productService.GetProductByIdAsync(id, categoryId);
            
            if (product == null)
            {
                return NotFound(new { error = $"Product with ID {id} not found" });
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {Id}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the product" });
        }
    }

    /// <summary>
    /// Search products by name or description
    /// </summary>
    /// <param name="q">Search term</param>
    /// <returns>List of matching products</returns>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Product>>> SearchProducts([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { error = "Search term 'q' is required" });
        }

        try
        {
            var products = await _productService.SearchProductsAsync(q);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products with term: {SearchTerm}", q);
            return StatusCode(500, new { error = "An error occurred while searching products" });
        }
    }

    /// <summary>
    /// Get variants for a specific product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="categoryId">Category ID (partition key)</param>
    /// <returns>List of product variants</returns>
    [HttpGet("{id}/variants")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductVariant>>> GetProductVariants(
        string id,
        [FromQuery] string categoryId)
    {
        if (string.IsNullOrWhiteSpace(categoryId))
        {
            return BadRequest(new { error = "categoryId query parameter is required" });
        }

        try
        {
            var product = await _productService.GetProductByIdAsync(id, categoryId);
            
            if (product == null)
            {
                return NotFound(new { error = $"Product with ID {id} not found" });
            }

            if (!product.HasVariants || product.Variants == null)
            {
                return NotFound(new { error = "This product does not have variants" });
            }

            return Ok(product.Variants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving variants for product {Id}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving product variants" });
        }
    }

    // ===========================
    // POST Endpoint
    // ===========================

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="product">Product details</param>
    /// <returns>Created product</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Basic validation
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            return BadRequest(new { error = "Product name is required" });
        }

        if (string.IsNullOrWhiteSpace(product.CategoryId))
        {
            return BadRequest(new { error = "Category ID is required" });
        }

        if (string.IsNullOrWhiteSpace(product.Sku))
        {
            return BadRequest(new { error = "SKU is required" });
        }

        try
        {
            var created = await _productService.CreateProductAsync(product);
            
            return CreatedAtAction(
                nameof(GetProductById),
                new { id = created.Id, categoryId = created.CategoryId },
                created
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, new { error = "An error occurred while creating the product" });
        }
    }

    // ===========================
    // PUT Endpoint
    // ===========================

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="categoryId">Category ID (partition key)</param>
    /// <param name="product">Updated product details</param>
    /// <returns>Updated product</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Product>> UpdateProduct(
        string id,
        [FromQuery] string categoryId,
        [FromBody] Product product)
    {
        if (string.IsNullOrWhiteSpace(categoryId))
        {
            return BadRequest(new { error = "categoryId query parameter is required" });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _productService.UpdateProductAsync(id, categoryId, product);
            
            if (updated == null)
            {
                return NotFound(new { error = $"Product with ID {id} not found" });
            }

            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {Id}", id);
            return StatusCode(500, new { error = "An error occurred while updating the product" });
        }
    }

    // ===========================
    // DELETE Endpoint
    // ===========================

    /// <summary>
    /// Delete a product (soft delete)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="categoryId">Category ID (partition key)</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProduct(
        string id,
        [FromQuery] string categoryId)
    {
        if (string.IsNullOrWhiteSpace(categoryId))
        {
            return BadRequest(new { error = "categoryId query parameter is required" });
        }

        try
        {
            var deleted = await _productService.DeleteProductAsync(id, categoryId);
            
            if (!deleted)
            {
                return NotFound(new { error = $"Product with ID {id} not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {Id}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the product" });
        }
    }
}