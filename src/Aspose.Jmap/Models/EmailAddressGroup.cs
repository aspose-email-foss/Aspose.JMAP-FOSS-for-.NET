using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Aspose.Jmap;

/// <summary>
/// Group syntax as in From/To headers, e.g. 'Team: a@x, b@x;'.
/// </summary>
public class EmailAddressGroup
{
    /// <summary>
    /// Gets or sets the name of the group. May be null.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Gets or sets the list of email addresses belonging to the group.
    /// </summary>
    [JsonPropertyName("addresses")]
    public IReadOnlyList<EmailAddress> Addresses { get; init; } = System.Array.Empty<EmailAddress>();
}
