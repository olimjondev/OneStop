using OneStop.Domain.Enums;

namespace OneStop.Domain.Entities;

/// <summary>
/// Represents a product available for purchase.
/// </summary>
public class Product
{
    public string ProductId { get; }
    public string Name { get; }
    public ProductCategory Category { get; }
    public decimal UnitPrice { get; }

    public Product(string productId, string name, ProductCategory category, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be empty.", nameof(productId));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty.", nameof(name));
        
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        ProductId = productId;
        Name = name;
        Category = category;
        UnitPrice = unitPrice;
    }

    /// <summary>
    /// Calculates the line total for a given quantity.
    /// </summary>
    /// <param name="quantity">The quantity of items.</param>
    /// <returns>The total price for the line item.</returns>
    public decimal CalculateLineTotal(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative.", nameof(quantity));
        
        return UnitPrice * quantity;
    }

    /// <summary>
    /// Checks if this product belongs to the specified category.
    /// </summary>
    /// <param name="category">The category to check against.</param>
    /// <returns>True if the product belongs to the category.</returns>
    public bool BelongsToCategory(ProductCategory category) => Category == category;
}
