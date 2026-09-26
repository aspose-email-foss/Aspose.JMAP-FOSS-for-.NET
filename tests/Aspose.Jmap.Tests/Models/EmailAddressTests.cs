using System.Text.Json;
using System.Text.Json.Serialization;
using Aspose.Jmap;
using Xunit;

namespace Aspose.Jmap.Tests.Models;

/// <summary>
/// Tests for <see cref="EmailAddress"/> serialization and deserialization.
/// </summary>
public class EmailAddressTests
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [Fact]
    public void Serialize_WithAllProperties_ShouldMatchExpectedJson()
    {
        // Arrange
        var address = new EmailAddress
        {
            Name = "John Doe",
            Email = "john.doe@example.com"
        };

        // Act
        string json = JsonSerializer.Serialize(address, _jsonOptions);

        // Assert
        const string expectedJson = """{"name":"John Doe","email":"john.doe@example.com"}""";
        Assert.Equal(expectedJson, json);
    }

    [Fact]
    public void Deserialize_WithNullName_ShouldSetNameToNull()
    {
        // Arrange
        const string json = """{"name":null,"email":"jane.doe@example.org"}""";

        // Act
        var address = JsonSerializer.Deserialize<EmailAddress>(json, _jsonOptions)!;

        // Assert
        Assert.NotNull(address);
        Assert.Null(address.Name);
        Assert.Equal("jane.doe@example.org", address.Email);
    }
}
