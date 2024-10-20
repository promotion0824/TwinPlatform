using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Repository;


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Willow.Rules.Model;

/// <summary>
/// Extended data required for requests.
/// </summary>
/// <remarks>
/// Always extend this type which means not having to add additonal sql columns.
/// </remarks>
public class RuleExecutionRequestExtendedData
{
	/// <summary>
	/// The progress type
	/// </summary>
	/// <remarks>
	/// Used for reporting
	/// </remarks>
	public ProgressType ProgressType { get; init; }

	/// <summary>
	/// The Id of the rule if used by the command (or empty)
	/// </summary>
	public string RuleId { get; init; } = string.Empty;

	/// <summary>
	/// An indicator whether to delete insights from command
	/// </summary>
	public bool DeleteFromCommand { get; init; }

	/// <summary>
	/// An indicator whether to delete all actors during insight deletes
	/// </summary>
	public bool DeleteActors { get; init; }

	/// <summary>
	/// An indicator whether to delete all timeseries during insight deletes
	/// </summary>
	public bool DeleteTimeSeries { get; init; }

	/// <summary>
	/// Force the operation (for any operation that is normally cached)
	/// </summary>
	/// <remarks>
	/// For fetching twins this will clear the disk before fecthing (to handle deletions)
	/// For building rule instances this will clear the cache (for new twins or deleted rules)
	/// For execution it does nothing
	/// </remarks>
	public bool Force { get; init; }

	/// <summary>
	/// Recreates the search index before updating it
	/// </summary>
	public bool RecreateIndex { get; init; }

	/// <summary>
	/// The start date for process range messages.
	/// </summary>
	/// <remarks>
	/// If null, then the rules engine would start at the end of the previous request or fall back to 15 days
	/// </remarks>
	public DateTime? StartDate { get; init; }

	/// <summary>
	/// The end date that we want to run rule evaluation up to. This may be extended any number of times
	/// and execution will resume from the <see cref="CompletedEndDate"/> going forward until this date is hit.
	/// </summary>
	/// <remarks>
	/// This can be bumped forward at any time, but if the start date goes backwards, it's a new job / generation.
	/// </remarks>
	public DateTime? TargetEndDate { get; init; }

	/// <summary>
	/// An indicator whether to clear the command id reference on the insight during command insight deletes
	/// </summary>
	public bool ClearCommandId { get; init; }

	/// <summary>
	/// An indicator whether to skip twin updates from ADT
	/// </summary>
	public bool OnlyRefreshTwins { get; init; }

	/// <summary>
	/// An indicator whether to skip relationship updates from ADT
	/// </summary>
	public bool OnlyRefreshRelationships { get; init; }

	/// <summary>
	/// An indicator whether to refresh AI search after the operations
	/// </summary>
	public bool RefreshSearchAfterwards { get; init; }

	/// <summary>
	/// An indicator whether to execute a cache/rebuild but only for calc points
	/// </summary>
	public bool CalculatedPointsOnly { get; init; }

	/// <summary>
	/// An indicator whether to delete (or otherwise, add) rule during a git sync
	/// </summary>
	public bool DeleteRule { get; init; }

	/// <summary>
	/// The folder containing the object of interest for git sync to process
	/// </summary>
	public string SyncFolder { get; init; } = string.Empty;

	/// <summary>
	/// The folder where the object used to reside for git sync to process
	/// </summary>
	public string OldSyncFolder { get; init; } = string.Empty;

	/// <summary>
	/// An indicator for git sync to know whether to queue rebuild requests for the rules that were 
	/// "synced" (pulled) from remote. This does not affect delete requests-- git sync will always
	/// queue delete requests for any deletions pulled from remote.
	/// </summary>
	public bool RebuildSyncedRules { get; init; }

	/// <summary>
	/// An indicator for git sync to know whether to queue rebuild requests for the rules that were
	/// uploaded.
	/// </summary>
	public bool RebuildUploadedRules { get; init; }

	/// <summary>
	/// An indicator for git sync to know whether rules were uploaded or not.
	/// </summary>
	public bool UploadedRules { get; init; }

	/// <summary>
	/// An indicator for git sync to only clone (and initialize/import into db)
	/// </summary>
	public bool CloneOnly { get; init; }

