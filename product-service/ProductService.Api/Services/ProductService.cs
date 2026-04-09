using Microsoft.Azure.Cosmos;
using ProductService.Api.Models;
using Azure.Messaging.EventGrid;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
namespace ProductService.Api.Services;

/// <summary>
/// Product service implementation using Cosmos DB
/// </summary>
public class ProductService : IProductService
{
    private readonly Container _container;
    private readonly ILogger<ProductService> _logger;
    private readonly EventGridPublisherClient _eventGridClient;
    private readonly TelemetryClient _telemetryClient;
    private readonly ICacheService _cache;

    public ProductService(
        CosmosClient cosmosClient,
        IConfiguration configuration,
        ILogger<ProductService> logger,
        EventGridPublisherClient eventGridClient,
        TelemetryClient telemetryClient,
        ICacheService cache)
    {
        _logger = logger;
        _eventGridClient = eventGridClient;
        _telemetryClient = telemetryClient;
        _cache = cache;

        var databaseName = configuration["CosmosDb:DatabaseName"]
            ?? throw new InvalidOperationException("CosmosDb:DatabaseName not configured");
        var containerName = configuration["CosmosDb:ContainerName"]
            ?? throw new InvalidOperationException("CosmosDb:ContainerName not configured");

        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    // ===========================
    // Validation Helper
    // ===========================

    private void ValidateProduct(Product product)
    {
        if (product.HasVariants)
        {
            // Variant product validation
            if (product.Variants == null || !product.Variants.Any())
            {
                throw new InvalidOperationException(
                    "Variant products must have at least one variant");
            }

            // Ensure each variant has required fields
            foreach (var variant in product.Variants)
            {
                if (string.IsNullOrWhiteSpace(variant.VariantId))
                    throw new InvalidOperationException("Variant must have a variantId");

                if (string.IsNullOrWhiteSpace(variant.Sku))
                    throw new InvalidOperationException("Variant must have a SKU");

                if (variant.Price <= 0)
                    throw new InvalidOperationException("Variant price must be greater than 0");
            }

            // Calculate and set price range
            var minPrice = product.Variants.Min(v => v.Price);
            var maxPrice = product.Variants.Max(v => v.Price);
            product.PriceRange = new PriceRange { Min = minPrice, Max = maxPrice };
        }
        else
        {
            // Simple product validation
            if (!product.Price.HasValue || product.Price <= 0)
            {
                throw new InvalidOperationException(
                    "Simple products must have a valid price");
            }

            // Clear variant-related fields
            product.Variants = null;
            product.PriceRange = null;
        }
    }

    // ===========================
    // Read Operations
    // ===========================

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        _logger.LogInformation("Getting all products");

        try
        {
            var query = _container.GetItemQueryIterator<Product>(
                "SELECT * FROM c WHERE c.isDeleted = false"
            );

            var results = new List<Product>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response);
            }

            _logger.LogInformation("Retrieved {Count} products", results.Count);

            // Track metric
            _telemetryClient.TrackMetric("ProductCatalog.TotalProducts", results.Count);

