using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using Aspose.Jmap;

namespace Aspose.Jmap.Tests.Models;

/// <summary>
/// Tests for the <see cref="Invocation"/> model covering JSON (de)serialization and edge cases.
/// </summary>
public class InvocationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = false
    };

    [Fact]
    public void RoundtripSerialization_ShouldPreserveAllProperties()
    {
        // Arrange
        var original = new Invocation
        {
            Name = "Email/get",
            Arguments = new Dictionary<string, object>
            {
                ["accountId"] = "user@example.test",
                ["ids"] = new List<string> { "id1", "id2" }
            },
            MethodCallId = "a1"
        };

        // Act
        string json = JsonSerializer.Serialize(original, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<Invocation>(json, JsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Name, deserialized!.Name);
        Assert.Equal(original.MethodCallId, deserialized.MethodCallId);
        Assert.NotNull(deserialized.Arguments);
        Assert.Equal(original.Arguments.Count, deserialized.Arguments.Count);

        // Compare the arguments by re‑serializing them; this works even when values become JsonElement.
        string originalArgsJson = JsonSerializer.Serialize(original.Arguments, JsonOptions);
        string deserializedArgsJson = JsonSerializer.Serialize(deserialized.Arguments, JsonOptions);
        Assert.Equal(originalArgsJson, deserializedArgsJson);
    }

    [Fact]
    public void EmptyArguments_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        var original = new Invocation
        {
            Name = "Mailbox/get",
            Arguments = new Dictionary<string, object>(),
            MethodCallId = "b2"
        };

        // Act
        string json = JsonSerializer.Serialize(original, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<Invocation>(json, JsonOptions);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Name, deserialized!.Name);
        Assert.Equal(original.MethodCallId, deserialized.MethodCallId);
        Assert.NotNull(deserialized.Arguments);
        Assert.Empty(deserialized.Arguments);
    }
}
