using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using Aspose.Jmap;

namespace Aspose.Jmap.Tests;

/// <summary>
/// Tests for the <see cref="Identity"/> model serialization and deserialization.
/// </summary>
public class IdentityTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = false
    };

    [Fact]
    public void Deserialize_FullJson_PopulatesAllProperties()
    {
        const string json = """
        {
            "id": "id123",
            "name": "John Doe",
            "email": "john.doe@example.com",
            "replyTo": [
                { "name": "Reply One", "email": "reply1@example.com" },
                { "name": null, "email": "reply2@example.com" }
            ],
            "bcc": [
                { "name": "Bcc One", "email": "bcc1@example.com" }
            ],
            "textSignature": "Best regards",
            "htmlSignature": "<p>Best regards</p>",
            "mayDelete": true
        }
        """;

        var identity = JsonSerializer.Deserialize<Identity>(json, JsonOptions)!;

        Assert.Equal("id123", identity.Id);
        Assert.Equal("John Doe", identity.Name);
        Assert.Equal("john.doe@example.com", identity.Email);
        Assert.NotNull(identity.ReplyTo);
        Assert.Equal(2, identity.ReplyTo!.Count);
        Assert.Equal("Reply One", identity.ReplyTo[0].Name);
        Assert.Equal("reply1@example.com", identity.ReplyTo[0].Email);
        Assert.Null(identity.ReplyTo[1].Name);
        Assert.Equal("reply2@example.com", identity.ReplyTo[1].Email);
        Assert.NotNull(identity.Bcc);
        Assert.Single(identity.Bcc!);
        Assert.Equal("Bcc One", identity.Bcc![0].Name);
        Assert.Equal("bcc1@example.com", identity.Bcc![0].Email);
        Assert.Equal("Best regards", identity.TextSignature);
        Assert.Equal("<p>Best regards</p>", identity.HtmlSignature);
        Assert.True(identity.MayDelete);
    }

    [Fact]
    public void Deserialize_MissingOptionalFields_LeavesNulls()
    {
        const string json = """
        {
            "id": "id456",
            "name": "",
            "email": "jane.doe@example.com",
            "textSignature": "",
            "htmlSignature": ""
        }
        """;

        var identity = JsonSerializer.Deserialize<Identity>(json, JsonOptions)!;

        Assert.Equal("id456", identity.Id);
        Assert.Equal(string.Empty, identity.Name);
        Assert.Equal("jane.doe@example.com", identity.Email);
        Assert.Null(identity.ReplyTo);
        Assert.Null(identity.Bcc);
        Assert.Equal(string.Empty, identity.TextSignature);
        Assert.Equal(string.Empty, identity.HtmlSignature);
        Assert.Null(identity.MayDelete);
    }

    [Fact]
    public void Serialize_AndDeserialize_RoundTripPreservesData()
    {
        var original = new Identity
        {
            Id = "id789",
            Name = "Alice",
            Email = "alice@example.com",
            ReplyTo = new List<EmailAddress>
            {
                new EmailAddress { Name = "Reply Alice", Email = "reply.alice@example.com" }
            },
            Bcc = null,
            TextSignature = "Thanks",
            HtmlSignature = "<p>Thanks</p>",
            MayDelete = false
        };

        string json = JsonSerializer.Serialize(original, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<Identity>(json, JsonOptions)!;

        Assert.Equal(original.Id, deserialized.Id);
        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(original.Email, deserialized.Email);
        Assert.NotNull(deserialized.ReplyTo);
        Assert.Single(deserialized.ReplyTo!);
        Assert.Equal("Reply Alice", deserialized.ReplyTo![0].Name);
        Assert.Equal("reply.alice@example.com", deserialized.ReplyTo![0].Email);
        Assert.Null(deserialized.Bcc);
        Assert.Equal(original.TextSignature, deserialized.TextSignature);
        Assert.Equal(original.HtmlSignature, deserialized.HtmlSignature);
        Assert.Equal(original.MayDelete, deserialized.MayDelete);
    }
}