	/// <summary>
	/// The email of the user if used by the command (or empty)
	/// </summary>
	public string UserEmail { get; init; } = string.Empty;
}

/// <summary>
/// An execution request to queue work for the processor
/// </summary>
/// <remarks>
/// This is sent over service bus
/// </remarks>
public class RuleExecutionRequest : IId
{
	/// <summary>
	/// The correlation Id for this message
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// Customer environment (TBD if we need to make this more secure)
	/// </summary>
	public string CustomerEnvironmentId { get; init; }

	/// <summary>
	/// The id identifying the progress for this execution
	/// </summary>
	public string ProgressId { get; init; }

	/// <summary>
	/// The command
	/// </summary>
	public RuleExecutionCommandType Command { get; init; }

	/// <summary>
	/// The user that requested the job
	/// </summary>
	public string RequestedBy { get; init; }

	/// <summary>
	/// Indicator whether the execution has been requested. "Requested" requests are ignored in queue
	/// </summary>
	public bool Requested { get; set; }

	/// <summary>
	/// The date the request was made
	/// </summary>
	public DateTimeOffset RequestedDate { get; init; } = DateTimeOffset.UtcNow;

	/// <summary>
	/// Extended data required for requests.
	/// </summary>
	/// <remarks>
	/// Always extend this type which means not having to add additonal sql columns.
	/// </remarks>
	public RuleExecutionRequestExtendedData ExtendedData { get; init; } = new RuleExecutionRequestExtendedData();

	/// <summary>
	/// The progress type
	/// </summary>
	public ProgressType ProgressType => ExtendedData.ProgressType;

	/// <summary>
	/// The Id of the rule if used by the command (or empty)
	/// </summary>
	public string CorrelationId => Id;

	/// <summary>
	/// The Id of the rule if used by the command (or empty)
	/// </summary>
	[JsonIgnore]
	public string RuleId => ExtendedData.RuleId;

	/// <summary>
	/// An indicator whether to delete insights from command
	/// </summary>
	[JsonIgnore]
	public bool DeleteFromCommand => ExtendedData.DeleteFromCommand;

	/// <summary>
	/// An indicator whether to delete all actors during insight deletes
	/// </summary>
	[JsonIgnore]
	public bool DeleteActors => ExtendedData.DeleteActors;

	/// <summary>
	/// An indicator whether to delete all timeseries during insight deletes
	/// </summary>
	[JsonIgnore]
	public bool DeleteTimeSeries => ExtendedData.DeleteTimeSeries;

	/// <summary>
	/// An indicator whether to execute a cache/rebuild but only for calc points
	/// </summary>
	[JsonIgnore]
	public bool CalculatedPointsOnly => ExtendedData.CalculatedPointsOnly;

	/// <summary>
	/// An indicator whether to delete (or otherwise, add) rule during a git sync
	/// </summary>
	[JsonIgnore]
	public bool DeleteRule => ExtendedData.DeleteRule;

	/// <summary>
	/// The folder containing the object of interest for git sync to process
	/// </summary>
	[JsonIgnore]
	public string SyncFolder => ExtendedData.SyncFolder;

	/// <summary>
	/// The folder where the object used to reside for git sync to process
	/// </summary>
	[JsonIgnore]
	public string OldSyncFolder => ExtendedData.OldSyncFolder;

	/// <summary>
	/// An indicator for git sync to know whether to queue rebuild requests for the rules that were 
	/// "synced" (pulled) from remote. This does not affect delete requests-- git sync will always
	/// queue delete requests for any deletions pulled from remote.
	/// </summary>
	[JsonIgnore]
	public bool RebuildSyncedRules => ExtendedData.RebuildSyncedRules;

	/// <summary>
	/// An indicator for git sync to know whether to queue rebuild requests for the rules that were
	/// uploaded.
	/// </summary>
	[JsonIgnore]
	public bool RebuildUploadedRules => ExtendedData.RebuildUploadedRules;

	/// <summary>
	/// An indicator for git sync to know whether rules were uploaded or not.
	/// </summary>
	[JsonIgnore]
	public bool UploadedRules => ExtendedData.UploadedRules;

