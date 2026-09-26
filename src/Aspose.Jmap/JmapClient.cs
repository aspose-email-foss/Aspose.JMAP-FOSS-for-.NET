#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Aspose.Jmap;

/// <summary>
/// Options used to configure a <see cref="JmapClient"/> instance.
/// </summary>
public sealed class JmapClientOptions
{
    /// <summary>
    /// The URL of the JMAP session resource (e.g. https://example.com/.well-known/jmap).
    /// </summary>
    public Uri SessionUrl { get; init; } = null!;

    /// <summary>
    /// Username for HTTP Basic authentication.
    /// </summary>
    public string Username { get; init; } = null!;

    /// <summary>
    /// Password for HTTP Basic authentication.
    /// </summary>
    public string Password { get; init; } = null!;

    /// <summary>
    /// Optional OAuth 2.0 bearer token (RFC 6750). When set to a non-empty value, it is sent as
    /// <c>Authorization: Bearer &lt;token&gt;</c> instead of HTTP Basic authentication built from
    /// <see cref="Username"/> and <see cref="Password"/>.
    /// </summary>
    public string? BearerToken { get; init; }

    /// <summary>
    /// Optional transport implementation. If <c>null</c>, <see cref="HttpJmapTransport"/> is used.
    /// </summary>
    public IJmapTransport? Transport { get; init; }
}

/// <summary>
/// Represents the response from the UploadBlob endpoint.
/// </summary>
public sealed class UploadResponse
{
    /// <summary>
    /// The account identifier associated with the uploaded blob.
    /// </summary>
    [JsonPropertyName("accountId")]
    public string AccountId { get; init; } = null!;

    /// <summary>
    /// The identifier of the uploaded blob.
    /// </summary>
    [JsonPropertyName("blobId")]
    public string BlobId { get; init; } = null!;

    /// <summary>
    /// The MIME type of the uploaded blob.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = null!;

    /// <summary>
    /// The size of the uploaded blob in bytes.
    /// </summary>
    [JsonPropertyName("size")]
    public uint Size { get; init; }
}

/// <summary>
/// High‑level client for JMAP core operations.
/// </summary>
public sealed partial class JmapClient : IDisposable, IAsyncDisposable
{
    private readonly JmapClientOptions _options;
    private readonly IJmapTransport _transport;
    private Session? _session;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="JmapClient"/> class.
    /// </summary>
    /// <param name="options">The client configuration options.</param>
    public JmapClient(JmapClientOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _transport = options.Transport ?? new HttpJmapTransport();
    }

    /// <summary>
    /// Gets the most recent <see cref="Session"/> obtained via <see cref="ConnectAsync"/>.
    /// </summary>
    public Session? Session => _session;

