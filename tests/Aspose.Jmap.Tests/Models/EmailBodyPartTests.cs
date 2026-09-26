using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using Aspose.Jmap;

namespace Aspose.Jmap.Tests;

/// <summary>
/// Tests for <see cref="EmailBodyPart"/> serialization and deserialization.
/// </summary>
public class EmailBodyPartTests
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = null,
        WriteIndented = false
    };

    [Fact]
    public void Deserialize_FullObject_ShouldPopulateAllProperties()
    {
        // Arrange
        const string json = """
        {
            "partId":"1",
            "blobId":"blob123",
            "size":1024,
            "headers":[{"name":"Subject","value":"Test"}],
            "name":"test.txt",
            "type":"text/plain",
            "charset":"utf-8",
            "disposition":"attachment",
            "cid":"<cid>",
            "language":["en","fr"],
            "location":"/path",
            "subParts":[
                {
                    "partId":"1.1",
                    "size":512,
                    "headers":[],
                    "type":"text/html"
                }
            ]
        }
        """;

        // Act
        var part = JsonSerializer.Deserialize<EmailBodyPart>(json, _jsonOptions)!;

        // Assert
        Assert.Equal("1", part.PartId);
        Assert.Equal("blob123", part.BlobId);
        Assert.Equal((uint)1024, part.Size);
        Assert.Single(part.Headers);
        Assert.Equal("Subject", part.Headers[0].Name);
        Assert.Equal("Test", part.Headers[0].Value);
        Assert.Equal("test.txt", part.Name);
        Assert.Equal("text/plain", part.Type);
        Assert.Equal("utf-8", part.Charset);
        Assert.Equal("attachment", part.Disposition);
        Assert.Equal("<cid>", part.Cid);
        Assert.NotNull(part.Language);
        Assert.Equal(new[] { "en", "fr" }, part.Language);
        Assert.Equal("/path", part.Location);
        Assert.NotNull(part.SubParts);
        var sub = part.SubParts![0];
        Assert.Equal("1.1", sub.PartId);
        Assert.Null(sub.BlobId);
        Assert.Equal((uint)512, sub.Size);
        Assert.Empty(sub.Headers);
        Assert.Equal("text/html", sub.Type);
        Assert.Null(sub.Charset);
        Assert.Null(sub.Disposition);
        Assert.Null(sub.Cid);
        Assert.Null(sub.Language);
        Assert.Null(sub.Location);
        Assert.Null(sub.SubParts);
    }

    [Fact]
    public void Deserialize_MinimalObject_ShouldLeaveOptionalPropertiesNull()
    {
        // Arrange
        const string json = """
        {
            "size":10,
            "headers":[],
            "type":"text/plain"
        }
        """;

        // Act
        var part = JsonSerializer.Deserialize<EmailBodyPart>(json, _jsonOptions)!;

        // Assert
        Assert.Null(part.PartId);
        Assert.Null(part.BlobId);
        Assert.Equal((uint)10, part.Size);
        Assert.Empty(part.Headers);
        Assert.Null(part.Name);
        Assert.Equal("text/plain", part.Type);
        Assert.Null(part.Charset);
        Assert.Null(part.Disposition);
        Assert.Null(part.Cid);
        Assert.Null(part.Language);
        Assert.Null(part.Location);
        Assert.Null(part.SubParts);
    }

    [Fact]
    public void Serialize_FullObject_ShouldContainAllProperties()
    {
        // Arrange
        var part = new EmailBodyPart
        {
            PartId = "1",
            BlobId = "blob123",
            Size = 1024,
            Headers = new List<EmailHeader>
            {
                new EmailHeader { Name = "Subject", Value = "Test" }
            },
            Name = "test.txt",
            Type = "text/plain",
            Charset = "utf-8",
            Disposition = "attachment",
            Cid = "<cid>",
            Language = new List<string> { "en", "fr" },
            Location = "/path",
            SubParts = new List<EmailBodyPart>
            {
                new EmailBodyPart
                {
                    PartId = "1.1",
                    Size = 512,
                    Headers = new List<EmailHeader>(),
                    Type = "text/html"
                }
            }
        };

        // Act
        var json = JsonSerializer.Serialize(part, _jsonOptions);
        var doc = JsonDocument.Parse(json).RootElement;

        // Assert
        Assert.Equal("1", doc.GetProperty("partId").GetString());
        Assert.Equal("blob123", doc.GetProperty("blobId").GetString());
        Assert.Equal(1024u, doc.GetProperty("size").GetUInt32());
        Assert.Equal("test.txt", doc.GetProperty("name").GetString());
        Assert.Equal("text/plain", doc.GetProperty("type").GetString());
        Assert.Equal("utf-8", doc.GetProperty("charset").GetString());
        Assert.Equal("attachment", doc.GetProperty("disposition").GetString());
        Assert.Equal("<cid>", doc.GetProperty("cid").GetString());
        Assert.Equal("/path", doc.GetProperty("location").GetString());

        var language = doc.GetProperty("language");
        Assert.Equal(2, language.GetArrayLength());
        Assert.Equal("en", language[0].GetString());
        Assert.Equal("fr", language[1].GetString());

        var subParts = doc.GetProperty("subParts");
        Assert.Equal(1, subParts.GetArrayLength());
        var sub = subParts[0];
        Assert.Equal("1.1", sub.GetProperty("partId").GetString());
        Assert.Equal(512u, sub.GetProperty("size").GetUInt32());
        Assert.Equal("text/html", sub.GetProperty("type").GetString());
    }
}
