using FluentAssertions;
using OrderService.Data.Entities;
using Xunit;

namespace OrderService.Tests.EntityTests;

public class OrderItemTests
{
    [Fact]
    public void OrderItem_Creation_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var item = new OrderItem();

        // Assert
        item.Quantity.Should().Be(1);
        item.FulfillmentStatus.Should().Be("Pending");
        item.FulfilledQuantity.Should().Be(0);
        item.IsDeleted.Should().BeFalse();
        item.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void OrderItem_IsFullyFulfilled_WhenFulfilledQuantityEqualsQuantity_ShouldBeTrue()
    {
        // Arrange
        var item = new OrderItem
        {
            Quantity = 5,
            FulfilledQuantity = 5
        };

        // Act & Assert
        item.IsFullyFulfilled.Should().BeTrue();
    }

    [Fact]
    public void OrderItem_IsFullyFulfilled_WhenPartiallyFulfilled_ShouldBeFalse()
    {
        // Arrange
        var item = new OrderItem
        {
            Quantity = 5,
            FulfilledQuantity = 3
        };

        // Act & Assert
        item.IsFullyFulfilled.Should().BeFalse();
    }

    [Fact]
    public void OrderItem_RemainingQuantity_ShouldCalculateCorrectly()
    {
        // Arrange
        var item = new OrderItem
        {
            Quantity = 10,
            FulfilledQuantity = 4
        };

        // Act & Assert
        item.RemainingQuantity.Should().Be(6);
    }

    [Fact]
    public void OrderItem_RemainingQuantity_WhenOverFulfilled_ShouldReturnZero()
    {
        // Arrange
        var item = new OrderItem
        {
            Quantity = 5,
            FulfilledQuantity = 7
        };

        // Act & Assert
        item.RemainingQuantity.Should().Be(0);
    }

    [Fact]
    public void OrderItem_DiscountPercentage_ShouldCalculateCorrectly()
    {
        // Arrange
        var item = new OrderItem
        {
            LineTotal = 100m,
            DiscountAmount = 25m
        };

        // Act & Assert
        item.DiscountPercentage.Should().Be(25m);
    }

    [Fact]
    public void OrderItem_DiscountPercentage_WhenLineTotalIsZero_ShouldReturnZero()
    {
        // Arrange
        var item = new OrderItem
        {
            LineTotal = 0m,
            DiscountAmount = 10m
        };

        // Act & Assert
        item.DiscountPercentage.Should().Be(0m);
    }
}