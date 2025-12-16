using System.Text.Json.Serialization;

namespace OneStop.Presentation.Api.Contracts;

/// <summary>
/// Standard error response.
/// </summary>
public sealed record ErrorResponse
{
    /// <summary>
    /// Error type identifier.
    /// </summary>
    /// <example>ValidationError</example>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    /// <example>One or more validation errors occurred.</example>
    [JsonPropertyName("message")]
    public required string Message { get; init; }

    /// <summary>
    /// Detailed validation errors by field. Null for non-validation errors.
    /// </summary>
    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, string[]>? Errors { get; init; }
}
