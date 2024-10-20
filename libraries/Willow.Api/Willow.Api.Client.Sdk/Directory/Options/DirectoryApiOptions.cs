namespace Willow.Api.Client.Sdk.Directory.Options;

/// <summary>
/// The options for the Directory API.
/// </summary>
public class DirectoryApiOptions : IApiOptions
{
    /// <summary>
    /// Gets or sets the base address for the Directory API.
    /// </summary>
    public string BaseAddress { get; set; } = default!;
}
