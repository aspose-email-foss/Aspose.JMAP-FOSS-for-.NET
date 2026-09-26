global using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Aspose.Jmap;

/// <summary>
/// Represents a single JMAP method invocation tuple consisting of a name, arguments,
/// and a client‑chosen <c>methodCallId</c>. This type is public because it appears in
/// the public <c>MethodCalls</c> and <c>MethodResponses</c> properties of request and
/// response envelopes.
///
/// Per RFC 8620 section 3.2, this serializes as a 3-element JSON ARRAY
/// <c>[name, arguments, methodCallId]</c> on the wire, never as an object with named
/// properties - see the <c>[JsonConverter(typeof(InvocationArrayConverter))]</c> partial
/// declaration and <c>InvocationArrayConverter</c> in JmapClient.Submission.cs, which is
/// what actually wires up this array-based (de)serialization for this partial class.
/// </summary>
public partial class Invocation
{
    /// <summary>
    /// The name of the JMAP method to invoke.
    /// </summary>
    public string Name { get; init; } = default!;

    /// <summary>
    /// The arguments object for the method call.
    /// </summary>
    public IReadOnlyDictionary<string, object> Arguments { get; init; } = default!;

    /// <summary>
    /// Client‑chosen tag used to correlate the request and response.
    /// </summary>
    public string MethodCallId { get; init; } = default!;
}
