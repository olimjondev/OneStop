namespace OneStop.Domain.Entities;

/// <summary>
/// Represents a discount promotion that applies percentage discounts to specific products.
/// </summary>
public class DiscountPromotion
{
    private readonly HashSet<string> _eligibleProductIds;

    public string Id { get; }
    public string Name { get; }
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public decimal DiscountPercentage { get; }
    public IReadOnlySet<string> EligibleProductIds => _eligibleProductIds;

    public DiscountPromotion(
        string id,
        string name,
        DateTime startDate,
        DateTime endDate,
        decimal discountPercentage,
        IEnumerable<string> eligibleProductIds)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Promotion ID cannot be empty.", nameof(id));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Promotion name cannot be empty.", nameof(name));
        
        if (endDate < startDate)
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));
        
        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ArgumentException("Discount percentage must be between 0 and 100.", nameof(discountPercentage));

        Id = id;
        Name = name;
        StartDate = startDate.Date;
        EndDate = endDate.Date;
        DiscountPercentage = discountPercentage;
        _eligibleProductIds = new HashSet<string>(eligibleProductIds ?? [], StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the promotion is active on the specified date.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <returns>True if the promotion is active on the date.</returns>
    public bool IsActiveOn(DateTime date)
    {
        var checkDate = date.Date;
        return checkDate >= StartDate && checkDate <= EndDate;
    }

    /// <summary>
    /// Checks if the promotion applies to the specified product.
    /// </summary>
    /// <param name="productId">The product ID to check.</param>
    /// <returns>True if the promotion applies to the product.</returns>
    public bool AppliesToProduct(string productId)
    {
        return _eligibleProductIds.Contains(productId);
    }

    /// <summary>
    /// Calculates the discount amount for a given line total.
    /// </summary>
    /// <param name="lineTotal">The line total before discount.</param>
    /// <returns>The discount amount.</returns>
    public decimal CalculateDiscount(decimal lineTotal)
    {
        if (lineTotal < 0)
            throw new ArgumentException("Line total cannot be negative.", nameof(lineTotal));
        
        return Math.Round(lineTotal * (DiscountPercentage / 100m), 2);
    }
}
