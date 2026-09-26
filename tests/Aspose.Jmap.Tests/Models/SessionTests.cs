using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Aspose.Jmap;

namespace Aspose.Jmap.Tests.Models;

public sealed class SessionTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [Fact]
    public void Session_SerializationDeserialization_RoundtripPreservesData()
    {
        // Arrange
        var original = new Session
        {
            Capabilities = new Dictionary<string, object>(),
            Accounts = new Dictionary<string, Account>
            {
                {
                    "account1",
                    new Account
                    {
                        Name = null,
                        IsPersonal = null,
                        IsReadOnly = null,
                        AccountCapabilities = null
                    }
                }
            },
            PrimaryAccounts = new Dictionary<string, string>
            {
                { "urn:ietf:params:jmap:mail", "account1" }
            },
            Username = "user@example.test",
            ApiUrl = "https://example.test/jmap/api",
            DownloadUrl = "https://example.test/jmap/download/{accountId}/{blobId}/{type}/{name}",
            UploadUrl = "https://example.test/jmap/upload/{accountId}",
            EventSourceUrl = "https://example.test/jmap/eventsource/{accountId}",
            State = "state123"
        };

        // Act
        string json = JsonSerializer.Serialize(original, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<Session>(json, JsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Username, deserialized!.Username);
        Assert.Equal(original.ApiUrl, deserialized.ApiUrl);
        Assert.Equal(original.DownloadUrl, deserialized.DownloadUrl);
        Assert.Equal(original.UploadUrl, deserialized.UploadUrl);
        Assert.Equal(original.EventSourceUrl, deserialized.EventSourceUrl);
        Assert.Equal(original.State, deserialized.State);

        Assert.Equal(original.PrimaryAccounts.Count, deserialized.PrimaryAccounts.Count);
        foreach (var kvp in original.PrimaryAccounts)
        {
            Assert.True(deserialized.PrimaryAccounts.ContainsKey(kvp.Key));
            Assert.Equal(kvp.Value, deserialized.PrimaryAccounts[kvp.Key]);
        }

        Assert.Equal(original.Accounts.Count, deserialized.Accounts.Count);
        foreach (var kvp in original.Accounts)
        {
            Assert.True(deserialized.Accounts.ContainsKey(kvp.Key));
            var originalAcc = kvp.Value;
            var deserializedAcc = deserialized.Accounts[kvp.Key];

            Assert.Equal(originalAcc.Name, deserializedAcc.Name);
            Assert.Equal(originalAcc.IsPersonal, deserializedAcc.IsPersonal);
            Assert.Equal(originalAcc.IsReadOnly, deserializedAcc.IsReadOnly);
            Assert.Equal(originalAcc.AccountCapabilities, deserializedAcc.AccountCapabilities);
        }

        Assert.Empty(deserialized.Capabilities);
    }

    [Fact]
    public void Session_Deserialization_WithMissingOptionalFields_ShouldSucceed()
    {
        // Arrange: JSON includes only required fields; optional Account fields are omitted.
        const string json = """
        {
            "capabilities": {},
            "accounts": {
                "account1": {}
            },
            "primaryAccounts": {
                "urn:ietf:params:jmap:mail": "account1"
            },
            "username": "user@example.test",
            "apiUrl": "https://example.test/jmap/api",
            "downloadUrl": "https://example.test/jmap/download/{accountId}/{blobId}/{type}/{name}",
            "uploadUrl": "https://example.test/jmap/upload/{accountId}",
            "eventSourceUrl": "https://example.test/jmap/eventsource/{accountId}",
            "state": "state123"
        }
        """;

        // Act
        var session = JsonSerializer.Deserialize<Session>(json, JsonOptions);

        // Assert
        Assert.NotNull(session);
        Assert.Equal("user@example.test", session!.Username);
        Assert.Single(session.Accounts);
        var account = session.Accounts["account1"];
        Assert.Null(account.Name);
        Assert.Null(account.IsPersonal);
        Assert.Null(account.IsReadOnly);
        Assert.Null(account.AccountCapabilities);
    }
}
