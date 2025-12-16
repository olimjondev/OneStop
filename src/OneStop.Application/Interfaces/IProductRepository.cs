using OneStop.Domain.Entities;

namespace OneStop.Application.Interfaces;

/// <summary>
/// Repository for product data access.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Gets a product by its ID.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The product if found, null otherwise.</returns>
    Task<Product?> GetByIdAsync(string productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple products by their IDs.
    /// </summary>
    /// <param name="productIds">The product IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary of found products keyed by product ID.</returns>
    Task<IReadOnlyDictionary<string, Product>> GetByIdsAsync(
        IEnumerable<string> productIds, 
        CancellationToken cancellationToken = default);
}
