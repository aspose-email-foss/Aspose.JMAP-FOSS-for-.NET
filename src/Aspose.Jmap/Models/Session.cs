using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Aspose.Jmap;

/// <summary>
/// The JMAP Session resource. Fetched once (GET, no body) from a well‑known URL and cached;
/// describes server capabilities, accounts, and the URL templates used for every subsequent request.
/// </summary>
public sealed class Session
{
    /// <summary>
    /// Keyed by capability URN, e.g. "urn:ietf:params:jmap:core" → CoreCapability.
    /// </summary>
    [JsonPropertyName("capabilities")]
    public required IReadOnlyDictionary<string, object> Capabilities { get; init; }

    /// <summary>
    /// Map of account id to account information.
    /// </summary>
    [JsonPropertyName("accounts")]
    public required IReadOnlyDictionary<string, Account> Accounts { get; init; }

    /// <summary>
    /// Capability URN → the account id to use by default for that capability.
    /// </summary>
    [JsonPropertyName("primaryAccounts")]
    public required IReadOnlyDictionary<string, string> PrimaryAccounts { get; init; }

    /// <summary>
    /// The username of the authenticated user.
    /// </summary>
    [JsonPropertyName("username")]
    public required string Username { get; init; }

    /// <summary>
    /// Endpoint for POSTing JMAP method‑call requests.
    /// </summary>
    [JsonPropertyName("apiUrl")]
    public required string ApiUrl { get; init; }

    /// <summary>
    /// URL template with {accountId},{blobId},{type},{name} placeholders for downloading blobs.
    /// </summary>
    [JsonPropertyName("downloadUrl")]
    public required string DownloadUrl { get; init; }

    /// <summary>
    /// URL template with {accountId} placeholder; POST target for blob upload.
    /// </summary>
    [JsonPropertyName("uploadUrl")]
    public required string UploadUrl { get; init; }

    /// <summary>
    /// URL template for the push EventSource stream; client stores it but does not open a live stream.
    /// </summary>
    [JsonPropertyName("eventSourceUrl")]
    public required string EventSourceUrl { get; init; }

    /// <summary>
    /// Opaque string; changes whenever anything in the Session object changes.
    /// </summary>
    [JsonPropertyName("state")]
    public required string State { get; init; }
}

/// <summary>
/// Represents an individual account within a JMAP Session.
/// </summary>
public sealed class Account
{
    /// <summary>
    /// Human‑readable name of the account.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Indicates whether the account is personal.
    /// </summary>
    [JsonPropertyName("isPersonal")]
    public bool? IsPersonal { get; init; }

    /// <summary>
    /// Indicates whether the account is read‑only.
    /// </summary>
    [JsonPropertyName("isReadOnly")]
    public bool? IsReadOnly { get; init; }

    /// <summary>
    /// Capability URN → capability‑specific info for this account.
    /// </summary>
    [JsonPropertyName("accountCapabilities")]
    public IReadOnlyDictionary<string, object>? AccountCapabilities { get; init; }
}

/// <summary>
/// Core capability object, the value of capabilities["urn:ietf:params:jmap:core"].
/// </summary>
public sealed class CoreCapability
{
    /// <summary>
    /// Maximum size of a single upload, in bytes.
    /// </summary>
    [JsonPropertyName("maxSizeUpload")]
    public required uint MaxSizeUpload { get; init; }

    /// <summary>
    /// Maximum number of concurrent uploads allowed.
    /// </summary>
    [JsonPropertyName("maxConcurrentUpload")]
    public required uint MaxConcurrentUpload { get; init; }

    /// <summary>
    /// Maximum size of a single request, in bytes.
    /// </summary>
    [JsonPropertyName("maxSizeRequest")]
    public required uint MaxSizeRequest { get; init; }

    /// <summary>
    /// Maximum number of concurrent requests allowed.
    /// </summary>
    [JsonPropertyName("maxConcurrentRequests")]
    public required uint MaxConcurrentRequests { get; init; }

    /// <summary>
    /// Maximum number of method calls allowed in a single request.
    /// </summary>
    [JsonPropertyName("maxCallsInRequest")]
    public required uint MaxCallsInRequest { get; init; }

    /// <summary>
    /// Maximum number of objects that can be returned by a Get method.
    /// </summary>
    [JsonPropertyName("maxObjectsInGet")]
    public required uint MaxObjectsInGet { get; init; }

    /// <summary>
    /// Maximum number of objects that can be set by a Set method.
    /// </summary>
    [JsonPropertyName("maxObjectsInSet")]
    public required uint MaxObjectsInSet { get; init; }

    /// <summary>
    /// List of collation algorithm identifiers supported by the server.
    /// </summary>
    [JsonPropertyName("collationAlgorithms")]
    public required IReadOnlyList<string> CollationAlgorithms { get; init; }
}
