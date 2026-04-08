using FluentAssertions;
using OrderService.Data.Entities;
using Xunit;

namespace OrderService.Tests.EntityTests;

public class OrderStatusTests
{
    [Fact]
    public void OrderStatus_Creation_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var status = new OrderStatus();

        // Assert
        status.IsActive.Should().BeTrue();
        status.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        status.Orders.Should().BeEmpty();
    }

    [Fact]
    public void OrderStatus_WithOrders_ShouldMaintainCollection()
    {
        // Arrange
        var status = new OrderStatus
        {
            StatusCode = "PENDING",
            StatusName = "Pending"
        };

        var order1 = new Order { OrderNumber = "ORD-001" };
        var order2 = new Order { OrderNumber = "ORD-002" };

        // Act
        status.Orders.Add(order1);
        status.Orders.Add(order2);

        // Assert
        status.Orders.Should().HaveCount(2);
        status.Orders.Should().Contain(order1);
        status.Orders.Should().Contain(order2);
    }
}