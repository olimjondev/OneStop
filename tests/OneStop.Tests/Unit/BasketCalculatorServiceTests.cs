using FluentAssertions;
using OneStop.Domain.Entities;
using OneStop.Domain.Enums;
using OneStop.Domain.Exceptions;
using OneStop.Domain.Services;
using OneStop.Domain.ValueObjects;

namespace OneStop.Tests.Unit;

/// <summary>
/// Unit tests for BasketCalculatorService.
/// Pure domain logic tests - no mocks required.
/// </summary>
public class BasketCalculatorServiceTests
{
    private readonly BasketCalculatorService _sut = new();

    #region Test Data Factories

    private static Product CreateProduct(string id, ProductCategory category, decimal price) =>
        new(id, $"Product {id}", category, price);

    private static (BasketLineItem LineItem, Product Product) CreateLineItem(
        string productId, 
        int quantity, 
        ProductCategory category, 
        decimal unitPrice) =>
        (new BasketLineItem(productId, quantity), CreateProduct(productId, category, unitPrice));

    private static DiscountPromotion CreateDiscountPromotion(
        string id,
        decimal discountPercent,
        params string[] productIds) =>
        new(id, $"Promo {id}", 
            DateTime.Today.AddDays(-1), 
            DateTime.Today.AddDays(1), 
            discountPercent, 
            productIds);

    private static PointsPromotion CreatePointsPromotion(
        string id,
        ProductCategory? category,
        int pointsPerDollar) =>
        new(id, $"Points {id}",
            DateTime.Today.AddDays(-1),
            DateTime.Today.AddDays(1),
            category,
            pointsPerDollar);

    #endregion

    #region Basic Calculation Tests

    [Fact]
    public void Calculate_SingleItem_NoPromotions_ReturnsCorrectTotal()
    {
        // Arrange
        var lineItems = new List<(BasketLineItem, Product)>
        {
            CreateLineItem("PRD01", 3, ProductCategory.Fuel, 1.2m)
        };

        // Act
        var result = _sut.Calculate(lineItems, [], null, hasLoyaltyCard: false);

        // Assert
        result.TotalAmount.Should().Be(3.6m);
        result.DiscountApplied.Should().Be(0m);
        result.GrandTotal.Should().Be(3.6m);
        result.PointsEarned.Should().Be(0);
    }

    [Fact]
    public void Calculate_MultipleItems_NoPromotions_ReturnsSumOfLineTotals()
    {
        // Arrange
        var lineItems = new List<(BasketLineItem, Product)>
        {
            CreateLineItem("PRD01", 3, ProductCategory.Fuel, 1.2m),    // 3.60
            CreateLineItem("PRD02", 10, ProductCategory.Fuel, 1.3m)   // 13.00
        };

        // Act
        var result = _sut.Calculate(lineItems, [], null, hasLoyaltyCard: false);

        // Assert
        result.TotalAmount.Should().Be(16.6m);
        result.DiscountApplied.Should().Be(0m);
        result.GrandTotal.Should().Be(16.6m);
    }

    #endregion

    #region Discount Calculation Tests

    [Fact]
    public void Calculate_WithDiscount_AppliesDiscountToEligibleProduct()
    {
        // Arrange
        var lineItems = new List<(BasketLineItem, Product)>
        {
            CreateLineItem("PRD01", 3, ProductCategory.Fuel, 1.2m),    // 3.60 - no discount
            CreateLineItem("PRD02", 10, ProductCategory.Fuel, 1.3m)   // 13.00 - 20% discount = 2.60
        };
        var discounts = new List<DiscountPromotion>
        {
            CreateDiscountPromotion("DP001", 20m, "PRD02")
        };

        // Act
        var result = _sut.Calculate(lineItems, discounts, null, hasLoyaltyCard: false);

        // Assert
        result.TotalAmount.Should().Be(16.6m);
        result.DiscountApplied.Should().Be(2.6m);
        result.GrandTotal.Should().Be(14.0m);
    }

    [Fact]
    public void Calculate_DiscountPromotion_NoProductsInJunctionTable_NoDiscountApplied()
    {
        // Arrange: DP002 "Happy Promo" has no products
        var lineItems = new List<(BasketLineItem, Product)>
        {
            CreateLineItem("PRD04", 2, ProductCategory.Shop, 2.3m)
        };
        var discounts = new List<DiscountPromotion>
        {
            CreateDiscountPromotion("DP002", 15m) // No product IDs
        };

        // Act
        var result = _sut.Calculate(lineItems, discounts, null, hasLoyaltyCard: false);

        // Assert
        result.TotalAmount.Should().Be(4.6m);
        result.DiscountApplied.Should().Be(0m);
        result.GrandTotal.Should().Be(4.6m);
    }

