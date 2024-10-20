namespace Willow.LiveData.TelemetryDataQuality.Models.TimeSeries;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A buffered window of time series values.
/// </summary>
/// <remarks>Original code source from Activate Technology code base.</remarks>
internal class TimeSeriesBuffer
{
    /// <summary>
    /// Buffer used to keep a set of <see cref="TimedValue"/>s limited to <see cref="MaxCountToKeep"/> or <see cref="MaxTimeToKeep"/>.
    /// </summary>
    private List<TimedValue> points = [];

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
    public DateTimeOffset GetLastSeen()
    {
        return LastOrDefault().Timestamp;
    }

    /// <summary>
    /// Get the timestamp of the first point in the time series buffer.
    /// </summary>
    /// <returns>The timestamp of the first point, or the default value if no points exist.</returns>
    public DateTimeOffset GetFirstSeen()
    {
        return FirstOrDefault().Timestamp;
    }

    /// <summary>
    /// Gets the last value in the points' collection.
    /// </summary>
    /// <returns>The last TimedValue object in the points' collection.</returns>
    public TimedValue Last()
    {
        return points.Last();
    }

    /// <summary>
    /// Gets the last value or the default.
    /// </summary>
    /// <returns>The last value in the TimeSeriesBuffer, or the default value if the buffer is empty.</returns>
    private TimedValue LastOrDefault()
    {
        return points.Count > 0 ? points[^1] : default;
    }

    /// <summary>
    /// Gets the first value or the default.
    /// </summary>
    /// <returns>The first value in the points list, or the default value if the points list is empty.</returns>
    private TimedValue FirstOrDefault()
    {
        return points.Count > 0 ? points[0] : default;
    }

    /// <summary>
    /// Adds a new point to the TimeSeriesBuffer.
    /// </summary>
    /// <param name="newValue">The new point to be added.</param>
    /// <returns>True if the point is successfully added, otherwise false.</returns>
    protected bool AddPoint(in TimedValue newValue)
    {
        if (!IsValidIncomingPoint(newValue))
        {
            return false;
        }

        // Remove anything after this timestamp IF we have gone backward in time
        if (points.Count > 0 && points.Last().Timestamp > newValue.Timestamp)
        {
            if (points.First().Timestamp > newValue.Timestamp)
            {
                // Entire set is beyond the new start time
                points.Clear();
            }
            else
            {
                var timestamp = newValue.Timestamp;

                // Prune the set one by one, removing anything past this point
                points.RemoveAll(v => v.Timestamp > timestamp);
                TrimExcess();
            }
        }

        if (points.Count > 0)
        {
            var last = points.Last();

            if (IsTheSame(last, newValue) || last.Timestamp == newValue.Timestamp)
            {
                return false;
            }

            LastGap = newValue.Timestamp - last.Timestamp;
        }

        EnsureCapacity();

        // no compression
        points.Add(newValue);

        return true;
    }

    /// <summary>
    /// Apply limits to the points in the TimeSeriesBuffer.
    /// </summary>
    /// <param name="now">The current timestamp.</param>
    /// <param name="defaultMaxTimeToKeep">The default maximum time to keep points.</param>
    /// <param name="canRemoveAllPoints">Whether all points can be removed.</param>
    /// <returns>The count of points removed.</returns>
    public int ApplyLimits(DateTime now,
        TimeSpan defaultMaxTimeToKeep,
        bool canRemoveAllPoints = true)
    {
        var minDate = now - defaultMaxTimeToKeep;

        if (this.MaxTimeToKeep is null)
        {
            return this.ApplyLimits(this.MaxCountToKeep, minDate, canRemoveAllPoints);
        }

        minDate = now.Subtract(this.MaxTimeToKeep.Value);

        return ApplyLimits(MaxCountToKeep, minDate, canRemoveAllPoints);
    }