	/// <summary>
	/// An indicator for git sync to only clone (and initialize/import into db)
	/// </summary>
	[JsonIgnore]
	public bool CloneOnly => ExtendedData.CloneOnly;

	/// <summary>
	/// The email of the user if used by the command (or empty)
	/// </summary>
	[JsonIgnore]
	public string UserEmail => ExtendedData.UserEmail;

	/// <summary>
	/// Force the operation (for any operation that is normally cached)
	/// </summary>
	/// <remarks>
	/// For fetching twins this will clear the disk before fecthing (to handle deletions)
	/// For building rule instances this will clear the cache (for new twins or deleted rules)
	/// For execution it does nothing
	/// </remarks>
	[JsonIgnore]
	public bool Force => ExtendedData.Force;

	/// <summary>
	/// Recreates the search index before updating it
	/// </summary>
	public bool RecreateIndex => ExtendedData.RecreateIndex;

	/// <summary>
	/// The start date for process range messages.
	/// </summary>
	/// <remarks>
	/// If null, then the rules engine would start at the end of the previous request or fall back to 15 days
	/// </remarks>
	[JsonIgnore]
	public DateTime? StartDate => ExtendedData.StartDate;

	/// <summary>
	/// The end date that we want to run rule evaluation up to. This may be extended any number of times
	/// and execution will resume from the <see cref="CompletedEndDate"/> going forward until this date is hit.
	/// </summary>
	/// <remarks>
	/// This can be bumped forward at any time, but if the start date goes backwards, it's a new job / generation.
	/// </remarks>
	[JsonIgnore]
	public DateTime? TargetEndDate => ExtendedData.TargetEndDate;

	/// <summary>
	/// An indicator whether to clear the command id reference on the insight during command insight deletes
	/// </summary>
	[JsonIgnore]
	public bool ClearCommandId => ExtendedData.ClearCommandId;

	/// <summary>
	/// Creates a heartbeat request
	/// </summary>
	public static RuleExecutionRequest CreateHeartbeatRequest(string customerEnvironmentId)
	{
		return new RuleExecutionRequest
		{
			Command = RuleExecutionCommandType.CheckHeartBeat,
			Id = Guid.NewGuid().ToString(),
			CustomerEnvironmentId = customerEnvironmentId
		};
	}

	/// <summary>
	/// Creates a rule expansion request to regenerate rule instances
	/// </summary>
	public static RuleExecutionRequest CreateRuleExpansionRequest(string customerEnvironmentId, bool force, string requestedBy, string? ruleId = null, bool calculatedPointsOnly = false)
	{
		return new RuleExecutionRequest
		{
			Id = Guid.NewGuid().ToString(),
			ProgressId = Progress.RuleExpansionId,
			Command = RuleExecutionCommandType.BuildRule,
			CustomerEnvironmentId = customerEnvironmentId,
			RequestedBy = requestedBy,
			ExtendedData = new RuleExecutionRequestExtendedData()
			{
				ProgressType = ProgressType.RuleGeneration,
				Force = force,
				RuleId = (ruleId ?? string.Empty),
				StartDate = DateTime.UtcNow,
				TargetEndDate = DateTime.UtcNow,
				CalculatedPointsOnly = calculatedPointsOnly
			}
		};
	}

	/// <summary>
	/// Creates a rule execution request
	/// </summary>
	public static RuleExecutionRequest CreateBatchExecutionRequest(string customerEnvironmentId, DateTime startDate, string requestedBy)
	{
		return new RuleExecutionRequest
		{
			Id = Guid.NewGuid().ToString(),
			ProgressId = Progress.RuleExecutionId,
			Command = RuleExecutionCommandType.ProcessDateRange,
			CustomerEnvironmentId = customerEnvironmentId,
			RequestedBy = requestedBy,
			ExtendedData = new RuleExecutionRequestExtendedData()
			{
				ProgressType = ProgressType.RuleExecution,
				StartDate = startDate,
				TargetEndDate = DateTime.UtcNow
			}
		};
	}

