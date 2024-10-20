#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
using System;

namespace Willow.Rules.DTO;

/// <summary>
/// Tracks rule execution from messages received on Service Bus
/// </summary>
public class RuleExecutionProgressDto : ProgressDetailsDto
{
	/// <summary>
	/// An empty progress dto
	/// </summary>
	public static RuleExecutionProgressDto Empty = new RuleExecutionProgressDto
	{
		Percentage = 0.0,
		Speed = 0.0,
		StartTimeSeriesTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero),
		CurrentTimeSeriesTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero),
		EndTimeSeriesTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)
	};

	/// <summary>
	/// Rule execution speed as a x on real-time
	/// </summary>
	public double Speed { get; init; }

	/// <summary>
	/// Start of tTime series execution time
	/// </summary>
	public DateTimeOffset StartTimeSeriesTime { get; set; }

	/// <summary>
	/// Time series execution time
	/// </summary>
	public DateTimeOffset CurrentTimeSeriesTime { get; set; }

	/// <summary>
	/// End of time series execution time
	/// </summary>
	public DateTimeOffset EndTimeSeriesTime { get; set; }

	/// <summary>
	/// Equals for Entity Framework
	/// </summary>
	public override bool Equals(ProgressDetailsDto? other)
	{
		return other is RuleExecutionProgressDto cp && (cp.Percentage, cp.StartTimeSeriesTime, cp.CurrentTimeSeriesTime, cp.EndTimeSeriesTime) ==
			(this.Percentage, this.StartTimeSeriesTime, this.CurrentTimeSeriesTime, this.EndTimeSeriesTime);
	}
}
