#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Aspose.Jmap;

namespace Aspose.Jmap.Tests;

/// <summary>
/// Simple fake transport that records sent requests and returns pre‑configured responses.
/// </summary>
internal sealed class FakeTransport : IJmapTransport
{
    private readonly Queue<JmapHttpResponse> _responses = new();
    public readonly List<JmapHttpRequest> SentRequests = new();

    public void EnqueueResponse(JmapHttpResponse response) => _responses.Enqueue(response);

    public Task<JmapHttpResponse> SendAsync(JmapHttpRequest request, CancellationToken cancellationToken = default)
    {
        SentRequests.Add(request);
        if (_responses.Count == 0)
            throw new InvalidOperationException("No response enqueued for FakeTransport.");
        return Task.FromResult(_responses.Dequeue());
    }
}

public sealed class JmapClientTests
{
    private static string BasicAuthHeader(string username, string password) =>
        $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"))}";

    private static JmapHttpResponse CreateSessionResponse()
    {
        // Include all required Session properties.
        var sessionJson = JsonSerializer.Serialize(new
        {
            apiUrl = "https://jmap.example.test/api",
            uploadUrl = "https://jmap.example.test/upload/{accountId}",
            downloadUrl = "https://jmap.example.test/download/{accountId}/{blobId}/{type}/{name}",
            capabilities = new { },
            accounts = new { },
            primaryAccounts = new { },
            username = "user@example.test",
            eventSourceUrl = "https://jmap.example.test/events",
            state = "state123"
        });

        return new JmapHttpResponse
        {
            StatusCode = 200,
            Headers = new Dictionary<string, string>(),
            Body = Encoding.UTF8.GetBytes(sessionJson)
        };
    }

    [Fact]
    public async Task ConnectAsync_FetchesSessionAndSetsProperties()
    {
        // Arrange
        var transport = new FakeTransport();
        transport.EnqueueResponse(CreateSessionResponse());

        var options = new JmapClientOptions
        {
            SessionUrl = new Uri("https://jmap.example.test/.well-known/jmap"),
            Username = "alice",
            Password = "secret",
            Transport = transport
        };
        await using var client = new JmapClient(options);

        // Act
        await client.ConnectAsync();

        // Assert
        Assert.NotNull(client.Session);
        Assert.Equal("https://jmap.example.test/api", client.Session!.ApiUrl);
        Assert.Equal("https://jmap.example.test/upload/{accountId}", client.Session.UploadUrl);
        Assert.Equal("https://jmap.example.test/download/{accountId}/{blobId}/{type}/{name}", client.Session.DownloadUrl);

        var request = transport.SentRequests[0];
        Assert.Equal("GET", request.Method);
        Assert.Equal(options.SessionUrl.ToString(), request.Url);
        // connect() must authenticate: real JMAP servers (e.g. Stalwart) 401 an
        // unauthenticated session fetch. With Username/Password set and no BearerToken,
        // that is HTTP Basic auth.
        Assert.True(request.Headers.TryGetValue("Authorization", out var auth));
        Assert.Equal(BasicAuthHeader(options.Username, options.Password), auth);
    }

    [Fact]
    public async Task ConnectAsync_WithBearerToken_UsesBearerAuthorizationHeader()
    {
        // Arrange
        var transport = new FakeTransport();
        transport.EnqueueResponse(CreateSessionResponse());

        var options = new JmapClientOptions
        {
            SessionUrl = new Uri("https://jmap.example.test/.well-known/jmap"),
            BearerToken = "my-oauth-token",
            Transport = transport
        };
        await using var client = new JmapClient(options);

        // Act
        await client.ConnectAsync();

        // Assert
        var request = transport.SentRequests[0];
        Assert.True(request.Headers.TryGetValue("Authorization", out var auth));
        Assert.Equal("Bearer my-oauth-token", auth);
    }

