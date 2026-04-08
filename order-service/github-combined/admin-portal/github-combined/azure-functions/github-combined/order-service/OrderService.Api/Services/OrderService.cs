using Microsoft.EntityFrameworkCore;
using OrderService.Data.Data;
using OrderService.Data.Entities;
using OrderService.Api.DTOs;
using Microsoft.ApplicationInsights;

namespace OrderService.Api.Services;

/// <summary>
/// Order service implementation with business logic
/// </summary>
public class OrderService : IOrderService
{
    private readonly OrdersDbContext _context;
    private readonly ILogger<OrderService> _logger;
    private readonly IMessagePublisher _messagePublisher;
    private readonly TelemetryClient _telemetryClient;

    public OrderService(OrdersDbContext context, ILogger<OrderService> logger,
        IMessagePublisher messagePublisher, TelemetryClient telemetryClient)
    {
        _context = context;
        _logger = logger;
        _messagePublisher = messagePublisher;
        _telemetryClient = telemetryClient;
    }

    // ===========================
    // GET Operations
    // ===========================

    public async Task<IEnumerable<OrderResponse>> GetAllOrdersAsync()
    {
        _logger.LogInformation("Retrieving all orders");

        var orders = await _context.Orders
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .Take(100) // Limit to 100 most recent
            .ToListAsync();

        _telemetryClient.TrackMetric("Orders.TotalOrders", orders.Count);

        return orders.Select(MapToOrderResponse);
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(int orderId)
    {
        _logger.LogInformation("Retrieving order by ID: {OrderId}", orderId);

        var order = await _context.Orders
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
        {
            _logger.LogWarning("Order not found: {OrderId}", orderId);
            return null;
        }

        return MapToOrderResponse(order);
    }

    public async Task<OrderResponse?> GetOrderByNumberAsync(string orderNumber)
    {
        _logger.LogInformation("Retrieving order by number: {OrderNumber}", orderNumber);

        var order = await _context.Orders
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

        if (order == null)
        {
            _logger.LogWarning("Order not found: {OrderNumber}", orderNumber);
            return null;
        }

        return MapToOrderResponse(order);
    }

    public async Task<IEnumerable<OrderResponse>> GetOrdersByCustomerEmailAsync(string email)
    {
        _logger.LogInformation("Retrieving orders for customer: {Email}", email);

        var orders = await _context.Orders
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerEmail.ToLower() == email.ToLower())
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return orders.Select(MapToOrderResponse);
    }

    public async Task<IEnumerable<OrderResponse>> GetOrdersByStatusAsync(int statusId)
    {
        _logger.LogInformation("Retrieving orders by status: {StatusId}", statusId);

        var orders = await _context.Orders
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
            .Where(o => o.OrderStatusId == statusId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return orders.Select(MapToOrderResponse);
    }

    // ===========================
    // CREATE Operations
    // ===========================

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        _logger.LogInformation("Creating new order for customer: {Email}", request.CustomerEmail);

        // Validate request
        ValidateCreateOrderRequest(request);

        // Generate order number
        var orderNumber = GenerateOrderNumber();

        // Calculate amounts
        var subtotal = request.Items.Sum(i => i.Quantity * i.UnitPrice);
        var taxAmount = CalculateTax(subtotal);
        var shippingAmount = CalculateShipping(subtotal);
        var total = subtotal + taxAmount + shippingAmount;

        // Create order entity
        var order = new Order
        {
            OrderNumber = orderNumber,
            OrderDate = DateTime.UtcNow,

            CustomerEmail = request.CustomerEmail,
            CustomerName = request.CustomerName,

            ShippingAddressLine1 = request.ShippingAddressLine1,
            ShippingAddressLine2 = request.ShippingAddressLine2,
            ShippingCity = request.ShippingCity,
            ShippingState = request.ShippingState,
            ShippingZipCode = request.ShippingZipCode,
            ShippingCountry = request.ShippingCountry,

            BillingAddressLine1 = request.BillingAddressLine1,
            BillingAddressLine2 = request.BillingAddressLine2,
            BillingCity = request.BillingCity,
            BillingState = request.BillingState,
            BillingZipCode = request.BillingZipCode,
            BillingCountry = request.BillingCountry,

            SubtotalAmount = subtotal,
            TaxAmount = taxAmount,
            ShippingAmount = shippingAmount,
            DiscountAmount = 0,
            TotalAmount = total,

            OrderStatusId = 1, // PENDING status
            PaymentStatus = "Pending",
            FulfillmentStatus = "Pending",

            CustomerNotes = request.CustomerNotes,

            CreatedDate = DateTime.UtcNow,
            CreatedBy = "API",
            ModifiedDate = DateTime.UtcNow,
            ModifiedBy = "API"
        };

        // Add order items
        foreach (var itemRequest in request.Items)
        {
            var lineTotal = itemRequest.Quantity * itemRequest.UnitPrice;

            var orderItem = new OrderItem
            {
                ProductSku = itemRequest.ProductSku,
                ProductName = itemRequest.ProductName,
                ProductDescription = itemRequest.ProductDescription,
                VariantSku = itemRequest.VariantSku,
                VariantName = itemRequest.VariantName,
                Quantity = itemRequest.Quantity,
                UnitPrice = itemRequest.UnitPrice,
                LineTotal = lineTotal,
                DiscountAmount = 0,
                FinalLineTotal = lineTotal,
                FulfillmentStatus = "Pending",
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            order.OrderItems.Add(orderItem);
        }

        // Save to database
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created order: {OrderNumber} (ID: {OrderId})",
            order.OrderNumber, order.OrderId);

        // Reload with includes for response
        var createdOrder = await _context.Orders
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
            .FirstAsync(o => o.OrderId == order.OrderId);

        //Publish message to Service Bus
        await _messagePublisher.PublishOrderCreatedAsync(
        order.OrderId,
        order.OrderNumber);

        return MapToOrderResponse(createdOrder);
    }

