using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aspose.Jmap;

/// <summary>
/// Represents a low‑level HTTP request used by the JMAP client transport.
/// </summary>
public sealed class JmapHttpRequest
{
    /// <summary>
    /// Gets the HTTP method (e.g., "POST").
    /// </summary>
    public string Method { get; init; } = default!;

    /// <summary>
    /// Gets the request URL as a string.
    /// </summary>
    public string Url { get; init; } = default!;

    /// <summary>
    /// Gets the request headers. The dictionary is case‑insensitive.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the optional raw request body bytes. May be <c>null</c> for requests without a
    /// payload. Deliberately <c>byte[]</c>, not <c>string</c>: a string body would force
    /// every caller - including blob uploads, which are not text - through a lossy UTF-8
    /// round trip. JMAP method-call bodies are UTF-8-encoded JSON text; blob uploads are the
    /// blob's raw bytes verbatim.
    /// </summary>
    public byte[]? Body { get; init; }
}

/// <summary>
/// Represents a low‑level HTTP response returned by the JMAP client transport.
/// </summary>
public sealed class JmapHttpResponse
{
    /// <summary>
    /// Gets the HTTP status code (e.g., 200).
    /// </summary>
    public int StatusCode { get; init; }

    /// <summary>
    /// Gets the response headers. The dictionary is case‑insensitive.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the optional raw response body bytes. May be <c>null</c> if the response has no
    /// content. Typically JSON text, but a blob download's body is arbitrary binary data -
    /// see the note on <see cref="JmapHttpRequest.Body"/>.
    /// </summary>
    public byte[]? Body { get; init; }
}

/// <summary>
/// Defines the contract for a transport that can send <see cref="JmapHttpRequest"/> instances
/// and receive <see cref="JmapHttpResponse"/> instances.
/// </summary>
public interface IJmapTransport
{
    /// <summary>
    /// Sends the specified request asynchronously.
    /// </summary>
    /// <param name="request">The JMAP HTTP request to send.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that resolves to the HTTP response.</returns>
    /// <exception cref="JmapNetworkException">Thrown when a low‑level network error occurs.</exception>
    Task<JmapHttpResponse> SendAsync(JmapHttpRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default transport implementation that uses <see cref="System.Net.Http.HttpClient"/> to
/// perform HTTP communication with a JMAP server.
/// </summary>
public sealed class HttpJmapTransport : IJmapTransport, IDisposable, IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpJmapTransport"/> class.
    /// </summary>
    /// <param name="httpClient">
    /// An optional <see cref="HttpClient"/> instance. If <c>null</c>, a new instance is created.
    /// </param>
    public HttpJmapTransport(HttpClient? httpClient = null)
    {
        // AllowAutoRedirect is disabled deliberately: .NET's HttpClient strips the
        // Authorization header on ANY automatic redirect (a hardcoded security
        // precaution, even for same-host redirects), and real JMAP servers commonly
        // 307-redirect the well-known discovery URL to the actual session endpoint
        // (e.g. Stalwart: "/.well-known/jmap" -> "/jmap/session") - losing auth there
        // silently returns an unauthenticated/empty session instead of failing loudly.
        // SendAsync below follows redirects itself, preserving every original header on a
        // same-origin hop but dropping the credential headers (Authorization / Cookie /
        // Proxy-Authorization) once a redirect crosses to a different origin, so a hostile
        // redirect cannot leak them to another host.
        _httpClient = httpClient ?? new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
    }

    /// <inheritdoc />
    public async Task<JmapHttpResponse> SendAsync(JmapHttpRequest request, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HttpJmapTransport));

        const int maxRedirects = 5;
        var currentUrl = request.Url;
        var originalOrigin = new Uri(request.Url).GetLeftPart(UriPartial.Authority);
        // Once a redirect crosses to a different origin, the caller's credentials must not
        // be forwarded to it.
        var stripCredentials = false;
        HttpResponseMessage httpResponse;

        for (var attempt = 0; ; attempt++)
        {
            var httpMethod = new HttpMethod(request.Method);
            var httpRequest = new HttpRequestMessage(httpMethod, currentUrl);

            // Content-Type is applied to the body content object below (not to
            // httpRequest.Headers, which rejects content-specific headers) - captured here
            // so the CALLER's actual content type (e.g. "message/rfc822" for a blob upload)
            // is preserved instead of being silently overwritten with a hardcoded default.
            string? contentType = null;
            foreach (var header in request.Headers)
            {
                if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    contentType = header.Value;
                    continue;
                }
                if (header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (stripCredentials && (
                        header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ||
                        header.Key.Equals("Cookie", StringComparison.OrdinalIgnoreCase) ||
                        header.Key.Equals("Proxy-Authorization", StringComparison.OrdinalIgnoreCase)))
                    continue;

                if (!httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value))
                {
                    httpRequest.Content ??= new ByteArrayContent(Array.Empty<byte>());
                    httpRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (request.Body is not null)
            {
                var content = new ByteArrayContent(request.Body);
                content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(
                    contentType ?? "application/json");
                httpRequest.Content = content;
            }

            try
            {
                httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException ex)
            {
                throw new JmapNetworkException("Network error while sending JMAP request.", ex);
            }

            var isRedirect = httpResponse.StatusCode is System.Net.HttpStatusCode.MovedPermanently
                or System.Net.HttpStatusCode.Found
                or System.Net.HttpStatusCode.SeeOther
                or System.Net.HttpStatusCode.TemporaryRedirect
                or System.Net.HttpStatusCode.PermanentRedirect;
            if (!isRedirect || httpResponse.Headers.Location is null || attempt >= maxRedirects)
                break;

            var nextUri = httpResponse.Headers.Location.IsAbsoluteUri
                ? httpResponse.Headers.Location
                : new Uri(new Uri(currentUrl), httpResponse.Headers.Location);
            if (!string.Equals(nextUri.GetLeftPart(UriPartial.Authority), originalOrigin, StringComparison.Ordinal))
                stripCredentials = true;
            currentUrl = nextUri.ToString();
            httpResponse.Dispose();
        }

        // Intermediate redirect responses are disposed in the loop above; the final response
        // used here must be disposed too, rather than left to the GC/finalizer, once its
        // headers/body have been fully read into the plain JmapHttpResponse below.
        using (httpResponse)
        {
            var responseHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in httpResponse.Headers)
                responseHeaders[header.Key] = string.Join(", ", header.Value);

            if (httpResponse.Content != null)
            {
                foreach (var header in httpResponse.Content.Headers)
                    responseHeaders[header.Key] = string.Join(", ", header.Value);
            }

            byte[]? responseBody = null;
            if (httpResponse.Content != null)
                responseBody = await httpResponse.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);

            return new JmapHttpResponse
            {
                StatusCode = (int)httpResponse.StatusCode,
                Headers = responseHeaders,
                Body = responseBody
            };
        }
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="HttpJmapTransport"/> and optionally disposes of the managed resources.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient.Dispose();
            _disposed = true;
        }
    }

    /// <summary>
    /// Asynchronously releases the unmanaged resources used by the <see cref="HttpJmapTransport"/>.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
