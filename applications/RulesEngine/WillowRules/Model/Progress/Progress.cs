using System;
using System.Collections.Generic;
using Willow.Rules.Repository;

// EF
#nullable disable

namespace Willow.Rules.Model;

/// <summary>
/// The type of the progress record
/// </summary>
public enum ProgressType
{
	Cache = 1,
	RuleGeneration = 2,
	RuleExecution = 3,
	DeleteCommandInsights = 4,
	ReverseSyncInsights = 5,
	DeleteAllInsights = 6,
	DeleteRule = 7,
	SearchIndexRefresh = 8,
	DeleteAllMatchingInsights = 9,
	GitSync = 10,
	ProcessCalculatedPoints = 11,
	DeleteAllMatchingCommands = 12,
	RunDiagnostics = 13,
	SyncCommandEnabled = 14
}

/// <summary>
/// The status of the progress record
/// </summary>
public enum ProgressStatus
{
	InProgress = 1,
	Completed = 2,
	Failed = 3,
	Queued = 4
}

/// <summary>
/// Tracks progress by the processor back-end, may be rendered by the front-end
/// </summary>
/// <remarks>
/// A capped collection of these is maintained. The ui displays the most recent of each type
/// and may have an option to expand the list to see a history of rule progress work.
///
/// Service Bus messages send these to the UI so it can update the UI without having to poll
/// the database constantly and so we can monitor progress centrally if necessary.
/// </remarks>
public class Progress : IId
{
	public const string CacheId = "Cache";

	/// <summary>
	/// Batch execution
	/// </summary>
	public const string RuleExecutionId = "Execution";
	public const string RealtimeExecutionId = "RealtimeExecution";
	public const string RuleExpansionId = "Expansion";
	public const string DeleteCommandInsightsId = "DeleteCommandInsights";
	public const string ReverseSyncCommandInsightsId = "ReverseSyncCommandInsights";
	public const string DeleteAllMatchingInsightsId = "DeleteAllMatchingInsights";
	public const string DeleteAllMatchingCommandsId = "DeleteAllMatchingCommands";
	public const string DeleteAllInsightsId = "DeleteAllInsights";
	public const string DeleteRuleId = "DeleteSkill";
	public const string SearchIndexRefreshId = "SearchIndexRefresh";
	public const string GitSyncId = "GitSync";
	public const string ProcessCalculatedPointsId = "ProcessCalculatedPoints";
	public const string RunDiagnosticsId = "RunDiagnostics";
	public const string SyncCommandEnabledId = "SyncCommandEnabled";

	/// <summary>
	/// The Id for persistence
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// Type of the progress item
	/// </summary>
	/// <remarks>
	/// This field should perhaps be indexed but not essential: it's a small set
	/// </remarks>
	public ProgressType Type { get; set; }

	/// <summary>
	/// If the progress is for a specific entity, that's specified here, otherwise ""
	/// </summary>
	public string EntityId { get; set; }

	/// <summary>
	/// Correlation Id for progress that is in response to a request to do work
	/// </summary>
	public string CorrelationId { get; set; }

	/// <summary>
	/// Timestamp for sorting to find most recent progress for any type
	/// </summary>
	/// <remarks>
	/// This field should be indexed
	/// </remarks>
	public DateTimeOffset LastUpdated { get; set; }

	/// <summary>
	/// Percentage progress
	/// </summary>
	public double Percentage { get; set; }

	/// <summary>
	/// Details of the progress, breakdown by sub component with count and total for each
	/// </summary>
	public IList<ProgressInner> InnerProgress { get; }

	/// <summary>
	/// Server started at this time
	/// </summary>
	public DateTimeOffset StartTime { get; set; }

	/// <summary>
	/// Server expects to end at this time
	/// </summary>
	public DateTimeOffset Eta { get; set; }

	/// <summary>
	/// The user that requested the job
	/// </summary>
	public string RequestedBy { get; set; }

	/// <summary>
	/// The time the user requested
	/// </summary>
	public DateTimeOffset DateRequested { get; set; }

	/// <summary>
	/// The status of the progress
	/// </summary>
	public ProgressStatus Status { get; set; }

	/// <summary>
	/// The reason for failure
	/// </summary>
	public string FailedReason { get; set; }

	#region Expansion
	public string RuleId { get; set; }

	public string RuleName { get; set; }
	#endregion

	#region Execution
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

	#endregion

	/// <summary>
	/// Serialization constructor
	/// </summary>
	public Progress()
	{
	}

	/// <summary>
	/// Creates a new <see cref="Progress" /> item
	/// </summary>
	public Progress(string id,
		DateTimeOffset timeStamp,
		ProgressType type,
		double percentage,
		List<ProgressInner> innerProgress,
		string correlationId,
		string entityId,
		string requestedBy,
		DateTimeOffset dateRequested)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace.", nameof(id));
		}

		if (string.IsNullOrWhiteSpace(correlationId))
		{
			throw new ArgumentException($"'{nameof(correlationId)}' cannot be null or whitespace.", nameof(correlationId));
		}

		this.Id = id;
		this.LastUpdated = timeStamp;
		this.Type = type;
		this.CorrelationId = correlationId;
		this.EntityId = entityId ?? "";
		this.Percentage = percentage;
		this.InnerProgress = innerProgress;
		this.RequestedBy = requestedBy;
		this.DateRequested = dateRequested;
		this.Status = ProgressStatus.InProgress;
		// other fields set by caller
	}
}
