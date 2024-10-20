namespace Willow.FeatureFlagsProvider.FeatureFlags;

/// <summary>
/// The config cat settings.
/// </summary>
public class ConfigCatSettings
{
    /// <summary>
    /// Gets or sets the config cat api key.
    /// </summary>
    public string SdkKey { get; set; } = default!;

    /// <summary>
    /// Gets or sets the polling interval in seconds.
    /// </summary>
    public uint PollingIntervalInSeconds { get; set; }
}