    [Fact]
    public async Task EchoAsync_WithBearerToken_SendsBearerAuthorizationHeaderInsteadOfBasic()
    {
        // Arrange
        var transport = new FakeTransport();
        transport.EnqueueResponse(CreateSessionResponse());

        var options = new JmapClientOptions
        {
            SessionUrl = new Uri("https://jmap.example.test/.well-known/jmap"),
            // Username/Password intentionally also set, to confirm BearerToken takes precedence
            // rather than being combined with or overridden by Basic auth.
            Username = "frank",
            Password = "pwd",
            BearerToken = "another-token",
            Transport = transport
        };
        await using var client = new JmapClient(options);
        await client.ConnectAsync();

        var echoArgs = new Dictionary<string, object> { ["hello"] = true };
        var responseEnvelope = new
        {
            sessionState = "s1",
            methodResponses = new[]
            {
                new object[] { "Core/echo", echoArgs, "c1" }
            }
        };
        transport.EnqueueResponse(new JmapHttpResponse
        {
            StatusCode = 200,
            Headers = new Dictionary<string, string>(),
            Body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(responseEnvelope))
        });

        // Act
        await client.EchoAsync(echoArgs);

        // Assert
        var request = transport.SentRequests[1];
        Assert.True(request.Headers.TryGetValue("Authorization", out var auth));
        Assert.Equal("Bearer another-token", auth);
        Assert.NotEqual(BasicAuthHeader(options.Username, options.Password), auth);
    }

    [Fact]
    public async Task EchoAsync_ReturnsEchoedArguments()
    {
        // Arrange
        var transport = new FakeTransport();
        transport.EnqueueResponse(CreateSessionResponse());

        var options = new JmapClientOptions
        {
            SessionUrl = new Uri("https://jmap.example.test/.well-known/jmap"),
            Username = "bob",
            Password = "pwd",
            Transport = transport
        };
        await using var client = new JmapClient(options);
        await client.ConnectAsync();

        var echoArgs = new Dictionary<string, object>
        {
            ["hello"] = true,
            ["high"] = 5
        };

        var responseEnvelope = new
        {
            sessionState = "s1",
            methodResponses = new[]
            {
                new object[] { "Core/echo", echoArgs, "c1" }
            }
        };
        var responseJson = JsonSerializer.Serialize(responseEnvelope);
        transport.EnqueueResponse(new JmapHttpResponse
        {
            StatusCode = 200,
            Headers = new Dictionary<string, string>(),
            Body = Encoding.UTF8.GetBytes(responseJson)
        });

        // Act
        var result = await client.EchoAsync(echoArgs);

        // Assert - result's values deserialize as JsonElement (System.Text.Json's default
        // behavior for Dictionary<string, object>), so compare via JSON representation
        // rather than object equality against the native-typed echoArgs.
        Assert.Equal(JsonSerializer.Serialize(echoArgs), JsonSerializer.Serialize(result));

        var request = transport.SentRequests[1];
        Assert.Equal("POST", request.Method);
        Assert.Equal(client.Session!.ApiUrl, request.Url);
        Assert.True(request.Headers.TryGetValue("Authorization", out var auth));
        Assert.Equal(BasicAuthHeader(options.Username, options.Password), auth);
        Assert.False(request.Headers.ContainsKey("Content-Type"));

        var sentEnvelope = JsonSerializer.Deserialize<JmapRequestEnvelope>(request.Body!);
        Assert.NotNull(sentEnvelope);
        Assert.Contains(sentEnvelope!.MethodCalls, mc => mc.Name == "Core/echo");
    }

    [Fact]
    public async Task EchoAsync_ProtocolError_ThrowsJmapProtocolException()
    {
        // Arrange
        var transport = new FakeTransport();
        transport.EnqueueResponse(CreateSessionResponse());

        var options = new JmapClientOptions
        {
            SessionUrl = new Uri("https://jmap.example.test/.well-known/jmap"),
            Username = "carol",
            Password = "pwd",
            Transport = transport
        };
        await using var client = new JmapClient(options);
        await client.ConnectAsync();

        var errorResponse = new
        {
            sessionState = "s1",
            methodResponses = new[]
            {
                new object[] { "error", new { type = "unknownMethod", description = "Method not found" }, "c1" }
            }
        };
        var errorJson = JsonSerializer.Serialize(errorResponse);
        transport.EnqueueResponse(new JmapHttpResponse
        {
            StatusCode = 200,
            Headers = new Dictionary<string, string>(),
            Body = Encoding.UTF8.GetBytes(errorJson)
        });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<JmapProtocolException>(async () =>
            await client.EchoAsync(new Dictionary<string, object>()));
        Assert.Equal("unknownMethod", ex.ErrorType);
        Assert.Equal("Method not found", ex.Description);
    }

    [Fact]
    public async Task UploadBlobAsync_UploadsAndParsesResponse()
    {
        // Arrange
        var transport = new FakeTransport();
        transport.EnqueueResponse(CreateSessionResponse());

        var options = new JmapClientOptions
        {
            SessionUrl = new Uri("https://jmap.example.test/.well-known/jmap"),
            Username = "dave",
            Password = "pwd",
            Transport = transport
        };
        await using var client = new JmapClient(options);
        await client.ConnectAsync();

        var uploadResponse = new
        {
            accountId = "A1",
            blobId = "B1",
            type = "text/plain",
            size = 123u
        };
        var uploadJson = JsonSerializer.Serialize(uploadResponse);
        transport.EnqueueResponse(new JmapHttpResponse
        {
            StatusCode = 201,
            Headers = new Dictionary<string, string>(),
            Body = Encoding.UTF8.GetBytes(uploadJson)
        });

        var content = Encoding.UTF8.GetBytes("Hello world");
        var contentType = "text/plain";

        // Act
        var result = await client.UploadBlobAsync("A1", content, contentType);

        // Assert
        Assert.Equal("A1", result.AccountId);
        Assert.Equal("B1", result.BlobId);
        Assert.Equal("text/plain", result.Type);
        Assert.Equal(123u, result.Size);

        var request = transport.SentRequests[1];
        Assert.Equal("POST", request.Method);
        var expectedUrl = client.Session!.UploadUrl.Replace("{accountId}", Uri.EscapeDataString("A1"));
        Assert.Equal(expectedUrl, request.Url);
        Assert.True(request.Headers.TryGetValue("Authorization", out var auth));
        Assert.Equal(BasicAuthHeader(options.Username, options.Password), auth);
        Assert.True(request.Headers.TryGetValue("Content-Type", out var ct));
        Assert.Equal(contentType, ct);
        Assert.Equal(content, request.Body);
    }

    [Fact]
    public async Task DownloadBlobAsync_ReturnsBytesAndHandlesNullBody()
    {
        // Arrange
        var transport = new FakeTransport();
        transport.EnqueueResponse(CreateSessionResponse());

        var options = new JmapClientOptions
        {
            SessionUrl = new Uri("https://jmap.example.test/.well-known/jmap"),
            Username = "eve",
            Password = "pwd",
            Transport = transport
        };
        await using var client = new JmapClient(options);
        await client.ConnectAsync();

        // First response: body present. Not valid UTF-8 (starts like a JPEG magic number) -
        // a prior version round-tripped this through Encoding.UTF8.GetString/GetBytes,
        // corrupting real (non-text) attachment content.
        byte[] binaryPayload = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };
        transport.EnqueueResponse(new JmapHttpResponse
        {
            StatusCode = 200,
            Headers = new Dictionary<string, string>(),
            Body = binaryPayload
        });

        // Act
        var bytes = await client.DownloadBlobAsync("A2", "B2", "text/plain", "file.txt");

        // Assert
        Assert.Equal(binaryPayload, bytes);
        var request = transport.SentRequests[1];
        var expectedUrl = client.Session!.DownloadUrl
            .Replace("{accountId}", Uri.EscapeDataString("A2"))
            .Replace("{blobId}", Uri.EscapeDataString("B2"))
            .Replace("{type}", Uri.EscapeDataString("text/plain"))
            .Replace("{name}", Uri.EscapeDataString("file.txt"));
        Assert.Equal("GET", request.Method);
        Assert.Equal(expectedUrl, request.Url);
        Assert.True(request.Headers.TryGetValue("Authorization", out var auth));
        Assert.Equal(BasicAuthHeader(options.Username, options.Password), auth);

        // Second response: null body -> empty array
        transport.EnqueueResponse(new JmapHttpResponse
        {
            StatusCode = 200,
            Headers = new Dictionary<string, string>(),
            Body = null
        });

        var empty = await client.DownloadBlobAsync("A2", "B2");
        Assert.Empty(empty);
    }
}
