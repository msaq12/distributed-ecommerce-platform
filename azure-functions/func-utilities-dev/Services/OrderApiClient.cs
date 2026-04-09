using System.Net.Http.Json;
using System.Text.Json;
using func_utilities_dev.Models;
using Microsoft.Extensions.Logging;

namespace func_utilities_dev.Services;

public class OrderApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrderApiClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OrderApiClient(IHttpClientFactory httpClientFactory, ILogger<OrderApiClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient(nameof(OrderApiClient));
        _logger = logger;
    }

    public async Task<OrderDetails?> GetOrderByIdAsync(int orderId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/orders/{orderId}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Order {OrderId} not found", orderId);
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OrderDetails>(JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve order {OrderId}", orderId);
            throw;
        }
    }

    public async Task<OrderStatistics?> GetOrderStatisticsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/orders/statistics");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OrderStatistics>(JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve order statistics");
            throw;
        }
    }
}
