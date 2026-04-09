using System.Net;
using System.Text.Json;
using func_utilities_dev.Models;
using func_utilities_dev.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace func_utilities_dev.Functions;

public class InvoiceGenerationFunction
{
    private readonly OrderApiClient _orderApiClient;
    private readonly ILogger<InvoiceGenerationFunction> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public InvoiceGenerationFunction(OrderApiClient orderApiClient, ILogger<InvoiceGenerationFunction> logger)
    {
        _orderApiClient = orderApiClient;
        _logger = logger;
    }

    [Function(nameof(InvoiceGenerationFunction))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "invoices/generate")] HttpRequestData req)
    {
        _logger.LogInformation("InvoiceGenerationFunction triggered");

        InvoiceRequest? invoiceRequest;
        try
        {
            invoiceRequest = await req.ReadFromJsonAsync<InvoiceRequest>();
        }
        catch
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Invalid request body.");
            return badRequest;
        }

        if (invoiceRequest is null || invoiceRequest.OrderId <= 0)
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("OrderId is required.");
            return badRequest;
        }

        _logger.LogInformation("Generating invoice for order {OrderId}", invoiceRequest.OrderId);

        var order = await _orderApiClient.GetOrderByIdAsync(invoiceRequest.OrderId);
        if (order is null)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync($"Order {invoiceRequest.OrderId} not found.");
            return notFound;
        }

        var invoice = new InvoiceResponse
        {
            InvoiceNumber = $"INV-{order.OrderNumber}",
            GeneratedAt = DateTime.UtcNow,
            Order = new InvoiceOrderSection
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                CustomerName = order.CustomerName,
                CustomerEmail = order.CustomerEmail,
                Status = order.Status
            },
            BillTo = new InvoiceAddressSection
            {
                Name = order.CustomerName,
                Address = order.BillingAddress
            },
            ShipTo = new InvoiceAddressSection
            {
                Name = order.CustomerName,
                Address = order.ShippingAddress
            },
            LineItems = order.Items.Select(item => new InvoiceLineItem
            {
                Sku = item.ProductSku,
                Description = string.IsNullOrWhiteSpace(item.VariantName)
                    ? item.ProductName
                    : $"{item.ProductName} — {item.VariantName}",
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Total = item.FinalLineTotal
            }).ToList(),
            Subtotal = order.SubtotalAmount,
            Tax = order.TaxAmount,
            Shipping = order.ShippingAmount,
            Total = order.TotalAmount,
            Footer = "Thank you for shopping with Furniture Dropship."
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonSerializer.Serialize(invoice, JsonOptions));

        _logger.LogInformation("Invoice {InvoiceNumber} generated successfully", invoice.InvoiceNumber);
        return response;
    }
}
