using Microsoft.EntityFrameworkCore;
using OrderService.Data.Data;
using OrderService.Data.Entities;

namespace OrderService.Tests.Helpers;

public static class TestDbContextFactory
{
    public static OrdersDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new OrdersDbContext(options);

        // Seed with order statuses
        SeedOrderStatuses(context);

        return context;
    }

    private static void SeedOrderStatuses(OrdersDbContext context)
    {
        context.OrderStatuses.AddRange(
            new OrderStatus
            {
                OrderStatusId = 1,
                StatusCode = "PENDING",
                StatusName = "Pending",
                StatusDescription = "Order has been placed",
                DisplayOrder = 1,
                IsActive = true
            },
            new OrderStatus
            {
                OrderStatusId = 2,
                StatusCode = "CONFIRMED",
                StatusName = "Confirmed",
                StatusDescription = "Order has been confirmed",
                DisplayOrder = 2,
                IsActive = true
            },
            new OrderStatus
            {
                OrderStatusId = 3,
                StatusCode = "PROCESSING",
                StatusName = "Processing",
                StatusDescription = "Order is being processed",
                DisplayOrder = 3,
                IsActive = true
            },
            new OrderStatus
            {
                OrderStatusId = 4,
                StatusCode = "SHIPPED",
                StatusName = "Shipped",
                StatusDescription = "Order has been shipped",
                DisplayOrder = 4,
                IsActive = true
            },
            new OrderStatus
            {
                OrderStatusId = 5,
                StatusCode = "DELIVERED",
                StatusName = "Delivered",
                StatusDescription = "Order has been delivered",
                DisplayOrder = 5,
                IsActive = true
            },
            new OrderStatus
            {
                OrderStatusId = 6,
                StatusCode = "CANCELLED",
                StatusName = "Cancelled",
                StatusDescription = "Order has been cancelled",
                DisplayOrder = 6,
                IsActive = true
            }
        );

        context.SaveChanges();
    }
}