    // ===========================
    // UPDATE Operations
    // ===========================

    public async Task<OrderResponse?> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request)
    {
        _logger.LogInformation("Updating order status: {OrderId} to status {StatusId}",
            orderId, request.OrderStatusId);

        var order = await _context.Orders
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
        {
            _logger.LogWarning("Order not found: {OrderId}", orderId);
            return null;
        }

        // Verify status exists
        var statusExists = await _context.OrderStatuses
            .AnyAsync(s => s.OrderStatusId == request.OrderStatusId && s.IsActive);

        if (!statusExists)
        {
            throw new InvalidOperationException($"Invalid status ID: {request.OrderStatusId}");
        }

        // Update order
        order.OrderStatusId = request.OrderStatusId;

        if (!string.IsNullOrWhiteSpace(request.InternalNotes))
        {
            order.InternalNotes = request.InternalNotes;
        }

        order.ModifiedDate = DateTime.UtcNow;
        order.ModifiedBy = "API";

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated order {OrderNumber} to status {StatusId}",
            order.OrderNumber, request.OrderStatusId);

        // Reload to get updated status name
        order = await _context.Orders
            .Include(o => o.OrderStatus)
            .Include(o => o.OrderItems)
            .FirstAsync(o => o.OrderId == orderId);

        return MapToOrderResponse(order);
    }

    // ===========================
    // STATISTICS
    // ===========================

    public async Task<Dictionary<string, object>> GetOrderStatisticsAsync()
    {
        _logger.LogInformation("Calculating order statistics");

        var stats = new Dictionary<string, object>();

        // Total orders
        stats["totalOrders"] = await _context.Orders.CountAsync();

        // Total revenue
        stats["totalRevenue"] = await _context.Orders.SumAsync(o => o.TotalAmount);

        // Average order value
        stats["averageOrderValue"] = await _context.Orders.AverageAsync(o => o.TotalAmount);

        // Orders by status
        var ordersByStatus = await _context.Orders
            .Include(o => o.OrderStatus)
            .GroupBy(o => o.OrderStatus!.StatusName)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        stats["ordersByStatus"] = ordersByStatus;

        // Recent orders (last 24 hours)
        var yesterday = DateTime.UtcNow.AddDays(-1);
        stats["ordersLast24Hours"] = await _context.Orders
            .CountAsync(o => o.OrderDate >= yesterday);

        return stats;
    }

    // ===========================
    // HELPER METHODS
    // ===========================

    private void ValidateCreateOrderRequest(CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerEmail))
            throw new ArgumentException("Customer email is required");

        if (string.IsNullOrWhiteSpace(request.CustomerName))
            throw new ArgumentException("Customer name is required");

        if (request.Items == null || !request.Items.Any())
            throw new ArgumentException("Order must have at least one item");

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
                throw new ArgumentException($"Invalid quantity for product {item.ProductSku}");

            if (item.UnitPrice < 0)
                throw new ArgumentException($"Invalid price for product {item.ProductSku}");
        }
    }

    private string GenerateOrderNumber()
    {
        // Format: ORD-YYYYMMDD-XXXXXX (e.g., ORD-20240112-123456)
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = new Random().Next(100000, 999999);
        return $"ORD-{datePart}-{randomPart}";
    }

    private decimal CalculateTax(decimal subtotal)
    {
        // Simple 8% tax rate (would be more complex in production)
        return Math.Round(subtotal * 0.08m, 2);
    }

    private decimal CalculateShipping(decimal subtotal)
    {
        // Free shipping over $100, otherwise $10
        return subtotal >= 100 ? 0 : 10.00m;
    }

    private OrderResponse MapToOrderResponse(Order order)
    {
        return new OrderResponse
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            OrderDate = order.OrderDate,

            CustomerEmail = order.CustomerEmail,
            CustomerName = order.CustomerName,

            ShippingAddress = order.ShippingAddressFull,
            BillingAddress = $"{order.BillingAddressLine1}, {order.BillingCity}, {order.BillingState} {order.BillingZipCode}",

            SubtotalAmount = order.SubtotalAmount,
            TaxAmount = order.TaxAmount,
            ShippingAmount = order.ShippingAmount,
            DiscountAmount = order.DiscountAmount,
            TotalAmount = order.TotalAmount,

            Status = order.OrderStatus?.StatusName ?? "Unknown",
            PaymentStatus = order.PaymentStatus,
            FulfillmentStatus = order.FulfillmentStatus,

            Items = order.OrderItems.Select(i => new OrderItemResponse
            {
                OrderItemId = i.OrderItemId,
                ProductSku = i.ProductSku,
                ProductName = i.ProductName,
                VariantName = i.VariantName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.LineTotal,
                DiscountAmount = i.DiscountAmount,
                FinalLineTotal = i.FinalLineTotal,
                FulfillmentStatus = i.FulfillmentStatus
            }).ToList(),

            CreatedDate = order.CreatedDate,
            ModifiedDate = order.ModifiedDate
        };
    }
}
