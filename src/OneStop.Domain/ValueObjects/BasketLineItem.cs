namespace OneStop.Domain.ValueObjects;

/// <summary>
/// Represents a line item in a basket with product and quantity.
/// </summary>
public sealed class BasketLineItem : IEquatable<BasketLineItem>
{
    public string ProductId { get; }
    public int Quantity { get; }

    public BasketLineItem(string productId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be empty.", nameof(productId));
        
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        ProductId = productId;
        Quantity = quantity;
    }

    public bool Equals(BasketLineItem? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ProductId == other.ProductId && Quantity == other.Quantity;
    }

    public override bool Equals(object? obj) => Equals(obj as BasketLineItem);

    public override int GetHashCode() => HashCode.Combine(ProductId, Quantity);

    public static bool operator ==(BasketLineItem? left, BasketLineItem? right) => Equals(left, right);

    public static bool operator !=(BasketLineItem? left, BasketLineItem? right) => !Equals(left, right);
}
