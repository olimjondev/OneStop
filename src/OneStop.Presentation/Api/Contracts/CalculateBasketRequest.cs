using System.Text.Json.Serialization;

namespace OneStop.Presentation.Api.Contracts;

/// <summary>
/// Request to calculate basket totals.
/// </summary>
public sealed record CalculateBasketRequest
{
    /// <summary>
    /// Unique identifier of the customer.
    /// </summary>
    /// <example>8e4e8991-aaee-495b-9f24-52d5d0e509c5</example>
    [JsonPropertyName("CustomerId")]
    public required Guid CustomerId { get; init; }

    /// <summary>
    /// Customer's loyalty card number. Empty or null if guest.
    /// </summary>
    /// <example>CTX0000001</example>
    [JsonPropertyName("LoyaltyCard")]
    public string? LoyaltyCard { get; init; }

    /// <summary>
    /// Transaction date in dd-MMM-yyyy format.
    /// </summary>
    /// <example>10-Jan-2020</example>
    [JsonPropertyName("TransactionDate")]
    public required string TransactionDate { get; init; }

    /// <summary>
    /// List of items in the basket.
    /// </summary>
    [JsonPropertyName("Basket")]
    public required IReadOnlyList<BasketItem> Basket { get; init; }

    /// <summary>
    /// A single item in the basket.
    /// </summary>
    public sealed record BasketItem
    {
        /// <summary>
        /// Product identifier.
        /// </summary>
        /// <example>PRD01</example>
        [JsonPropertyName("ProductId")]
        public required string ProductId { get; init; }

        /// <summary>
        /// Quantity of the product.
        /// </summary>
        /// <example>3</example>
        [JsonPropertyName("Quantity")]
        public required int Quantity { get; init; }
    }
}