            return results;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error getting all products");
            _telemetryClient.TrackException(ex);
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string categoryId)
    {
        var cacheKey = $"products:category:{categoryId}";
        var lockKey = $"lock:{cacheKey}";

        try
        {
            // Try cache first
            var cachedProducts = await _cache.GetAsync<List<Product>>(cacheKey);
            if (cachedProducts != null)
            {
                _logger.LogInformation("Cache HIT for category: {CategoryId} ({Count} products)",
                    categoryId, cachedProducts.Count);
                return cachedProducts;
            }

            // Acquire lock
            var lockAcquired = await _cache.LockAsync(lockKey, TimeSpan.FromSeconds(10));
            if (!lockAcquired)
            {
                await Task.Delay(100);
                return await GetProductsByCategoryAsync(categoryId);
            }

            try
            {
                // Double-check cache
                cachedProducts = await _cache.GetAsync<List<Product>>(cacheKey);
                if (cachedProducts != null) return cachedProducts;

                // Query Cosmos DB
                _logger.LogInformation("Cache MISS for category: {CategoryId}, querying Cosmos DB", categoryId);
                var query = new QueryDefinition("SELECT * FROM c WHERE c.categoryId = @categoryId AND c.isActive = true")
                    .WithParameter("@categoryId", categoryId);

                var iterator = _container.GetItemQueryIterator<Product>(query, requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(categoryId)
                });

                var products = new List<Product>();
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    products.AddRange(response);
                }

                // Cache for 10 minutes (category data changes less frequently)
                await _cache.SetAsync(cacheKey, products, TimeSpan.FromMinutes(10));
                _logger.LogInformation("Cached {Count} products for category: {CategoryId}",
                    products.Count, categoryId);

                return products;
            }
            finally
            {
                await _cache.UnlockAsync(lockKey);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products for category: {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task<Product?> GetProductByIdAsync(string id, string categoryId)
    {
        var cacheKey = $"product:{categoryId}:{id}";
        var lockKey = $"lock:{cacheKey}";

        using var operation = _telemetryClient.StartOperation<DependencyTelemetry>("Redis.GetProduct");

        try
        {
            // 1. Try cache first
            var cachedProduct = await _cache.GetAsync<Product>(cacheKey);
            if (cachedProduct != null)
            {
                operation.Telemetry.Success = true;
                operation.Telemetry.ResultCode = "CacheHit";
                _logger.LogInformation("Cache HIT for product: {Id}", id);
                return cachedProduct;
            }

            // 2. Cache miss - acquire distributed lock
            var lockAcquired = await _cache.LockAsync(lockKey, TimeSpan.FromSeconds(10));
            if (!lockAcquired)
            {
                // Another request is loading - wait and retry
                _logger.LogDebug("Lock not acquired for {Id}, retrying...", id);
                await Task.Delay(100);
                return await GetProductByIdAsync(id, categoryId); // Recursive retry
            }

            try
            {
                // 3. Double-check cache (another request might have populated it while we waited)
                cachedProduct = await _cache.GetAsync<Product>(cacheKey);
                if (cachedProduct != null)
                {
                    _logger.LogInformation("Cache HIT after lock for product: {Id}", id);
                    return cachedProduct;
                }

                // 4. Query Cosmos DB
                _logger.LogInformation("Cache MISS for product: {Id}, querying Cosmos DB", id);
                var response = await _container.ReadItemAsync<Product>(
                    id,
                    new PartitionKey(categoryId)
                );

                if (response.Resource.IsDeleted)
                {
                    _logger.LogWarning("Product {Id} is marked as deleted", id);
                    return null;
                }

                var product = response.Resource;

                // 5. Store in cache (15 minute TTL)
                await _cache.SetAsync(cacheKey, product, TimeSpan.FromMinutes(15));
                _logger.LogInformation("Cached product: {Id}", id);

                // Track business event - product viewed
                _telemetryClient.TrackEvent("Product.Viewed", new Dictionary<string, string>
            {
                { "ProductId", id },
                { "CategoryId", categoryId },
                { "ProductName", product.Name },
                { "SKU", product.Sku }
            });

                operation.Telemetry.Success = true;
                operation.Telemetry.ResultCode = "CacheMiss";
                return product;
            }
            finally
            {
                // 6. Always release lock
                await _cache.UnlockAsync(lockKey);
            }
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Product {Id} not found in category {CategoryId}", id, categoryId);
            return null;
        }
        catch (Exception ex)
        {
            operation.Telemetry.Success = false;
            _logger.LogError(ex, "Error retrieving product by ID: {Id}", id);
            throw;
        }
    }

    // ===========================
    // Write Operations
    // ===========================

    public async Task<Product> CreateProductAsync(Product product)
    {
        _logger.LogInformation("Creating product: {Name} (SKU: {Sku})",
            product.Name, product.Sku);

        try
        {
            // Validate product type and structure
            ValidateProduct(product);

            // Set audit fields
            product.Id = Guid.NewGuid().ToString();
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            product.IsDeleted = false;

            var response = await _container.CreateItemAsync(
                product,
                new PartitionKey(product.CategoryId)
            );

            _logger.LogInformation("Created product with ID: {Id} (Type: {Type})",
                response.Resource.Id,
                response.Resource.HasVariants ? "Variant" : "Simple");

            // Track business event - product created
            _telemetryClient.TrackEvent("Product.Created", new Dictionary<string, string>
            {
                { "ProductId", response.Resource.Id },
                { "CategoryId", response.Resource.CategoryId },
                { "ProductName", response.Resource.Name },
                { "ProductType", response.Resource.HasVariants ? "Variant" : "Simple" },
                { "SKU", response.Resource.Sku }
            }, new Dictionary<string, double>
            {
                { "Price", (double)(response.Resource.HasVariants ? (response.Resource.PriceRange?.Min ?? 0) : (response.Resource.Price ?? 0)) }
            });

            return response.Resource;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error creating product");
            _telemetryClient.TrackException(ex);
            throw;
        }
    }

    public async Task<Product?> UpdateProductAsync(string id, string categoryId, Product product)
    {
        _logger.LogInformation("Updating product: {Id}", id);

        try
        {
            // Verify product exists
            var existing = await GetProductByIdAsync(id, categoryId);
            if (existing == null)
            {
                _logger.LogWarning("Product {Id} not found for update", id);
                return null;
            }

            // Validate product type and structure
            ValidateProduct(product);

            // Preserve system fields
            product.Id = id;
            product.CategoryId = categoryId; // Cannot change partition key
            product.CreatedAt = existing.CreatedAt;
            product.CreatedBy = existing.CreatedBy;
            product.UpdatedAt = DateTime.UtcNow;

            var response = await _container.ReplaceItemAsync(
                product,
                id,
                new PartitionKey(categoryId)
            );

            _logger.LogInformation("Updated product: {Id} (Type: {Type})",
                id,
                product.HasVariants ? "Variant" : "Simple");

            // Track business event - product updated
            _telemetryClient.TrackEvent("Product.Updated", new Dictionary<string, string>
            {
                { "ProductId", id },
                { "CategoryId", categoryId },
                { "ProductName", product.Name },
                { "SKU", product.Sku }
            });

            await PublishProductEventAsync("Updated", id, categoryId, new
            {
                ProductId = id,
                CategoryId = categoryId,
                Name = product.Name,
                Sku = product.Sku,
                UpdatedAt = product.UpdatedAt
            });

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Product {Id} not found for update", id);
            return null;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error updating product {Id}", id);
            _telemetryClient.TrackException(ex);
            throw;
        }
    }

    public async Task<bool> DeleteProductAsync(string id, string categoryId)
    {
        _logger.LogInformation("Deleting product: {Id}", id);

        try
        {
            // Soft delete - mark as deleted instead of removing
            var product = await GetProductByIdAsync(id, categoryId);
            if (product == null)
            {
                _logger.LogWarning("Product {Id} not found for deletion", id);
                return false;
            }

            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;

            await _container.ReplaceItemAsync(
                product,
                id,
                new PartitionKey(categoryId)
            );

            _logger.LogInformation("Soft deleted product: {Id}", id);

            // Track business event - product deleted
            _telemetryClient.TrackEvent("Product.Deleted", new Dictionary<string, string>
            {
                { "ProductId", id },
                { "CategoryId", categoryId },
                { "ProductName", product.Name }
            });

            await PublishProductEventAsync("Deleted", id, categoryId, new
            {
                ProductId = id,
                CategoryId = categoryId,
                DeletedAt = product.UpdatedAt
            });

            return true;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error deleting product {Id}", id);
            _telemetryClient.TrackException(ex);
            throw;
        }
    }

    // ===========================
    // Search Operations
    // ===========================

    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
    {
        _logger.LogInformation("Searching products with term: {SearchTerm}", searchTerm);

        try
        {
            // Simple search across name and description
            var queryDefinition = new QueryDefinition(
                @"SELECT * FROM c
                  WHERE c.isDeleted = false
                  AND (CONTAINS(LOWER(c.name), @searchTerm)
                       OR CONTAINS(LOWER(c.description), @searchTerm))"
            ).WithParameter("@searchTerm", searchTerm.ToLower());

            var query = _container.GetItemQueryIterator<Product>(queryDefinition);

            var results = new List<Product>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response);
            }

            _logger.LogInformation("Found {Count} products matching '{SearchTerm}'",
                results.Count, searchTerm);

            // Track search event
            _telemetryClient.TrackEvent("Product.Searched", new Dictionary<string, string>
            {
                { "SearchTerm", searchTerm }
            }, new Dictionary<string, double>
            {
                { "ResultCount", results.Count }
            });

            return results;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error searching products");
            _telemetryClient.TrackException(ex);
            throw;
        }
    }

    private async Task PublishProductEventAsync(string eventType, string productId, string categoryId, object data)
    {
        _logger.LogInformation("START PublishProductEventAsync - EventType: {EventType}, ProductId: {ProductId}", eventType, productId);

        try
        {
            _logger.LogInformation("Creating EventGridEvent...");
            var eventGridEvent = new EventGridEvent(
                subject: $"products/{categoryId}/{productId}",
                eventType: $"Product.{eventType}",
                dataVersion: "1.0",
                data: data
            );

            _logger.LogInformation("Sending event to Event Grid...");
            await _eventGridClient.SendEventAsync(eventGridEvent);

            _logger.LogInformation("Published {EventType} event for product {ProductId}", eventType, productId);

            // Track Event Grid publish
            _telemetryClient.TrackEvent("EventGrid.EventPublished", new Dictionary<string, string>
            {
                { "EventType", $"Product.{eventType}" },
                { "ProductId", productId },
                { "CategoryId", categoryId }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FAILED to publish {EventType} event for product {ProductId}", eventType, productId);
            _telemetryClient.TrackException(ex, new Dictionary<string, string>
            {
                { "EventType", eventType },
                { "ProductId", productId },
                { "Operation", "PublishEventGrid" }
            });
        }
    }
}
