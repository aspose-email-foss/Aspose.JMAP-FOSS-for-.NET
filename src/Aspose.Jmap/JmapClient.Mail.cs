#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Aspose.Jmap;

/// <summary>
/// Extension methods for <see cref="JmapClient"/> that implement the JMAP mail module API.
/// </summary>
public static class JmapClientMailExtensions
{
    private const string MailCapability = "urn:ietf:params:jmap:mail";

    #region Mailbox/Get

    /// <summary>
    /// Retrieves a list of mailboxes matching the supplied criteria.
    /// </summary>
    /// <param name="client">The JMAP client instance.</param>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="ids">Optional list of mailbox ids to retrieve.</param>
    /// <param name="properties">Optional list of mailbox properties to return.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The list of matching <see cref="Mailbox"/> objects.</returns>
    public static async Task<IReadOnlyList<Mailbox>> ListMailboxesAsync(
        this JmapClient client,
        string accountId,
        IReadOnlyList<string>? ids = null,
        IReadOnlyList<string>? properties = null,
        CancellationToken cancellationToken = default)
    {
        var callId = Guid.NewGuid().ToString();
        var arguments = new Dictionary<string, object> { ["accountId"] = accountId };
        if (ids is not null) arguments["ids"] = ids;
        if (properties is not null) arguments["properties"] = properties;

        var invocation = new Invocation
        {
            Name = "Mailbox/get",
            Arguments = arguments,
            MethodCallId = callId
        };

        var envelope = await client.SendRequestAsync(
                new[] { invocation },
                new[] { MailCapability },
                cancellationToken)
            .ConfigureAwait(false);

        var response = envelope.MethodResponses[0];
        var dto = JsonSerializer.Deserialize<MailboxGetResponse>(
            JsonSerializer.Serialize(response.Arguments))
            ?? throw new JmapProtocolException("invalidResponse", "Failed to deserialize Mailbox/get response.");

        return dto.List;
    }

    /// <summary>
    /// Retrieves a single mailbox by its identifier.
    /// </summary>
    /// <param name="client">The JMAP client instance.</param>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="id">The mailbox identifier.</param>
    /// <param name="properties">Optional list of mailbox properties to return.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The requested <see cref="Mailbox"/> or <c>null</c> if not found.</returns>
    public static async Task<Mailbox?> GetMailboxAsync(
        this JmapClient client,
        string accountId,
        string id,
        IReadOnlyList<string>? properties = null,
        CancellationToken cancellationToken = default)
    {
        var list = await client.ListMailboxesAsync(accountId, new[] { id }, properties, cancellationToken)
            .ConfigureAwait(false);
        return list.Count > 0 ? list[0] : null;
    }

    #endregion

    #region Mailbox/Set

    /// <summary>
    /// Creates new mailboxes.
    /// </summary>
    /// <param name="client">The JMAP client instance.</param>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="create">A map of client‑chosen ids to <see cref="Mailbox"/> objects to create.</param>
    /// <param name="ifInState">Optional state token for conditional creation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The full <see cref="MailboxSetResponse"/> from the server.</returns>
    public static async Task<MailboxSetResponse> CreateMailboxAsync(
        this JmapClient client,
        string accountId,
        IReadOnlyDictionary<string, Mailbox> create,
        string? ifInState = null,
        CancellationToken cancellationToken = default)
    {
        var callId = Guid.NewGuid().ToString();
        var arguments = new Dictionary<string, object>
        {
            ["accountId"] = accountId,
            ["create"] = create
        };
        if (ifInState is not null) arguments["ifInState"] = ifInState;

        var invocation = new Invocation
        {
            Name = "Mailbox/set",
            Arguments = arguments,
            MethodCallId = callId
        };

        var envelope = await client.SendRequestAsync(
                new[] { invocation },
                new[] { MailCapability },
                cancellationToken)
            .ConfigureAwait(false);

        var response = envelope.MethodResponses[0];
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(response.Arguments));
        var root = doc.RootElement;

