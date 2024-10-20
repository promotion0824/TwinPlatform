namespace Willow.Caching.Telemetry.DataQuality.Models.TimeSeries;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A buffered window of time series values.
/// </summary>
/// <remarks>Original code source from Activate Technology code base.</remarks>
public class TimeSeriesBuffer
{
    /// <summary>
    /// Buffer used to keep a set of <see cref="TimedValue"/>s limited to <see cref="MaxCountToKeep"/> or <see cref="MaxTimeToKeep"/>.
    /// </summary>
    protected List<TimedValue> points = [];

    /// <summary>
    /// Gets count of elements in buffer.
    /// </summary>
    public int Count => points.Count;

    /// <summary>
    /// Gets or sets the maximum age of time series values to keep.
    /// </summary>
    /// <remarks>
    /// This is enforced only on save to database.
    /// </remarks>
    public TimeSpan? MaxTimeToKeep { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of time series values to keep.
    /// </summary>
    /// <remarks>
    /// This is enforced only on save to database.
    /// </remarks>
    public int? MaxCountToKeep { get; set; }

    /// <summary>
    /// Gets or sets unit of measure.
    /// </summary>
    public string UnitOfMeasure { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets all the points.
    /// </summary>
    public IEnumerable<TimedValue> Points
    {
        get => this.points;
        set => this.points = [..value];
    }

    /// <summary>
    /// Gets or sets the gap between the last value and the one before.
    /// </summary>
    public TimeSpan LastGap { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Get the timestamp of the last point in the TimeSeriesBuffer.
    /// </summary>
    /// <returns>The timestamp of the last point.</returns>
    public virtual DateTimeOffset GetLastSeen()
    {
        return LastOrDefault().Timestamp;
    }

    /// <summary>
    /// Get the timestamp of the first point in the time series buffer.
    /// </summary>
    /// <returns>The timestamp of the first point, or the default value if no points exist.</returns>
    public virtual DateTimeOffset GetFirstSeen()
    {
        return FirstOrDefault().Timestamp;
    }

    /// <summary>
    /// Gets the last value in the points' collection.
    /// </summary>
    /// <returns>The last TimedValue object in the points' collection.</returns>
    public virtual TimedValue Last()
    {
        return points.Last();
    }

    /// <summary>
    /// Gets the last value or the default.
    /// </summary>
    /// <returns>The last value in the TimeSeriesBuffer, or the default value if the buffer is empty.</returns>
    public virtual TimedValue LastOrDefault()
    {
        return points.Count > 0 ? points[^1] : default;
    }

    /// <summary>
    /// Gets the first value or the default.
    /// </summary>
    /// <returns>The first value in the points list, or the default value if the points list is empty.</returns>
    public virtual TimedValue FirstOrDefault()
    {
        return points.Count > 0 ? points[0] : default;
    }
}
