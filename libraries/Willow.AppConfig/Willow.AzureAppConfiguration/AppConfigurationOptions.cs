namespace Willow.AzureAppConfiguration;

/// <summary>
/// The app configuration options.
/// </summary>
public class AppConfigurationOptions
{
    /// <summary>
    /// Gets or sets the endpoint for the app configuration.
    /// </summary>
    public string Endpoint { get; set; } = default!;

    /// <summary>
    /// Gets or sets the connection string for the app configuration.
    /// </summary>
    public string? ConnectionString { get; set; } = default!;
}
