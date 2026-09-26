using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Aspose.Jmap;

/// <summary>
/// One node of the Email's MIME bodyStructure tree.
/// </summary>
public sealed class EmailBodyPart
{
    /// <summary>
    /// Gets or sets the part identifier.
    /// </summary>
    [JsonPropertyName("partId")]
    public string? PartId { get; init; }

    /// <summary>
    /// Gets or sets the blob identifier.
    /// </summary>
    [JsonPropertyName("blobId")]
    public string? BlobId { get; init; }

    /// <summary>
    /// Gets or sets the size of the part in octets.
    /// </summary>
    [JsonPropertyName("size")]
    public uint Size { get; init; }

    /// <summary>
    /// Gets or sets the list of headers for this part.
    /// </summary>
    [JsonPropertyName("headers")]
    public IReadOnlyList<EmailHeader> Headers { get; init; } = new List<EmailHeader>();

    /// <summary>
    /// Gets or sets the filename, derived from Content-Disposition or Content-Type.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Gets or sets the MIME type of the part (e.g., "text/plain").
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the character set of the part.
    /// </summary>
    [JsonPropertyName("charset")]
    public string? Charset { get; init; }

    /// <summary>
    /// Gets or sets the disposition of the part (e.g., "inline", "attachment").
    /// </summary>
    [JsonPropertyName("disposition")]
    public string? Disposition { get; init; }

    /// <summary>
    /// Gets or sets the Content-Id of the part.
    /// </summary>
    [JsonPropertyName("cid")]
    public string? Cid { get; init; }

    /// <summary>
    /// Gets or sets the list of languages applicable to this part.
    /// </summary>
    [JsonPropertyName("language")]
    public IReadOnlyList<string>? Language { get; init; }

    /// <summary>
    /// Gets or sets the location of the part.
    /// </summary>
    [JsonPropertyName("location")]
    public string? Location { get; init; }

    /// <summary>
    /// Gets or sets the list of sub‑parts for multipart bodies.
    /// </summary>
    [JsonPropertyName("subParts")]
    public IReadOnlyList<EmailBodyPart>? SubParts { get; init; }
}
