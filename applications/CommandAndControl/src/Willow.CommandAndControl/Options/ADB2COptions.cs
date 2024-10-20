namespace Willow.CommandAndControl.Options;

/// <summary>
/// ADB2C Options for client calls.
/// </summary>
public class ADB2COptions
{
    /// <summary>
    /// The name used in app settings file.
    /// </summary>
    public const string CONFIG = "AzureAdB2C";

    /// <summary>
    /// Gets or sets the authority for B2C.
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client ID for B2C.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the known authorities for B2C.
    /// </summary>
    public string[] KnownAuthorities { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the scopes for calling the API from the React App.
    /// </summary>
    public string[] B2CScopes { get; set; } = Array.Empty<string>();
}
