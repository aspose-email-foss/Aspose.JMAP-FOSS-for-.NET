using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Aspose.Jmap;

/// <summary>
/// SMTP MAIL FROM / RCPT TO envelope for a submission, distinct from the message's own From/To headers.
/// </summary>
public class Envelope
{
    /// <summary>
    /// The SMTP MAIL FROM address.
    /// </summary>
    [JsonPropertyName("mailFrom")]
    public Address MailFrom { get; init; } = null!;

    /// <summary>
    /// The SMTP RCPT TO addresses.
    /// </summary>
    [JsonPropertyName("rcptTo")]
    public IReadOnlyList<Address> RcptTo { get; init; } = null!;
}

/// <summary>
/// Represents an address used in an envelope, including optional SMTP parameters.
/// </summary>
public class Address
{
    /// <summary>
    /// The email address.
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; init; } = null!;

    /// <summary>
    /// SMTP MAIL/RCPT parameters, e.g. {"RET": "HDRS"}.
    /// </summary>
    [JsonPropertyName("parameters")]
    public IReadOnlyDictionary<string, string?>? Parameters { get; init; }
}
