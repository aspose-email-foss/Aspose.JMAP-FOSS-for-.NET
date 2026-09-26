using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aspose.Jmap;
using Xunit;

namespace Aspose.Jmap.Tests.Models;

/// <summary>
/// Tests for <see cref="EmailAddressGroup"/> serialization and deserialization.
/// </summary>
public class EmailAddressGroupTests
{
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        WriteIndented = false
    };

    [Fact]
    public void Serialize_WithNameAndAddresses_ProducesExpectedJson()
    {
        // Arrange
        var group = new EmailAddressGroup
        {
            Name = "Team",
            Addresses = new List<EmailAddress>
            {
                new EmailAddress { Name = "Alice", Email = "alice@example.com" },
                new EmailAddress { Name = null, Email = "bob@example.com" }
            }
        };

        const string expectedJson = """{"name":"Team","addresses":[{"name":"Alice","email":"alice@example.com"},{"name":null,"email":"bob@example.com"}]}""";

        // Act
        string json = JsonSerializer.Serialize(group, _options);

        // Assert
        Assert.Equal(expectedJson, json);
    }

    [Fact]
    public void Deserialize_WithNameAndAddresses_ProducesExpectedObject()
    {
        // Arrange
        const string json = """{"name":"Team","addresses":[{"name":"Alice","email":"alice@example.com"},{"name":null,"email":"bob@example.com"}]}""";

        // Act
        var group = JsonSerializer.Deserialize<EmailAddressGroup>(json, _options)!;

        // Assert
        Assert.Equal("Team", group.Name);
        Assert.NotNull(group.Addresses);
        Assert.Equal(2, group.Addresses.Count);

        var first = group.Addresses[0];
        Assert.Equal("Alice", first.Name);
        Assert.Equal("alice@example.com", first.Email);

        var second = group.Addresses[1];
        Assert.Null(second.Name);
        Assert.Equal("bob@example.com", second.Email);
    }

    [Fact]
    public void Serialize_WithNullNameAndEmptyAddresses_IncludesNullName()
    {
        // Arrange
        var group = new EmailAddressGroup
        {
            Name = null,
            Addresses = new List<EmailAddress>()
        };

        const string expectedJson = """{"name":null,"addresses":[]}""";

        // Act
        string json = JsonSerializer.Serialize(group, _options);

        // Assert
        Assert.Equal(expectedJson, json);
    }

    [Fact]
    public void Deserialize_WithMissingNameAndEmptyAddresses_HandlesDefaults()
    {
        // Arrange
        const string json = """{"addresses":[]}""";

        // Act
        var group = JsonSerializer.Deserialize<EmailAddressGroup>(json, _options)!;

        // Assert
        Assert.Null(group.Name);
        Assert.NotNull(group.Addresses);
        Assert.Empty(group.Addresses);
    }
}