    [Fact]
    public void Calculate_MultipleDiscounts_DifferentProducts_BothApplied()
    {
        // Arrange
        var lineItems = new List<(BasketLineItem, Product)>
        {
            CreateLineItem("PRD01", 10, ProductCategory.Fuel, 1.0m),  // $10.00 - 10% = $1.00 discount
            CreateLineItem("PRD02", 10, ProductCategory.Fuel, 2.0m)   // $20.00 - 20% = $4.00 discount
        };
        var discounts = new List<DiscountPromotion>
        {
            CreateDiscountPromotion("DP001", 10m, "PRD01"),
            CreateDiscountPromotion("DP002", 20m, "PRD02")
        };

        // Act
        var result = _sut.Calculate(lineItems, discounts, null, hasLoyaltyCard: false);

        // Assert
        result.TotalAmount.Should().Be(30.0m);
        result.DiscountApplied.Should().Be(5.0m);
        result.GrandTotal.Should().Be(25.0m);
    }

    #endregion

    #region Points Calculation Tests

    [Fact]
    public void Calculate_WithPointsPromotion_NoLoyaltyCard_NoPointsEarned()
    {
        // Arrange
        var lineItems = new List<(BasketLineItem, Product)>
        {
            CreateLineItem("PRD01", 10, ProductCategory.Fuel, 1.0m)
        };
        var pointsPromo = CreatePointsPromotion("PP001", null, 2);

        // Act
        var result = _sut.Calculate(lineItems, [], pointsPromo, hasLoyaltyCard: false);

        // Assert
        result.PointsEarned.Should().Be(0);
    }

    [Fact]
    public void Calculate_WithPointsPromotion_WithLoyaltyCard_PointsEarned()
    {
        // Arrange: $10 at 2 points per dollar = 20 points
        var lineItems = new List<(BasketLineItem, Product)>
        {
            CreateLineItem("PRD01", 10, ProductCategory.Fuel, 1.0m)
        };
        var pointsPromo = CreatePointsPromotion("PP001", null, 2);

        // Act
        var result = _sut.Calculate(lineItems, [], pointsPromo, hasLoyaltyCard: true);

        // Assert
        result.PointsEarned.Should().Be(20);
    }

    [Fact]
    public void Calculate_PointsOnPostDiscountAmount()
    {
        // Arrange: $13.00 - 20% = $10.40 at 2 points per dollar = 20 points (floored)
        var lineItems = new List<(BasketLineItem, Product)>
        {
            CreateLineItem("PRD02", 10, ProductCategory.Fuel, 1.3m)
        };
        var discounts = new List<DiscountPromotion>
        {
            CreateDiscountPromotion("DP001", 20m, "PRD02")
        };
        var pointsPromo = CreatePointsPromotion("PP001", null, 2);

        // Act
        var result = _sut.Calculate(lineItems, discounts, pointsPromo, hasLoyaltyCard: true);

        // Assert
        result.GrandTotal.Should().Be(10.4m);
        result.PointsEarned.Should().Be(20); // floor(10.4 * 2) = 20
    }

    [Fact]
    public void Calculate_PointsPromotion_CategoryFilter_OnlyMatchingCategoryEarnsPoints()
    {
        // Arrange: Fuel promo, but basket has shop item
        var lineItems = new List<(BasketLineItem, Product)>
        {
            CreateLineItem("PRD04", 10, ProductCategory.Shop, 1.0m)  // Shop product
        };
        var pointsPromo = CreatePointsPromotion("PP002", ProductCategory.Fuel, 3);

        // Act
        var result = _sut.Calculate(lineItems, [], pointsPromo, hasLoyaltyCard: true);

        // Assert
        result.PointsEarned.Should().Be(0); // Shop items don't qualify for Fuel promo
    }

    [Fact]
    public void Calculate_PointsPromotion_AnyCategory_AllProductsEarnPoints()
    {
        // Arrange: "Any" category = null filter
        var lineItems = new List<(BasketLineItem, Product)>
        {
            CreateLineItem("PRD01", 5, ProductCategory.Fuel, 1.0m),  // $5.00
            CreateLineItem("PRD04", 5, ProductCategory.Shop, 1.0m)   // $5.00
        };
        var pointsPromo = CreatePointsPromotion("PP001", null, 2); // null = Any

        // Act
        var result = _sut.Calculate(lineItems, [], pointsPromo, hasLoyaltyCard: true);

        // Assert
        result.PointsEarned.Should().Be(20); // floor(10 * 2) = 20
    }

    [Fact]
    public void Calculate_PointsPromotion_MixedCategories_OnlyMatchingEarnsPoints()
    {
        // Arrange: Shop promo with mixed basket
        var lineItems = new List<(BasketLineItem, Product)>
        {
            CreateLineItem("PRD01", 10, ProductCategory.Fuel, 1.0m),  // $10 - no points
            CreateLineItem("PRD04", 10, ProductCategory.Shop, 1.0m)   // $10 - earns points
        };
        var pointsPromo = CreatePointsPromotion("PP003", ProductCategory.Shop, 4);

        // Act
        var result = _sut.Calculate(lineItems, [], pointsPromo, hasLoyaltyCard: true);

        // Assert
        result.PointsEarned.Should().Be(40); // floor(10 * 4) = 40 (only shop items)
    }