    /// <summary>
    /// Builds the value of the <c>Authorization</c> header for outgoing requests. Uses OAuth 2.0
    /// Bearer authentication (RFC 6750) when <see cref="JmapClientOptions.BearerToken"/> is set to a
    /// non-empty value; otherwise falls back to HTTP Basic authentication built from
    /// <see cref="JmapClientOptions.Username"/> and <see cref="JmapClientOptions.Password"/>.
    /// </summary>
    private string BuildAuthHeader()
    {
        if (!string.IsNullOrEmpty(_options.BearerToken))
            return $"Bearer {_options.BearerToken}";

        return $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.Username}:{_options.Password}"))}";
    }

    /// <summary>
    /// Retrieves the JMAP session resource and stores it for subsequent calls.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();

        var request = new JmapHttpRequest
        {
            Method = "GET",
            Url = _options.SessionUrl.ToString(),
            Headers = new Dictionary<string, string>
            {
                ["Authorization"] = BuildAuthHeader(),
            },
        };

        JmapHttpResponse httpResponse;
        try
        {
            httpResponse = await _transport.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not JmapException)
        {
            throw new JmapNetworkException("Network error while fetching JMAP session.", ex);
        }

        if (httpResponse.StatusCode != 200)
            throw new JmapNetworkException($"Unexpected HTTP status {httpResponse.StatusCode} while fetching session.");

        if (httpResponse.Body is null)
            throw new JmapNetworkException("Session response contained no body.");

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var rawSession = JsonSerializer.Deserialize<Session>(httpResponse.Body, jsonOptions)
                   ?? throw new JmapNetworkException("Failed to deserialize session object.");

        // Resolve possibly-relative URLs (RFC 8620 permits them; real servers, e.g.
        // Stalwart, commonly send relative apiUrl/uploadUrl/downloadUrl/eventSourceUrl)
        // against the session URL's origin. Done via plain string logic, not `new
        // Uri(base, relative)`, to avoid any risk of percent-encoding the literal "{"/"}"
        // characters in these URI-template placeholders (e.g. "/upload/{accountId}") -
        // the same class of corruption several other language targets in this codebase
        // hit with their respective built-in URL-resolution APIs.
        string ResolveUrl(string maybeRelative)
        {
            if (string.IsNullOrEmpty(maybeRelative)) return maybeRelative;
            if (maybeRelative.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                maybeRelative.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return maybeRelative;
            var origin = $"{_options.SessionUrl.Scheme}://{_options.SessionUrl.Authority}";
            return maybeRelative.StartsWith("/") ? origin + maybeRelative : origin + "/" + maybeRelative;
        }

        _session = new Session
        {
            Capabilities = rawSession.Capabilities,
            Accounts = rawSession.Accounts,
            PrimaryAccounts = rawSession.PrimaryAccounts,
            Username = rawSession.Username,
            ApiUrl = ResolveUrl(rawSession.ApiUrl),
            DownloadUrl = ResolveUrl(rawSession.DownloadUrl),
            UploadUrl = ResolveUrl(rawSession.UploadUrl),
            EventSourceUrl = ResolveUrl(rawSession.EventSourceUrl),
            State = rawSession.State,
        };
    }

    /// <summary>
    /// Sends a JMAP request envelope containing the supplied method calls.
    /// </summary>
    /// <param name="methodCalls">A list of method call invocations.</param>
    /// <param name="usingCapabilities">A list of capability URNs required for the request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The parsed <see cref="JmapResponseEnvelope"/>.</returns>
    /// <exception cref="JmapProtocolException">Thrown when the server returns an error invocation.</exception>
    public async Task<JmapResponseEnvelope> SendRequestAsync(
        IReadOnlyList<Invocation> methodCalls,
        IReadOnlyList<string> usingCapabilities,
        CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();

        if (_session is null)
            throw new InvalidOperationException("Client is not connected. Call ConnectAsync first.");

        var envelope = new JmapRequestEnvelope
        {
            Using = usingCapabilities,
            MethodCalls = methodCalls
        };

        // WhenWritingNull is required, not cosmetic: model classes (e.g. Mailbox.Id) declare
        // server-assigned properties as nullable with the INTENT that they be omitted from a
        // create payload, but nothing else enforces that - without this, a null property
        // still serializes as an explicit `"id": null`, and real JMAP servers (e.g. Stalwart)
        // reject that as an attempt to set an immutable property, not as "omitted".
        var jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        string requestBody = JsonSerializer.Serialize(envelope, jsonOptions);

        var request = new JmapHttpRequest
        {
            Method = "POST",
            Url = _session.ApiUrl,
            Headers = new Dictionary<string, string>
            {
                { "Authorization", BuildAuthHeader() }
                // Content-Type will be set by the transport (application/json).
            },
            Body = Encoding.UTF8.GetBytes(requestBody)
        };

        JmapHttpResponse httpResponse = await _transport.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (httpResponse.StatusCode != 200)
            throw new JmapNetworkException($"Unexpected HTTP status {httpResponse.StatusCode}.");

        if (httpResponse.Body is null)
            throw new JmapNetworkException("Response body was null.");

        // Parse response manually because Invocation is represented as an array in JSON.
        using var doc = JsonDocument.Parse(httpResponse.Body);
        var root = doc.RootElement;

        var sessionState = root.GetProperty("sessionState").GetString()!;

        var methodResponses = root.GetProperty("methodResponses")
            .EnumerateArray()
            .Select(ParseInvocation)
            .ToArray();

        // Detect protocol‑level error invocations.
        foreach (var resp in methodResponses)
        {
            if (resp.Name == "error")
            {
                if (resp.Arguments is IReadOnlyDictionary<string, object> dict)
                {
                    static string? AsString(object? value) => value switch
                    {
                        string s => s,
                        JsonElement je when je.ValueKind == JsonValueKind.String => je.GetString(),
                        _ => null,
                    };
                    string type = dict.TryGetValue("type", out var t) ? AsString(t) ?? "unknown" : "unknown";
                    string description = dict.TryGetValue("description", out var d) ? AsString(d) ?? string.Empty : string.Empty;
                    throw new JmapProtocolException(type, description);
                }

                throw new JmapProtocolException("unknown", "An error response was received without details.");
            }
        }

        return new JmapResponseEnvelope
        {
            SessionState = sessionState,
            MethodResponses = methodResponses
        };
    }

    private static Invocation ParseInvocation(JsonElement element)
    {
        // Expected format: ["MethodName", {args}, "callId"]
        var name = element[0].GetString()!;
        var argsJson = element[1].GetRawText();
        var arguments = JsonSerializer.Deserialize<IReadOnlyDictionary<string, object>>(argsJson)!;
        var callId = element[2].GetString()!;
        return new Invocation
        {
            Name = name,
            Arguments = arguments,
            MethodCallId = callId
        };
    }

    /// <summary>
    /// Calls the Core/echo method, which returns the supplied arguments unchanged.
    /// </summary>
    /// <param name="arguments">Arbitrary arguments to be echoed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The echoed arguments as a dictionary.</returns>
    public async Task<IReadOnlyDictionary<string, object>> EchoAsync(
        IReadOnlyDictionary<string, object> arguments,
        CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();

        var invocation = new Invocation
        {
            Name = "Core/echo",
            Arguments = arguments,
            MethodCallId = Guid.NewGuid().ToString()
        };

        var response = await SendRequestAsync(
            new[] { invocation },
            new[] { "urn:ietf:params:jmap:core" },
            cancellationToken).ConfigureAwait(false);

        return response.MethodResponses[0].Arguments;
    }

    /// <summary>
    /// Uploads a binary blob to the server.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="content">The raw bytes to upload.</param>
    /// <param name="contentType">The MIME type of the content.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The server's upload response.</returns>
    public async Task<UploadResponse> UploadBlobAsync(
        string accountId,
        byte[] content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();

        if (_session is null)
            throw new InvalidOperationException("Client is not connected. Call ConnectAsync first.");

        var uploadUrl = _session.UploadUrl.Replace("{accountId}", Uri.EscapeDataString(accountId));

        var request = new JmapHttpRequest
        {
            Method = "POST",
            Url = uploadUrl,
            Headers = new Dictionary<string, string>
            {
                { "Authorization", BuildAuthHeader() },
                { "Content-Type", contentType }
            },
            Body = content
        };

        var httpResponse = await _transport.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (httpResponse.StatusCode != 200 && httpResponse.StatusCode != 201)
            throw new JmapNetworkException($"Unexpected HTTP status {httpResponse.StatusCode} during blob upload.");

        if (httpResponse.Body is null)
            throw new JmapNetworkException("Upload response contained no body.");

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = JsonSerializer.Deserialize<UploadResponse>(httpResponse.Body, jsonOptions)
                     ?? throw new JmapNetworkException("Failed to deserialize upload response.");
        return result;
    }

    /// <summary>
    /// Downloads a previously uploaded blob.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="blobId">The identifier of the blob to download.</param>
    /// <param name="type">Optional MIME type placeholder for URL expansion.</param>
    /// <param name="name">Optional filename placeholder for URL expansion.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The raw bytes of the blob.</returns>
    public async Task<byte[]> DownloadBlobAsync(
        string accountId,
        string blobId,
        string? type = null,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        EnsureNotDisposed();

        if (_session is null)
            throw new InvalidOperationException("Client is not connected. Call ConnectAsync first.");

        var url = _session.DownloadUrl
            .Replace("{accountId}", Uri.EscapeDataString(accountId))
            .Replace("{blobId}", Uri.EscapeDataString(blobId));

        if (type != null)
            url = url.Replace("{type}", Uri.EscapeDataString(type));
        if (name != null)
            url = url.Replace("{name}", Uri.EscapeDataString(name));

        var request = new JmapHttpRequest
        {
            Method = "GET",
            Url = url,
            Headers = new Dictionary<string, string>
            {
                { "Authorization", BuildAuthHeader() }
            }
        };

        var httpResponse = await _transport.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (httpResponse.StatusCode != 200)
            throw new JmapNetworkException($"Unexpected HTTP status {httpResponse.StatusCode} during blob download.");

        if (httpResponse.Body is null)
            return Array.Empty<byte>();

        return httpResponse.Body;
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(JmapClient));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (_transport is IAsyncDisposable asyncDisp)
                await asyncDisp.DisposeAsync().ConfigureAwait(false);
            else if (_transport is IDisposable disp)
                disp.Dispose();

            _disposed = true;
        }
    }
}
