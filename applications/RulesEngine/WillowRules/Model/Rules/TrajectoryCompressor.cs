using System;
using WillowRules.Filters;

namespace Willow.Rules.Model;

/// <summary>
/// Trajectory compression using narrowing cones
/// </summary>
/// <remarks>
/// Removes mid-points for colinear sequences, resets when the curve changes direction
/// which captures the shape of the curve accurately and preserves the approximate
/// area under the curve.
///
/// Code donated by Ian Mercer: https://blog.abodit.com/posts/2022-01-26-TimeSeriesCompression/
/// No ackowledgement necessary.
/// </remarks>
public class TrajectoryCompressor
{
	/// <summary>
	/// Allowed deviation from sriaght line is a percentage of the variance of the signal
	/// </summary>
	public double Percentage { get; }

	/// <summary>
	/// Creates a new <see cref="TrajectoryCompressor" />
	/// </summary>
	/// <param name="percentage">Try 0.05 for a 5% variance allowed from a straight line</param>
	public TrajectoryCompressor(double percentage)
	{
		this.Percentage = percentage;
	}

	public void Add(TrajectoryCompressorState state,
				DateTimeOffset timestamp, double value,
				Action<DateTimeOffset, double> write,
				Action<DateTimeOffset, DateTimeOffset, double> update)
	{
		// Keep track of the last delta regardless of any compression going on
		// this is used by the DELTA() and DELTA_TIME functions
		state.LastDelta = state.hasPrevious ? value - state.previousValue : 0.0;
		state.LastDeltaTime = state.hasPrevious ? (timestamp - state.previousTime).TotalSeconds : 0.0;
		// Keep track of the RMS amplitude from the mean (i.e. the variance),
		// averaged over time so that percentage can be applied
		// to an average amplitude and not an instantaneous value
		state.Count++;
		state.Sum += value;
		state.SumSquare += value * value;

		double rmsEstimate = Math.Sqrt(state.Variance);

		// State has never been initialized, we have no points in the compressor
		if (state.startTime.Year < 2000)
		{
			write(timestamp, value);
			state.startTime = timestamp;
			state.startValue = value;
			state.hasPrevious = false;
			return;
		}

		// Add allowed error to value using percentage applied to average amplitude
		double upper_estimate = value + rmsEstimate * this.Percentage;
		double lower_estimate = value - rmsEstimate * this.Percentage;

		// Do we have two points in the compressor, if not ...
		if (!state.hasPrevious)
		{
			write(timestamp, value);
			state.hasPrevious = true;
			state.previousTime = timestamp;
			state.previousValue = value;

			// and calculate initial min max slope
			state.upper_slope = (upper_estimate - state.startValue) / (timestamp - state.startTime).TotalMilliseconds;
			state.lower_slope = (lower_estimate - state.startValue) / (timestamp - state.startTime).TotalMilliseconds;

			return;
		}

		if (timestamp == state.previousTime)
		{
			// ignore simultaneous values
			return;
		}

		var duration = (timestamp - state.startTime);
		var durationMs = duration.TotalMilliseconds;

		// Use the current slope and start value to calculate the upper and lower bound allowed
		double upper_bound = durationMs * state.upper_slope + state.startValue;
		double lower_bound = durationMs * state.lower_slope + state.startValue;

		bool outside = (value > upper_bound || value < lower_bound);

		bool tooLong = duration > TimeSpan.FromHours(12);

		if (outside || tooLong)
		{
			// The new point is outside the cone of allowed values
			// so we allow previousValue to stand and start a new segment
			state.startTime = state.previousTime;
			state.startValue = state.previousValue;
			state.previousTime = timestamp;
			state.previousValue = value;
			state.hasPrevious = true;
			write(timestamp, value);          // And write out the current value

			// Calculate the new duration for just these two points, not all the way back to start
			duration = (timestamp - state.startTime);
			durationMs = duration.TotalMilliseconds;

			// and calculate new min max slope
			state.upper_slope = (upper_estimate - state.startValue) / durationMs;
			state.lower_slope = (lower_estimate - state.startValue) / durationMs;

			return;
		}

		// value is within expected range, it can replace previous
		update(state.previousTime, timestamp, value);

		// Keep track of it so we can keep updating
		state.previousTime = timestamp;
		state.previousValue = value;

		// and then narrow the allowed range
		if (upper_estimate < upper_bound)
			state.upper_slope = (upper_estimate - state.startValue) / durationMs;
		if (lower_estimate > lower_bound)
			state.lower_slope = (lower_estimate - state.startValue) / durationMs;
	}
}
