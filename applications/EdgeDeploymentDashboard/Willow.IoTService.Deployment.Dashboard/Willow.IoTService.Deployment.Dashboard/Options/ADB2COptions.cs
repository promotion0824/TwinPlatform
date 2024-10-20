namespace Willow.IoTService.Deployment.Dashboard.Options;

/// <summary>
///     ADB2C Options for client calls.
/// </summary>
public class ADB2COptions
{
    /// <summary>
    ///     Name used in app settings file.
    /// </summary>
    public const string CONFIG = "AzureAdB2C";

    /// <summary>
    ///     Gets or sets the Authority for B2C.
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets Client Id for B2C.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the Known authorities for B2C.
    /// </summary>
    public string[] KnownAuthorities { get; set; } = Array.Empty<string>();

    /// <summary>
    ///     Gets or sets the Scopes for calling the API from the React App.
    /// </summary>
    public string[] B2CScopes { get; set; } = Array.Empty<string>();
}
