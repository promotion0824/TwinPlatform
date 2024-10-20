using System;
using Willow.Rules.Repository;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Willow.Rules.Model;

/// <summary>
/// A git sync request to queue work for the git sync background service.
/// </summary>
/// <remarks>
/// This is sent over the <see cref="GitSyncOrchestrator"/>.
/// </remarks>
public class GitSyncRequest : IId
{
	/// <summary>
	/// The correlation Id for this message
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// Customer environment
	/// </summary>
	public string CustomerEnvironmentId { get; init; }

	/// <summary>
	/// The user that requested the git sync
	/// </summary>
	public string RequestedBy { get; init; }

	/// <summary>
	/// The date the request was made
	/// </summary>
	public DateTimeOffset RequestedDate { get; init; }

	/// <summary>
	/// The email of the user if used by the request (or empty)
	/// </summary>
	/// <value></value>
	public string UserEmail { get; init; } = string.Empty;

	/// <summary>
	/// The Id of the rule if used by the request (or empty)
	/// </summary>
	public string RuleId { get; init; } = string.Empty;

	/// <summary>
	/// The folder containing the object of interest for git sync to process
	/// </summary>
	public string SyncFolder { get; init; } = string.Empty;

	/// <summary>
	/// The folder where the object used to reside for git sync to process
	/// </summary>
	public string OldSyncFolder { get; init; } = string.Empty;

	/// <summary>
	/// An indicator whether to delete (or otherwise, add) rule during a git sync
	/// </summary>
	public bool DeleteRule { get; init; }

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
	/// The start date for the git sync request
	/// </summary>
	public DateTime? StartDate { get; init; }

	/// <summary>
	/// The target end date for the git sync request
	/// </summary>
	/// <value></value>
	public DateTime? TargetEndDate { get; init; }
}
