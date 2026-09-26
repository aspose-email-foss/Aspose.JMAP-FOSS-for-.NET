#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Aspose.Jmap;

/// <summary>
/// Provides methods for the JMAP <c>submission</c> capability.
/// </summary>
public sealed partial class JmapClient
{
    private const string SubmissionCapability = "urn:ietf:params:jmap:submission";

    /// <summary>
    /// Merges two JSON object elements: every property of <paramref name="overlay"/> is
    /// added to (or replaces the same-named property of) <paramref name="baseObj"/>.
    /// Used to reconstruct a full object from a base plus a partial "set" response entry
    /// (RFC 8620 section 5.3).
    /// </summary>
    private static JsonElement MergeJsonOverlay(JsonElement baseObj, JsonElement overlay)
    {
        var merged = new Dictionary<string, JsonElement>();
        if (baseObj.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in baseObj.EnumerateObject()) merged[prop.Name] = prop.Value;
        }
        if (overlay.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in overlay.EnumerateObject()) merged[prop.Name] = prop.Value;
        }
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(merged));
        return doc.RootElement.Clone();
    }

    /// <summary>
    /// Sends an email by creating an <see cref="EmailSubmission"/>. The underlying JMAP method is <c>EmailSubmission/set</c>.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="create">
    /// A map where each key is a client‑chosen creation identifier and each value is an <see cref="EmailSubmission"/> describing the submission to create.
    /// </param>
    /// <param name="onSuccessUpdateEmail">
    /// Optional map of creation identifiers to patch objects that will be applied to the related <c>Email</c> objects on successful submission.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A map of creation identifiers to the created <see cref="EmailSubmission"/> objects as returned by the server.
    /// </returns>
    /// <exception cref="JmapNetworkException">Thrown when a transport error occurs.</exception>
    /// <exception cref="JmapProtocolException">Thrown when the server returns a protocol error.</exception>
    public async Task<IReadOnlyDictionary<string, EmailSubmission>> SendAsync(
        string accountId,
        IReadOnlyDictionary<string, EmailSubmission> create,
        IReadOnlyDictionary<string, object>? onSuccessUpdateEmail = null,
        CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();

        var callId = Guid.NewGuid().ToString();

        var arguments = new Dictionary<string, object>
        {
            ["accountId"] = accountId,
            ["create"] = create
        };

        if (onSuccessUpdateEmail != null)
            arguments["onSuccessUpdateEmail"] = onSuccessUpdateEmail;

        var invocation = new Invocation
        {
            Name = "EmailSubmission/set",
            Arguments = arguments,
            MethodCallId = callId
        };

        var response = await SendRequestAsync(
            new[] { invocation },
            new[] { SubmissionCapability },
            cancellationToken).ConfigureAwait(false);

        var methodResponse = response.MethodResponses[0];

        if (!methodResponse.Arguments.TryGetValue("created", out var createdObj) ||
            createdObj is not JsonElement createdElement)
        {
            return new Dictionary<string, EmailSubmission>();
        }

        // Per RFC 8620 section 5.3, each "created" entry is PARTIAL: the server only
        // includes fields it assigned/defaulted (id, sendAt, undoStatus, ...), not the
        // full object - merge it onto the client-submitted object (server fields win)
        // before deserializing, since EmailSubmission's required properties (identityId,
        // emailId) would otherwise be left at their default(!) value.
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = new Dictionary<string, EmailSubmission>();
        foreach (var prop in createdElement.EnumerateObject())
        {
            var merged = create.TryGetValue(prop.Name, out var submitted)
                ? MergeJsonOverlay(JsonSerializer.SerializeToElement(submitted), prop.Value)
                : prop.Value;
            result[prop.Name] = JsonSerializer.Deserialize<EmailSubmission>(merged.GetRawText(), jsonOptions)
                ?? throw new JmapProtocolException("invalidResponse", $"Failed to deserialize created submission '{prop.Name}'.");
        }

        return result;
    }

    /// <summary>
    /// Cancels a previously created email submission by setting its <c>undoStatus</c> to <c>canceled</c>.
    /// The underlying JMAP method is <c>EmailSubmission/set</c>.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="submissionId">The identifier of the <see cref="EmailSubmission"/> to cancel.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A map containing the updated <see cref="EmailSubmission"/> objects (or <c>null</c> if the server does not return the object).
    /// </returns>
    /// <exception cref="JmapNetworkException">Thrown when a transport error occurs.</exception>
    /// <exception cref="JmapProtocolException">Thrown when the server returns a protocol error.</exception>
    public async Task<IReadOnlyDictionary<string, EmailSubmission?>> CancelSendAsync(
        string accountId,
        string submissionId,
        CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();

        var callId = Guid.NewGuid().ToString();

        // A PatchObject key is a JSON Pointer (RFC 6901) relative to the object being
        // patched - a bare top-level property name has no leading slash; "/undoStatus"
        // would instead point at a property literally named the empty string, which a
        // real JMAP server rejects/ignores.
        var updatePatch = new Dictionary<string, object>
        {
            ["undoStatus"] = "canceled"
        };

        var updateMap = new Dictionary<string, object>
        {
            [submissionId] = updatePatch
        };

        var arguments = new Dictionary<string, object>
        {
            ["accountId"] = accountId,
            ["update"] = updateMap
        };

        var invocation = new Invocation
        {
            Name = "EmailSubmission/set",
            Arguments = arguments,
            MethodCallId = callId
        };

        var response = await SendRequestAsync(
            new[] { invocation },
            new[] { SubmissionCapability },
            cancellationToken).ConfigureAwait(false);

        var methodResponse = response.MethodResponses[0];

        if (!methodResponse.Arguments.TryGetValue("updated", out var updatedObj) ||
            updatedObj is not JsonElement updatedElement)
        {
            return new Dictionary<string, EmailSubmission?>();
        }

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var updated = JsonSerializer.Deserialize<IReadOnlyDictionary<string, EmailSubmission?>>(updatedElement.GetRawText(), jsonOptions)
                     ?? new Dictionary<string, EmailSubmission?>();

        return updated;
    }

    /// <summary>
    /// Retrieves a list of <see cref="EmailSubmission"/> objects for the specified account.
    /// The underlying JMAP method is <c>EmailSubmission/get</c>.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read‑only list of <see cref="EmailSubmission"/> objects.</returns>
    /// <exception cref="JmapNetworkException">Thrown when a transport error occurs.</exception>
    /// <exception cref="JmapProtocolException">Thrown when the server returns a protocol error.</exception>
    public async Task<IReadOnlyList<EmailSubmission>> ListSubmissionsAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();

        var callId = Guid.NewGuid().ToString();

        var arguments = new Dictionary<string, object>
        {
            ["accountId"] = accountId
        };

        var invocation = new Invocation
        {
            Name = "EmailSubmission/get",
            Arguments = arguments,
            MethodCallId = callId
        };

        var response = await SendRequestAsync(
            new[] { invocation },
            new[] { SubmissionCapability },
            cancellationToken).ConfigureAwait(false);

        var methodResponse = response.MethodResponses[0];

        if (!methodResponse.Arguments.TryGetValue("list", out var listObj) ||
            listObj is not JsonElement listElement)
        {
            return Array.Empty<EmailSubmission>();
        }

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var list = JsonSerializer.Deserialize<IReadOnlyList<EmailSubmission>>(listElement.GetRawText(), jsonOptions)
                   ?? Array.Empty<EmailSubmission>();

        return list;
    }
}

