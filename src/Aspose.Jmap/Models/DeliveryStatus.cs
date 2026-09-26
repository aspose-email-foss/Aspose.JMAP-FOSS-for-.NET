using System.Text.Json.Serialization;

namespace Aspose.Jmap;

/// <summary>
/// Per-recipient delivery outcome, keyed by recipient email address on EmailSubmission.deliveryStatus.
/// </summary>
public class DeliveryStatus
{
    /// <summary>
    /// Gets the SMTP reply string for the recipient.
    /// </summary>
    [JsonPropertyName("smtpReply")]
    public string? SmtpReply { get; init; }

    /// <summary>
    /// Gets the delivery status for the recipient. Possible values: "queued", "yes", "no", "unknown".
    /// </summary>
    [JsonPropertyName("delivered")]
    public string? Delivered { get; init; }

    /// <summary>
    /// Gets the displayed status for the recipient. Possible values: "unknown", "yes".
    /// </summary>
    [JsonPropertyName("displayed")]
    public string? Displayed { get; init; }
}
