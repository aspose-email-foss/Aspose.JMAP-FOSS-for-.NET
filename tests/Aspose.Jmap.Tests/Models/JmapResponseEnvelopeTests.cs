using System.Collections.Generic;
using System.Text.Json;
using Aspose.Jmap;
using Xunit;

namespace Aspose.Jmap.Tests.Models;

/// <summary>
/// Tests for <see cref="JmapResponseEnvelope"/> serialization and deserialization.
/// </summary>
public sealed class JmapResponseEnvelopeTests
{
    private const string FullJson = """
{
  "methodResponses": [
    ["Mailbox/get", { "accountId": "user@example.test", "ids": ["mailbox1"] }, "c1"]
  ],
  "createdIds": {
    "clientId1": "serverIdA",
    "clientId2": "serverIdB"
  },
  "sessionState": "12345"
}
""";

    private const string WithoutCreatedIdsJson = """
{
  "methodResponses": [
    ["Identity/get", { "accountId": "user@example.test" }, "c2"]
  ],
  "sessionState": "abcde"
}
""";

    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Deserialize_FullJson_PopulatesAllProperties()
    {
        var envelope = JsonSerializer.Deserialize<JmapResponseEnvelope>(FullJson, _options)!;

        Assert.Equal("12345", envelope.SessionState);
        Assert.NotNull(envelope.CreatedIds);
        Assert.Equal(2, envelope.CreatedIds!.Count);
        Assert.Equal("serverIdA", envelope.CreatedIds["clientId1"]);
        Assert.Equal("serverIdB", envelope.CreatedIds["clientId2"]);

        Assert.NotNull(envelope.MethodResponses);
        Assert.Single(envelope.MethodResponses);
        var invocation = envelope.MethodResponses[0];
        Assert.Equal("Mailbox/get", invocation.Name);
        Assert.Equal("c1", invocation.MethodCallId);
        Assert.NotNull(invocation.Arguments);
        Assert.True(invocation.Arguments.ContainsKey("accountId"));
        var accountIdElement = Assert.IsType<JsonElement>(invocation.Arguments["accountId"]);
        Assert.Equal("user@example.test", accountIdElement.GetString());

        Assert.True(invocation.Arguments.ContainsKey("ids"));
        var idsElement = Assert.IsType<JsonElement>(invocation.Arguments["ids"]);
        // ids is an array; ensure it contains the expected value
        var ids = idsElement.EnumerateArray();
        Assert.Contains(ids, e => e.GetString() == "mailbox1");
    }

    [Fact]
    public void Deserialize_WithoutCreatedIds_SetsCreatedIdsToNull()
    {
        var envelope = JsonSerializer.Deserialize<JmapResponseEnvelope>(WithoutCreatedIdsJson, _options)!;

        Assert.Equal("abcde", envelope.SessionState);
        Assert.Null(envelope.CreatedIds);
        Assert.NotNull(envelope.MethodResponses);
        Assert.Single(envelope.MethodResponses);
        var invocation = envelope.MethodResponses[0];
        Assert.Equal("Identity/get", invocation.Name);
        Assert.Equal("c2", invocation.MethodCallId);
        Assert.NotNull(invocation.Arguments);
        var accountIdElement = Assert.IsType<JsonElement>(invocation.Arguments["accountId"]);
        Assert.Equal("user@example.test", accountIdElement.GetString());
    }

    [Fact]
    public void Serialize_AndDeserialize_RoundTripPreservesData()
    {
        var original = new JmapResponseEnvelope
        {
            SessionState = "roundtrip",
            CreatedIds = new Dictionary<string, string>
            {
                ["tempId"] = "realId"
            },
            MethodResponses = new List<Invocation>
            {
                new Invocation
                {
                    Name = "Email/query",
                    MethodCallId = "q1",
                    Arguments = new Dictionary<string, object>
                    {
                        ["accountId"] = "user@example.test",
                        ["filter"] = new Dictionary<string, object>
                        {
                            ["text"] = "hello"
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(original, _options);
        var deserialized = JsonSerializer.Deserialize<JmapResponseEnvelope>(json, _options)!;

        Assert.Equal(original.SessionState, deserialized.SessionState);
        Assert.NotNull(deserialized.CreatedIds);
        Assert.Equal(original.CreatedIds!.Count, deserialized.CreatedIds!.Count);
        Assert.Equal(original.CreatedIds["tempId"], deserialized.CreatedIds["tempId"]);

        Assert.NotNull(deserialized.MethodResponses);
        Assert.Single(deserialized.MethodResponses);
        var inv = deserialized.MethodResponses[0];
        Assert.Equal("Email/query", inv.Name);
        Assert.Equal("q1", inv.MethodCallId);
        Assert.True(inv.Arguments.ContainsKey("accountId"));
        var accountIdElem = Assert.IsType<JsonElement>(inv.Arguments["accountId"]);
        Assert.Equal("user@example.test", accountIdElem.GetString());

        Assert.True(inv.Arguments.ContainsKey("filter"));
        var filterElem = Assert.IsType<JsonElement>(inv.Arguments["filter"]);
        var textProp = filterElem.GetProperty("text");
        Assert.Equal("hello", textProp.GetString());
    }
}
