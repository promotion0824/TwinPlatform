// POCO class, serialized to DB
#nullable disable

using System;
using Willow.Rules.Repository;

namespace Willow.Rules.Model;

/// <summary>
/// Represents changes/events on an Insight
/// </summary>
public class InsightChange : IId
{
	/// <summary>
	/// Constructor for InsightChange
	/// </summary>
	public InsightChange(string insightId, InsightStatus status, DateTimeOffset timestamp)
	{
		if (string.IsNullOrEmpty(insightId))
		{
			throw new ArgumentException($"'{nameof(insightId)}' cannot be null or empty.", nameof(insightId));
		}

		Id = Guid.NewGuid().ToString();
		InsightId = insightId;
		Timestamp = timestamp;
		Status = status;
	}

	/// <summary>
	/// EF Constructor
	/// </summary>
	private InsightChange()
	{
	}

	/// <summary>
	/// The Id of the insight change (a GUID)
	/// </summary>
	public string Id { get; init; }

	/// <summary>
	/// The insight id which it belongs to
	/// </summary>
	public string InsightId { get; init; }

	/// <summary>
	/// The time of the change
	/// </summary>
	public DateTimeOffset Timestamp { get; init; }

	/// <summary>
	/// The status of the insight at the time of the change
	/// </summary>
	public InsightStatus Status { get; init; }
}

