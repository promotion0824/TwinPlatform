using Newtonsoft.Json;
using System;
using System.Diagnostics;
using Willow.Rules.Repository;

// EF
#nullable disable

namespace Willow.Rules.Model;

/// <summary>
/// An occurrence of the same insight at a different time
/// </summary>
[DebuggerDisplay("{Started}-{Ended} Faulted:{IsFaulted}")]
public class InsightOccurrence : IId
{
	/// <summary>
	/// Id
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// Is this interval valid, i.e. was there enough data to compute a result
	/// </summary>
	public bool IsValid { get; init; }

	/// <summary>
	/// Is this an interval during which a fault existed?
	/// </summary>
	public bool IsFaulted { get; init; }

	/// <summary>
	/// Start of interval
	/// </summary>
	public DateTimeOffset Started { get; set; }

	/// <summary>
	/// End of interval, may be updated regularly
	/// </summary>
	public DateTimeOffset Ended { get; set; }

	/// <summary>
	/// Text for the interval
	/// </summary>
	public string Text { get; set; }

	/// <summary>
	/// The insight id which it belongs to
	/// </summary>
	public string InsightId { get; init; }

	/// <summary>
	/// The parent Insight. Used by EF for one-to-many to Insight
	/// </summary>
	[JsonIgnore]
	public Insight Insight { get; init; }

	/// <summary>
	/// EF Constructor
	/// </summary>
	[JsonConstructor]
	private InsightOccurrence()
	{
	}

	/// <summary>
	/// Creates a new <see cref="InsightOccurrence"/>
	/// </summary>
	public InsightOccurrence(Insight insight, bool isValid, bool isFaulted, DateTimeOffset started, DateTimeOffset ended, string text)
	{
		Id = Guid.NewGuid().ToString();
		IsFaulted = isFaulted;
		IsValid = isValid;
		Started = started;
		Ended = ended;
		Text = text ?? throw new ArgumentNullException(nameof(text));
		this.Insight = insight ?? throw new ArgumentNullException(nameof(insight));
		InsightId = insight.Id;
	}
}
