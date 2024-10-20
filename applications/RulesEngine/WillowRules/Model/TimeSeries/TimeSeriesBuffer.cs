using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

// POCO class serialized to DB
#nullable disable
namespace Willow.Rules.Model;

/// <summary>
/// A buffered window of time series values
/// </summary>
public class TimeSeriesBuffer
{
	/// <summary>
	/// The maximum age of time series values to keep
	/// </summary>
	/// <remarks>
	/// This is enforced only on save to database
	/// </remarks>
	public TimeSpan? MaxTimeToKeep { get; set; }

	/// <summary>
	/// The maximum number of time series values to keep
	/// </summary>
	/// <remarks>
	/// This is enforced only on save to database
	/// </remarks>
	public int? MaxCountToKeep { get; set; }

	/// <summary>
	/// Unit of measure
	/// </summary>
	public string UnitOfMeasure { get; set; }

	/// <summary>
	/// Get or set all the points
	/// </summary>
	public IEnumerable<TimedValue> Points
	{
		get { return this.points; }
		set { this.points = new List<TimedValue>(value); }
	}

	/// <summary>
	/// Gets the points in reverse order (efficiently)
	/// </summary>
	[JsonIgnore]
	public IEnumerable<TimedValue> PointsReversed
	{
		get { for (int i = points.Count - 1; i >= 0; i--) yield return points[i]; }
	}

	/// <summary>
	/// Buffer used to keep a set of <see cref="TTimedValue"/>s limited to <see cref="MaxCountToKeep"/> or <see cref="MaxTimeToKeep"/>
	/// </summary>
	protected List<TimedValue> points = new();

	/// <summary>
	/// Count of elements in buffer
	/// </summary>
	public int Count => points.Count;

	/// <summary>
	/// The compression state used by a trajectory compressor
	/// </summary>
	public TrajectoryCompressorState CompressionState { get; set; }

	/// <summary>
	/// The gap between the last value and the one before
	/// </summary>
	/// <remarks>
	/// When we reset a TimeSeries we have to rebuild the last gap but the data is compressed
	/// so we have to track the last gap on each compressed point too so we can use that
	/// </remarks>
	public TimeSpan LastGap { get; set; } = TimeSpan.Zero;

	/// <summary>
	/// Get last point double value
	/// </summary>
	public double? GetLastValueDouble()
	{
		return LastOrDefault().ValueDouble;
	}

	/// <summary>
	/// Get last point bool value
	/// </summary>
	public bool? GetLastValueBool()
	{
		return LastOrDefault().ValueBool;
	}

	/// <summary>
	/// Get last point text value
	/// </summary>
	public string GetLastValueText()
	{
		return LastOrDefault().ValueText;
	}

	/// <summary>
	/// Get last point timestamp
	/// </summary>
	public DateTimeOffset GetLastSeen()
	{
		return LastOrDefault().Timestamp;
	}

	/// <summary>
	/// Get last point timestamp
	/// </summary>
	public DateTimeOffset GetFirstSeen()
	{
		return FirstOrDefault().Timestamp;
	}

	/// <summary>
	/// Gets the last value which is much faster than the Last() extension for anthing other than an IList
	/// </summary>
	public TimedValue Last()
	{
		return points.Last();
	}

	/// <summary>
	/// Gets the last value or the default
	/// </summary>
	public TimedValue LastOrDefault()
	{
		if (points.Count > 0)
		{
			return points[points.Count - 1];
		}

		return default;
	}

	/// <summary>
	/// Gets the first value or the default
	/// </summary>
	public TimedValue FirstOrDefault()
	{
		if (points.Count > 0)
		{
			return points[0];
		}

		return default;
	}

	/// <summary>
	/// Indicates whether this timeseries is for a capability
	/// </summary>
	public bool IsCapability()
	{
		return this is TimeSeries;
	}

