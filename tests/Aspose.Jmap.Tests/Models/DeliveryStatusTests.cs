using System.Text.Json;
using System.Text.Json.Serialization;
using Aspose.Jmap;
using Xunit;

namespace Aspose.Jmap.Tests.Models;

/// <summary>
/// Tests for <see cref="DeliveryStatus"/> serialization and deserialization.
/// </summary>
public class DeliveryStatusTests
{
    [Fact]
    public void Serialize_AllProperties_ShouldContainExpectedJson()
    {
        // Arrange
        var status = new DeliveryStatus
        {
            // The model's setters are internal; use reflection to set values for testing.
        };
        // Use JsonSerializer to set internal properties via a temporary JSON round‑trip.
        var json = @"{
            ""smtpReply"": ""250 OK"",
            ""delivered"": ""yes"",
            ""displayed"": ""unknown""
        }";
        var deserialized = JsonSerializer.Deserialize<DeliveryStatus>(json)!;

        // Act
        var serialized = JsonSerializer.Serialize(deserialized, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        });

        // Assert
        using var doc = JsonDocument.Parse(serialized);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("smtpReply", out var smtpReply));
        Assert.Equal("250 OK", smtpReply.GetString());

        Assert.True(root.TryGetProperty("delivered", out var delivered));
        Assert.Equal("yes", delivered.GetString());

        Assert.True(root.TryGetProperty("displayed", out var displayed));
        Assert.Equal("unknown", displayed.GetString());
    }

    [Fact]
    public void Deserialize_MissingOptionalFields_ShouldSetNull()
    {
        // Arrange
        var json = @"{ ""smtpReply"": ""550 No such user"" }";

        // Act
        var result = JsonSerializer.Deserialize<DeliveryStatus>(json)!;

        // Assert
        Assert.Equal("550 No such user", result.SmtpReply);
        Assert.Null(result.Delivered);
        Assert.Null(result.Displayed);
    }

    [Fact]
    public void Deserialize_ExplicitNullFields_ShouldSetNull()
    {
        // Arrange
        var json = @"{
            ""smtpReply"": null,
            ""delivered"": null,
            ""displayed"": ""yes""
        }";

        // Act
        var result = JsonSerializer.Deserialize<DeliveryStatus>(json)!;

        // Assert
        Assert.Null(result.SmtpReply);
        Assert.Null(result.Delivered);
        Assert.Equal("yes", result.Displayed);
    }
}
