// EF
#nullable disable

using System;

namespace Willow.Rules.Model;

/// <summary>
/// Used in a progress update to represent inner details of the state
/// </summary>
/// <remarks>
/// Counts and totals for any components of the overall progress
/// </remarks>
public class ProgressInner
{
	/// <summary>
	/// Creates a new <see cref="ProgressInner" />
	/// </summary>
	public ProgressInner(string name, int count, int total)
	{
		this.ItemName = name;
		this.CurrentCount = count;
		this.TotalCount = total;
	}

	/// <summary>
	/// Serialization constructor
	/// </summary>
	public ProgressInner()
	{
	}

	/// <summary>
	/// Name of the item, e.g. Twins, Relationships, RuleInstances, ...
	/// </summary>
	public string ItemName { get; set; } = "NOT SET";

	/// <summary>
	/// Count of how many we have processed so far
	/// </summary>
	public int CurrentCount { get; set; }

	/// <summary>
	/// Count of how many there are in total
	/// </summary>
	public int TotalCount { get; set; }

	/// <summary>
	/// Calculates ETA based off elapsed time past and current total vs final total
	/// </summary>
	public DateTimeOffset GetEta(long elapsedMilliseconds)
	{
		var eta = (elapsedMilliseconds / (CurrentCount + 1)) * (TotalCount - CurrentCount);
		return DateTimeOffset.Now.AddMilliseconds(eta);
	}

	/// <summary>
	/// Gets Percentage complete beteen current and final
	/// </summary>
	public double PercentageComplete { get => CurrentCount / (TotalCount + 1); }
}
