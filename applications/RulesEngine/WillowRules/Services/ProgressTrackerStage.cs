using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Willow.Rules.Services;

/// <summary>
/// Represents one stage in a progress tracker
/// </summary>
public class ProgressTrackerStage : IEquatable<ProgressTrackerStage>
{
	private const string SingleSource = "single_source";

	/// <summary>
	/// How slow one unit of this stage is compared to other stages
	/// </summary>
	public double Weight { get; set; }

	/// <summary>
	/// The name shown in the progress user interface
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Counts and totals by source
	/// </summary>
	private ConcurrentDictionary<string, (long count, long total)> counts = new();

	/// <summary>
	/// Count of items processed
	/// </summary>
	public long Count => counts.Values.Sum(x => x.count);

	/// <summary>
	/// Total items expected to be processed (optionally per-source, e.g. multiple ADTs)
	/// </summary>
	public long Total => counts.Values.Sum(x => x.total);

	/// <summary>
	/// Ignored in percentage calculations
	/// </summary>
	public bool IsIgnored { get; set; } = false;

	/// <summary>
	/// Creates a new <see cref="ProgressTrackerStage" />
	/// </summary>
	public ProgressTrackerStage(string name, double weight)
	{
		Name = name ?? throw new ArgumentNullException(nameof(name));
		Weight = weight;
	}

	/// <summary>
	/// Creates a new <see cref="ProgressTrackerStage" /> with an estimated total count
	/// </summary>
	public ProgressTrackerStage(string name, double weight, int estimate)
	{
		Name = name ?? throw new ArgumentNullException(nameof(name));
		Weight = weight;
		counts.AddOrUpdate(SingleSource, (k) => (0, estimate), (k, _) => (0, estimate));
	}

	/// <summary>
	/// Track a count and total for a given source (part of the stage tracker)
	/// </summary>
	public void Track(string source, long count, long total)
	{
		counts.AddOrUpdate(source, (u) => (count, total),
			(s, p) => (count, total));
		// If we have individual sources, remove the single source estimate
		counts.TryRemove(SingleSource, out _);
	}

	/// <summary>
	/// Track a count and total (since source)
	/// </summary>
	public void Track(int count, int total)
	{
		counts.AddOrUpdate(SingleSource, (u) => (count, total),
			(s, p) => (count, total));
	}

	/// <summary>
	/// Hashset equals
	/// </summary>
	public bool Equals(ProgressTrackerStage? other)
	{
		return this.Name.Equals(other?.Name);
	}
}
