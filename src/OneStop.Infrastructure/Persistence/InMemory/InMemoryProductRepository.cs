using OneStop.Application.Interfaces;
using OneStop.Domain.Entities;
using OneStop.Domain.Enums;

namespace OneStop.Infrastructure.Persistence.InMemory;

/// <summary>
/// In-memory implementation of the product repository with seeded data.
/// </summary>
public class InMemoryProductRepository : IProductRepository
{
    private static readonly Dictionary<string, Product> Products = new(StringComparer.OrdinalIgnoreCase)
    {
        ["PRD01"] = new Product("PRD01", "Vortex 95", ProductCategory.Fuel, 1.2m),
        ["PRD02"] = new Product("PRD02", "Vortex 98", ProductCategory.Fuel, 1.3m),
        ["PRD03"] = new Product("PRD03", "Diesel", ProductCategory.Fuel, 1.1m),
        ["PRD04"] = new Product("PRD04", "Twix 55g", ProductCategory.Shop, 2.3m),
        ["PRD05"] = new Product("PRD05", "Mars 72g", ProductCategory.Shop, 5.1m),
        ["PRD06"] = new Product("PRD06", "SNICKERS 72G", ProductCategory.Shop, 3.4m),
        ["PRD07"] = new Product("PRD07", "Bounty 3 63g", ProductCategory.Shop, 6.9m),
        ["PRD08"] = new Product("PRD08", "Snickers 50g", ProductCategory.Shop, 4.0m)
    };

    /// <inheritdoc />
    public Task<Product?> GetByIdAsync(string productId, CancellationToken cancellationToken = default)
    {
        Products.TryGetValue(productId, out var product);
        return Task.FromResult(product);
    }

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<string, Product>> GetByIdsAsync(
        IEnumerable<string> productIds, 
        CancellationToken cancellationToken = default)
    {
        var result = productIds
            .Where(id => Products.ContainsKey(id))
            .ToDictionary(
                id => id, 
                id => Products[id], 
                StringComparer.OrdinalIgnoreCase);

        return Task.FromResult<IReadOnlyDictionary<string, Product>>(result);
    }
}
