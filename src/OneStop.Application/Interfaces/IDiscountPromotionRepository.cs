using OneStop.Domain.Entities;

namespace OneStop.Application.Interfaces;

/// <summary>
/// Repository for discount promotion data access.
/// </summary>
public interface IDiscountPromotionRepository
{
    /// <summary>
    /// Gets all discount promotions active on the specified date.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of active discount promotions.</returns>
    Task<IReadOnlyList<DiscountPromotion>> GetActiveOnDateAsync(
        DateTime date, 
        CancellationToken cancellationToken = default);
}
