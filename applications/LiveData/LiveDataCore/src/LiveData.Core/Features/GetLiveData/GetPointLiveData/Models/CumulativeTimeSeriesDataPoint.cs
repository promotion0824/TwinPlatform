namespace Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData;

/// <summary>
/// Cumulative time series data point.
/// </summary>
public class CumulativeTimeSeriesDataPoint : TimeSeriesDataPoint
{
    /// <summary>
    /// Gets or sets a value indicating whether the data point is interpolated.
    /// </summary>
    public bool IsInterpolated { get; set; }
}
