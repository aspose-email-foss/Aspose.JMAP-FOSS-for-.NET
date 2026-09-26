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
/// Unit tests for the submission‑related methods on <see cref="JmapClient"/>.
/// </summary>
public class JmapClientSubmissionTests
{
    private const string DummySessionUrl = "https://example.com/.well-known/jmap";

    private static string CreateSessionJson()
    {
        // Minimal but complete session JSON required for deserialization.
        var session = new
        {
            state = "state123",
            apiUrl = "https://example.com/api",
            downloadUrl = "https://example.com/download/{accountId}/{blobId}/{type}/{name}",
            uploadUrl = "https://example.com/upload/{accountId}",
            eventSourceUrl = "https://example.com/event",
            username = "user@example.test",
            primaryAccounts = new Dictionary<string, string>
            {
                ["urn:ietf:params:jmap:mail"] = "u1"
            },
            accounts = new Dictionary<string, object>
            {
                ["u1"] = new { name = "User", isPersonal = true }
            },
            capabilities = new Dictionary<string, object>()
        };
        return JsonSerializer.Serialize(session);
    }

    private sealed class FakeTransport : IJmapTransport
    {
        private readonly Queue<JmapHttpResponse> _responses = new();
        public JmapHttpRequest? LastRequest { get; private set; }

        public FakeTransport(IEnumerable<JmapHttpResponse> responses)
        {
            foreach (var r in responses)
                _responses.Enqueue(r);
        }

        public Task<JmapHttpResponse> SendAsync(JmapHttpRequest request, CancellationToken ct)
        {
            LastRequest = request;
            if (_responses.Count == 0)
                throw new InvalidOperationException("No more fake responses configured.");
            return Task.FromResult(_responses.Dequeue());
        }
    }

    private static async Task<JmapClient> CreateConnectedClientAsync(params JmapHttpResponse[] responses)
    {
        var options = new JmapClientOptions
        {
            SessionUrl = new Uri(DummySessionUrl),
            Username = "user",
            Password = "pass",
            Transport = new FakeTransport(responses)
        };
        var client = new JmapClient(options);
        await client.ConnectAsync().ConfigureAwait(false);
        return client;
    }

    private static FakeTransport GetTransport(JmapClient client)
    {
        var field = typeof(JmapClient).GetField("_transport", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        return (FakeTransport)field.GetValue(client)!;
    }

    [Fact]
    public async Task SendAsync_ReturnsCreatedSubmissions_AndSendsCorrectInvocation()
    {
        // Arrange
        var sessionResponse = new JmapHttpResponse { StatusCode = 200, Body = Encoding.UTF8.GetBytes(CreateSessionJson()) };
        var methodResponseBody = @"{
            ""using"": [""urn:ietf:params:jmap:submission""],

            ""sessionState"": ""s1"",
            ""methodResponses"": [
                [""EmailSubmission/set"", {
                    ""accountId"": ""u1"",
                    ""newState"": ""1"",
                    ""created"": {
                        ""sub1"": { ""id"": ""s1"", ""identityId"": ""id1"", ""emailId"": ""e1"", ""sendAt"": ""2026-08-18T10:00:00Z"", ""undoStatus"": ""final"" }
                    }
                }, ""c2""]
            ]
        }";
        var methodResponse = new JmapHttpResponse { StatusCode = 200, Body = Encoding.UTF8.GetBytes(methodResponseBody) };
        var client = await CreateConnectedClientAsync(sessionResponse, methodResponse);
        var transport = GetTransport(client);

        var create = new Dictionary<string, EmailSubmission>
        {
            ["sub1"] = new EmailSubmission { IdentityId = "id1", EmailId = "#draft1" }
        };

        // Act
        var result = await client.SendAsync("u1", create);

        // Assert result
        Assert.Single(result);
        Assert.True(result.TryGetValue("sub1", out var submission));
        Assert.Equal("s1", submission.Id);
        Assert.Equal("final", submission.UndoStatus);

        // Assert request payload
        Assert.NotNull(transport.LastRequest);
        Assert.Equal("POST", transport.LastRequest.Method);
        var doc = JsonDocument.Parse(transport.LastRequest.Body!);
        var methodCalls = doc.RootElement.GetProperty("methodCalls");
        var firstCall = methodCalls[0];
        Assert.Equal("EmailSubmission/set", firstCall[0].GetString());
        var args = firstCall[1];
        Assert.Equal("u1", args.GetProperty("accountId").GetString());
        Assert.True(args.TryGetProperty("create", out var createProp));
        Assert.True(createProp.TryGetProperty("sub1", out var sub1Prop));
        Assert.Equal("id1", sub1Prop.GetProperty("identityId").GetString());
    }

