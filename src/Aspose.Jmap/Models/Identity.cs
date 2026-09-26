using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Aspose.Jmap;

/// <summary>
/// A sending identity (name/email/replyTo used as the From when submitting mail); analogous to configuring a MailAddress + display name on Aspose's SmtpClient.
/// </summary>
public class Identity
{
    /// <summary>
    /// Server-assigned identifier. Omitted when constructing a create payload.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// The display name for the identity. Default is an empty string.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The email address for the identity. Required.
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Optional list of reply-to addresses.
    /// </summary>
    [JsonPropertyName("replyTo")]
    public IReadOnlyList<EmailAddress>? ReplyTo { get; set; }

    /// <summary>
    /// Optional list of blind carbon copy (BCC) addresses.
    /// </summary>
    [JsonPropertyName("bcc")]
    public IReadOnlyList<EmailAddress>? Bcc { get; set; }

    /// <summary>
    /// Text signature appended to outgoing messages. Default is an empty string.
    /// </summary>
    [JsonPropertyName("textSignature")]
    public string TextSignature { get; set; } = string.Empty;

    /// <summary>
    /// HTML signature appended to outgoing messages. Default is an empty string.
    /// </summary>
    [JsonPropertyName("htmlSignature")]
    public string HtmlSignature { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the identity may be deleted. Server-assigned; omitted when constructing a create payload.
    /// </summary>
    [JsonPropertyName("mayDelete")]
    public bool? MayDelete { get; init; }
}
