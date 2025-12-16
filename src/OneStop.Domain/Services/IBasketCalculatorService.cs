using OneStop.Domain.Entities;
using OneStop.Domain.Models;
using OneStop.Domain.ValueObjects;

namespace OneStop.Domain.Services;

/// <summary>
/// Service for calculating basket totals, discounts, and loyalty points.
/// </summary>
public interface IBasketCalculatorService
{
    /// <summary>
    /// Calculates the basket totals including discounts and loyalty points.
    /// </summary>
    /// <param name="lineItems">The basket line items with resolved products.</param>
    /// <param name="activeDiscountPromotions">Active discount promotions on the transaction date.</param>
    /// <param name="activePointsPromotion">Active points promotion on the transaction date (only one allowed).</param>
    /// <param name="hasLoyaltyCard">Whether the customer has a loyalty card.</param>
    /// <returns>The calculation result.</returns>
    BasketCalculationResult Calculate(
        IReadOnlyList<(BasketLineItem LineItem, Product Product)> lineItems,
        IReadOnlyList<DiscountPromotion> activeDiscountPromotions,
        PointsPromotion? activePointsPromotion,
        bool hasLoyaltyCard);
}