	/// <summary>
	/// Creates a single rule execution request
	/// </summary>
	public static RuleExecutionRequest CreateSingleRuleExecutionRequest(string customerEnvironmentId, DateTime startDate, string ruleId, string requestedBy)
	{
		return new RuleExecutionRequest
		{
			Id = Guid.NewGuid().ToString(),
			ProgressId = Progress.RuleExecutionId,
			Command = RuleExecutionCommandType.ProcessDateRange,
			CustomerEnvironmentId = customerEnvironmentId,
			RequestedBy = requestedBy,
			ExtendedData = new RuleExecutionRequestExtendedData()
			{
				ProgressType = ProgressType.RuleExecution,
				StartDate = startDate,
				TargetEndDate = DateTime.UtcNow,
				RuleId = ruleId
			}
		};
	}

	/// <summary>
	/// Creates a rule execution request
	/// </summary>
	public static RuleExecutionRequest CreateRealtimeExecutionRequest(string customerEnvironmentId, string requestedBy)
	{
		return new RuleExecutionRequest
		{
			//Assign a static Guid for upsert to work properly and minimize the stacking of Realtime requests
			Id = "5152b270-7ea7-4ad3-8e66-13814a321026",
			ProgressId = Progress.RealtimeExecutionId,
			Command = RuleExecutionCommandType.ProcessDateRange,
			CustomerEnvironmentId = customerEnvironmentId,
			RequestedBy = requestedBy,
			ExtendedData = new RuleExecutionRequestExtendedData()
			{
				//don't assign the start date. Let the rule execution start where it left off
				TargetEndDate = DateTimeOffset.Now.UtcDateTime,
				ProgressType = ProgressType.RuleExecution
			}
		};
	}

	/// <summary>
	/// Creates a cache refresh request
	/// </summary>
	public static RuleExecutionRequest CreateCacheRefreshRequest(string customerEnvironmentId, bool force, string requestedBy, bool refreshSearchAfterwards = true, bool refreshTwins = true, bool refreshRelationships = true)
	{
		return new RuleExecutionRequest
		{
			Id = Guid.NewGuid().ToString(),
			ProgressId = Progress.CacheId,
			Command = RuleExecutionCommandType.UpdateCache,
			CustomerEnvironmentId = customerEnvironmentId,
			RequestedBy = requestedBy,
			ExtendedData = new RuleExecutionRequestExtendedData()
			{
				ProgressType = ProgressType.Cache,
				Force = force,
				StartDate = DateTime.UtcNow,
				TargetEndDate = DateTime.UtcNow,
				OnlyRefreshTwins = refreshTwins,
				OnlyRefreshRelationships = refreshRelationships,
				RefreshSearchAfterwards = refreshSearchAfterwards
			}
		};
	}

	/// <summary>
	/// Creates a search index refresh request
	/// </summary>
	public static RuleExecutionRequest CreateSearchIndexRefreshRequest(string customerEnvironmentId, bool force, string requestedBy, bool recreateIndex = false, bool onlyRefreshTwins = false)
	{
		return new RuleExecutionRequest
		{
			Id = Guid.NewGuid().ToString(),
			ProgressId = Progress.SearchIndexRefreshId,
			Command = RuleExecutionCommandType.RebuildSearchIndex,
			CustomerEnvironmentId = customerEnvironmentId,
			RequestedBy = requestedBy,
			ExtendedData = new RuleExecutionRequestExtendedData()
			{
				ProgressType = ProgressType.SearchIndexRefresh,
				Force = force,
				RecreateIndex = recreateIndex,
				StartDate = DateTime.UtcNow,
				TargetEndDate = DateTime.UtcNow,
				OnlyRefreshTwins = onlyRefreshTwins
			}
		};
	}

	/// <summary>
	/// Creates a command insights delete request
	/// </summary>
	public static RuleExecutionRequest CreateDeleteCommandInsightsRequest(string customerEnvironmentId, bool clearCommandId, string requestedBy)
	{
		return new RuleExecutionRequest
		{
			Id = Guid.NewGuid().ToString(),
			ProgressId = Progress.DeleteCommandInsightsId,
			Command = RuleExecutionCommandType.DeleteCommandInsights,
			CustomerEnvironmentId = customerEnvironmentId,
			RequestedBy = requestedBy,
			ExtendedData = new RuleExecutionRequestExtendedData()
			{
				ProgressType = ProgressType.DeleteCommandInsights,
				ClearCommandId = clearCommandId
			}
		};
	}

