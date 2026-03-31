using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderService.Data.Data;
using OrderService.Data.Entities;
using OrderService.Tests.Helpers;
using Xunit;

namespace OrderService.Tests.RepositoryTests;

public class OrderRepositoryTests : IDisposable
{
    private readonly OrdersDbContext _context;

    public OrderRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemoryContext();
    }

    [Fact]
    public async Task CreateOrder_ShouldAddOrderToDatabase()
    {
        // Arrange
        var order = new Order
        {
            OrderNumber = "ORD-2024-001",
            OrderDate = DateTime.UtcNow,
            CustomerEmail = "test@example.com",
            CustomerName = "Test Customer",
            ShippingAddressLine1 = "123 Test St",
            ShippingCity = "Test City",
            ShippingState = "TS",
            ShippingZipCode = "12345",
            ShippingCountry = "USA",
            BillingAddressLine1 = "123 Test St",
            BillingCity = "Test City",
            BillingState = "TS",
            BillingZipCode = "12345",
            BillingCountry = "USA",
            SubtotalAmount = 100.00m,
            TaxAmount = 8.00m,
            ShippingAmount = 10.00m,
            DiscountAmount = 0m,
            TotalAmount = 118.00m,
            OrderStatusId = 1,
            PaymentStatus = "Pending",
            FulfillmentStatus = "Pending"
        };

        // Act
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Assert
        var savedOrder = await _context.Orders.FirstOrDefaultAsync(o => o.OrderNumber == "ORD-2024-001");
        savedOrder.Should().NotBeNull();
        savedOrder!.CustomerEmail.Should().Be("test@example.com");
        savedOrder.TotalAmount.Should().Be(118.00m);
    }

    [Fact]
    public async Task CreateOrder_WithItems_ShouldSaveOrderAndItems()
    {
        // Arrange
        var order = new Order
        {
            OrderNumber = "ORD-2024-002",
            CustomerEmail = "test2@example.com",
            CustomerName = "Test Customer 2",
            ShippingAddressLine1 = "456 Test Ave",
            ShippingCity = "Test City",
            ShippingState = "TS",
            ShippingZipCode = "12345",
            BillingAddressLine1 = "456 Test Ave",
            BillingCity = "Test City",
            BillingState = "TS",
            BillingZipCode = "12345",
            SubtotalAmount = 200.00m,
            TaxAmount = 16.00m,
            ShippingAmount = 15.00m,
            TotalAmount = 231.00m,
            OrderStatusId = 1,
            OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductSku = "SOFA-001",
                    ProductName = "Modern Sofa",
                    Quantity = 1,
                    UnitPrice = 150.00m,
                    LineTotal = 150.00m,
                    FinalLineTotal = 150.00m,
                    FulfillmentStatus = "Pending"
                },
                new OrderItem
                {
                    ProductSku = "CHAIR-001",
                    ProductName = "Office Chair",
                    Quantity = 2,
                    UnitPrice = 25.00m,
                    LineTotal = 50.00m,
                    FinalLineTotal = 50.00m,
                    FulfillmentStatus = "Pending"
                }
            }
        };

        // Act
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Assert
        var savedOrder = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderNumber == "ORD-2024-002");

        savedOrder.Should().NotBeNull();
        savedOrder!.OrderItems.Should().HaveCount(2);
        savedOrder.OrderItems.Should().Contain(i => i.ProductSku == "SOFA-001");
        savedOrder.OrderItems.Should().Contain(i => i.ProductSku == "CHAIR-001");
    }

    [Fact]
    public async Task UpdateOrder_ShouldModifyExistingOrder()
    {
        // Arrange
        var order = new Order
        {
            OrderNumber = "ORD-2024-003",
            CustomerEmail = "test3@example.com",
            CustomerName = "Test Customer 3",
            ShippingAddressLine1 = "789 Test Blvd",
            ShippingCity = "Test City",
            ShippingState = "TS",
            ShippingZipCode = "12345",
            BillingAddressLine1 = "789 Test Blvd",
            BillingCity = "Test City",
            BillingState = "TS",
            BillingZipCode = "12345",
            SubtotalAmount = 100.00m,
            TotalAmount = 118.00m,
            OrderStatusId = 1
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        order.OrderStatusId = 2; // Change to Confirmed
        order.PaymentStatus = "Paid";
        order.ModifiedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Assert
        var updatedOrder = await _context.Orders.FindAsync(order.OrderId);
        updatedOrder.Should().NotBeNull();
        updatedOrder!.OrderStatusId.Should().Be(2);
        updatedOrder.PaymentStatus.Should().Be("Paid");
    }

    [Fact]
    public async Task DeleteOrder_WithCascade_ShouldDeleteOrderItems()
    {
        // Arrange
        var order = new Order
        {
            OrderNumber = "ORD-2024-004",
            CustomerEmail = "test4@example.com",
            CustomerName = "Test Customer 4",
            ShippingAddressLine1 = "321 Test Dr",
            ShippingCity = "Test City",
            ShippingState = "TS",
            ShippingZipCode = "12345",
            BillingAddressLine1 = "321 Test Dr",
            BillingCity = "Test City",
            BillingState = "TS",
            BillingZipCode = "12345",
            SubtotalAmount = 50.00m,
            TotalAmount = 58.00m,
            OrderStatusId = 1,
            OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductSku = "TABLE-001",
                    ProductName = "Dining Table",
                    Quantity = 1,
                    UnitPrice = 50.00m,
                    LineTotal = 50.00m,
                    FinalLineTotal = 50.00m
                }
            }
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        var orderId = order.OrderId;

        // Act
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        // Assert
        var deletedOrder = await _context.Orders.FindAsync(orderId);
        var orphanedItems = await _context.OrderItems.Where(i => i.OrderId == orderId).ToListAsync();
        
        deletedOrder.Should().BeNull();
        orphanedItems.Should().BeEmpty(); // Cascade delete should remove items
    }

    [Fact]
    public async Task GetOrder_WithInclude_ShouldLoadRelatedData()
    {
        // Arrange
        var order = new Order
        {
            OrderNumber = "ORD-2024-005",
            CustomerEmail = "test5@example.com",
            CustomerName = "Test Customer 5",
            ShippingAddressLine1 = "555 Test Ln",
            ShippingCity = "Test City",
            ShippingState = "TS",
            ShippingZipCode = "12345",
            BillingAddressLine1 = "555 Test Ln",
            BillingCity = "Test City",
            BillingState = "TS",
            BillingZipCode = "12345",
            SubtotalAmount = 75.00m,
            TotalAmount = 87.50m,
            OrderStatusId = 1,
            OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductSku = "LAMP-001",
                    ProductName = "Floor Lamp",
                    Quantity = 1,
                    UnitPrice = 75.00m,
                    LineTotal = 75.00m,
                    FinalLineTotal = 75.00m
                }
            }
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var retrievedOrder = await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.OrderStatus)
            .FirstOrDefaultAsync(o => o.OrderNumber == "ORD-2024-005");

        // Assert
        retrievedOrder.Should().NotBeNull();
        retrievedOrder!.OrderItems.Should().HaveCount(1);
        retrievedOrder.OrderStatus.Should().NotBeNull();
        retrievedOrder.OrderStatus!.StatusCode.Should().Be("PENDING");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}