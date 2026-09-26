using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Aspose.Jmap;

namespace Aspose.Jmap.Tests.Models;

/// <summary>
/// Tests for <see cref="Envelope"/> and <see cref="Address"/> (de)serialization.
/// </summary>
public class EnvelopeTests
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = null,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [Fact]
    public void SerializeDeserialize_NormalCase_RoundtripPreservesData()
    {
        // Arrange
        var envelope = new Envelope
        {
            MailFrom = new Address
            {
                Email = "sender@example.com",
                Parameters = new Dictionary<string, string?>
                {
                    { "RET", "HDRS" },
                    { "SIZE", null }
                }
            },
            RcptTo = new List<Address>
            {
                new Address
                {
                    Email = "recipient1@example.com",
                    Parameters = new Dictionary<string, string?>
                    {
                        { "NOTIFY", "SUCCESS" }
                    }
                },
                new Address
                {
                    Email = "recipient2@example.com",
                    Parameters = null
                }
            }
        };

        // Act
        string json = JsonSerializer.Serialize(envelope, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<Envelope>(json, JsonOptions)!;

        // Assert
        Assert.Equal(envelope.MailFrom.Email, deserialized.MailFrom.Email);
        Assert.NotNull(deserialized.MailFrom.Parameters);
        Assert.Equal(envelope.MailFrom.Parameters!.Count, deserialized.MailFrom.Parameters!.Count);
        foreach (var kvp in envelope.MailFrom.Parameters)
        {
            Assert.True(deserialized.MailFrom.Parameters.ContainsKey(kvp.Key));
            Assert.Equal(kvp.Value, deserialized.MailFrom.Parameters[kvp.Key]);
        }

        Assert.Equal(envelope.RcptTo.Count, deserialized.RcptTo.Count);
        for (int i = 0; i < envelope.RcptTo.Count; i++)
        {
            var expected = envelope.RcptTo[i];
            var actual = deserialized.RcptTo[i];
            Assert.Equal(expected.Email, actual.Email);
            if (expected.Parameters is null)
            {
                Assert.Null(actual.Parameters);
            }
            else
            {
                Assert.NotNull(actual.Parameters);
                Assert.Equal(expected.Parameters.Count, actual.Parameters!.Count);
                foreach (var kvp in expected.Parameters)
                {
                    Assert.True(actual.Parameters.ContainsKey(kvp.Key));
                    Assert.Equal(kvp.Value, actual.Parameters[kvp.Key]);
                }
            }
        }
    }

    [Fact]
    public void Serialize_NullParameters_OmitsProperty()
    {
        // Arrange
        var envelope = new Envelope
        {
            MailFrom = new Address
            {
                Email = "sender@example.com",
                Parameters = null
            },
            RcptTo = new List<Address>
            {
                new Address
                {
                    Email = "recipient@example.com",
                    Parameters = null
                }
            }
        };

        // Act
        string json = JsonSerializer.Serialize(envelope, JsonOptions);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Assert: mailFrom should not contain "parameters"
        var mailFrom = root.GetProperty("mailFrom");
        Assert.False(mailFrom.TryGetProperty("parameters", out _));

        // Assert: rcptTo[0] should not contain "parameters"
        var rcptTo0 = root.GetProperty("rcptTo")[0];
        Assert.False(rcptTo0.TryGetProperty("parameters", out _));
    }

    [Fact]
    public void Deserialize_MissingParameters_SetsNull()
    {
        // Arrange
        const string json = """
        {
            "mailFrom": { "email": "sender@example.com" },
            "rcptTo": [
                { "email": "recipient@example.com" }
            ]
        }
        """;

        // Act
        var envelope = JsonSerializer.Deserialize<Envelope>(json, JsonOptions)!;

        // Assert
        Assert.Equal("sender@example.com", envelope.MailFrom.Email);
        Assert.Null(envelope.MailFrom.Parameters);
        Assert.Single(envelope.RcptTo);
        Assert.Equal("recipient@example.com", envelope.RcptTo[0].Email);
        Assert.Null(envelope.RcptTo[0].Parameters);
    }
}