	/// <summary>
	/// Creates a delete all insights request
	/// </summary>
	public static RuleExecutionRequest CreateDeleteAllInsightsRequest(string customerEnvironmentId, bool deleteFromCommand, bool deleteActors, bool deleteTimeseries, string requestedBy)
	{
		return new RuleExecutionRequest
		{
			Id = Guid.NewGuid().ToString(),
			ProgressId = Progress.DeleteAllInsightsId,
			Command = RuleExecutionCommandType.DeleteAllInsights,
			CustomerEnvironmentId = customerEnvironmentId,
			RequestedBy = requestedBy,
			ExtendedData = new RuleExecutionRequestExtendedData()
			{
				ProgressType = ProgressType.DeleteAllInsights,
				DeleteFromCommand = deleteFromCommand,
				DeleteActors = deleteActors,
				DeleteTimeSeries = deleteTimeseries,
			}
		};
	}

	/// <summary>
	/// Creates a delete all matching insights request
	/// </summary>
	public static RuleExecutionRequest CreateDeleteAllMatchingInsightsRequest(string customerEnvironmentId, string ruleId, string requestedBy)
	{
		return new RuleExecutionRequest
		{
			Id = Guid.NewGuid().ToString(),
			ProgressId = Progress.DeleteAllMatchingInsightsId,
			Command = RuleExecutionCommandType.DeleteAllMatchingInsights,
			CustomerEnvironmentId = customerEnvironmentId,
			RequestedBy = requestedBy,
			ExtendedData = new RuleExecutionRequestExtendedData()
			{
				ProgressType = ProgressType.DeleteAllMatchingInsights,
				RuleId = ruleId
			}
		};
	}

	/// <summary>
	/// Creates a delete all matching commands request
	/// </summary>
	public static RuleExecutionRequest CreateDeleteAllMatchingCommandsRequest(string customerEnvironmentId, string ruleId, string requestedBy)
	{
		return new RuleExecutionRequest
		{
			Id = Guid.NewGuid().ToString(),
			ProgressId = Progress.DeleteAllMatchingCommandsId,
			Command = RuleExecutionCommandType.DeleteAllMatchingCommands,
			CustomerEnvironmentId = customerEnvironmentId,
			RequestedBy = requestedBy,
			ExtendedData = new RuleExecutionRequestExtendedData()
			{
				ProgressType = ProgressType.DeleteAllMatchingCommands,
				RuleId = ruleId
			}
		};
	}

	/// <summary>
	/// Creates a reverse insights request
	/// </summary>
	public static RuleExecutionRequest CreateReverseSyncInsightsRequest(string customerEnvironmentId, string requestedBy)
	{
		return new RuleExecutionRequest
		{
			Id = Guid.NewGuid().ToString(),
			ProgressId = Progress.ReverseSyncCommandInsightsId,
			Command = RuleExecutionCommandType.ReverseSyncInsights,
			CustomerEnvironmentId = customerEnvironmentId,
			RequestedBy = requestedBy,
			ExtendedData = new RuleExecutionRequestExtendedData()
			{
				ProgressType = ProgressType.ReverseSyncInsights
			}
		};
	}

	/// <summary>
	/// Creates a cancellation request
	/// </summary>
	public static RuleExecutionRequest CreateCancelRequest(string customerEnvironmentId, string correlationId, string progressId)
	{
		return new RuleExecutionRequest
		{
			Command = RuleExecutionCommandType.Cancel,
			Id = correlationId,
			ProgressId = progressId,
			CustomerEnvironmentId = customerEnvironmentId
		};
	}

	/// <summary>
	/// Creates a delete rule request
	/// </summary>
	public static RuleExecutionRequest CreateDeleteRuleRequest(string customerEnvironmentId, string ruleId, string requestedBy)
	{
		return new RuleExecutionRequest
		{
			Id = Guid.NewGuid().ToString(),
			ProgressId = Progress.DeleteRuleId,
			Command = RuleExecutionCommandType.DeleteRule,
			CustomerEnvironmentId = customerEnvironmentId,
			RequestedBy = requestedBy,
			ExtendedData = new RuleExecutionRequestExtendedData()
			{
				ProgressType = ProgressType.DeleteRule,
				RuleId = ruleId,
				StartDate = DateTime.UtcNow,
				TargetEndDate = DateTime.UtcNow
			}
		};
	}

