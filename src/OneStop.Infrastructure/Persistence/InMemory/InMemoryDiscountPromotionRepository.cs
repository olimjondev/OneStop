using OneStop.Application.Interfaces;
using OneStop.Domain.Entities;

namespace OneStop.Infrastructure.Persistence.InMemory;

/// <summary>
/// In-memory implementation of the discount promotion repository with seeded data.
/// </summary>
public class InMemoryDiscountPromotionRepository : IDiscountPromotionRepository
{
    private static readonly List<DiscountPromotion> Promotions =
    [
        new DiscountPromotion(
            id: "DP001",
            name: "Fuel Discount Promo",
            startDate: new DateTime(2020, 1, 1),
            endDate: new DateTime(2020, 2, 15),
            discountPercentage: 20m,
            eligibleProductIds: ["PRD02"]), // Note: duplicate in original data treated as single entry
        
        new DiscountPromotion(
            id: "DP002",
            name: "Happy Promo",
            startDate: new DateTime(2020, 3, 2),
            endDate: new DateTime(2020, 3, 20),
            discountPercentage: 15m,
            eligibleProductIds: []) // No products in junction table = no products get discount
    ];

    /// <inheritdoc />
    public Task<IReadOnlyList<DiscountPromotion>> GetActiveOnDateAsync(
        DateTime date, 
        CancellationToken cancellationToken = default)
    {
        var activePromotions = Promotions
            .Where(p => p.IsActiveOn(date))
            .ToList();

        return Task.FromResult<IReadOnlyList<DiscountPromotion>>(activePromotions);
    }
}
