using System;
using WillowRules.Filters;

namespace Willow.Rules.Model;

/// <summary>
/// State classes provided by caller to be used by the <see cref="TrajectoryCompressor"/>
/// </summary>
public class TrajectoryCompressorState : IEquatable<TrajectoryCompressorState>
{
	/// <summary>
	/// The start time for the current 'run'
	/// </summary>
	public DateTimeOffset startTime;

	/// <summary>
	/// The start value for the current 'run'
	/// </summary>
	public double startValue;

	/// <summary>
	/// The time of the previous point
	/// </summary>
	public DateTimeOffset previousTime;

	/// <summary>
	/// The value of the previous point
	/// </summary>
	public double previousValue;

	/// <summary>
	/// The slope of the upper bound line
	/// </summary>
	public double upper_slope;

	/// <summary>
	/// The slope of the lower bound line
	/// </summary>
	public double lower_slope;

	/// <summary>
	/// Whether we have two points yet
	/// </summary>
	internal bool hasPrevious;

	/// <summary>
	/// Sum of values
	/// </summary>
	public double Sum { get; set; }

	/// <summary>
	/// Sum of square of values
	/// </summary>
	public double SumSquare { get; set; }

	/// <summary>
	/// Count of values
	/// </summary>
	public long Count { get; set; }

	/// <summary>
	/// The delta between the last two real points (not compressed points)
	/// </summary>
	public double LastDelta { get; set; }

	/// <summary>
	/// The delta between the last two real points (not compressed points) (in seconds)
	/// </summary>
	public double LastDeltaTime { get; set; }

	/// <summary>
	/// Average
	/// </summary>
	public double Average => Sum / (Count + 0.0001);

	/// <summary>
	/// Variance of signal from mean
	/// </summary>
	/// <remarks>
	/// When there is just one/two points, returns 0.01 so that first point is not lost
	/// </remarks>
	public double Variance
	{
		get
		{
			double average = Average;
			return Count < 2 ? 0.01 : SumSquare / Count - (2 * average * Sum) / Count + average * average;
		}
	}

	/// <summary>
	/// Creates a new <see cref="TrajectoryCompressorState" />
	/// </summary>
	public TrajectoryCompressorState()
	{
		this.startTime = DateTimeOffset.MinValue;
		this.hasPrevious = false;
		this.Sum = 0;
		this.Count = 0;
		this.SumSquare = 0;
	}

	/// <summary>
	/// Equality test
	/// </summary>
	/// <remarks>
	/// Only needs to check some key values
	/// </remarks>
	public bool Equals(TrajectoryCompressorState? other)
	{
		return other is TrajectoryCompressorState t &&
			(t.hasPrevious, t.lower_slope, t.previousTime, t.previousValue, t.startTime, t.startValue, t.upper_slope)
			.Equals((this.hasPrevious, this.lower_slope, this.previousTime, this.previousValue, this.startTime, this.startValue, this.upper_slope));
		throw new NotImplementedException();
	}
}
