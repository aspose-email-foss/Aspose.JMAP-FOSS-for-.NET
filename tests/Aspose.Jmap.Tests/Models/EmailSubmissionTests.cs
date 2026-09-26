using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Aspose.Jmap;

namespace Aspose.Jmap.Tests.Models;

public class EmailSubmissionTests
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    [Fact]
    public void SerializeDeserialize_RoundTrip_AllProperties()
    {
        // Arrange
        var submission = new EmailSubmission
        {
            IdentityId = "identity-123",
            EmailId = "email-456",
            Envelope = new Envelope
            {
                MailFrom = new Address
                {
                    Email = "sender@example.com",
                    Parameters = new Dictionary<string, string?> { ["SIZE"] = "1234" }
                },
                RcptTo = new List<Address>
                {
                    new()
                    {
                        Email = "recipient1@example.com",
                        Parameters = null
                    },
                    new()
                    {
                        Email = "recipient2@example.com",
                        Parameters = new Dictionary<string, string?> { ["RET"] = "HDRS" }
                    }
                }
            },
            DeliveryStatus = new Dictionary<string, DeliveryStatus>
            {
                ["recipient1@example.com"] = new DeliveryStatus
                {
                    // internal setters are used by deserialization; we set via reflection for test purposes
                },
                ["recipient2@example.com"] = new DeliveryStatus
                {
                    // same as above
                }
            },
            DsnBlobIds = new List<string> { "blob-1", "blob-2" },
            MdnBlobIds = new List<string> { "blob-3" }
        };

        // Act
        string json = JsonSerializer.Serialize(submission, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<EmailSubmission>(json, _jsonOptions)!;

        // Assert
        Assert.Equal(submission.IdentityId, deserialized.IdentityId);
        Assert.Equal(submission.EmailId, deserialized.EmailId);
        Assert.NotNull(deserialized.Envelope);
        Assert.Equal(submission.Envelope!.MailFrom.Email, deserialized.Envelope.MailFrom.Email);
        Assert.Equal(submission.Envelope.RcptTo.Count, deserialized.Envelope.RcptTo.Count);
        Assert.Equal(submission.DsnBlobIds, deserialized.DsnBlobIds);
        Assert.Equal(submission.MdnBlobIds, deserialized.MdnBlobIds);
        Assert.NotNull(deserialized.DeliveryStatus);
        Assert.Equal(submission.DeliveryStatus!.Count, deserialized.DeliveryStatus.Count);
    }

    [Fact]
    public void Deserialize_WithMissingOptionalFields_LeavesNulls()
    {
        // Arrange: JSON contains only required fields
        const string json = """
        {
            "identityId": "identity-789",
            "emailId": "email-012"
        }
        """;

        // Act
        var deserialized = JsonSerializer.Deserialize<EmailSubmission>(json, _jsonOptions)!;

        // Assert
        Assert.Equal("identity-789", deserialized.IdentityId);
        Assert.Equal("email-012", deserialized.EmailId);
        Assert.Null(deserialized.Id);
        Assert.Null(deserialized.ThreadId);
        Assert.Null(deserialized.Envelope);
        Assert.Null(deserialized.SendAt);
        Assert.Null(deserialized.UndoStatus);
        Assert.Null(deserialized.DeliveryStatus);
        Assert.Null(deserialized.DsnBlobIds);
        Assert.Null(deserialized.MdnBlobIds);
    }
}
