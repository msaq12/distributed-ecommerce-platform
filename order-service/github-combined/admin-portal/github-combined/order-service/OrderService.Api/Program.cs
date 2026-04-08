using Microsoft.EntityFrameworkCore;
using OrderService.Data.Data;
using OrderService.Api.Services;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.ApplicationInsights.Extensibility;

var builder = WebApplication.CreateBuilder(args);

// ===========================
// LOGGING CONFIGURATION
// ===========================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ===========================
// KEY VAULT INTEGRATION
// ===========================
var keyVaultUri = builder.Configuration["KeyVault:Uri"];

if (!string.IsNullOrEmpty(keyVaultUri))
{
    try
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUri),
            new DefaultAzureCredential());

        Console.WriteLine($"✓ Key Vault configured: {keyVaultUri}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠ WARNING: Could not connect to Key Vault: {ex.Message}");
        Console.WriteLine("⚠ Falling back to connection string from appsettings.json");
    }
}
else
{
    Console.WriteLine("WARNING: Key Vault URI not configured");
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
// DATABASE CONFIGURATION
// ===========================
builder.Services.AddDbContext<OrdersDbContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    // Try Key Vault first, then fallback to appsettings.json
    var connectionString = configuration["SqlConnectionString"]
        ?? configuration.GetConnectionString("OrdersDb");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("SQL connection string not found in Key Vault or appsettings.json");
    }

    Console.WriteLine("✓ Database connection configured");

    options.UseSqlServer(connectionString);
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
});

// Service Bus client (Singleton)
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration["ServiceBusConnectionString"];

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException(
            "ServiceBusConnectionString not found in configuration");
    }

    return new ServiceBusClient(connectionString);
});

// ===========================
// SERVICES REGISTRATION
// ===========================
builder.Services.AddScoped<IOrderService, OrderService.Api.Services.OrderService>();
builder.Services.AddScoped<IMessagePublisher, ServiceBusMessagePublisher>();

// ===========================
// CONTROLLERS & API
// ===========================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Order Service API",
        Version = "v1",
        Description = "Furniture Dropshipping - Order Management Service"
    });
});

// ===========================
// CORS (for development)
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
// BUILD APP
// ===========================
var app = builder.Build();

// ===========================
// MIDDLEWARE PIPELINE
// ===========================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// ===========================
// STARTUP LOGGING
// ===========================
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Order Service API starting...");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("Swagger UI available at: {BaseUrl}", app.Urls.FirstOrDefault() ?? "http://localhost:5000");

app.Run();
