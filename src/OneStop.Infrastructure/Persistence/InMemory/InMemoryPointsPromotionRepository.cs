using OneStop.Application.Interfaces;
using OneStop.Domain.Entities;
using OneStop.Domain.Enums;

namespace OneStop.Infrastructure.Persistence.InMemory;

/// <summary>
/// In-memory implementation of the points promotion repository with seeded data.
/// </summary>
public class InMemoryPointsPromotionRepository : IPointsPromotionRepository
{
    private static readonly List<PointsPromotion> Promotions =
    [
        new PointsPromotion(
            id: "PP001",
            name: "New Year Promo",
            startDate: new DateTime(2020, 1, 1),
            endDate: new DateTime(2020, 1, 30),
            categoryFilter: null, // "Any" - all categories
            pointsPerDollar: 2),
        
        new PointsPromotion(
            id: "PP002",
            name: "Fuel Promo",
            startDate: new DateTime(2020, 2, 5),
            endDate: new DateTime(2020, 2, 15),
            categoryFilter: ProductCategory.Fuel,
            pointsPerDollar: 3),
        
        new PointsPromotion(
            id: "PP003",
            name: "Shop Promo",
            startDate: new DateTime(2020, 3, 1),
            endDate: new DateTime(2020, 3, 20),
            categoryFilter: ProductCategory.Shop,
            pointsPerDollar: 4)
    ];

    /// <inheritdoc />
    public Task<PointsPromotion?> GetActiveOnDateAsync(
        DateTime date, 
        CancellationToken cancellationToken = default)
    {
        // Per requirement: only one points promotion can be active at any time
        var activePromotion = Promotions.FirstOrDefault(p => p.IsActiveOn(date));
        return Task.FromResult(activePromotion);
    }
}
