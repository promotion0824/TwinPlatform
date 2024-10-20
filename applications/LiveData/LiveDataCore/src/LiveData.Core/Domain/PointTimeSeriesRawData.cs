namespace Willow.LiveData.Core.Domain;

using System;

/// <summary>
/// Represents raw data for a point time series.
/// </summary>
public class PointTimeSeriesRawData : TimeSeriesRawData
{
    /// <summary>
    /// Gets or sets the PointEntityId (TrendId).
    /// </summary>
    public Guid PointEntityId { get; set; }
}
