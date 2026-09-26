using System.Text.Json;
using Aspose.Jmap;
using Xunit;

namespace Aspose.Jmap.Tests.Models;

/// <summary>
/// Tests for the <see cref="EmailHeader"/> model serialization and deserialization.
/// </summary>
public class EmailHeaderTests
{
    [Fact]
    public void Serialize_ShouldProduceExpectedJson()
    {
        var header = new EmailHeader
        {
            Name = "Subject",
            Value = "Hello World"
        };

        string json = JsonSerializer.Serialize(header);
        Assert.Equal("{\"name\":\"Subject\",\"value\":\"Hello World\"}", json);
    }

    [Fact]
    public void Deserialize_ShouldPopulateProperties()
    {
        const string json = "{\"name\":\"From\",\"value\":\"alice@example.com\"}";
        EmailHeader? header = JsonSerializer.Deserialize<EmailHeader>(json);

        Assert.NotNull(header);
        Assert.Equal("From", header!.Name);
        Assert.Equal("alice@example.com", header.Value);
    }

    [Fact]
    public void Serialize_WithEmptyStrings_ShouldRoundTripCorrectly()
    {
        var header = new EmailHeader
        {
            Name = string.Empty,
            Value = string.Empty
        };

        string json = JsonSerializer.Serialize(header);
        Assert.Equal("{\"name\":\"\",\"value\":\"\"}", json);

        EmailHeader? roundTrip = JsonSerializer.Deserialize<EmailHeader>(json);
        Assert.NotNull(roundTrip);
        Assert.Equal(string.Empty, roundTrip!.Name);
        Assert.Equal(string.Empty, roundTrip.Value);
    }
}
