using System.Text.Json.Serialization;

namespace OneStop.Presentation.Api.Contracts;

/// <summary>
/// Response containing calculated basket totals.
/// </summary>
public sealed record CalculateBasketResponse
{
    /// <summary>
    /// Unique identifier of the customer.
    /// </summary>
    /// <example>8e4e8991-aaee-495b-9f24-52d5d0e509c5</example>
    [JsonPropertyName("CustomerId")]
    public required Guid CustomerId { get; init; }

    /// <summary>
    /// Customer's loyalty card number. Null if guest.
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
    /// Total amount before discounts.
    /// </summary>
    /// <example>16.60</example>
    [JsonPropertyName("TotalAmount")]
    public required decimal TotalAmount { get; init; }

    /// <summary>
    /// Total discount applied.
    /// </summary>
    /// <example>2.60</example>
    [JsonPropertyName("DiscountApplied")]
    public required decimal DiscountApplied { get; init; }

    /// <summary>
    /// Final amount after discounts.
    /// </summary>
    /// <example>14.00</example>
    [JsonPropertyName("GrandTotal")]
    public required decimal GrandTotal { get; init; }

    /// <summary>
    /// Loyalty points earned. Zero if no loyalty card.
    /// </summary>
    /// <example>28</example>
    [JsonPropertyName("PointsEarned")]
    public required int PointsEarned { get; init; }
}