    /// <summary>
    /// Apply limits to the TimeSeriesBuffer by removing points that exceed the specified constraints.
    /// </summary>
    /// <param name="maxCapacity">Maximum capacity to keep.</param>
    /// <param name="minDate">Minimum date before which data can be removed.</param>
    /// <param name="canRemoveAllPoints">Specifies whether all points can be removed if necessary. Default is true.</param>
    /// <returns>The number of points removed.</returns>
    private int ApplyLimits(int? maxCapacity, DateTimeOffset? minDate, bool canRemoveAllPoints = true)
    {
        var removed = 0;

        // If the first point is stupidly far before the minDate, simply chop it off.
        // We have had cases where one point was left hanging about and the two-points
        // logic below didn't ever remove it
        // This also occurs on Actor timeseries where old variables that aren't used anymore hangs around with one point months ago
        var minCount = canRemoveAllPoints ? 0 : 2;

        while (points.Count > minCount
               && minDate.HasValue
               && points[0].Timestamp.AddDays(7) < minDate.Value)
        {
            points.RemoveAt(0);
            removed++;
        }

        // always keep at least two and at least one before the minDate
        if (minDate.HasValue)
        {
            while (points.Count > 2 && points[1].Timestamp < minDate.Value)
            {
                points.RemoveAt(0);
                removed++;
            }
        }

        //we can't apply default max counts as this could prune timeseries as temporals
        //and potentially never activate the rest of the expressions because of insufficient data
        if (maxCapacity.HasValue)
        {
            // but also guard against a stupidly high number of values being retained
            var maxAllowed = maxCapacity;
            while (points.Count > 2 && points.Count > maxAllowed)
            {
                points.RemoveAt(0);
                removed++;
            }
        }

        if (removed > 0)
        {
            // Reduce the size of the List<T> if possible
            TrimExcess();
        }

        return removed;
    }

    /// <summary>
    /// Sets the maximum buffer time if the incoming value is greater than the current.
    /// </summary>
    /// <param name="maxTime">The maximum buffer time to set.</param>
    public void SetMaxBufferTime(TimeSpan maxTime)
    {
        if (maxTime > MaxTimeToKeep.GetValueOrDefault(TimeSpan.MinValue) && maxTime > TimeSpan.Zero)
        {
            MaxTimeToKeep = maxTime;
        }
    }

    /// <summary>
    /// Sets the maximum number of points to keep in the buffer.
    /// </summary>
    /// <param name="maxCountToKeep">The maximum number of points to keep.</param>
    public void SetMaxBufferCount(int? maxCountToKeep)
    {
        MaxCountToKeep = maxCountToKeep;
    }

    /// <summary>
    /// Determines whether an incoming point is valid for adding to the TimeSeriesBuffer.
    /// </summary>
    /// <param name="newValue">The incoming point to validate.</param>
    /// <returns>True if the point is valid, otherwise false.</returns>
    protected static bool IsValidIncomingPoint(TimedValue newValue)
    {
        var timestamp = newValue.Timestamp;

        if (timestamp == DateTimeOffset.MinValue)
        {
            // Don't add bogus values to the buffer
            return false;
        }

        if (timestamp == DateTimeOffset.MaxValue)
        {
            // Don't add bogus values to the buffer
            return false;
        }

        if (!string.IsNullOrEmpty(newValue.ValueText))
        {
            // String values are not supported by downstream apps
            return false;
        }

        if (!newValue.ValueDouble.HasValue)
        {
            return true;
        }

        return !double.IsPositiveInfinity(newValue.ValueDouble.Value)
               && !double.IsNegativeInfinity(newValue.ValueDouble.Value)
               && !double.IsNaN(newValue.ValueDouble.Value);
    }

    /// <summary>
    /// Make sure there is room for one more element by growing buffer in steps of 100 not doubling.
    /// </summary>
    private void EnsureCapacity()
    {
        if (points.Capacity != points.Count)
        {
            return;
        }

        //for large buffers, increment by % which is less re-allocations
        var bufferIncrement = Math.Max(5, (int)(points.Count * 0.05));
        this.points.Capacity += bufferIncrement;
    }

    /// <summary>
    /// If there's too much room, trim it back.
    /// </summary>
    private void TrimExcess()
    {
        points.TrimExcess();
    }

    /// <summary>
    /// Checks whether the provided point is of similar attributes as this point indicating it is a duplicate.
    /// </summary>
    private static bool IsTheSame(TimedValue value, TimedValue newValue)
    {
        return value.Timestamp == newValue.Timestamp &&
               value.BoolValue == newValue.BoolValue &&
               value.ValueDouble == newValue.ValueDouble;
    }
}
