using System.Text.Json.Serialization;

namespace Aspose.Jmap;

/// <summary>
/// One entry of an Email/query 'sort' argument.
/// </summary>
public sealed class Comparator
{
    /// <summary>
    /// The name of the property to sort by, e.g. receivedAt, from, subject, size.
    /// </summary>
    [JsonPropertyName("property")]
    public string Property { get; init; } = null!;

    /// <summary>
    /// Indicates whether the sort is ascending. Default is true.
    /// </summary>
    [JsonPropertyName("isAscending")]
    public bool IsAscending { get; init; } = true;

    /// <summary>
    /// Collation to use for string comparison, or null if not specified.
    /// </summary>
    [JsonPropertyName("collation")]
    public string? Collation { get; init; }
}
