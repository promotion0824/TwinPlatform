#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Willow.Rules.Repository;

namespace Willow.Rules.Model;

/// <summary>
/// Scan state for a rule
/// </summary>
public enum ScanState
{
	Unknown = 0,
	Active = 1,
	Completed = 2
}

/// <summary>
/// Metadata for a rule, updated in real-time as rule instances are created by the back-end
/// </summary>
public class RuleMetadata : IId
{
	/// <summary>
	/// The ID of the rule for which this metadata was created
	/// </summary>
	[JsonProperty("id")]
	public string Id { get; init; }

	/// <summary>
	/// EF construcotr
	/// </summary>
	internal RuleMetadata()
	{
	}

	/// <summary>
	/// UI Constructor
	/// </summary>
	public RuleMetadata(string ruleId, string? createdBy = null)
	{
		Id = ruleId;
		CreatedBy = createdBy ?? "";
		ModifiedBy = createdBy ?? "";
		Created = DateTimeOffset.Now;
		LastModified = DateTimeOffset.Now;
		ScanComplete = false;
		ScanStarted = false;
		ScanState = ScanState.Unknown;
		ScanStateAsOf = DateTimeOffset.Now;
		RuleInstanceCount = 0;
		ValidInstanceCount = 0;
		InsightsGenerated = 0;
		RuleInstanceStatus = 0;
		CommandsGenerated = 0;
		Version = 1;
		ETag = "METADATA ETAG NOT SET YET";
	}

	/// <summary>
	/// The user who created the rule
	/// </summary>
	public string CreatedBy { get; init; }

	/// <summary>
	/// The user who last modified the rule
	/// </summary>
	public string ModifiedBy { get; set; }

	/// <summary>
	/// The last time the rule was modified
	/// </summary>
	public DateTimeOffset LastModified { get; set; }

	/// <summary>
	/// When the rule was created
	/// </summary>
	public DateTimeOffset Created { get; init; }

	/// <summary>
	/// Scan started
	/// </summary>
	public bool ScanStarted { get; set; }

	/// <summary>
	/// Counts are updated
	/// </summary>
	public bool ScanComplete { get; set; }

	/// <summary>
	/// State of being scanned, completed, failed
	/// </summary>
	public ScanState ScanState { get; set; }

	/// <summary>
	/// The last scan error
	/// </summary>
	/// <remarks>
	/// May not be null for EF
	/// </remarks>
	public string ScanError { get; set; } = "";

	/// <summary>
	/// Datetimeoffset when the scan state was last updated
	/// </summary>
	public DateTimeOffset ScanStateAsOf { get; set; }

	/// <summary>
	/// The count of instances that the rule expanded to
	/// </summary>
	public int RuleInstanceCount { get; set; }

	/// <summary>
	/// The count of valid instances of this rule
	/// </summary>
	public int ValidInstanceCount { get; set; }

	/// <summary>
	/// The overall rule instance status
	/// </summary>
	public RuleInstanceStatus RuleInstanceStatus { get; set; }

	/// <summary>
	/// Count of how many FAULTED insights this rule has generated
	/// </summary>
	/// <remarks>
	/// Replace this with a more useful statistics object tracking day by day for some window
	/// </remarks>
	public int InsightsGenerated { get; set; }

	/// <summary>
	/// Count of how many commands were generated
	/// </summary>
	public int CommandsGenerated { get; set; }

	/// <summary>
	/// The earliest date the rule has ever executed against
	/// </summary>
	public DateTimeOffset EarliestExecutionDate { get; set; } = DateTimeOffset.MaxValue;

	/// <summary>
	/// Extension data
	/// </summary>
	public RuleMetadataExtensionData ExtensionData { get; init; } = new RuleMetadataExtensionData();

	/// <summary>
	/// ETag for rule instance generation
	/// </summary>
	public string ETag { get; set; }

	/// <summary>
	///The version of the rule. Usually increments for rule expression updates
	/// </summary>
	public int Version { get; set; }

	/// <summary>
	/// Sets the metadata properties for a failed scan
	/// </summary>
	public void ScanFailed(string scanError)
	{
		this.ScanStarted = false;
		this.ScanComplete = false;
		this.ScanError = scanError;
		this.ScanState = ScanState.Unknown;
		this.ScanStateAsOf = DateTimeOffset.Now;
	}

	/// <summary>
	/// Increments the metadata version
	/// </summary>
	public void IncrementVersion(string user, string reason)
	{
		Version++;
		AddLog($"Version incremented to V{Version}. Reason: {reason}", user);
	}

	/// <summary>
	/// Sets the metadata properties for a scan that is about to start
	/// </summary>
	public void ScanStarting()
	{
		this.ScanStarted = true;
		this.ScanComplete = false;
		this.ScanError = "";
		this.ScanState = ScanState.Active;
		this.ScanStateAsOf = DateTimeOffset.Now;
		this.InsightsGenerated = 0;
		this.CommandsGenerated = 0;
		this.RuleInstanceCount = 0;
		this.ValidInstanceCount = 0;
		this.RuleInstanceStatus = 0;
	}

	/// <summary>
	/// Sets the metadata properties for a scan that completed
	/// </summary>
	public void ScanCompleted(Rule rule)
	{
		this.ETag = rule.ETag;
		// already updated these:
		// metadata.RuleInstanceCount = ruleInstances.Count();
		// metadata.ValidInstanceCount = ruleInstances.Count(v => v.Valid);
		this.ScanComplete = true;
		this.ScanState = ScanState.Completed;
		this.ScanStateAsOf = DateTimeOffset.Now;
	}

	/// <summary>
	/// Add audit log
	/// </summary>
	public void AddLog(string log, string user)
	{
		var logs = ExtensionData.Logs;

		var lastLog = logs.LastOrDefault();

		if (lastLog?.Message == log)
		{
			lastLog.User = user;
			lastLog.Date = DateTime.UtcNow;
		}
		else
		{
			logs.Add(new AuditLogEntry()
			{
				Date = DateTime.UtcNow,
				Message = log,
				User = user
			});
		}

		if (logs.Count > 50)
		{
			logs.RemoveAt(0);
		}
	}
}

/// <summary>
/// Extension data for rule metadata
/// </summary>
public class RuleMetadataExtensionData
{
	/// <summary>
	/// Audit logs for the rules
	/// </summary>
	public List<AuditLogEntry> Logs { get; init; } = new List<AuditLogEntry>();
}

/// <summary>
/// Audit Log entry for rule metadata
/// </summary>
public class AuditLogEntry
{
	/// <summary>
	/// Date of log entry
	/// </summary>
	public DateTimeOffset Date { get; set; }

	/// <summary>
	/// Message for log
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	/// Which user the log is for
	/// </summary>
	public string User { get; set; }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
