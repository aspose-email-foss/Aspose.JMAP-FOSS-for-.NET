using System.Text.Json.Serialization;

namespace Aspose.Jmap;

/// <summary>
/// Represents an email header as defined by the JMAP Mail specification.
/// </summary>
public class EmailHeader
{
    /// <summary>
    /// Gets or sets the name of the header.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Gets or sets the value of the header.
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; set; } = default!;
}
