namespace Willow.Rules.Model;

/// <summary>
/// The command to run (None, UpdateCache, BuildRule, ProcessDateRange)
/// </summary>
public enum RuleExecutionCommandType
{
	/// <summary>
	/// No-op
	/// </summary>
	None = 0,

	/// <summary>
	/// Update cache
	/// </summary>
	UpdateCache = 1,

	/// <summary>
	/// Build one or all rules
	/// </summary>
	BuildRule = 2,

	/// <summary>
	/// Process a date range of rules
	/// </summary>
	ProcessDateRange = 3,

	/// <summary>
	/// Check that the rules engine processor is running for this environment
	/// </summary>
	CheckHeartBeat = 4,

	/// <summary>
	/// Cancel the selected job
	/// </summary>
	Cancel = 5,

	/// <summary>
	/// Pause the selected job
	/// </summary>
	Pause = 6,

	/// <summary>
	/// Continue the selected job
	/// </summary>
	Continue = 7,

	/// <summary>
	/// Delete all insights which are flagged not to sync to Command but which have already been syncd to Command
	/// </summary>
	DeleteCommandInsights = 8,

	/// <summary>
	/// Reverse sync command insight Id back to rules engine insights
	/// </summary>
	ReverseSyncInsights = 9,

	/// <summary>
	///  Delete all insights from Rules Engine, optionally deleting from Command
	/// </summary>
	DeleteAllInsights = 10,

	/// <summary>
	/// Delete rule and related data
	/// </summary>
	DeleteRule = 11,

	/// <summary>
	/// Rebuild search index
	/// </summary>
	RebuildSearchIndex = 12,

	/// <summary>
	///  Delete all insights from Rules Engine, optionally deleting from Command
	/// </summary>
	DeleteAllMatchingInsights = 13,

	/// <summary>
	/// Sync rules with remote Git repository
	/// </summary>
	GitSync = 14,

	/// <summary>
	/// Process calculated points to ADT
	/// </summary>
	ProcessCalculatedPoints = 15,

	/// <summary>
	///  Delete all commands from Rules Engine, optionally deleting from Command
	/// </summary>
	DeleteAllMatchingCommands = 16,

	/// <summary>
	///  Creates a process for diagnostics logs request
	/// </summary>
	RunDiagnostics = 17,

	/// <summary>
	///  Sync CommandEnabled flag to rule instances and insights
	/// </summary>
	SyncCommandEnabled = 18
}
