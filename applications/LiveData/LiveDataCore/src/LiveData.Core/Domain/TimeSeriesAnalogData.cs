namespace Willow.LiveData.Core.Domain;

/// <summary>
/// Represents time series analog data.
/// </summary>
public class TimeSeriesAnalogData : TimeSeriesData
{
    /// <summary>
    /// Gets or sets the average value.
    /// </summary>
    public decimal? Average { get; set; }

    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    public decimal? Minimum { get; set; }

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    public decimal? Maximum { get; set; }
}
