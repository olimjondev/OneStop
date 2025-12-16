using OneStop.Domain.Entities;
using OneStop.Domain.Exceptions;
using OneStop.Domain.Models;
using OneStop.Domain.ValueObjects;

namespace OneStop.Domain.Services;

/// <summary>
/// Pure domain service for basket calculations.
/// Contains only business logic - no I/O, no async, no external dependencies.
/// </summary>
public class BasketCalculatorService : IBasketCalculatorService
{
    /// <inheritdoc />
    public BasketCalculationResult Calculate(
        IReadOnlyList<(BasketLineItem LineItem, Product Product)> lineItems,
        IReadOnlyList<DiscountPromotion> activeDiscountPromotions,
        PointsPromotion? activePointsPromotion,
        bool hasLoyaltyCard)
    {
        ArgumentNullException.ThrowIfNull(lineItems);
        ArgumentNullException.ThrowIfNull(activeDiscountPromotions);

        if (lineItems.Count == 0)
            throw new ArgumentException("Basket cannot be empty.", nameof(lineItems));

        // Validate no product has multiple active discount promotions
        ValidateNoOverlappingDiscounts(lineItems, activeDiscountPromotions);

        var totalAmount = 0m;
        var totalDiscount = 0m;
        var pointsQualifyingAmount = 0m;

        foreach (var (lineItem, product) in lineItems)
        {
            var lineTotal = product.CalculateLineTotal(lineItem.Quantity);
            totalAmount += lineTotal;

            // Find applicable discount for this product
            var applicableDiscount = FindApplicableDiscount(product.ProductId, activeDiscountPromotions);
            var lineDiscount = applicableDiscount?.CalculateDiscount(lineTotal) ?? 0m;
            totalDiscount += lineDiscount;

            // Calculate points-qualifying amount (post-discount) if promotion applies to this category
            if (activePointsPromotion is not null && activePointsPromotion.AppliesToCategory(product.Category))
            {
                pointsQualifyingAmount += lineTotal - lineDiscount;
            }
        }

        // Calculate points only if customer has loyalty card
        var pointsEarned = hasLoyaltyCard && activePointsPromotion is not null
            ? activePointsPromotion.CalculatePoints(pointsQualifyingAmount)
            : 0;

        return new BasketCalculationResult(totalAmount, totalDiscount, pointsEarned);
    }

    /// <summary>
    /// Validates that no product is covered by multiple active discount promotions.
    /// </summary>
    /// <remarks>
    /// Per requirement, validation should prevent overlapping discounts for the same product.
    /// If this occurs at runtime, it indicates a data integrity issue.
    /// Alternative approach: Apply highest discount. We chose to throw as per requirements.
    /// </remarks>
    private static void ValidateNoOverlappingDiscounts(
        IReadOnlyList<(BasketLineItem LineItem, Product Product)> lineItems,
        IReadOnlyList<DiscountPromotion> activeDiscountPromotions)
    {
        foreach (var (lineItem, _) in lineItems)
        {
            var applicablePromotions = activeDiscountPromotions
                .Where(p => p.AppliesToProduct(lineItem.ProductId))
                .ToList();

            if (applicablePromotions.Count > 1)
            {
                throw new DomainException(
                    $"Product '{lineItem.ProductId}' is covered by multiple active discount promotions: " +
                    $"{string.Join(", ", applicablePromotions.Select(p => p.Id))}. " +
                    "Only one discount promotion should be active for a product at any given time.");
            }
        }
    }

    /// <summary>
    /// Finds the applicable discount promotion for a product.
    /// </summary>
    private static DiscountPromotion? FindApplicableDiscount(
        string productId, 
        IReadOnlyList<DiscountPromotion> activeDiscountPromotions)
    {
        return activeDiscountPromotions.FirstOrDefault(p => p.AppliesToProduct(productId));
    }
}