	/// <summary>
	/// Singleton default trajectory compressor
	/// </summary>
	protected static TrajectoryCompressor defaultTrajectoryCompressor = new TrajectoryCompressor(0.05);

	public bool AddPoint(
		in TimedValue newValue,
		bool applyCompression,
		bool reApplyCompression = true,
		double? compression = null)
	{
		return AddPoint(in newValue, applyCompression, compression.HasValue ? new TrajectoryCompressor(compression.Value) : defaultTrajectoryCompressor, reApplyCompression);
	}

	/// <summary>
	/// Adds a point to the buffer
	/// </summary>
	/// <remarks>
	/// We always keep the last value that is prior to the start date so we can interpolate
	/// right up to the start date. If there is only ever one of a value we will still keep it.
	/// </remarks>
	public bool AddPoint(
	in TimedValue newValue,
	bool applyCompression,
	TrajectoryCompressor compressor,
	bool reApplyCompression = true)
	{
		if (!IsValidIncomingPoint(newValue))
		{
			return false;
		}

		if (applyCompression && reApplyCompression)
		{
			//every 24hrs
			if (points.LastOrDefault().Timestamp.Date != newValue.Timestamp.Date)
			{
				ReApplyCompression();
			}
		}

		// Remove anything after this timestamp IF we have gone backward in time
		if (this.points.Count > 0 && this.points.Last().Timestamp > newValue.Timestamp)
		{
			if (this.points.First().Timestamp > newValue.Timestamp)
			{
				// Entire set is beyond the new start time
				this.points.Clear();
			}
			else
			{
				DateTimeOffset timestamp = newValue.Timestamp;
				// Prune the set one by one, removing anything past this point
				points.RemoveAll(v => v.Timestamp > timestamp);
				TrimExcess();
			}

			this.CompressionState = new TrajectoryCompressorState();  // and reset the compression state
		}

		if (this.points.Count > 0)
		{
			var last = points.Last();

			if(last.IsTheSame(newValue) || last.Timestamp == newValue.Timestamp)
			{
				return false;
			}

			LastGap = newValue.Timestamp - last.Timestamp;
		}

		if (applyCompression && this.points.Count > 0)
		{
			if (this.CompressionState is null)
			{
				this.CompressionState = new TrajectoryCompressorState();

				foreach (var current in this.points.ToList())
				{
					compressor.Add(this.CompressionState, current.Timestamp, current.NumericValue,
						(d, v) => { },
						(d, u, v) => { this.points.Remove(current); });  // next replaces current so simply remove current
				}
			}

			bool add = false;
			bool update = false;
			// Either add it to the end or replace the last value
			// can't pass "in" params to anonymous methods
			compressor.Add(this.CompressionState, newValue.Timestamp, newValue.NumericValue,
				(d, v) =>
				{
					add = true;
				},
				(d, u, v) =>
				{
					update = true;
				});

			if (add)
			{
				EnsureCapacity();
				this.points.Add(newValue);
			}
			else if (update)
			{
				this.points[points.Count - 1] = newValue;
			}
		}
		else
		{
			EnsureCapacity();
			// no compression
			this.points.Add(newValue);
		}

		return true;
	}

	/// <summary>
	/// Tries to get the last and previous points
	/// </summary>
	public bool TryGetLastAndPrevious(out TimedValue lastValue, out TimedValue previousValue)
	{
		lastValue = default;
		previousValue = default;

		if (Count > 1)
		{
			lastValue = points[points.Count - 1];
			previousValue = points[points.Count - 2];
			return true;
		}

		return false;
	}

	/// <summary>
	/// Gets the last real delta between points avoiding compression
	/// </summary>
	public double GetLastDelta()
	{
		if (Count < 2) return 0.0;

		if (this.CompressionState is not null)
		{
			return this.CompressionState.LastDelta;
		}

		if (TryGetLastAndPrevious(out var last, out var previous))
		{
			return last.NumericValue - previous.NumericValue;
		}

		return 0.0;
	}

