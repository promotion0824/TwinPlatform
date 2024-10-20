namespace Willow.LiveData.TelemetryDataQuality.Options;

/// <summary>
/// Represents the options for TwinsCache.
/// </summary>
internal sealed record TwinsCacheOption
{
    public const string Section = "TwinsCache";

    /// <summary>
    /// Gets the number of hours before the cache should be refreshed.
    /// </summary>
    public int RefreshCacheHours { get; init; } = 4;

    /// <summary>
    /// Gets or sets the page size for retrieving from twins-api.
    /// </summary>
    public int PageSize { get; set; }
}