    #endregion

    #region Combined Discount and Points Tests

    [Fact]
    public void Calculate_DiscountAndPoints_BothApplied()
    {
        // Arrange: Sample scenario from requirements
        // PRD01: 3 x $1.20 = $3.60 (no discount)
        // PRD02: 10 x $1.30 = $13.00 - 20% = $10.40
        // Total: $16.60, Discount: $2.60, Grand: $14.00
        // Points: floor(14.00 * 2) = 28
        var lineItems = new List<(BasketLineItem, Product)>
        {
            CreateLineItem("PRD01", 3, ProductCategory.Fuel, 1.2m),
            CreateLineItem("PRD02", 10, ProductCategory.Fuel, 1.3m)
        };
        var discounts = new List<DiscountPromotion>
        {
            CreateDiscountPromotion("DP001", 20m, "PRD02")
        };
        var pointsPromo = CreatePointsPromotion("PP001", null, 2);

        // Act
        var result = _sut.Calculate(lineItems, discounts, pointsPromo, hasLoyaltyCard: true);

        // Assert
        result.TotalAmount.Should().Be(16.6m);
        result.DiscountApplied.Should().Be(2.6m);
        result.GrandTotal.Should().Be(14.0m);
        result.PointsEarned.Should().Be(28);
    }

    #endregion

    #region Edge Cases and Validation Tests

    [Fact]
    public void Calculate_EmptyBasket_ThrowsArgumentException()
    {
        // Arrange
        var lineItems = new List<(BasketLineItem, Product)>();

        // Act
        var act = () => _sut.Calculate(lineItems, [], null, hasLoyaltyCard: false);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Calculate_NullLineItems_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _sut.Calculate(null!, [], null, hasLoyaltyCard: false);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Calculate_OverlappingDiscounts_SameProduct_ThrowsDomainException()
    {
        // Arrange: Same product in two active discount promotions
        var lineItems = new List<(BasketLineItem, Product)>
        {
            CreateLineItem("PRD01", 10, ProductCategory.Fuel, 1.0m)
        };
        var discounts = new List<DiscountPromotion>
        {
            CreateDiscountPromotion("DP001", 10m, "PRD01"),
            CreateDiscountPromotion("DP002", 20m, "PRD01")
        };

        // Act
        var act = () => _sut.Calculate(lineItems, discounts, null, hasLoyaltyCard: false);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*PRD01*multiple active discount promotions*");
    }

    [Fact]
    public void Calculate_NoActivePromotions_ReturnsZeroDiscountAndPoints()
    {
        // Arrange
        var lineItems = new List<(BasketLineItem, Product)>
        {
            CreateLineItem("PRD01", 10, ProductCategory.Fuel, 1.0m)
        };

        // Act
        var result = _sut.Calculate(lineItems, [], null, hasLoyaltyCard: true);

        // Assert
        result.TotalAmount.Should().Be(10.0m);
        result.DiscountApplied.Should().Be(0m);
        result.GrandTotal.Should().Be(10.0m);
        result.PointsEarned.Should().Be(0);
    }

    [Fact]
    public void Calculate_PointsFloored_NoPartialPoints()
    {
        // Arrange: $3.33 at 2 points/$ = 6.66 → 6 points
        var lineItems = new List<(BasketLineItem, Product)>
        {
            CreateLineItem("PRD01", 1, ProductCategory.Fuel, 3.33m)
        };
        var pointsPromo = CreatePointsPromotion("PP001", null, 2);

        // Act
        var result = _sut.Calculate(lineItems, [], pointsPromo, hasLoyaltyCard: true);

        // Assert
        result.PointsEarned.Should().Be(6);
    }

    [Fact]
    public void Calculate_Rounding_TwoDecimalPlaces()
    {
        // Arrange: Ensure rounding to 2 decimal places
        var lineItems = new List<(BasketLineItem, Product)>
        {
            CreateLineItem("PRD01", 3, ProductCategory.Fuel, 1.111m)  // 3.333
        };
        var discounts = new List<DiscountPromotion>
        {
            CreateDiscountPromotion("DP001", 33.33m, "PRD01")  // 33.33% of 3.333 = 1.110889
        };

        // Act
        var result = _sut.Calculate(lineItems, discounts, null, hasLoyaltyCard: false);

        // Assert
        result.TotalAmount.Should().Be(3.33m); // rounded
        result.DiscountApplied.Should().Be(1.11m); // rounded
        result.GrandTotal.Should().Be(2.22m);
    }

    #endregion
}
