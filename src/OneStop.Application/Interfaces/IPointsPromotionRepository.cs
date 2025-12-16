using OneStop.Domain.Entities;

namespace OneStop.Application.Interfaces;

/// <summary>
/// Repository for points promotion data access.
/// </summary>
public interface IPointsPromotionRepository
{
    /// <summary>
    /// Gets the active points promotion for the specified date.
    /// Only one points promotion can be active at any given time.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The active points promotion if one exists, null otherwise.</returns>
    Task<PointsPromotion?> GetActiveOnDateAsync(
        DateTime date, 
        CancellationToken cancellationToken = default);
}
