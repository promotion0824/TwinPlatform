namespace Willow.LiveData.Core.Domain;

/// <summary>
/// Represents time series binary data.
/// </summary>
public class TimeSeriesBinaryData : TimeSeriesData
{
    /// <summary>
    /// Gets or sets the On count.
    /// </summary>
    public int OnCount { get; set; }

    /// <summary>
    /// Gets or sets the Off count.
    /// </summary>
    public int OffCount { get; set; }
}
