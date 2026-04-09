using Azure.Storage.Blobs;
using func_utilities_dev.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services.AddHttpClient(nameof(OrderApiClient), (sp, client) =>
{
    var orderServiceUrl = builder.Configuration["OrderServiceUrl"]
        ?? throw new InvalidOperationException("OrderServiceUrl is not configured.");
    client.BaseAddress = new Uri(orderServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddSingleton<OrderApiClient>();

builder.Services.AddSingleton(_ =>
{
    var connectionString = builder.Configuration["BlobStorageConnectionString"]
        ?? throw new InvalidOperationException("BlobStorageConnectionString is not configured.");
    return new BlobServiceClient(connectionString);
});

builder.Build().Run();
