#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Aspose.Jmap;
using Xunit;

namespace Aspose.Jmap.Tests;

/// <summary>
/// Unit tests for the mail module extension methods on <see cref="JmapClient"/>.
/// </summary>
public class JmapClientMailTests
{
    private const string SessionUrl = "https://example.com/.well-known/jmap";

    // A complete session object containing all required properties for deserialization.
    private static readonly string SessionJson = JsonSerializer.Serialize(new
    {
        state = "testState",
        apiUrl = "https://example.com/api",
        downloadUrl = "https://example.com/download/{accountId}/{blobId}/{type}/{name}",
        uploadUrl = "https://example.com/upload/{accountId}",
        eventSourceUrl = "https://example.com/event",
        username = "user",
        primaryAccounts = new Dictionary<string, string>
        {
            ["urn:ietf:params:jmap:mail"] = "u1"
        },
        accounts = new Dictionary<string, object>
        {
            ["u1"] = new
            {
                name = "User",
                isPersonal = true,
                isReadOnly = false,
                accountId = "u1",
                capabilities = new Dictionary<string, object>
                {
                    ["urn:ietf:params:jmap:mail"] = new { }
                }
            }
        },
        capabilities = new Dictionary<string, object>
        {
            ["urn:ietf:params:jmap:core"] = new { },
            ["urn:ietf:params:jmap:mail"] = new { }
        }
    });

    private static readonly string MailboxGetResponseJson = JsonSerializer.Serialize(new
    {
        @using = new[] { "urn:ietf:params:jmap:mail" },
        sessionState = "s1",
        methodResponses = new object[]
        {
            new object[]
            {
                "Mailbox/get",
                new
                {
                    accountId = "u1",
                    state = "1",
                    list = new[]
                    {
                        new
                        {
                            id = "mb1",
                            name = "Inbox",
                            parentId = (string?)null,
                            role = "inbox",
                            sortOrder = 0,
                            totalEmails = 3,
                            unreadEmails = 1,
                            totalThreads = 3,
                            unreadThreads = 1,
                            myRights = new
                            {
                                mayReadItems = true,
                                mayAddItems = true,
                                mayRemoveItems = true,
                                maySetSeen = true,
                                maySetKeywords = true,
                                mayCreateChild = true,
                                mayRename = false,
                                mayDelete = false,
                                maySubmit = true
                            },
                            isSubscribed = true
                        }
                    },
                    notFound = Array.Empty<string>()
                },
                "c1"
            }
        }
    });

    private static readonly string MailboxGetEmptyResponseJson = JsonSerializer.Serialize(new
    {
        @using = new[] { "urn:ietf:params:jmap:mail" },
        sessionState = "s1",
        methodResponses = new object[]
        {
            new object[]
            {
                "Mailbox/get",
                new
                {
                    accountId = "u1",
                    state = "1",
                    list = Array.Empty<object>(),
                    notFound = Array.Empty<string>()
                },
                "c2"
            }
        }
    });

    private static readonly string MailboxSetErrorResponseJson = JsonSerializer.Serialize(new
    {
        @using = new[] { "urn:ietf:params:jmap:mail" },
        sessionState = "s1",
        methodResponses = new object[]
        {
            new object[]
            {
                "Mailbox/set",
                new
                {
                    accountId = "u1",
                    oldState = (string?)null,
                    newState = "2",
                    notCreated = new Dictionary<string, object>
                    {
                        ["c1"] = new
                        {
                            type = "invalidProperties",
                            description = "Bad properties"
                        }
                    }
                },
                "c3"
            }
        }
    });

    private static readonly string ProtocolErrorResponseJson = JsonSerializer.Serialize(new
    {
        @using = new[] { "urn:ietf:params:jmap:mail" },
        sessionState = "s1",
        methodResponses = new object[]
        {
            new object[]
            {
                "error",
                new
                {
                    type = "unknownMethod",
                    description = "Method not found"
                },
                "c4"
            }
        }
    });

    private static readonly string EmailSetResponseJson = JsonSerializer.Serialize(new
    {
        @using = new[] { "urn:ietf:params:jmap:mail" },
        sessionState = "s1",
        methodResponses = new object[]
        {
            new object[]
            {
                "Email/set",
                new
                {
                    accountId = "u1",
                    oldState = (string?)null,
                    newState = "1",
                    created = (object?)null,
                    updated = (object?)null,
                    destroyed = Array.Empty<string>()
                },
                "c5"
            }
        }
    });

    /// <summary>
    /// A fake transport that records requests and returns canned responses based on request content.
    /// </summary>
    private sealed class FakeTransport : IJmapTransport
    {
        public readonly List<JmapHttpRequest> Requests = new();

        private readonly Func<JmapHttpRequest, JmapHttpResponse> _responseFactory;

        public FakeTransport(Func<JmapHttpRequest, JmapHttpResponse> responseFactory) => _responseFactory = responseFactory;

        public Task<JmapHttpResponse> SendAsync(JmapHttpRequest request, CancellationToken ct)
        {
            Requests.Add(request);
            return Task.FromResult(_responseFactory(request));
        }
    }

    private static JmapClient CreateConnectedClient(FakeTransport transport)
    {
        var options = new JmapClientOptions
        {
            SessionUrl = new Uri(SessionUrl),
            Username = "user",
            Password = "pass",
            Transport = transport
        };
        var client = new JmapClient(options);
        client.ConnectAsync().GetAwaiter().GetResult(); // sync for test setup
        return client;
    }

