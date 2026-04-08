using FluentAssertions;
using OrderService.Data.Entities;
using Xunit;

namespace OrderService.Tests.EntityTests;

public class OrderTests
{
    [Fact]
    public void Order_Creation_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var order = new Order();

        // Assert
        order.OrderNumber.Should().BeEmpty();
        order.OrderDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        order.PaymentStatus.Should().Be("Pending");
        order.FulfillmentStatus.Should().Be("Pending");
        order.CreatedBy.Should().Be("System");
        order.ModifiedBy.Should().Be("System");
        order.IsDeleted.Should().BeFalse();
        order.OrderItems.Should().BeEmpty();
        order.ShippingCountry.Should().Be("USA");
        order.BillingCountry.Should().Be("USA");
    }

    [Fact]
    public void Order_ShippingAddressFull_ShouldFormatCorrectly()
    {
        // Arrange
        var order = new Order
        {
            ShippingAddressLine1 = "123 Main St",
            ShippingAddressLine2 = "Apt 4B",
            ShippingCity = "Seattle",
            ShippingState = "WA",
            ShippingZipCode = "98101"
        };

        // Act
        var fullAddress = order.ShippingAddressFull;

        // Assert
        fullAddress.Should().Be("123 Main St, Apt 4B, Seattle, WA 98101");
    }

    [Fact]
    public void Order_ShippingAddressFull_WithoutLine2_ShouldFormatCorrectly()
    {
        // Arrange
        var order = new Order
        {
            ShippingAddressLine1 = "456 Oak Ave",
            ShippingCity = "Portland",
            ShippingState = "OR",
            ShippingZipCode = "97201"
        };

        // Act
        var fullAddress = order.ShippingAddressFull;

        // Assert
        fullAddress.Should().Be("456 Oak Ave, Portland, OR 97201");
    }

    [Fact]
    public void Order_ItemCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var order = new Order
        {
            OrderItems = new List<OrderItem>
            {
                new OrderItem { ProductName = "Sofa" },
                new OrderItem { ProductName = "Chair" },
                new OrderItem { ProductName = "Table" }
            }
        };

        // Act & Assert
        order.ItemCount.Should().Be(3);
    }

    [Fact]
    public void Order_TotalQuantity_ShouldSumAllItemQuantities()
    {
        // Arrange
        var order = new Order
        {
            OrderItems = new List<OrderItem>
            {
                new OrderItem { Quantity = 2 },
                new OrderItem { Quantity = 3 },
                new OrderItem { Quantity = 1 }
            }
        };

        // Act & Assert
        order.TotalQuantity.Should().Be(6);
    }

    [Fact]
    public void Order_WithNoItems_ShouldReturnZeroCounts()
    {
        // Arrange
        var order = new Order();

        // Act & Assert
        order.ItemCount.Should().Be(0);
        order.TotalQuantity.Should().Be(0);
    }
}