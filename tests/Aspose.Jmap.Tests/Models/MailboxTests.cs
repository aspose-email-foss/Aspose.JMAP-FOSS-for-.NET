using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Aspose.Jmap;

namespace Aspose.Jmap.Tests.Models;

/// <summary>
/// Tests for the <see cref="Mailbox"/> and <see cref="MailboxRights"/> model classes,
/// focusing on JSON (de)serialization behavior.
/// </summary>
public class MailboxTests
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = null,
        WriteIndented = false
    };

    [Fact]
    public void Deserialize_FullJson_SetsAllProperties()
    {
        // Arrange
        const string json = """
        {
            "id": "mailbox123",
            "name": "Inbox",
            "parentId": null,
            "role": null,
            "sortOrder": 10,
            "totalEmails": 250,
            "unreadEmails": 42,
            "totalThreads": 120,
            "unreadThreads": 7,
            "myRights": {
                "mayReadItems": true,
                "mayAddItems": false,
                "mayRemoveItems": true,
                "maySetSeen": false,
                "maySetKeywords": true,
                "mayCreateChild": false,
                "mayRename": true,
                "mayDelete": false,
                "maySubmit": true
            },
            "isSubscribed": true
        }
        """;

        // Act
        var mailbox = JsonSerializer.Deserialize<Mailbox>(json, JsonOptions)!;

        // Assert
        Assert.Equal("mailbox123", mailbox.Id);
        Assert.Equal("Inbox", mailbox.Name);
        Assert.Null(mailbox.ParentId);
        Assert.Null(mailbox.Role);
        Assert.Equal(10u, mailbox.SortOrder);
        Assert.Equal(250u, mailbox.TotalEmails);
        Assert.Equal(42u, mailbox.UnreadEmails);
        Assert.Equal(120u, mailbox.TotalThreads);
        Assert.Equal(7u, mailbox.UnreadThreads);
        Assert.True(mailbox.IsSubscribed);

        Assert.NotNull(mailbox.MyRights);
        var rights = mailbox.MyRights!;
        Assert.True(rights.MayReadItems);
        Assert.False(rights.MayAddItems);
        Assert.True(rights.MayRemoveItems);
        Assert.False(rights.MaySetSeen);
        Assert.True(rights.MaySetKeywords);
        Assert.False(rights.MayCreateChild);
        Assert.True(rights.MayRename);
        Assert.False(rights.MayDelete);
        Assert.True(rights.MaySubmit);
    }

    [Fact]
    public void Serialize_RoundTrip_MaintainsDataIntegrity()
    {
        // Arrange: create a mailbox via deserialization so internal setters are populated
        const string originalJson = """
        {
            "id": "mailbox456",
            "name": "Sent",
            "parentId": "parentA",
            "role": "sent",
            "sortOrder": 5,
            "totalEmails": 100,
            "unreadEmails": 0,
            "totalThreads": 80,
            "unreadThreads": 0,
            "myRights": {
                "mayReadItems": true,
                "mayAddItems": true,
                "mayRemoveItems": true,
                "maySetSeen": true,
                "maySetKeywords": true,
                "mayCreateChild": true,
                "mayRename": true,
                "mayDelete": true,
                "maySubmit": true
            },
            "isSubscribed": false
        }
        """;

        var mailbox = JsonSerializer.Deserialize<Mailbox>(originalJson, JsonOptions)!;

        // Act: serialize back to JSON and deserialize again
        var serializedJson = JsonSerializer.Serialize(mailbox, JsonOptions);
        var roundTripMailbox = JsonSerializer.Deserialize<Mailbox>(serializedJson, JsonOptions)!;

        // Assert: all properties match the original instance
        Assert.Equal(mailbox.Id, roundTripMailbox.Id);
        Assert.Equal(mailbox.Name, roundTripMailbox.Name);
        Assert.Equal(mailbox.ParentId, roundTripMailbox.ParentId);
        Assert.Equal(mailbox.Role, roundTripMailbox.Role);
        Assert.Equal(mailbox.SortOrder, roundTripMailbox.SortOrder);
        Assert.Equal(mailbox.TotalEmails, roundTripMailbox.TotalEmails);
        Assert.Equal(mailbox.UnreadEmails, roundTripMailbox.UnreadEmails);
        Assert.Equal(mailbox.TotalThreads, roundTripMailbox.TotalThreads);
        Assert.Equal(mailbox.UnreadThreads, roundTripMailbox.UnreadThreads);
        Assert.Equal(mailbox.IsSubscribed, roundTripMailbox.IsSubscribed);

        Assert.NotNull(roundTripMailbox.MyRights);
        var originalRights = mailbox.MyRights!;
        var roundTripRights = roundTripMailbox.MyRights!;

        Assert.Equal(originalRights.MayReadItems, roundTripRights.MayReadItems);
        Assert.Equal(originalRights.MayAddItems, roundTripRights.MayAddItems);
        Assert.Equal(originalRights.MayRemoveItems, roundTripRights.MayRemoveItems);
        Assert.Equal(originalRights.MaySetSeen, roundTripRights.MaySetSeen);
        Assert.Equal(originalRights.MaySetKeywords, roundTripRights.MaySetKeywords);
        Assert.Equal(originalRights.MayCreateChild, roundTripRights.MayCreateChild);
        Assert.Equal(originalRights.MayRename, roundTripRights.MayRename);
        Assert.Equal(originalRights.MayDelete, roundTripRights.MayDelete);
        Assert.Equal(originalRights.MaySubmit, roundTripRights.MaySubmit);
    }

    [Fact]
    public void Deserialize_WithNullOptionalFields_HandlesGracefully()
    {
        // Arrange: JSON where optional fields are explicitly null
        const string json = """
        {
            "id": "mailbox789",
            "name": "Archive",
            "parentId": null,
            "role": null,
            "sortOrder": 0,
            "totalEmails": 0,
            "unreadEmails": 0,
            "totalThreads": 0,
            "unreadThreads": 0,
            "myRights": null,
            "isSubscribed": false
        }
        """;

        // Act
        var mailbox = JsonSerializer.Deserialize<Mailbox>(json, JsonOptions)!;

        // Assert
        Assert.Equal("mailbox789", mailbox.Id);
        Assert.Equal("Archive", mailbox.Name);
        Assert.Null(mailbox.ParentId);
        Assert.Null(mailbox.Role);
        Assert.Equal(0u, mailbox.SortOrder);
        Assert.Equal(0u, mailbox.TotalEmails);
        Assert.Equal(0u, mailbox.UnreadEmails);
        Assert.Equal(0u, mailbox.TotalThreads);
        Assert.Equal(0u, mailbox.UnreadThreads);
        Assert.Null(mailbox.MyRights);
        Assert.False(mailbox.IsSubscribed);
    }
}
