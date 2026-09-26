using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Aspose.Jmap;

/// <summary>
/// The JSON body returned from apiUrl. Deliberately NOT named bare "Response" for the same reason
/// JmapRequestEnvelope isn't named "Request" - see that object's description.
/// </summary>
public class JmapResponseEnvelope
{
    /// <summary>
    /// Gets the list of method responses.
    /// </summary>
    [JsonPropertyName("methodResponses")]
    public IReadOnlyList<Invocation> MethodResponses { get; init; } = null!;

    /// <summary>
    /// Gets the map of client-supplied creation IDs to server-assigned IDs, if any.
    /// </summary>
    [JsonPropertyName("createdIds")]
    public IReadOnlyDictionary<string, string>? CreatedIds { get; init; }

    /// <summary>
    /// Gets the current session state string.
    /// </summary>
    [JsonPropertyName("sessionState")]
    public string SessionState { get; init; } = null!;
}