        Dictionary<string, Mailbox>? createdOut = null;
        if (root.TryGetProperty("created", out var createdEl) && createdEl.ValueKind == JsonValueKind.Object)
        {
            createdOut = new Dictionary<string, Mailbox>();
            foreach (var prop in createdEl.EnumerateObject())
            {
                // Per RFC 8620 section 5.3, a "created" entry is PARTIAL: the server only
                // includes fields it assigned/defaulted (typically just the new id), not
                // the full object - merge it onto the client-submitted object (server
                // fields win) before deserializing, since Mailbox's required properties
                // (e.g. Name) would otherwise be left at their default(!) value.
                var merged = create.TryGetValue(prop.Name, out var submitted)
                    ? MergeJsonOverlay(JsonSerializer.SerializeToElement(submitted), prop.Value)
                    : prop.Value;
                createdOut[prop.Name] = JsonSerializer.Deserialize<Mailbox>(merged.GetRawText())
                    ?? throw new JmapProtocolException("invalidResponse", $"Failed to deserialize created mailbox '{prop.Name}'.");
            }
        }

        return new MailboxSetResponse
        {
            AccountId = root.GetProperty("accountId").GetString()!,
            OldState = root.TryGetProperty("oldState", out var os) && os.ValueKind != JsonValueKind.Null ? os.GetString() : null,
            NewState = root.GetProperty("newState").GetString()!,
            Created = createdOut,
            Updated = root.TryGetProperty("updated", out var upd)
                ? JsonSerializer.Deserialize<IReadOnlyDictionary<string, Mailbox?>>(upd.GetRawText())
                : null,
            Destroyed = root.TryGetProperty("destroyed", out var des)
                ? JsonSerializer.Deserialize<IReadOnlyList<string>>(des.GetRawText())
                : null,
            NotCreated = root.TryGetProperty("notCreated", out var nc)
                ? JsonSerializer.Deserialize<IReadOnlyDictionary<string, SetError>>(nc.GetRawText())
                : null,
            NotUpdated = root.TryGetProperty("notUpdated", out var nu)
                ? JsonSerializer.Deserialize<IReadOnlyDictionary<string, SetError>>(nu.GetRawText())
                : null,
            NotDestroyed = root.TryGetProperty("notDestroyed", out var nd)
                ? JsonSerializer.Deserialize<IReadOnlyDictionary<string, SetError>>(nd.GetRawText())
                : null,
        };
    }

    /// <summary>
    /// Merges two JSON object elements: every property of <paramref name="overlay"/> is
    /// added to (or replaces the same-named property of) <paramref name="baseObj"/>.
    /// Used to reconstruct a full object from a base plus a partial "set" response entry.
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
    /// Deletes the specified mailboxes.
    /// </summary>
    /// <param name="client">The JMAP client instance.</param>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="destroy">A list of mailbox ids to delete.</param>
    /// <param name="onDestroyRemoveEmails">If true, also removes emails contained in the destroyed mailboxes.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The list of mailbox ids that were successfully destroyed.</returns>
    public static async Task<IReadOnlyList<string>> DeleteMailboxAsync(
        this JmapClient client,
        string accountId,
        IReadOnlyList<string> destroy,
        bool onDestroyRemoveEmails = false,
        CancellationToken cancellationToken = default)
    {
        var callId = Guid.NewGuid().ToString();
        var arguments = new Dictionary<string, object>
        {
            ["accountId"] = accountId,
            ["destroy"] = destroy,
            ["onDestroyRemoveEmails"] = onDestroyRemoveEmails
        };

        var invocation = new Invocation
        {
            Name = "Mailbox/set",
            Arguments = arguments,
            MethodCallId = callId
        };

        var envelope = await client.SendRequestAsync(
                new[] { invocation },
                new[] { MailCapability },
                cancellationToken)
            .ConfigureAwait(false);

        var response = envelope.MethodResponses[0];
        var dto = JsonSerializer.Deserialize<MailboxSetResponse>(
                JsonSerializer.Serialize(response.Arguments))
            ?? throw new JmapProtocolException("invalidResponse", "Failed to deserialize Mailbox/set response.");

        return dto.Destroyed ?? Array.Empty<string>();
    }

    #endregion

    #region Email/Query (ListMessages)

    /// <summary>
    /// Retrieves message ids matching the supplied filter and sort criteria.
    /// </summary>
    /// <param name="client">The JMAP client instance.</param>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="filter">Optional filter object (serialized as JSON).</param>
    /// <param name="sort">Optional list of <see cref="Comparator"/> objects.</param>
    /// <param name="position">Optional start position (default 0).</param>
    /// <param name="anchor">Optional anchor id.</param>
    /// <param name="anchorOffset">Optional offset from the anchor (default 0).</param>
    /// <param name="limit">Optional maximum number of ids to return.</param>
    /// <param name="calculateTotal">If true, the server calculates the total number of matches.</param>
    /// <param name="collapseThreads">If true, collapse results by thread.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The <see cref="EmailQueryResponse"/> containing matching ids and pagination info.</returns>
    public static async Task<EmailQueryResponse> ListMessagesAsync(
        this JmapClient client,
        string accountId,
        JsonElement? filter = null,
        IReadOnlyList<Comparator>? sort = null,
        int? position = null,
        string? anchor = null,
        int? anchorOffset = null,
        uint? limit = null,
        bool? calculateTotal = null,
        bool? collapseThreads = null,
        CancellationToken cancellationToken = default)
    {
        var callId = Guid.NewGuid().ToString();
        var arguments = new Dictionary<string, object> { ["accountId"] = accountId };
        if (filter.HasValue) arguments["filter"] = filter.Value;
        if (sort is not null) arguments["sort"] = sort;
        if (position.HasValue) arguments["position"] = position.Value;
        if (anchor is not null) arguments["anchor"] = anchor;
        if (anchorOffset.HasValue) arguments["anchorOffset"] = anchorOffset.Value;
        if (limit.HasValue) arguments["limit"] = limit.Value;
        if (calculateTotal.HasValue) arguments["calculateTotal"] = calculateTotal.Value;
        if (collapseThreads.HasValue) arguments["collapseThreads"] = collapseThreads.Value;

        var invocation = new Invocation
        {
            Name = "Email/query",
            Arguments = arguments,
            MethodCallId = callId
        };

        var envelope = await client.SendRequestAsync(
                new[] { invocation },
                new[] { MailCapability },
                cancellationToken)
            .ConfigureAwait(false);

        var response = envelope.MethodResponses[0];
        return JsonSerializer.Deserialize<EmailQueryResponse>(
                JsonSerializer.Serialize(response.Arguments))
            ?? throw new JmapProtocolException("invalidResponse", "Failed to deserialize Email/query response.");
    }

    #endregion

    #region Email/Get (FetchMessage)

    /// <summary>
    /// Retrieves full email objects for the specified ids.
    /// </summary>
    /// <param name="client">The JMAP client instance.</param>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="ids">The list of email ids to fetch.</param>
    /// <param name="properties">Optional list of email properties to return.</param>
    /// <param name="bodyProperties">Optional list of body properties to return.</param>
    /// <param name="fetchTextBodyValues">If true, include plain‑text body values.</param>
    /// <param name="fetchHTMLBodyValues">If true, include HTML body values.</param>
    /// <param name="fetchAllBodyValues">If true, include all body values.</param>
    /// <param name="maxBodyValueBytes">Optional maximum number of bytes per body value.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The <see cref="EmailGetResponse"/> containing the requested emails.</returns>
    public static async Task<EmailGetResponse> FetchMessageAsync(
        this JmapClient client,
        string accountId,
        IReadOnlyList<string> ids,
        IReadOnlyList<string>? properties = null,
        IReadOnlyList<string>? bodyProperties = null,
        bool fetchTextBodyValues = false,
        bool fetchHTMLBodyValues = false,
        bool fetchAllBodyValues = false,
        uint? maxBodyValueBytes = null,
        CancellationToken cancellationToken = default)
    {
        var callId = Guid.NewGuid().ToString();
        var arguments = new Dictionary<string, object>
        {
            ["accountId"] = accountId,
            ["ids"] = ids
        };
        if (properties is not null) arguments["properties"] = properties;
        if (bodyProperties is not null) arguments["bodyProperties"] = bodyProperties;
        if (fetchTextBodyValues) arguments["fetchTextBodyValues"] = true;
        if (fetchHTMLBodyValues) arguments["fetchHTMLBodyValues"] = true;
        if (fetchAllBodyValues) arguments["fetchAllBodyValues"] = true;
        if (maxBodyValueBytes.HasValue) arguments["maxBodyValueBytes"] = maxBodyValueBytes.Value;

        var invocation = new Invocation
        {
            Name = "Email/get",
            Arguments = arguments,
            MethodCallId = callId
        };

        var envelope = await client.SendRequestAsync(
                new[] { invocation },
                new[] { MailCapability },
                cancellationToken)
            .ConfigureAwait(false);

        var response = envelope.MethodResponses[0];
        return JsonSerializer.Deserialize<EmailGetResponse>(
                JsonSerializer.Serialize(response.Arguments))
            ?? throw new JmapProtocolException("invalidResponse", "Failed to deserialize Email/get response.");
    }

    #endregion

    #region Email/Set (MoveMessage, SetMessageKeyword, DeleteMessage)

    /// <summary>
    /// Updates messages (e.g., move between mailboxes) using the Email/set method.
    /// </summary>
    /// <param name="client">The JMAP client instance.</param>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="update">A map of message ids to patch objects describing the updates.</param>
    /// <param name="ifInState">Optional state token for conditional updates.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The full <see cref="EmailSetResponse"/> from the server.</returns>
    public static async Task<EmailSetResponse> MoveMessageAsync(
        this JmapClient client,
        string accountId,
        IReadOnlyDictionary<string, object> update,
        string? ifInState = null,
        CancellationToken cancellationToken = default)
    {
        var callId = Guid.NewGuid().ToString();
        var arguments = new Dictionary<string, object>
        {
            ["accountId"] = accountId,
            ["update"] = update
        };
        if (ifInState is not null) arguments["ifInState"] = ifInState;

        var invocation = new Invocation
        {
            Name = "Email/set",
            Arguments = arguments,
            MethodCallId = callId
        };

        var envelope = await client.SendRequestAsync(
                new[] { invocation },
                new[] { MailCapability },
                cancellationToken)
            .ConfigureAwait(false);

        var response = envelope.MethodResponses[0];
        return JsonSerializer.Deserialize<EmailSetResponse>(
                JsonSerializer.Serialize(response.Arguments))
            ?? throw new JmapProtocolException("invalidResponse", "Failed to deserialize Email/set response.");
    }

    /// <summary>
    /// Sets keywords on messages using the Email/set method.
    /// </summary>
    /// <param name="client">The JMAP client instance.</param>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="update">A map of message ids to keyword patch objects.</param>
    /// <param name="ifInState">Optional state token for conditional updates.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The full <see cref="EmailSetResponse"/> from the server.</returns>
    public static async Task<EmailSetResponse> SetMessageKeywordAsync(
        this JmapClient client,
        string accountId,
        IReadOnlyDictionary<string, object> update,
        string? ifInState = null,
        CancellationToken cancellationToken = default)
    {
        // Re‑use the same implementation as MoveMessageAsync – the caller supplies the appropriate patch.
        return await client.MoveMessageAsync(accountId, update, ifInState, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes the specified messages.
    /// </summary>
    /// <param name="client">The JMAP client instance.</param>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="destroy">A list of message ids to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The list of message ids that were successfully destroyed.</returns>
    public static async Task<IReadOnlyList<string>> DeleteMessageAsync(
        this JmapClient client,
        string accountId,
        IReadOnlyList<string> destroy,
        CancellationToken cancellationToken = default)
    {
        var callId = Guid.NewGuid().ToString();
        var arguments = new Dictionary<string, object>
        {
            ["accountId"] = accountId,
            ["destroy"] = destroy
        };

        var invocation = new Invocation
        {
            Name = "Email/set",
            Arguments = arguments,
            MethodCallId = callId
        };

        var envelope = await client.SendRequestAsync(
                new[] { invocation },
                new[] { MailCapability },
                cancellationToken)
            .ConfigureAwait(false);

        var response = envelope.MethodResponses[0];
        var dto = JsonSerializer.Deserialize<EmailSetResponse>(
                JsonSerializer.Serialize(response.Arguments))
            ?? throw new JmapProtocolException("invalidResponse", "Failed to deserialize Email/set response.");

        return dto.Destroyed ?? Array.Empty<string>();
    }

    #endregion

    #region Identity/Get

    /// <summary>
    /// Retrieves a list of identities for the given account.
    /// </summary>
    /// <param name="client">The JMAP client instance.</param>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="ids">Optional list of identity ids to retrieve.</param>
    /// <param name="properties">Optional list of identity properties to return.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The list of matching <see cref="Identity"/> objects.</returns>
    public static async Task<IReadOnlyList<Identity>> ListIdentitiesAsync(
        this JmapClient client,
        string accountId,
        IReadOnlyList<string>? ids = null,
        IReadOnlyList<string>? properties = null,
        CancellationToken cancellationToken = default)
    {
        var callId = Guid.NewGuid().ToString();
        var arguments = new Dictionary<string, object> { ["accountId"] = accountId };
        if (ids is not null) arguments["ids"] = ids;
        if (properties is not null) arguments["properties"] = properties;

        var invocation = new Invocation
        {
            Name = "Identity/get",
            Arguments = arguments,
            MethodCallId = callId
        };

        var envelope = await client.SendRequestAsync(
                new[] { invocation },
                new[] { MailCapability },
                cancellationToken)
            .ConfigureAwait(false);

        var response = envelope.MethodResponses[0];
        var dto = JsonSerializer.Deserialize<IdentityGetResponse>(
                JsonSerializer.Serialize(response.Arguments))
            ?? throw new JmapProtocolException("invalidResponse", "Failed to deserialize Identity/get response.");

        return dto.List;
    }

    #endregion

    #region Response DTOs

    public sealed class MailboxGetResponse
    {
        [JsonPropertyName("accountId")]
        public string AccountId { get; init; } = default!;

        [JsonPropertyName("state")]
        public string State { get; init; } = default!;

        [JsonPropertyName("list")]
        public IReadOnlyList<Mailbox> List { get; init; } = Array.Empty<Mailbox>();

        [JsonPropertyName("notFound")]
        public IReadOnlyList<string> NotFound { get; init; } = Array.Empty<string>();
    }

    public sealed class MailboxSetResponse
    {
        [JsonPropertyName("accountId")]
        public string AccountId { get; init; } = default!;

        [JsonPropertyName("oldState")]
        public string? OldState { get; init; }

        [JsonPropertyName("newState")]
        public string NewState { get; init; } = default!;

        [JsonPropertyName("created")]
        public IReadOnlyDictionary<string, Mailbox>? Created { get; init; }

        [JsonPropertyName("updated")]
        public IReadOnlyDictionary<string, Mailbox?>? Updated { get; init; }

        [JsonPropertyName("destroyed")]
        public IReadOnlyList<string>? Destroyed { get; init; }

        [JsonPropertyName("notCreated")]
        public IReadOnlyDictionary<string, SetError>? NotCreated { get; init; }

        [JsonPropertyName("notUpdated")]
        public IReadOnlyDictionary<string, SetError>? NotUpdated { get; init; }

        [JsonPropertyName("notDestroyed")]
        public IReadOnlyDictionary<string, SetError>? NotDestroyed { get; init; }
    }

    public sealed class EmailQueryResponse
    {
        [JsonPropertyName("accountId")]
        public string AccountId { get; init; } = default!;

        [JsonPropertyName("queryState")]
        public string QueryState { get; init; } = default!;

        [JsonPropertyName("canCalculateChanges")]
        public bool CanCalculateChanges { get; init; }

        [JsonPropertyName("position")]
        public uint Position { get; init; }

        [JsonPropertyName("ids")]
        public IReadOnlyList<string> Ids { get; init; } = Array.Empty<string>();

        [JsonPropertyName("total")]
        public uint? Total { get; init; }

        [JsonPropertyName("limit")]
        public uint? Limit { get; init; }
    }

    public sealed class EmailGetResponse
    {
        [JsonPropertyName("accountId")]
        public string AccountId { get; init; } = default!;

        [JsonPropertyName("state")]
        public string State { get; init; } = default!;

        [JsonPropertyName("list")]
        public IReadOnlyList<Email> List { get; init; } = Array.Empty<Email>();

        [JsonPropertyName("notFound")]
        public IReadOnlyList<string> NotFound { get; init; } = Array.Empty<string>();
    }

    public sealed class EmailSetResponse
    {
        [JsonPropertyName("accountId")]
        public string AccountId { get; init; } = default!;

        [JsonPropertyName("oldState")]
        public string? OldState { get; init; }

        [JsonPropertyName("newState")]
        public string NewState { get; init; } = default!;

        [JsonPropertyName("created")]
        public IReadOnlyDictionary<string, Email>? Created { get; init; }

        [JsonPropertyName("updated")]
        public IReadOnlyDictionary<string, Email?>? Updated { get; init; }

        [JsonPropertyName("destroyed")]
        public IReadOnlyList<string>? Destroyed { get; init; }

        [JsonPropertyName("notCreated")]
        public IReadOnlyDictionary<string, SetError>? NotCreated { get; init; }

        [JsonPropertyName("notUpdated")]
        public IReadOnlyDictionary<string, SetError>? NotUpdated { get; init; }

        [JsonPropertyName("notDestroyed")]
        public IReadOnlyDictionary<string, SetError>? NotDestroyed { get; init; }
    }

    public sealed class IdentityGetResponse
    {
        [JsonPropertyName("accountId")]
        public string AccountId { get; init; } = default!;

        [JsonPropertyName("state")]
        public string State { get; init; } = default!;

        [JsonPropertyName("list")]
        public IReadOnlyList<Identity> List { get; init; } = Array.Empty<Identity>();

        [JsonPropertyName("notFound")]
        public IReadOnlyList<string> NotFound { get; init; } = Array.Empty<string>();
    }

    #endregion
}