/// <summary>
/// Adds a JSON converter that serializes and deserializes <see cref="Invocation"/> as a JMAP array tuple
/// <c>[name, arguments, methodCallId]</c>.
/// </summary>
[JsonConverter(typeof(InvocationArrayConverter))]
public partial class Invocation { }

internal sealed class InvocationArrayConverter : JsonConverter<Invocation>
{
    public override Invocation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of array for Invocation.");

        // name
        reader.Read();
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Expected string for Invocation name.");
        var name = reader.GetString()!;

        // arguments
        reader.Read();
        var argumentsElement = JsonElement.ParseValue(ref reader);
        var arguments = JsonSerializer.Deserialize<IReadOnlyDictionary<string, object>>(argumentsElement.GetRawText(), options)
                        ?? new Dictionary<string, object>();

        // methodCallId
        reader.Read();
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Expected string for Invocation methodCallId.");
        var methodCallId = reader.GetString()!;

        // end array
        reader.Read();
        if (reader.TokenType != JsonTokenType.EndArray)
            throw new JsonException("Expected end of array for Invocation.");

        return new Invocation
        {
            Name = name,
            Arguments = arguments,
            MethodCallId = methodCallId
        };
    }

    public override void Write(Utf8JsonWriter writer, Invocation value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteStringValue(value.Name);
        JsonSerializer.Serialize(writer, value.Arguments, options);
        writer.WriteStringValue(value.MethodCallId);
        writer.WriteEndArray();
    }
}
