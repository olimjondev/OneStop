namespace OneStop.Domain.Models;

/// <summary>
/// Represents the result of a basket calculation.
/// </summary>
public sealed class BasketCalculationResult
{
    /// <summary>
    /// The total amount before any discounts.
    /// </summary>
    public decimal TotalAmount { get; }
    
    /// <summary>
    /// The total discount applied.
    /// </summary>
    public decimal DiscountApplied { get; }
    
    /// <summary>
    /// The final amount after discounts (TotalAmount - DiscountApplied).
    /// </summary>
    public decimal GrandTotal { get; }
    
    /// <summary>
    /// The loyalty points earned (0 if no loyalty card).
    /// </summary>
    public int PointsEarned { get; }

    public BasketCalculationResult(decimal totalAmount, decimal discountApplied, int pointsEarned)
    {
        if (totalAmount < 0)
            throw new ArgumentException("Total amount cannot be negative.", nameof(totalAmount));
        
        if (discountApplied < 0)
            throw new ArgumentException("Discount applied cannot be negative.", nameof(discountApplied));
        
        if (discountApplied > totalAmount)
            throw new ArgumentException("Discount cannot exceed total amount.", nameof(discountApplied));
        
        if (pointsEarned < 0)
            throw new ArgumentException("Points earned cannot be negative.", nameof(pointsEarned));

        TotalAmount = Math.Round(totalAmount, 2);
        DiscountApplied = Math.Round(discountApplied, 2);
        GrandTotal = Math.Round(totalAmount - discountApplied, 2);
        PointsEarned = pointsEarned;
    }
}
