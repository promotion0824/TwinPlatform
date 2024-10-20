using System;
using System.Collections.Generic;

namespace Willow.Rules.DTO;

/// <summary>
/// Base for messages from processor to web
/// </summary>
public abstract class ProgressDetailsDto : IEquatable<ProgressDetailsDto>
{
	/// <summary>
	/// An overall percentage completion
	/// </summary>
	public double Percentage { get; set; }

	/// <summary>
	/// Inner progress details
	/// </summary>
	public List<InnerProgressDto> InnerProgress { get; set; } = new List<InnerProgressDto>();

	public abstract bool Equals(ProgressDetailsDto? other);
}

/// <summary>
/// Used in a progress update to represent inner details of the state
/// </summary>
/// <remarks>
/// This replaces all the other fields that used to track counts and totals
/// </remarks>
public class InnerProgressDto
{
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
}
