using OneStop.Domain.Enums;

namespace OneStop.Domain.Entities;

/// <summary>
/// Represents a points promotion that awards loyalty points based on purchase amount.
/// </summary>
public class PointsPromotion
{
    public string Id { get; }
    public string Name { get; }
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    
    /// <summary>
    /// The category filter for this promotion. Null means "Any" (all categories qualify).
    /// </summary>
    public ProductCategory? CategoryFilter { get; }
    
    public int PointsPerDollar { get; }

    public PointsPromotion(
        string id,
        string name,
        DateTime startDate,
        DateTime endDate,
        ProductCategory? categoryFilter,
        int pointsPerDollar)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Promotion ID cannot be empty.", nameof(id));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Promotion name cannot be empty.", nameof(name));
        
        if (endDate < startDate)
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));
        
        if (pointsPerDollar < 0)
            throw new ArgumentException("Points per dollar cannot be negative.", nameof(pointsPerDollar));

        Id = id;
        Name = name;
        StartDate = startDate.Date;
        EndDate = endDate.Date;
        CategoryFilter = categoryFilter;
        PointsPerDollar = pointsPerDollar;
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
    /// Checks if the promotion applies to the specified product category.
    /// </summary>
    /// <param name="category">The product category to check.</param>
    /// <returns>True if the promotion applies to products in the category.</returns>
    public bool AppliesToCategory(ProductCategory category)
    {
        // Null CategoryFilter means "Any" - applies to all categories
        return CategoryFilter is null || CategoryFilter == category;
    }

    /// <summary>
    /// Calculates the points earned for a given amount.
    /// </summary>
    /// <param name="amount">The qualifying purchase amount (post-discount).</param>
    /// <returns>The points earned (floored to nearest whole number).</returns>
    public int CalculatePoints(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        
        return (int)Math.Floor(amount * PointsPerDollar);
    }
}
