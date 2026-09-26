using System.Text.Json.Serialization;

namespace Aspose.Jmap;

/// <summary>
/// One address in a header such as From/To/Cc (RFC 8621 section 4.1.2.3).
/// </summary>
public class EmailAddress
{
    /// <summary>
    /// The display name of the address, or null if not provided.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The email address (required).
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = default!;
}
