using System.Security.AccessControl;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Cosmos;
using ProductService.Api.Services;
using Azure.Messaging.EventGrid;
using Azure;
using Microsoft.ApplicationInsights.Extensibility;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ===========================
// Azure Key Vault Configuration
// TEMPORARY: Commented out for local testing
// Will re-enable on Day 7 for Azure deployment
// ===========================
var keyVaultUri = builder.Configuration["KeyVault:Uri"];

if (!string.IsNullOrEmpty(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        new DefaultAzureCredential()
    );
}

builder.Services.AddApplicationInsightsTelemetry(options =>
{
    var connString = builder.Configuration["ApplicationInsights:ConnectionString"];
    if (!string.IsNullOrEmpty(connString))
    {
        options.ConnectionString = connString;
    }
});


// ===========================
// Configure Services
// ===========================

// Add controllers
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Product Service API",
        Version = "v1",
        Description = "Furniture Product Catalog API using Azure Cosmos DB",
        Contact = new()
        {
            Name = "Your Name",
            Email = "your.email@example.com"
        }
    });

    // Include XML comments (optional - requires generating XML doc file)
    // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // c.IncludeXmlComments(xmlPath);
});

// ===========================
// Azure Cosmos DB Configuration
// ===========================

builder.Services.AddSingleton<CosmosClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<Program>>();

    // Get Cosmos DB credentials from Key Vault (or config)
    var endpoint = configuration["CosmosDbEndpoint"]
        ?? throw new InvalidOperationException("CosmosDbEndpoint not configured");
    var key = configuration["CosmosDbKey"]
        ?? throw new InvalidOperationException("CosmosDbKey not configured");

    logger.LogInformation("Initializing Cosmos DB client for endpoint: {Endpoint}", endpoint);

    var cosmosClientOptions = new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        },
        ConnectionMode = ConnectionMode.Direct,
        MaxRetryAttemptsOnRateLimitedRequests = 3,
        MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30)
    };

    return new CosmosClient(endpoint, key, cosmosClientOptions);
});

// Redis Cache
var redisConnectionString = builder.Configuration["RedisConnectionString"];
if (!string.IsNullOrWhiteSpace(redisConnectionString) &&
    redisConnectionString != "placeholder-to-be-replaced-by-keyvault")
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var configuration = ConfigurationOptions.Parse(redisConnectionString);
        configuration.AbortOnConnectFail = false; // Don't crash if Redis is down
        return ConnectionMultiplexer.Connect(configuration);
    });
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();

    Console.WriteLine("✅ Redis caching ENABLED");
}
else
{
    // No Redis - use NullCacheService (no-op)
    builder.Services.AddSingleton<ICacheService, NullCacheService>();
    Console.WriteLine("⚠️  WARNING: Redis connection string missing - caching DISABLED");
}

// Event Grid Client (Singleton)
builder.Services.AddSingleton<EventGridPublisherClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var logger = sp.GetRequiredService<ILogger<Program>>();

    var endpointSecretName = configuration["EventGrid:TopicEndpointSecretName"]
        ?? throw new InvalidOperationException("EventGrid:TopicEndpointSecretName is missing");
    var keySecretName = configuration["EventGrid:TopicKeySecretName"]
        ?? throw new InvalidOperationException("EventGrid:TopicKeySecretName is missing");

    var endpoint = configuration[endpointSecretName]
        ?? throw new InvalidOperationException($"Secret '{endpointSecretName}' is missing");
    var key = configuration[keySecretName]
        ?? throw new InvalidOperationException($"Secret '{keySecretName}' is missing");

    logger.LogInformation("Configuring Event Grid client: {Endpoint}", endpoint);

    return new EventGridPublisherClient(
        new Uri(endpoint),
        new AzureKeyCredential(key)
    );
});

// ===========================
// Application Services
// ===========================

builder.Services.AddScoped<IProductService, ProductService.Api.Services.ProductService>();

// ===========================
// CORS Configuration (for web apps)
// ===========================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ===========================
// Build Application
// ===========================

var app = builder.Build();

// ===========================
// Configure HTTP Pipeline
// ===========================

// Enable Swagger in all environments (for learning)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Service API v1");
    c.RoutePrefix = string.Empty; // Swagger UI at root URL
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// ===========================
// Startup Logging
// ===========================

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Product Service API starting...");
logger.LogInformation("Swagger UI available at: http://localhost:5239");

app.Run();
