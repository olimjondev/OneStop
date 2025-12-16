using FluentAssertions;
using OneStop.Application.Exceptions;
using OneStop.Application.Features.Basket;
using OneStop.Application.Interfaces;
using OneStop.Domain.Services;
using OneStop.Infrastructure.Persistence.InMemory;

namespace OneStop.Tests.Integration;

/// <summary>
/// Integration tests for CalculateBasketHandler.
/// Uses real in-memory repositories and domain service.
/// </summary>
public class CalculateBasketHandlerTests
{
    private readonly CalculateBasketHandler _handler;

    public CalculateBasketHandlerTests()
    {
        IProductRepository productRepo = new InMemoryProductRepository();
        IDiscountPromotionRepository discountRepo = new InMemoryDiscountPromotionRepository();
        IPointsPromotionRepository pointsRepo = new InMemoryPointsPromotionRepository();
        IBasketCalculatorService calculatorService = new BasketCalculatorService();

        _handler = new CalculateBasketHandler(
            productRepo,
            discountRepo,
            pointsRepo,
            calculatorService);
    }

    #region Sample Scenario Tests

    [Fact]
    public async Task Handle_SampleScenario_ReturnsCorrectCalculation()
    {
        // Arrange: 10-Jan-2020 scenario from requirements
        // PRD01: 3 x $1.20 = $3.60 (no discount - not in DP001)
        // PRD02: 10 x $1.30 = $13.00 - 20% = $10.40 (DP001 applies)
        // Total: $16.60, Discount: $2.60, Grand: $14.00
        // Points: PP001 active (2 pts/$), floor(14.00 * 2) = 28
        var command = new CalculateBasketCommand
        {
            CustomerId = Guid.Parse("8e4e8991-aaee-495b-9f24-52d5d0e509c5"),
            LoyaltyCard = "CTX0000001",
            TransactionDate = new DateTime(2020, 1, 10),
            Basket =
            [
                new CalculateBasketCommand.BasketItemDto { ProductId = "PRD01", Quantity = 3 },
                new CalculateBasketCommand.BasketItemDto { ProductId = "PRD02", Quantity = 10 }
            ]
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalAmount.Should().Be(16.6m);
        result.DiscountApplied.Should().Be(2.6m);
        result.GrandTotal.Should().Be(14.0m);
        result.PointsEarned.Should().Be(28);
    }

    #endregion

    #region Date-Based Promotion Tests

    [Fact]
    public async Task Handle_DateBeforePromotion_NoDiscountApplied()
    {
        // Arrange: Dec 31 2019 - before DP001 (Jan 1 - Feb 15, 2020)
        var command = new CalculateBasketCommand
        {
            CustomerId = Guid.NewGuid(),
            LoyaltyCard = "CARD001",
            TransactionDate = new DateTime(2019, 12, 31),
            Basket =
            [
                new CalculateBasketCommand.BasketItemDto { ProductId = "PRD02", Quantity = 10 }
            ]
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.DiscountApplied.Should().Be(0m);
        result.GrandTotal.Should().Be(13.0m);
    }

    [Fact]
    public async Task Handle_DateAfterPromotion_NoDiscountApplied()
    {
        // Arrange: Feb 16 2020 - after DP001 (Jan 1 - Feb 15, 2020)
        var command = new CalculateBasketCommand
        {
            CustomerId = Guid.NewGuid(),
            LoyaltyCard = "CARD001",
            TransactionDate = new DateTime(2020, 2, 16),
            Basket =
            [
                new CalculateBasketCommand.BasketItemDto { ProductId = "PRD02", Quantity = 10 }
            ]
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.DiscountApplied.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_FuelPromoDate_OnlyFuelProductsEarnPoints()
    {
        // Arrange: Feb 10 2020 - PP002 "Fuel Promo" active (Feb 5-15, Fuel only, 3 pts/$)
        // Also DP001 active for PRD02
        var command = new CalculateBasketCommand
        {
            CustomerId = Guid.NewGuid(),
            LoyaltyCard = "CARD001",
            TransactionDate = new DateTime(2020, 2, 10),
            Basket =
            [
                new CalculateBasketCommand.BasketItemDto { ProductId = "PRD01", Quantity = 10 }, // Fuel $12.00
                new CalculateBasketCommand.BasketItemDto { ProductId = "PRD04", Quantity = 10 }  // Shop $23.00
            ]
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Only fuel ($12) qualifies for points at 3 pts/$ = 36 points
        result.PointsEarned.Should().Be(36);
    }

    [Fact]
    public async Task Handle_ShopPromoDate_OnlyShopProductsEarnPoints()
    {
        // Arrange: Mar 10 2020 - PP003 "Shop Promo" active (Mar 1-20, Shop only, 4 pts/$)
        // Also DP002 active but no products
        var command = new CalculateBasketCommand
        {
            CustomerId = Guid.NewGuid(),
            LoyaltyCard = "CARD001",
            TransactionDate = new DateTime(2020, 3, 10),
            Basket =
            [
                new CalculateBasketCommand.BasketItemDto { ProductId = "PRD01", Quantity = 10 }, // Fuel $12.00
                new CalculateBasketCommand.BasketItemDto { ProductId = "PRD04", Quantity = 10 }  // Shop $23.00
            ]
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Only shop ($23) qualifies for points at 4 pts/$ = 92 points
        result.PointsEarned.Should().Be(92);
    }

    #endregion

    #region Loyalty Card Tests

    [Fact]
    public async Task Handle_NoLoyaltyCard_NoPointsEarned()
    {
        // Arrange
        var command = new CalculateBasketCommand
        {
            CustomerId = Guid.NewGuid(),
            LoyaltyCard = null,
            TransactionDate = new DateTime(2020, 1, 10),
            Basket =
            [
                new CalculateBasketCommand.BasketItemDto { ProductId = "PRD01", Quantity = 10 }
            ]
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PointsEarned.Should().Be(0);
    }

    [Fact]
    public async Task Handle_EmptyLoyaltyCard_NoPointsEarned()
    {
        // Arrange
        var command = new CalculateBasketCommand
        {
            CustomerId = Guid.NewGuid(),
            LoyaltyCard = "",
            TransactionDate = new DateTime(2020, 1, 10),
            Basket =
            [
                new CalculateBasketCommand.BasketItemDto { ProductId = "PRD01", Quantity = 10 }
            ]
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PointsEarned.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhitespaceLoyaltyCard_NoPointsEarned()
    {
        // Arrange
        var command = new CalculateBasketCommand
        {
            CustomerId = Guid.NewGuid(),
            LoyaltyCard = "   ",
            TransactionDate = new DateTime(2020, 1, 10),
            Basket =
            [
                new CalculateBasketCommand.BasketItemDto { ProductId = "PRD01", Quantity = 10 }
            ]
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PointsEarned.Should().Be(0);
    }

    #endregion

    #region Product Not Found Tests

    [Fact]
    public async Task Handle_UnknownProductId_ThrowsNotFoundException()
    {
        // Arrange
        var command = new CalculateBasketCommand
        {
            CustomerId = Guid.NewGuid(),
            LoyaltyCard = "CARD001",
            TransactionDate = new DateTime(2020, 1, 10),
            Basket =
            [
                new CalculateBasketCommand.BasketItemDto { ProductId = "UNKNOWN", Quantity = 1 }
            ]
        };

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*UNKNOWN*not found*");
    }

    [Fact]
    public async Task Handle_MultipleUnknownProducts_ThrowsWithAllIds()
    {
        // Arrange
        var command = new CalculateBasketCommand
        {
            CustomerId = Guid.NewGuid(),
            LoyaltyCard = "CARD001",
            TransactionDate = new DateTime(2020, 1, 10),
            Basket =
            [
                new CalculateBasketCommand.BasketItemDto { ProductId = "UNKNOWN1", Quantity = 1 },
                new CalculateBasketCommand.BasketItemDto { ProductId = "UNKNOWN2", Quantity = 1 }
            ]
        };

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<NotFoundException>();
        exception.Which.Message.Should().Contain("UNKNOWN1");
        exception.Which.Message.Should().Contain("UNKNOWN2");
    }

    [Fact]
    public async Task Handle_MixedKnownAndUnknown_ThrowsForUnknown()
    {
        // Arrange
        var command = new CalculateBasketCommand
        {
            CustomerId = Guid.NewGuid(),
            LoyaltyCard = "CARD001",
            TransactionDate = new DateTime(2020, 1, 10),
            Basket =
            [
                new CalculateBasketCommand.BasketItemDto { ProductId = "PRD01", Quantity = 1 },
                new CalculateBasketCommand.BasketItemDto { ProductId = "UNKNOWN", Quantity = 1 }
            ]
        };

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*UNKNOWN*");
    }

    #endregion

    #region Response Mapping Tests

    [Fact]
    public async Task Handle_ReturnsCorrectCustomerInfo()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var loyaltyCard = "MY-CARD-123";
        var transactionDate = new DateTime(2020, 1, 15);

        var command = new CalculateBasketCommand
        {
            CustomerId = customerId,
            LoyaltyCard = loyaltyCard,
            TransactionDate = transactionDate,
            Basket =
            [
                new CalculateBasketCommand.BasketItemDto { ProductId = "PRD01", Quantity = 1 }
            ]
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CustomerId.Should().Be(customerId);
        result.LoyaltyCard.Should().Be(loyaltyCard);
        result.TransactionDate.Should().Be(transactionDate);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task Handle_DateWithNoActivePromotions_ReturnsZeroDiscountAndPoints()
    {
        // Arrange: April 2020 - no promotions active
        var command = new CalculateBasketCommand
        {
            CustomerId = Guid.NewGuid(),
            LoyaltyCard = "CARD001",
            TransactionDate = new DateTime(2020, 4, 1),
            Basket =
            [
                new CalculateBasketCommand.BasketItemDto { ProductId = "PRD01", Quantity = 10 }
            ]
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalAmount.Should().Be(12.0m);
        result.DiscountApplied.Should().Be(0m);
        result.GrandTotal.Should().Be(12.0m);
        result.PointsEarned.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ProductNotInDiscountPromotion_NoDiscount()
    {
        // Arrange: PRD01 is not in DP001 (only PRD02 is)
        var command = new CalculateBasketCommand
        {
            CustomerId = Guid.NewGuid(),
            LoyaltyCard = "CARD001",
            TransactionDate = new DateTime(2020, 1, 10),
            Basket =
            [
                new CalculateBasketCommand.BasketItemDto { ProductId = "PRD01", Quantity = 10 }
            ]
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalAmount.Should().Be(12.0m);
        result.DiscountApplied.Should().Be(0m);
        result.GrandTotal.Should().Be(12.0m);
    }

    [Fact]
    public async Task Handle_CaseInsensitiveProductId()
    {
        // Arrange: Test case-insensitive product lookup
        var command = new CalculateBasketCommand
        {
            CustomerId = Guid.NewGuid(),
            LoyaltyCard = "CARD001",
            TransactionDate = new DateTime(2020, 1, 10),
            Basket =
            [
                new CalculateBasketCommand.BasketItemDto { ProductId = "prd01", Quantity = 10 } // lowercase
            ]
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalAmount.Should().Be(12.0m);
    }

    #endregion
}