	/// <summary>
	/// Gets the last real delta time between points avoiding compression
	/// </summary>
	public double GetLastDeltaTime()
	{
		if (Count < 2) return 0.0;

		if (this.CompressionState is not null)
		{
			return this.CompressionState.LastDeltaTime;
		}

		if (TryGetLastAndPrevious(out var last, out var previous))
		{
			return (last.Timestamp - previous.Timestamp).TotalSeconds;
		}

		return 0.0;
	}

	/// <summary>
	/// Gets a range of values from the specified start date
	/// </summary>
	public IEnumerable<TimedValue> GetRange(DateTimeOffset start, DateTimeOffset end)
	{
		var endIndex = -1;

		//find the range in reverse order. Chances are it'll be faster
		for (var i = points.Count - 1; i >= 0; i--)
		{
			var value = points[i];

			if (value.Timestamp <= end && value.Timestamp >= start)
			{
				endIndex = i;
				break;
			}
		}

		var startIndex = endIndex;

		for (var i = endIndex - 1; i >= 0; i--)
		{
			var value = points[i];

			if (value.Timestamp >= start)
			{
				startIndex = i;
			}
			else
			{
				break;
			}
		}

		return GetRange(startIndex, endIndex);
	}

	private IEnumerable<TimedValue> GetRange(int startindex, int endIndex)
	{
		if (endIndex < 0)
		{
			yield break;
		}

		for (var i = startindex; i <= endIndex; i++)
		{
			yield return points[i];
		}
	}

	/// <summary>
	/// Re-sorts the list by the points' time stamp
	/// </summary>
	public void Sort()
	{
		points.Sort();
	}

	/// <summary>
	/// Apply limits to points based on now and default max values
	/// </summary>
	public int ApplyLimits(DateTime now, TimeSpan defaultMaxTimeToKeep, TimeSpan timeCap, bool canRemoveAllPoints = true)
	{
		DateTime minDate = now - defaultMaxTimeToKeep;

		if (MaxTimeToKeep is not null)
		{
			minDate = now.Subtract(MaxTimeToKeep.Value);

			if (now - minDate > timeCap)
			{
				minDate = now - timeCap;
			}
		}

		return ApplyLimits(MaxCountToKeep, minDate, canRemoveAllPoints);
	}

