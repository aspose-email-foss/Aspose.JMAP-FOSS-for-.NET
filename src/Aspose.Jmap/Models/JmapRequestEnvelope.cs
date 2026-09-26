using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Aspose.Jmap;

/// <summary>
/// Represents the JMAP request envelope sent to the Session API URL.
/// Contains the capabilities used, the method calls to invoke, and an optional map of client‑generated IDs to server‑generated IDs.
/// </summary>
public sealed class JmapRequestEnvelope
{
    /// <summary>
    /// Capability URNs this request depends on, must include <c>urn:ietf:params:jmap:core</c>.
    /// </summary>
    [JsonPropertyName("using")]
    public required IReadOnlyList<string> Using { get; init; }

    /// <summary>
    /// The list of method invocations to be performed.
    /// </summary>
    [JsonPropertyName("methodCalls")]
    public required IReadOnlyList<Invocation> MethodCalls { get; init; }

    /// <summary>
    /// Optional map from client‑generated IDs to server‑generated IDs, used for creating objects.
    /// </summary>
    [JsonPropertyName("createdIds")]
    public IReadOnlyDictionary<string, string>? CreatedIds { get; init; }
}