    [Fact]
    public async Task CancelSendAsync_ReturnsUpdatedSubmission_AndSendsCorrectInvocation()
    {
        // Arrange
        var sessionResponse = new JmapHttpResponse { StatusCode = 200, Body = Encoding.UTF8.GetBytes(CreateSessionJson()) };
        var methodResponseBody = @"{
            ""using"": [""urn:ietf:params:jmap:submission""],

            ""sessionState"": ""s1"",
            ""methodResponses"": [
                [""EmailSubmission/set"", {
                    ""accountId"": ""u1"",
                    ""newState"": ""2"",
                    ""updated"": {
                        ""sub1"": { ""id"": ""s1"", ""identityId"": ""id1"", ""emailId"": ""e1"", ""undoStatus"": ""canceled"" }
                    }
                }, ""c3""]
            ]
        }";
        var methodResponse = new JmapHttpResponse { StatusCode = 200, Body = Encoding.UTF8.GetBytes(methodResponseBody) };
        var client = await CreateConnectedClientAsync(sessionResponse, methodResponse);
        var transport = GetTransport(client);

        // Act
        var result = await client.CancelSendAsync("u1", "sub1");

        // Assert result
        Assert.Single(result);
        Assert.True(result.TryGetValue("sub1", out var submission));
        Assert.NotNull(submission);
        Assert.Equal("canceled", submission!.UndoStatus);

        // Assert request payload
        Assert.NotNull(transport.LastRequest);
        var doc = JsonDocument.Parse(transport.LastRequest.Body!);
        var args = doc.RootElement.GetProperty("methodCalls")[0][1];
        Assert.True(args.TryGetProperty("update", out var updateProp));
        Assert.True(updateProp.TryGetProperty("sub1", out var patchProp));
        // Regression test: a PatchObject key is a JSON Pointer (RFC 6901) relative to the
        // patched object - a bare top-level property name has no leading slash. A prior
        // version sent "/undoStatus" (pointing at a differently-named property instead),
        // which a real JMAP server rejects/ignores, silently breaking cancel-send.
        Assert.Equal("canceled", patchProp.GetProperty("undoStatus").GetString());
        Assert.False(patchProp.TryGetProperty("/undoStatus", out _));
    }

    [Fact]
    public async Task ListSubmissionsAsync_ReturnsEmptyWhenListMissing()
    {
        // Arrange
        var sessionResponse = new JmapHttpResponse { StatusCode = 200, Body = Encoding.UTF8.GetBytes(CreateSessionJson()) };
        var methodResponseBody = @"{
            ""using"": [""urn:ietf:params:jmap:submission""],

            ""sessionState"": ""s1"",
            ""methodResponses"": [
                [""EmailSubmission/get"", {
                    ""accountId"": ""u1"",
                    ""state"": ""0""
                }, ""c4""]
            ]
        }";
        var methodResponse = new JmapHttpResponse { StatusCode = 200, Body = Encoding.UTF8.GetBytes(methodResponseBody) };
        var client = await CreateConnectedClientAsync(sessionResponse, methodResponse);

        // Act
        var list = await client.ListSubmissionsAsync("u1");

        // Assert
        Assert.Empty(list);
    }

    [Fact]
    public async Task ListSubmissionsAsync_ThrowsProtocolExceptionOnErrorResponse()
    {
        // Arrange
        var sessionResponse = new JmapHttpResponse { StatusCode = 200, Body = Encoding.UTF8.GetBytes(CreateSessionJson()) };
        var errorResponseBody = @"{
            ""using"": [""urn:ietf:params:jmap:submission""],

            ""sessionState"": ""s1"",
            ""methodResponses"": [
                [""error"", { ""type"": ""unknownMethod"", ""description"": ""Method not found"" }, ""c5""]
            ]
        }";
        var errorResponse = new JmapHttpResponse { StatusCode = 200, Body = Encoding.UTF8.GetBytes(errorResponseBody) };
        var client = await CreateConnectedClientAsync(sessionResponse, errorResponse);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<JmapProtocolException>(async () =>
            await client.ListSubmissionsAsync("u1"));
        Assert.Equal("unknownMethod", ex.ErrorType);
        Assert.Equal("Method not found", ex.Description);
    }
}
