namespace Willow.LiveData.Core.Domain;

using System;

/// <summary>
/// Represents time series data.
/// </summary>
public abstract class TimeSeriesData
{
    /// <summary>
    /// Gets or sets the timestamp of the time series data.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Creates a new instance of the current TimeSeriesData object with the same property values.
    /// </summary>
    /// <returns>A new instance of the current TimeSeriesData object.</returns>
    public virtual object Clone()
    {
        return this.MemberwiseClone();
    }
}
