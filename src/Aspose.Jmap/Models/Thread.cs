using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Aspose.Jmap;

/// <summary>
/// An ordered list of Email ids that make up a conversation.
/// </summary>
public class Thread
{
    /// <summary>
    /// Server-assigned identifier of the thread.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// Ordered list of Email ids that make up the conversation.
    /// </summary>
    [JsonPropertyName("emailIds")]
    public IReadOnlyList<string>? EmailIds { get; init; }
}
