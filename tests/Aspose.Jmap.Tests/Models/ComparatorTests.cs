using System.Text.Json;
using System.Text.Json.Serialization;
using Aspose.Jmap;
using Xunit;

namespace Aspose.Jmap.Tests.Models;

/// <summary>
/// Tests for the <see cref="Comparator"/> model's JSON (de)serialization.
/// </summary>
public class ComparatorTests
{
    private static readonly JsonSerializerOptions _options = new()
    {
        // Respect the JsonPropertyName attributes and ignore null values for cleaner output.
        PropertyNamingPolicy = null,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [Fact]
    public void Serialize_WithAllProperties_ProducesExpectedJson()
    {
        // Arrange
        var comparator = new Comparator
        {
            Property = "subject",
            IsAscending = false,
            Collation = "en-US"
        };

        // Act
        string json = JsonSerializer.Serialize(comparator, _options);

        // Assert
        const string expectedJson = "{\"property\":\"subject\",\"isAscending\":false,\"collation\":\"en-US\"}";
        Assert.Equal(expectedJson, json);
    }

    [Fact]
    public void Deserialize_MissingOptionalFields_SetsDefaultsAndNulls()
    {
        // Arrange
        const string json = "{\"property\":\"size\",\"isAscending\":true}";

        // Act
        Comparator? comparator = JsonSerializer.Deserialize<Comparator>(json, _options);

        // Assert
        Assert.NotNull(comparator);
        Assert.Equal("size", comparator!.Property);
        Assert.True(comparator.IsAscending);
        Assert.Null(comparator.Collation);
    }

    [Fact]
    public void Serialize_WithDefaults_OmitsNullCollation()
    {
        // Arrange
        var comparator = new Comparator
        {
            Property = "receivedAt"
            // IsAscending defaults to true, Collation left as null.
        };

        // Act
        string json = JsonSerializer.Serialize(comparator, _options);

        // Assert
        const string expectedJson = "{\"property\":\"receivedAt\",\"isAscending\":true}";
        Assert.Equal(expectedJson, json);
    }
}
