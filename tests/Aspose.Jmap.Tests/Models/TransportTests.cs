using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Aspose.Jmap;
using Xunit;

namespace Aspose.Jmap.Tests;

/// <summary>
/// Unit tests for the transport layer types defined in <c>Models/Transport.cs</c>.
/// </summary>
public class TransportTests
{
    [Fact]
    public void JmapHttpRequest_Serialization_RoundTrip_PreservesData()
    {
        var request = new JmapHttpRequest
        {
            Method = "POST",
            Url = "https://example.com/jmap",
            Headers = new Dictionary<string, string>
            {
                ["Accept"] = "application/json",
                ["X-Custom"] = "value"
            },
            Body = Encoding.UTF8.GetBytes("{\"foo\":\"bar\"}")
        };

        string json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<JmapHttpRequest>(json)!;

        Assert.Equal(request.Method, deserialized.Method);
        Assert.Equal(request.Url, deserialized.Url);
        Assert.Equal(request.Body, deserialized.Body);
        Assert.NotNull(deserialized.Headers);
        Assert.Equal(request.Headers.Count, deserialized.Headers.Count);
        foreach (var kvp in request.Headers)
        {
            Assert.True(deserialized.Headers.ContainsKey(kvp.Key));
            Assert.Equal(kvp.Value, deserialized.Headers[kvp.Key]);
        }
    }

    [Fact]
    public void JmapHttpResponse_Serialization_RoundTrip_PreservesData()
    {
        var response = new JmapHttpResponse
        {
            StatusCode = 201,
            Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json",
                ["X-Server"] = "test"
            },
            Body = Encoding.UTF8.GetBytes("{\"result\":\"ok\"}")
        };

        string json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<JmapHttpResponse>(json)!;

