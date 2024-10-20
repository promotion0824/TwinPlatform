namespace Willow.LiveData.TelemetryDataQuality.Options;

/// <summary>
/// Represents the options for TimeSeries Persistence.
/// </summary>
internal sealed record TimeSeriesPersistenceOption
{
    public const string Section = "TimeSeriesPersistence";

    /// <summary>
    /// Gets the number of hours between persisting to the database.
    /// </summary>
    public int IntervalInHours { get; init; } = 12;
}
