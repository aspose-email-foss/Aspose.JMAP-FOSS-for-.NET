using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Aspose.Jmap;

namespace Aspose.Jmap.Tests.Models;

/// <summary>
/// Tests for <see cref="JmapRequestEnvelope"/> serialization and deserialization.
/// </summary>
public class JmapRequestEnvelopeTests
{
    private static readonly JsonSerializerOptions _options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void RoundTrip_SerializationDeserialization_WithAllFields()
    {
        // Arrange
        var envelope = new JmapRequestEnvelope
        {
            Using = new List<string> { "urn:ietf:params:jmap:core" },
            MethodCalls = new List<Invocation>
            {
                new Invocation
                {
                    Name = "Email/get",
                    Arguments = new Dictionary<string, object> { { "accountId", "a1" } },
                    MethodCallId = "c1"
                }
            },
            CreatedIds = new Dictionary<string, string> { { "client1", "server1" } }
        };

        // Act
        var json = JsonSerializer.Serialize(envelope, _options);
        var deserialized = JsonSerializer.Deserialize<JmapRequestEnvelope>(json, _options)!;

        // Assert
        Assert.Equal(envelope.Using, deserialized.Using);
        Assert.Equal(envelope.MethodCalls.Count, deserialized.MethodCalls.Count);
        Assert.Equal(envelope.MethodCalls[0].Name, deserialized.MethodCalls[0].Name);
        Assert.Equal(envelope.MethodCalls[0].MethodCallId, deserialized.MethodCalls[0].MethodCallId);

        // The deserialized Arguments values are JsonElement when the target type is object.
        var originalArg = envelope.MethodCalls[0].Arguments["accountId"] as string;
        var deserializedArg = deserialized.MethodCalls[0].Arguments["accountId"];
        var deserializedValue = deserializedArg is JsonElement je ? je.GetString()! : deserializedArg?.ToString()!;
        Assert.Equal(originalArg, deserializedValue);

        Assert.NotNull(deserialized.CreatedIds);
        Assert.Equal(envelope.CreatedIds!["client1"], deserialized.CreatedIds!["client1"]);
    }

    [Fact]
    public void Deserialization_WithoutCreatedIds_SetsCreatedIdsToNull()
    {
        // Arrange
        const string json = """
        {
            "using": ["urn:ietf:params:jmap:core"],
            "methodCalls": [
                ["Email/get", { "accountId": "a1" }, "c1"]
            ]
        }
        """;

        // Act
        var envelope = JsonSerializer.Deserialize<JmapRequestEnvelope>(json, _options)!;

        // Assert
        Assert.NotNull(envelope.Using);
        Assert.Single(envelope.Using);
        Assert.Equal("urn:ietf:params:jmap:core", envelope.Using[0]);

        Assert.NotNull(envelope.MethodCalls);
        Assert.Single(envelope.MethodCalls);
        Assert.Equal("Email/get", envelope.MethodCalls[0].Name);
        Assert.Equal("c1", envelope.MethodCalls[0].MethodCallId);
        var arg = envelope.MethodCalls[0].Arguments["accountId"];
        var argValue = arg is JsonElement je ? je.GetString()! : arg?.ToString()!;
        Assert.Equal("a1", argValue);

        Assert.Null(envelope.CreatedIds);
    }

    [Fact]
    public void Serialization_OmitsCreatedIdsWhenNull()
    {
        // Arrange
        var envelope = new JmapRequestEnvelope
        {
            Using = new List<string> { "urn:ietf:params:jmap:core" },
            MethodCalls = new List<Invocation>
            {
                new Invocation
                {
                    Name = "Email/get",
                    Arguments = new Dictionary<string, object> { { "accountId", "a1" } },
                    MethodCallId = "c1"
                }
            },
            CreatedIds = null
        };

        // Act
        var json = JsonSerializer.Serialize(envelope, _options);

        // Assert
        Assert.DoesNotContain("\"createdIds\"", json);
        var deserialized = JsonSerializer.Deserialize<JmapRequestEnvelope>(json, _options)!;
        Assert.Null(deserialized.CreatedIds);
    }
}