	/// <summary>
	/// Apply limits to the points
	/// </summary>
	/// <returns>
	/// Count of points removed
	/// </returns>
	public int ApplyLimits(int? maxCapacity, DateTimeOffset? minDate, bool canRemoveAllPoints = true)
	{
		int removed = 0;

		// If the first point is stupidly far before the minDate, simply chop it off.
		// We have had cases where one point was left hanging about and the two-points
		// logic below didn't ever remove it
		// This also occurs on Actor timeseries where old variables that aren't used anymore hangs around with one point months ago

		int minCount = canRemoveAllPoints ? 0 : 2;

		while (points.Count > minCount
			&& minDate.HasValue
			&& points[0].Timestamp.AddDays(7) < minDate.Value
			)
		{
			this.points.RemoveAt(0);
			removed++;
		}

		// aways keep at least two and at least one before the minDate
		if (minDate.HasValue)
		{
			while (points.Count > 2 && points[1].Timestamp < minDate!.Value)
			{
				points.RemoveAt(0);
				removed++;
			}
		}

		//we can't apply default max counts as this could prune timeseries as temporals
		//and potentially never activate the rest of the expressions becuase of insufficient data
		if (maxCapacity.HasValue)
		{
			// but also guard against a stupidly high number of values being retained
			int maxAllowed = maxCapacity ?? 2500;
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
	/// Sets the max buffer in time if the incoming value is greater than the current
	/// </summary>
	public void SetMaxBufferTime(TimeSpan maxTime)
	{
		if (maxTime > MaxTimeToKeep.GetValueOrDefault(TimeSpan.MinValue) && maxTime > TimeSpan.Zero)
		{
			MaxTimeToKeep = maxTime;
		}
	}

	/// <summary>
	/// Sets the max buffer count
	/// </summary>
	public void SetMaxBufferCount(int? maxCountToKeep)
	{
		MaxCountToKeep = maxCountToKeep;
	}

	/// <summary>
	/// Removes points after a certain date
	/// </summary>
	/// <remarks>
	/// Used when a time series buffer will be working with items it already worked with before
	/// otherwise the list will contain duplicates
	/// </remarks>
	/// <param name="date"></param>
	public void RemovePointsAfter(DateTimeOffset date)
	{
		var result = points.RemoveAll(v => v.Timestamp > date);

		if (result != 0)
		{
			ResetState();
		}
	}

	/// <summary>
	/// Debug method to check the time series is in order
	/// </summary>
	public bool CheckTimeSeriesIsInOrder()
	{
		if (points.Count <= 1)
		{
			return true;
		}

		var node = points.First();

		foreach (var nextNode in points.Skip(1))
		{
			if (node.Timestamp > nextNode.Timestamp)
			{
				return false;
			}
			node = nextNode;
		}

		return true;
	}

	/// <summary>
	/// Re-crompress the buffer using different compression rates depending on the age of each point
	/// </summary>
	private void ReApplyCompression()
	{
		if (!Points.Any())
		{
			return;
		}

		var lastSeen = Points.Last().Timestamp;
		if (lastSeen - points.First().Timestamp < TimeSpan.FromDays(15))
		{
			return;
		}

		var currentState = CompressionState;

		CompressionState = null;

		var pointsCopy = points.ToArray();

		points.Clear();

		TrajectoryCompressor trajectoryCompressor = new TrajectoryCompressor(0.05);

		foreach (var point in pointsCopy)
		{
			//these values are already in the correct (default) compressions
			if (lastSeen - point.Timestamp < TimeSpan.FromDays(15))
			{
				points.Add(point);
				continue;
			}

			double compression = 0.05;

			var gap = lastSeen - point.Timestamp;

			// For anchored temporal the compression under 60 days should not be too aggressive, e.g. MAX([var], 1Mth, -1Mth)
			if (gap > TimeSpan.FromDays(60))
			{
				compression = 5;
			}
			else if (gap > TimeSpan.FromDays(31))
			{
				compression = 1;
			}
			else if (gap > TimeSpan.FromDays(15))
			{
				compression = 0.5;
			}

			if (trajectoryCompressor.Percentage != compression)
			{
				trajectoryCompressor = new TrajectoryCompressor(compression);
			}

			AddPoint(in point, applyCompression: true, trajectoryCompressor);
		}

		EnsureCapacity();

		//keep existing state for future values
		CompressionState = currentState;
	}

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

		if (newValue.ValueDouble.HasValue)
		{
			if (double.IsPositiveInfinity(newValue.ValueDouble.Value)
			 || double.IsNegativeInfinity(newValue.ValueDouble.Value)
			 || double.IsNaN(newValue.ValueDouble.Value))
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Make sure there is room for one more element by growing buffer in steps of 100 not doubling
	/// </summary>
	private void EnsureCapacity()
	{
		if (points.Capacity == points.Count)
		{
			//for large buffers, increment by % which is less re-allocations
			int bufferIncrement = Math.Max(5, (int)(points.Count * 0.05));
			points.Capacity = points.Capacity + bufferIncrement;
		}
	}

	/// <summary>
	/// Reset internal state
	/// </summary>
	private void ResetState()
	{
		CompressionState = null;
		LastGap = TimeSpan.Zero;
	}

	/// <summary>
	/// If there's too much room, trim it back
	/// </summary>
	private void TrimExcess()
	{
		points.TrimExcess();
	}
}
