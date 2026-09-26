using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Aspose.Jmap;

/// <summary>
/// Rights a user has on a mailbox. All properties are server-assigned and therefore have initters.
/// </summary>
public class MailboxRights
{
    /// <summary>
    /// Indicates whether the user may read items in the mailbox.
    /// </summary>
    [JsonPropertyName("mayReadItems")]
    public bool MayReadItems { get; init; }

    /// <summary>
    /// Indicates whether the user may add items to the mailbox.
    /// </summary>
    [JsonPropertyName("mayAddItems")]
    public bool MayAddItems { get; init; }

    /// <summary>
    /// Indicates whether the user may remove items from the mailbox.
    /// </summary>
    [JsonPropertyName("mayRemoveItems")]
    public bool MayRemoveItems { get; init; }

    /// <summary>
    /// Indicates whether the user may set the seen flag on items in the mailbox.
    /// </summary>
    [JsonPropertyName("maySetSeen")]
    public bool MaySetSeen { get; init; }

    /// <summary>
    /// Indicates whether the user may set keywords on items in the mailbox.
    /// </summary>
    [JsonPropertyName("maySetKeywords")]
    public bool MaySetKeywords { get; init; }

    /// <summary>
    /// Indicates whether the user may create child mailboxes.
    /// </summary>
    [JsonPropertyName("mayCreateChild")]
    public bool MayCreateChild { get; init; }

    /// <summary>
    /// Indicates whether the user may rename the mailbox.
    /// </summary>
    [JsonPropertyName("mayRename")]
    public bool MayRename { get; init; }

    /// <summary>
    /// Indicates whether the user may delete the mailbox.
    /// </summary>
    [JsonPropertyName("mayDelete")]
    public bool MayDelete { get; init; }

    /// <summary>
    /// Indicates whether the user may submit messages from the mailbox.
    /// </summary>
    [JsonPropertyName("maySubmit")]
    public bool MaySubmit { get; init; }
}

/// <summary>
/// A named set of Emails (JMAP's analogue of a mail folder / IMAP mailbox). Top-level, addressable JMAP data type.
/// </summary>
public class Mailbox
{
    /// <summary>
    /// Server-assigned identifier for the mailbox. Omitted when constructing a create payload.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// The name of the mailbox. Required.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the parent mailbox, or null if this mailbox has no parent.
    /// </summary>
    [JsonPropertyName("parentId")]
    public string? ParentId { get; set; }

    /// <summary>
    /// Role of the mailbox (e.g., inbox, sent, drafts, trash, junk, archive), or null if none.
    /// </summary>
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    /// <summary>
    /// Sort order of the mailbox. Default is 0.
    /// </summary>
    [JsonPropertyName("sortOrder")]
    public uint SortOrder { get; set; } = 0;

    /// <summary>
    /// Total number of emails in the mailbox. Server-assigned; omitted when constructing a create payload.
    /// </summary>
    [JsonPropertyName("totalEmails")]
    public uint? TotalEmails { get; init; }

    /// <summary>
    /// Number of unread emails in the mailbox. Server-assigned; omitted when constructing a create payload.
    /// </summary>
    [JsonPropertyName("unreadEmails")]
    public uint? UnreadEmails { get; init; }

    /// <summary>
    /// Total number of threads in the mailbox. Server-assigned; omitted when constructing a create payload.
    /// </summary>
    [JsonPropertyName("totalThreads")]
    public uint? TotalThreads { get; init; }

    /// <summary>
    /// Number of unread threads in the mailbox. Server-assigned; omitted when constructing a create payload.
    /// </summary>
    [JsonPropertyName("unreadThreads")]
    public uint? UnreadThreads { get; init; }

    /// <summary>
    /// Rights the authenticated user has on this mailbox. Server-assigned; omitted when constructing a create payload.
    /// </summary>
    [JsonPropertyName("myRights")]
    public MailboxRights? MyRights { get; init; }

    /// <summary>
    /// Indicates whether the client is subscribed to this mailbox. Default is false.
    /// </summary>
    [JsonPropertyName("isSubscribed")]
    public bool IsSubscribed { get; set; } = false;
}
