using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Aspose.Jmap;

namespace Aspose.Jmap.Tests.Models;

/// <summary>
/// Tests for the <see cref="Email"/> model (de)serialization.
/// </summary>
public class EmailTests
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = null,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    [Fact]
    public void Deserialize_FullJson_ReturnsExpectedObject()
    {
        const string json = """
        {
            "id":"email1",
            "blobId":"blob1",
            "threadId":"thread1",
            "mailboxIds":{"mailboxA":true},
            "keywords":{"$seen":true},
            "size":1024,
            "receivedAt":"2023-01-01T00:00:00Z",
            "messageId":["<msg1@example.com>"],
            "inReplyTo":null,
            "references":null,
            "sender":[{"name":"Sender","email":"sender@example.com"}],
            "from":[{"email":"from@example.com"}],
            "to":[{"email":"to@example.com"}],
            "cc":[],
            "bcc":null,
            "replyTo":null,
            "subject":"Test",
            "sentAt":"2023-01-01T00:00:00Z",
            "bodyStructure":{
                "partId":"1",
                "blobId":"blob1",
                "size":123,
                "headers":[],
                "type":"text/plain"
            },
            "bodyValues":{
                "1":{"value":"Hello","isEncodingProblem":false,"isTruncated":false}
            },
            "textBody":[
                {"partId":"2","blobId":"blob2","size":456,"headers":[],"type":"text/plain"}
            ],
            "htmlBody":[],
            "attachments":[],
            "hasAttachment":false,
            "preview":"preview text"
        }
        """;

        var email = JsonSerializer.Deserialize<Email>(json, _jsonOptions)!;

        Assert.Equal("email1", email.Id);
        Assert.Equal("blob1", email.BlobId);
        Assert.Equal("thread1", email.ThreadId);
        Assert.NotNull(email.MailboxIds);
        Assert.Single(email.MailboxIds);
        Assert.True(email.MailboxIds["mailboxA"]);
        Assert.NotNull(email.Keywords);
        Assert.Single(email.Keywords);
        Assert.True(email.Keywords["$seen"]);
        Assert.Equal((uint)1024, email.Size);
        Assert.Equal("2023-01-01T00:00:00Z", email.ReceivedAt);
        Assert.NotNull(email.MessageId);
        Assert.Single(email.MessageId!);
        Assert.Equal("<msg1@example.com>", email.MessageId![0]);
        Assert.Null(email.InReplyTo);
        Assert.Null(email.References);
        Assert.NotNull(email.Sender);
        Assert.Single(email.Sender!);
        Assert.Equal("Sender", email.Sender![0].Name);
        Assert.Equal("sender@example.com", email.Sender![0].Email);
        Assert.NotNull(email.From);
        Assert.Single(email.From!);
        Assert.Equal("from@example.com", email.From![0].Email);
        Assert.NotNull(email.To);
        Assert.Single(email.To!);
        Assert.Equal("to@example.com", email.To![0].Email);
        Assert.NotNull(email.Cc);
        Assert.Empty(email.Cc!);
        Assert.Null(email.Bcc);
        Assert.Null(email.ReplyTo);
        Assert.Equal("Test", email.Subject);
        Assert.Equal("2023-01-01T00:00:00Z", email.SentAt);
        Assert.NotNull(email.BodyStructure);
        Assert.Equal("1", email.BodyStructure!.PartId);
        Assert.Equal("blob1", email.BodyStructure!.BlobId);
        Assert.Equal((uint)123, email.BodyStructure!.Size);
        Assert.Equal("text/plain", email.BodyStructure!.Type);
        Assert.NotNull(email.BodyValues);
        Assert.Single(email.BodyValues!);
        var bodyValue = email.BodyValues!["1"];
        Assert.Equal("Hello", bodyValue.Value);
        Assert.False(bodyValue.IsEncodingProblem);
        Assert.False(bodyValue.IsTruncated);
        Assert.NotNull(email.TextBody);
        Assert.Single(email.TextBody!);
        Assert.Equal("2", email.TextBody![0].PartId);
        Assert.NotNull(email.HtmlBody);
        Assert.Empty(email.HtmlBody!);
        Assert.NotNull(email.Attachments);
        Assert.Empty(email.Attachments!);
        Assert.False(email.HasAttachment);
        Assert.Equal("preview text", email.Preview);
    }

    [Fact]
    public void Deserialize_MissingOptionalFields_UsesDefaults()
    {
        const string json = """
        {
            "id":"email2",
            "blobId":"blob2",
            "threadId":"thread2",
            "mailboxIds":{"mailboxB":true}
        }
        """;

        var email = JsonSerializer.Deserialize<Email>(json, _jsonOptions)!;

        Assert.Equal("email2", email.Id);
        Assert.Equal("blob2", email.BlobId);
        Assert.Equal("thread2", email.ThreadId);
        Assert.NotNull(email.MailboxIds);
        Assert.Single(email.MailboxIds);
        Assert.True(email.MailboxIds["mailboxB"]);

        // Optional collections should be empty/default, not null
        Assert.NotNull(email.Keywords);
        Assert.Empty(email.Keywords);
        Assert.Null(email.Size);
        Assert.Null(email.ReceivedAt);
        Assert.Null(email.MessageId);
        Assert.Null(email.InReplyTo);
        Assert.Null(email.References);
        Assert.Null(email.Sender);
        Assert.Null(email.From);
        Assert.Null(email.To);
        Assert.Null(email.Cc);
        Assert.Null(email.Bcc);
        Assert.Null(email.ReplyTo);
        Assert.Null(email.Subject);
        Assert.Null(email.SentAt);
        Assert.Null(email.BodyStructure);
        Assert.Null(email.BodyValues);
        Assert.Null(email.TextBody);
        Assert.Null(email.HtmlBody);
        Assert.Null(email.Attachments);
        Assert.Null(email.HasAttachment);
        Assert.Null(email.Preview);
    }

    [Fact]
    public void Serialize_AndDeserialize_RoundTripPreservesData()
    {
        var original = new Email
        {
            Id = "email3",
            BlobId = "blob3",
            ThreadId = "thread3",
            MailboxIds = new Dictionary<string, bool> { ["mailboxC"] = true },
            Keywords = new Dictionary<string, bool> { ["$draft"] = true },
            Size = 2048,
            ReceivedAt = "2023-02-02T12:34:56Z",
            MessageId = new List<string> { "<msg3@example.com>" },
            Subject = "RoundTrip",
            SentAt = "2023-02-02T12:00:00Z",
            BodyStructure = new EmailBodyPart
            {
                PartId = "3",
                BlobId = "blob3",
                Size = 789,
                Headers = new List<EmailHeader>(),
                Type = "text/plain"
            },
            BodyValues = new Dictionary<string, EmailBodyValue>
            {
                ["3"] = new EmailBodyValue
                {
                    Value = "World",
                    IsEncodingProblem = true,
                    IsTruncated = false
                }
            },
            TextBody = new List<EmailBodyPart>
            {
                new EmailBodyPart
                {
                    PartId = "4",
                    BlobId = "blob4",
                    Size = 321,
                    Headers = new List<EmailHeader>(),
                    Type = "text/plain"
                }
            },
            HasAttachment = true,
            Preview = "preview"
        };

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<Email>(json, _jsonOptions)!;

        Assert.Equal(original.Id, deserialized.Id);
        Assert.Equal(original.BlobId, deserialized.BlobId);
        Assert.Equal(original.ThreadId, deserialized.ThreadId);
        Assert.Equal(original.MailboxIds, deserialized.MailboxIds);
        Assert.Equal(original.Keywords, deserialized.Keywords);
        Assert.Equal(original.Size, deserialized.Size);
        Assert.Equal(original.ReceivedAt, deserialized.ReceivedAt);
        Assert.Equal(original.MessageId, deserialized.MessageId);
        Assert.Equal(original.Subject, deserialized.Subject);
        Assert.Equal(original.SentAt, deserialized.SentAt);
        Assert.NotNull(deserialized.BodyStructure);
        Assert.Equal(original.BodyStructure!.PartId, deserialized.BodyStructure!.PartId);
        Assert.Equal(original.BodyStructure.BlobId, deserialized.BodyStructure.BlobId);
        Assert.Equal(original.BodyStructure.Size, deserialized.BodyStructure.Size);
        Assert.Equal(original.BodyStructure.Type, deserialized.BodyStructure.Type);
        Assert.NotNull(deserialized.BodyValues);
        Assert.Equal(original.BodyValues!.Count, deserialized.BodyValues!.Count);
        var originalBodyValue = original.BodyValues["3"];
        var deserializedBodyValue = deserialized.BodyValues!["3"];
        Assert.Equal(originalBodyValue.Value, deserializedBodyValue.Value);
        Assert.Equal(originalBodyValue.IsEncodingProblem, deserializedBodyValue.IsEncodingProblem);
        Assert.Equal(originalBodyValue.IsTruncated, deserializedBodyValue.IsTruncated);
        Assert.NotNull(deserialized.TextBody);
        Assert.Single(deserialized.TextBody!);
        Assert.Equal(original.TextBody![0].PartId, deserialized.TextBody![0].PartId);
        Assert.Equal(original.HasAttachment, deserialized.HasAttachment);
        Assert.Equal(original.Preview, deserialized.Preview);
    }
}