    [Fact]
    public async Task ListMailboxesAsync_ReturnsMailboxes_AndSendsCorrectRequest()
    {
        var transport = new FakeTransport(req =>
            req.Method == "GET"
                ? new JmapHttpResponse { StatusCode = 200, Body = Encoding.UTF8.GetBytes(SessionJson) }
                : new JmapHttpResponse { StatusCode = 200, Body = Encoding.UTF8.GetBytes(MailboxGetResponseJson) });

        var client = CreateConnectedClient(transport);

        var result = await client.ListMailboxesAsync("u1");

        Assert.Single(result);
        Assert.Equal("mb1", result[0].Id);
        Assert.Equal("Inbox", result[0].Name);

        // first request is session GET, second is the POST for Mailbox/get
        Assert.Equal(2, transport.Requests.Count);
        var postRequest = transport.Requests[1];
        Assert.Equal("POST", postRequest.Method);
        Assert.Contains("\"Mailbox/get\"", Encoding.UTF8.GetString(postRequest.Body ?? Array.Empty<byte>()));
        Assert.Contains("\"accountId\":\"u1\"", Encoding.UTF8.GetString(postRequest.Body ?? Array.Empty<byte>()));
    }

    [Fact]
    public async Task GetMailboxAsync_ReturnsMailboxOrNull()
    {
        var transport = new FakeTransport(req =>
            req.Method == "GET"
                ? new JmapHttpResponse { StatusCode = 200, Body = Encoding.UTF8.GetBytes(SessionJson) }
                : new JmapHttpResponse { StatusCode = 200, Body = Encoding.UTF8.GetBytes(MailboxGetEmptyResponseJson) });

        var client = CreateConnectedClient(transport);

        var mailbox = await client.GetMailboxAsync("u1", "missing");

        Assert.Null(mailbox);
        Assert.Equal(2, transport.Requests.Count);
        var postRequest = transport.Requests[1];
        Assert.Contains("\"Mailbox/get\"", Encoding.UTF8.GetString(postRequest.Body ?? Array.Empty<byte>()));
        Assert.Contains("\"ids\":[\"missing\"]", Encoding.UTF8.GetString(postRequest.Body ?? Array.Empty<byte>()));
    }

    [Fact]
    public async Task CreateMailboxAsync_ReturnsResponseWithNotCreated()
    {
        var transport = new FakeTransport(req =>
            req.Method == "GET"
                ? new JmapHttpResponse { StatusCode = 200, Body = Encoding.UTF8.GetBytes(SessionJson) }
                : new JmapHttpResponse { StatusCode = 200, Body = Encoding.UTF8.GetBytes(MailboxSetErrorResponseJson) });

        var client = CreateConnectedClient(transport);

        var createMap = new Dictionary<string, Mailbox>
        {
            ["c1"] = new Mailbox { Name = "NewBox" }
        };

        var response = await client.CreateMailboxAsync("u1", createMap);

        Assert.NotNull(response.NotCreated);
        Assert.Contains("c1", response.NotCreated.Keys);
        var err = response.NotCreated["c1"];
        Assert.Equal("invalidProperties", err.Type);
        Assert.Equal("Bad properties", err.Description);

        Assert.Equal(2, transport.Requests.Count);
        var postRequest = transport.Requests[1];
        Assert.Contains("\"Mailbox/set\"", Encoding.UTF8.GetString(postRequest.Body ?? Array.Empty<byte>()));
        Assert.Contains("\"create\":{", Encoding.UTF8.GetString(postRequest.Body ?? Array.Empty<byte>()));
    }

    [Fact]
    public async Task ListMailboxesAsync_ProtocolError_ThrowsJmapProtocolException()
    {
        var transport = new FakeTransport(req =>
            req.Method == "GET"
                ? new JmapHttpResponse { StatusCode = 200, Body = Encoding.UTF8.GetBytes(SessionJson) }
                : new JmapHttpResponse { StatusCode = 200, Body = Encoding.UTF8.GetBytes(ProtocolErrorResponseJson) });

        var client = CreateConnectedClient(transport);

        var ex = await Assert.ThrowsAsync<JmapProtocolException>(async () =>
            await client.ListMailboxesAsync("u1"));

        Assert.Equal("unknownMethod", ex.ErrorType);
        Assert.Equal("Method not found", ex.Description);
        Assert.Equal(2, transport.Requests.Count);
    }

    [Fact]
    public async Task SetMessageKeywordAsync_DelegatesToMoveMessageAsync()
    {
        var transport = new FakeTransport(req =>
            req.Method == "GET"
                ? new JmapHttpResponse { StatusCode = 200, Body = Encoding.UTF8.GetBytes(SessionJson) }
                : new JmapHttpResponse { StatusCode = 200, Body = Encoding.UTF8.GetBytes(EmailSetResponseJson) });

        var client = CreateConnectedClient(transport);

        var update = new Dictionary<string, object>
        {
            ["e1"] = new { keywords = new Dictionary<string, bool> { ["$seen"] = true } }
        };

        var response = await client.SetMessageKeywordAsync("u1", update);

        // Verify the request shape; response content is not the focus of this test.
        Assert.Equal(2, transport.Requests.Count);
        var postRequest = transport.Requests[1];
        Assert.Contains("\"Email/set\"", Encoding.UTF8.GetString(postRequest.Body ?? Array.Empty<byte>()));
        Assert.Contains("\"update\":{", Encoding.UTF8.GetString(postRequest.Body ?? Array.Empty<byte>()));
        Assert.Contains("\"e1\"", Encoding.UTF8.GetString(postRequest.Body ?? Array.Empty<byte>()));
    }
}
