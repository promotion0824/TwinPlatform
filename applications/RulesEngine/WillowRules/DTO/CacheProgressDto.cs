#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using System;

namespace Willow.Rules.DTO;

/// <summary>
/// Dto for caching progress
/// </summary>
public class CacheProgressDto : ProgressDetailsDto
{
	public static CacheProgressDto Empty = new CacheProgressDto
	{
		Percentage = 0.0,
		TwinCount = 0,
		RelationshipCount = 0,
		TotalTwinCount = 0,
		TotalRelationshipCount = 0
	};

	public long TwinCount { get; set; }
	public long RelationshipCount { get; set; }
	public int TotalTwinCount { get; set; }
	public int TotalRelationshipCount { get; set; }

	/// <summary>
	/// Server started at this time
	/// </summary>
	public DateTimeOffset StartTime { get; set; }

	/// <summary>
	/// Server expects to end at this time
	/// </summary>
	public DateTimeOffset Eta { get; set; }

	/// <summary>
	/// Equals for Entity Framework
	/// </summary>
	public override bool Equals(ProgressDetailsDto? other)
	{
		return other is CacheProgressDto cp && (cp.Percentage, cp.StartTime, cp.TwinCount, cp.RelationshipCount, cp.Eta) ==
			(this.Percentage, this.StartTime, this.TwinCount, this.RelationshipCount, this.Eta);
	}
}