	/// <summary>
	/// Creates a git sync request
	/// </summary>
	public static RuleExecutionRequest CreateGitSyncRequest(string customerEnvironmentId, string requestedBy,
		string? userEmail = null, string? ruleId = null, string? syncFolder = null, string? oldSyncFolder = null, bool deleteRule = false,
		bool rebuildSyncedRules = true, bool rebuildUploadedRules = true, bool uploadedRules = false,
		bool cloneOnly = false)
	{
		return new RuleExecutionRequest
		{
			Id = Guid.NewGuid().ToString(),
			ProgressId = Progress.GitSyncId,
			Command = RuleExecutionCommandType.GitSync,
			CustomerEnvironmentId = customerEnvironmentId,
			RequestedBy = requestedBy,
			ExtendedData = new RuleExecutionRequestExtendedData()
			{
				ProgressType = ProgressType.GitSync,
				UserEmail = userEmail ?? string.Empty,
				RuleId = ruleId ?? string.Empty,
				SyncFolder = syncFolder ?? string.Empty,
				OldSyncFolder = oldSyncFolder ?? string.Empty,
				DeleteRule = deleteRule,
				RebuildSyncedRules = rebuildSyncedRules,
				RebuildUploadedRules = rebuildUploadedRules,
				UploadedRules = uploadedRules,
				CloneOnly = cloneOnly,
				StartDate = DateTime.UtcNow,
				TargetEndDate = DateTime.UtcNow
			}
		};
	}

	/// <summary>
	/// Creates a process calculated points request
	/// </summary>
	public static RuleExecutionRequest CreateProcessCalculatedPointsRequest(string customerEnvironmentId, string requestedBy, string? ruleId = null)
	{
		return new RuleExecutionRequest
		{
			Id = Guid.NewGuid().ToString(),
			ProgressId = Progress.ProcessCalculatedPointsId,
			Command = RuleExecutionCommandType.ProcessCalculatedPoints,
			CustomerEnvironmentId = customerEnvironmentId,
			RequestedBy = requestedBy,
			ExtendedData = new RuleExecutionRequestExtendedData()
			{
				ProgressType = ProgressType.ProcessCalculatedPoints,
				RuleId = ruleId ?? string.Empty,
				StartDate = DateTime.UtcNow,
				TargetEndDate = DateTime.UtcNow
			}
		};
	}

	/// <summary>
	/// Creates a process for diagnostics logs request
	/// </summary>
	public static RuleExecutionRequest CreatDiagnosticsRequest(string customerEnvironmentId, string requestedBy)
	{
		return new RuleExecutionRequest
		{
			Id = Guid.NewGuid().ToString(),
			ProgressId = Progress.RunDiagnosticsId,
			Command = RuleExecutionCommandType.RunDiagnostics,
			CustomerEnvironmentId = customerEnvironmentId,
			RequestedBy = requestedBy,
			ExtendedData = new RuleExecutionRequestExtendedData()
			{
				ProgressType = ProgressType.RunDiagnostics,
				StartDate = DateTime.UtcNow,
				TargetEndDate = DateTime.UtcNow
			}
		};
	}

	/// <summary>
	/// Sync CommandEnabled flag to rule instances and insights
	/// </summary>
	public static RuleExecutionRequest CreateSyncCommandEnabledRequest(string customerEnvironmentId, string requestedBy, string ruleId)
	{
		return new RuleExecutionRequest
		{
			Id = Guid.NewGuid().ToString(),
			ProgressId = Progress.SyncCommandEnabledId,
			Command = RuleExecutionCommandType.SyncCommandEnabled,
			CustomerEnvironmentId = customerEnvironmentId,
			RequestedBy = requestedBy,
			ExtendedData = new RuleExecutionRequestExtendedData()
			{
				RuleId = ruleId,
				ProgressType = ProgressType.SyncCommandEnabled,
				StartDate = DateTime.UtcNow,
				TargetEndDate = DateTime.UtcNow
			}
		};
	}
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
