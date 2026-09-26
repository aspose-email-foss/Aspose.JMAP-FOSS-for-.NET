using System.Text.Json;
using System.Text.Json.Serialization;
using Aspose.Jmap;
using Xunit;

namespace Aspose.Jmap.Tests.Models;

/// <summary>
/// Tests for <see cref="SearchSnippet"/> (de)serialization.
/// </summary>
public class SearchSnippetTests
{
    private static readonly JsonSerializerOptions _ignoreNullOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = null
    };

    [Fact]
    public void Deserialize_WithAllFields_ShouldPopulateProperties()
    {
        const string json = """{"emailId":"abc123","subject":"Hello <mark>World</mark>","preview":"This is a <mark>test</mark>"}""";

        SearchSnippet? snippet = JsonSerializer.Deserialize<SearchSnippet>(json);

        Assert.NotNull(snippet);
        Assert.Equal("abc123", snippet!.EmailId);
        Assert.Equal("Hello <mark>World</mark>", snippet.Subject);
        Assert.Equal("This is a <mark>test</mark>", snippet.Preview);
    }

    [Fact]
    public void Deserialize_WithMissingOptionalFields_ShouldSetNull()
    {
        const string json = """{"emailId":"def456"}""";

        SearchSnippet? snippet = JsonSerializer.Deserialize<SearchSnippet>(json);

        Assert.NotNull(snippet);
        Assert.Equal("def456", snippet!.EmailId);
        Assert.Null(snippet.Subject);
        Assert.Null(snippet.Preview);
    }

    [Fact]
    public void Serialize_AfterDeserialization_ShouldProduceIdenticalJson()
    {
        const string json = """{"emailId":"ghi789","subject":"Subject","preview":"Preview"}""";

        SearchSnippet? snippet = JsonSerializer.Deserialize<SearchSnippet>(json);
        Assert.NotNull(snippet);

        string serialized = JsonSerializer.Serialize(snippet, _ignoreNullOptions);
        Assert.Equal(json, serialized);
    }

    [Fact]
    public void Serialize_WithOnlyRequiredField_ShouldOmitOptionalNulls()
    {
        const string json = """{"emailId":"jkl012"}""";

        var snippet = new SearchSnippet { EmailId = "jkl012" };

        string serialized = JsonSerializer.Serialize(snippet, _ignoreNullOptions);
        Assert.Equal(json, serialized);
    }
}
