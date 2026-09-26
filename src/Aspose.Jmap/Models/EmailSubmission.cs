using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Aspose.Jmap;

/// <summary>
/// One attempt to submit an Email for delivery. Analogous to what Aspose.Email's SmtpClient.Send does
/// synchronously over SMTP; here creating an EmailSubmission is what actually dispatches the message
/// via the server's outbound MTA.
/// </summary>
public class EmailSubmission
{
    /// <summary>
    /// Server-assigned identifier of the EmailSubmission. Omitted when constructing a create payload.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// Identifier of the Identity to use for sending. Must reference an existing Identity.
    /// </summary>
    [JsonPropertyName("identityId")]
    public required string IdentityId { get; init; }

    /// <summary>
    /// Identifier of the Email to send; typically a draft created via Email/set just before this call.
    /// </summary>
    [JsonPropertyName("emailId")]
    public required string EmailId { get; init; }

    /// <summary>
    /// Server-assigned thread identifier. Omitted when constructing a create payload.
    /// </summary>
    [JsonPropertyName("threadId")]
    public string? ThreadId { get; init; }

    /// <summary>
    /// Optional envelope information. If null, the server derives it from the Email's From/To/Cc/Bcc headers.
    /// </summary>
    [JsonPropertyName("envelope")]
    public Envelope? Envelope { get; init; }

    /// <summary>
    /// Server-assigned UTC date string indicating when the submission is scheduled to be sent. Omitted when constructing a create payload.
    /// </summary>
    [JsonPropertyName("sendAt")]
    public string? SendAt { get; init; }

    /// <summary>
    /// Server-assigned undo status (e.g., "pending", "final", "canceled"). Omitted when constructing a create payload.
    /// </summary>
    [JsonPropertyName("undoStatus")]
    public string? UndoStatus { get; init; }

    /// <summary>
    /// Server-assigned delivery status map, keyed by recipient address. May be null.
    /// </summary>
    [JsonPropertyName("deliveryStatus")]
    public IReadOnlyDictionary<string, DeliveryStatus>? DeliveryStatus { get; init; }

    /// <summary>
    /// Server-assigned list of blob identifiers for DSN (Delivery Status Notification) blobs. Omitted when constructing a create payload.
    /// </summary>
    [JsonPropertyName("dsnBlobIds")]
    public IReadOnlyList<string>? DsnBlobIds { get; init; }

    /// <summary>
    /// Server-assigned list of blob identifiers for MDN (Message Disposition Notification) blobs. Omitted when constructing a create payload.
    /// </summary>
    [JsonPropertyName("mdnBlobIds")]
    public IReadOnlyList<string>? MdnBlobIds { get; init; }
}
