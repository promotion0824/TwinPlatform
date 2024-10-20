namespace Willow.Api.Client.Sdk.Marketplace.Dto;

/// <summary>
/// The details of an extension.
/// </summary>
public record ExtensionDetailDto
{
    /// <summary>
    /// Gets the unique identifier for the extension.
    /// </summary>
    public Guid ExtensionId { get; init; }

    /// <summary>
    /// Gets the name of the extension.
    /// </summary>
    public string Name { get; init; } = default!;

    /// <summary>
    /// Gets the status of the extension.
    /// </summary>
    public string Status { get; init; } = default!;

    /// <summary>
    /// Gets the date and time the extension was last udpated.
    /// </summary>
    public DateTime LastUpdatedAtUtc { get; init; }

    /// <summary>
    /// Gets the user who last updated the extension.
    /// </summary>
    public string LastUpdatedBy { get; init; } = default!;

    /// <summary>
    /// Gets the version of the extension.
    /// </summary>
    public string Version { get; init; } = default!;

    /// <summary>
    /// Gets the URI to the manifest file.
    /// </summary>
    public string ManifestFileUri { get; init; } = default!;

    /// <summary>
    /// Gets the metadata for the extension.
    /// </summary>
    public ExtensionMetadataDto Metadata { get; init; } = default!;
}

/// <summary>
/// The metadata for an extension.
/// </summary>
public record ExtensionMetadataDto
{
    /// <summary>
    /// Gets the author of the extension.
    /// </summary>
    public string Author { get; init; } = default!;

    /// <summary>
    /// Gets the changelog for the extension.
    /// </summary>
    public string ChangeLog { get; init; } = default!;

    /// <summary>
    /// Gets the displayname of the extension.
    /// </summary>
    public string DisplayName { get; init; } = default!;

    /// <summary>
    /// Gets the description of the extension.
    /// </summary>
    public string Description { get; init; } = default!;

    /// <summary>
    /// Gets the user who uploaded the extension.
    /// </summary>
    public string UploadedBy { get; init; } = default!;

    /// <summary>
    /// Gets when the extension was uploaded.
    /// </summary>
    public DateTime UploadedAtUtc { get; init; }

    /// <summary>
    /// Gets the logo of the extension.
    /// </summary>
    public string? Logo { get; init; }

    /// <summary>
    /// Gets the thumbnail of the extension.
    /// </summary>
    public string? Thumbnail { get; init; }
}
