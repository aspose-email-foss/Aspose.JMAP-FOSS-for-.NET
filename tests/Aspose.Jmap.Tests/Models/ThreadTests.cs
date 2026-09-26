using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Aspose.Jmap;

namespace Aspose.Jmap.Tests.Models;

/// <summary>
/// Tests for the <see cref="Thread"/> model's JSON (de)serialization behavior.
/// </summary>
public class ThreadTests
{
    /// <summary>
    /// Verifies that a fully populated <see cref="Thread"/> instance serializes to the expected JSON structure.
    /// </summary>
    [Fact]
    public void SerializeThread_ShouldProduceExpectedJson()
    {
        // Arrange
        var thread = new Thread
        {
            Id = "thread123",
            EmailIds = new List<string> { "email1", "email2" }
        };

        // Act
        string json = JsonSerializer.Serialize(thread);
        using var doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        // Assert
        Assert.True(root.TryGetProperty("id", out JsonElement idElement));
        Assert.Equal("thread123", idElement.GetString());

        Assert.True(root.TryGetProperty("emailIds", out JsonElement emailIdsElement));
        Assert.Equal(2, emailIdsElement.GetArrayLength());
        Assert.Equal("email1", emailIdsElement[0].GetString());
        Assert.Equal("email2", emailIdsElement[1].GetString());
    }

    /// <summary>
    /// Deserializes JSON that omits optional fields and ensures the corresponding properties are null.
    /// </summary>
    [Fact]
    public void DeserializeThread_WithMissingOptionalFields_ShouldSetNull()
    {
        // Arrange
        const string json = """{ "id": "thread123" }""";

        // Act
        Thread? thread = JsonSerializer.Deserialize<Thread>(json);

        // Assert
        Assert.NotNull(thread);
        Assert.Equal("thread123", thread!.Id);
        Assert.Null(thread.EmailIds);
    }

    /// <summary>
    /// Deserializes JSON with an empty <c>emailIds</c> array and verifies the property is an empty list rather than null.
    /// </summary>
    [Fact]
    public void DeserializeThread_WithEmptyEmailIdsArray_ShouldYieldEmptyList()
    {
        // Arrange
        const string json = """{ "id": "thread123", "emailIds": [] }""";

        // Act
        Thread? thread = JsonSerializer.Deserialize<Thread>(json);

        // Assert
        Assert.NotNull(thread);
        Assert.Equal("thread123", thread!.Id);
        Assert.NotNull(thread.EmailIds);
        Assert.Empty(thread.EmailIds);
    }
}
