using System.Text.Json.Serialization;

namespace Aspose.Jmap;

/// <summary>
/// Highlighted subject/preview snippet for an Email id matched by a filter's text search.
/// </summary>
public class SearchSnippet
{
    /// <summary>
    /// Identifier of the email.
    /// </summary>
    [JsonPropertyName("emailId")]
    public string EmailId { get; init; } = default!;

    /// <summary>
    /// May contain <mark></mark> tags around matches.
    /// </summary>
    [JsonPropertyName("subject")]
    public string? Subject { get; init; }

    /// <summary>
    /// Preview snippet for the email.
    /// </summary>
    [JsonPropertyName("preview")]
    public string? Preview { get; init; }
}