        Assert.Equal(response.StatusCode, deserialized.StatusCode);
        Assert.Equal(response.Body, deserialized.Body);
        Assert.NotNull(deserialized.Headers);
        Assert.Equal(response.Headers.Count, deserialized.Headers.Count);
        foreach (var kvp in response.Headers)
        {
            Assert.True(deserialized.Headers.ContainsKey(kvp.Key));
            Assert.Equal(kvp.Value, deserialized.Headers[kvp.Key]);
        }
    }

    [Fact]
    public async Task HttpJmapTransport_SendAsync_SendsCorrectHttpRequest_AndParsesResponse()
    {
        // Arrange
        var captured = new CapturedRequest();
        var handler = new CapturingHandler(captured);
        var httpClient = new HttpClient(handler);
        using var transport = new HttpJmapTransport(httpClient);

        var request = new JmapHttpRequest
        {
            Method = "POST",
            Url = "https://api.example.com/jmap",
            Headers = new Dictionary<string, string>
            {
                ["Accept"] = "application/json",
                ["X-Test"] = "123"
            },
            Body = Encoding.UTF8.GetBytes("{\"hello\":\"world\"}")
        };

        // Act
        JmapHttpResponse response = await transport.SendAsync(request, CancellationToken.None);

        // Assert request
        Assert.Equal(HttpMethod.Post, captured.Method);
        Assert.Equal(new Uri(request.Url), captured.Uri);
        Assert.Contains("application/json", captured.ContentHeaders["Content-Type"]);
        Assert.Equal(request.Body, captured.Body);
        Assert.Equal("application/json", captured.Headers["Accept"]);
        Assert.Equal("123", captured.Headers["X-Test"]);

        // Assert response
        Assert.Equal(200, response.StatusCode);
        Assert.Equal(Encoding.UTF8.GetBytes("{\"ok\":true}"), response.Body);
        Assert.Contains("application/json", response.Headers["Content-Type"]);
    }

    [Fact]
    public async Task HttpJmapTransport_SendAsync_WithNullBody_SendsNoContent()
    {
        // Arrange
        var captured = new CapturedRequest();
        var handler = new CapturingHandler(captured);
        var httpClient = new HttpClient(handler);
        using var transport = new HttpJmapTransport(httpClient);

        var request = new JmapHttpRequest
        {
            Method = "GET",
            Url = "https://api.example.com/jmap",
            Headers = new Dictionary<string, string>
            {
                ["Accept"] = "application/json"
            },
            Body = null
        };

        // Act
        JmapHttpResponse response = await transport.SendAsync(request, CancellationToken.None);

        // Assert request
        Assert.Equal(HttpMethod.Get, captured.Method);
        Assert.Null(captured.Body);
        Assert.Empty(captured.ContentHeaders);

        // Assert response
        Assert.Equal(200, response.StatusCode);
        Assert.Equal(Encoding.UTF8.GetBytes("{\"ok\":true}"), response.Body);
    }

    [Fact]
    public async Task HttpJmapTransport_SendAsync_PreservesBinaryBodyAndRealContentType()
    {
        // Regression test: a prior version unconditionally built the outgoing content as
        // `new StringContent(request.Body, Encoding.UTF8, "application/json")`, which both
        // corrupted non-text bodies via a UTF-8 round trip AND silently overwrote the
        // caller's real Content-Type (e.g. "image/jpeg" for a blob upload) with a hardcoded
        // "application/json" - so a server would classify every uploaded blob as JSON
        // regardless of its actual type.
        var captured = new CapturedRequest();
        var handler = new CapturingHandler(captured);
        var httpClient = new HttpClient(handler);
        using var transport = new HttpJmapTransport(httpClient);

        byte[] binaryPayload = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };
        var request = new JmapHttpRequest
        {
            Method = "POST",
            Url = "https://api.example.com/upload",
            Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "image/jpeg"
            },
            Body = binaryPayload
        };

        await transport.SendAsync(request, CancellationToken.None);

        Assert.Equal(binaryPayload, captured.Body);
        Assert.Contains("image/jpeg", captured.ContentHeaders["Content-Type"]);
    }

    [Fact]
    public async Task HttpJmapTransport_SendAsync_DisposesFinalResponse()
    {
        // Regression test: intermediate redirect responses were disposed in the redirect
        // loop, but the FINAL HttpResponseMessage (the one actually used to build the
        // returned JmapHttpResponse) was never disposed - left to the GC/finalizer instead
        // of deterministic cleanup.
        var trackingStream = new DisposeTrackingStream(Encoding.UTF8.GetBytes("{\"ok\":true}"));
        var handler = new SingleResponseHandler(() => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(trackingStream),
        });
        var httpClient = new HttpClient(handler);
        using var transport = new HttpJmapTransport(httpClient);

        var request = new JmapHttpRequest { Method = "GET", Url = "https://api.example.com/jmap" };
        await transport.SendAsync(request, CancellationToken.None);

        Assert.True(trackingStream.Disposed, "the final response's content stream should be disposed");
    }

    private sealed class DisposeTrackingStream : System.IO.MemoryStream
    {
        public bool Disposed { get; private set; }

        public DisposeTrackingStream(byte[] buffer) : base(buffer) { }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class SingleResponseHandler : HttpMessageHandler
    {
        private readonly Func<HttpResponseMessage> _factory;

        public SingleResponseHandler(Func<HttpResponseMessage> factory) => _factory = factory;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_factory());
    }

    [Fact]
    public async Task HttpJmapTransport_Disposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var handler = new CapturingHandler(new CapturedRequest());
        var httpClient = new HttpClient(handler);
        var transport = new HttpJmapTransport(httpClient);
        transport.Dispose();

        var request = new JmapHttpRequest
        {
            Method = "GET",
            Url = "https://api.example.com/jmap"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            await transport.SendAsync(request, CancellationToken.None));
    }

    // Helper types for capturing HttpClient interactions
    private sealed class CapturedRequest
    {
        public HttpMethod? Method { get; set; }
        public Uri? Uri { get; set; }
        public Dictionary<string, string> Headers { get; } = new();
        public Dictionary<string, string> ContentHeaders { get; } = new();
        public byte[]? Body { get; set; }
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly CapturedRequest _captured;

        public CapturingHandler(CapturedRequest captured) => _captured = captured;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _captured.Method = request.Method;
            _captured.Uri = request.RequestUri;

            foreach (var header in request.Headers)
                _captured.Headers[header.Key] = string.Join(", ", header.Value);

            if (request.Content != null)
            {
                foreach (var header in request.Content.Headers)
                    _captured.ContentHeaders[header.Key] = string.Join(", ", header.Value);

                _captured.Body = request.Content.ReadAsByteArrayAsync(cancellationToken).Result;
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"ok\":true}", Encoding.UTF8, "application/json")
            };
            // Add content header correctly
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            return Task.FromResult(response);
        }
    }
}
