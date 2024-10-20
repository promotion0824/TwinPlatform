using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RulesEngine.Processor.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Configuration;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using Willow.Rules.Sources;
using WillowRules.Extensions;
using WillowRules.Logging;
using static Willow.Rules.Services.FileService;

namespace Willow.Rules.Processor;

/// <summary>
/// Service for synchronizing Git working directory with remote fork by calling
/// APIs from <see cref="IGitService"/>.
/// </summary>
public interface IGitSyncProcessor
{
	/// <summary>
	/// Syncs the Git working directory with the remote fork. This consists of
	/// commiting changes (if any), pulling and resolving merge conflicts
	/// (if any), pushing changes (if any), and creating rebuild/delete requests
	/// if requested to do so.
	/// </summary>
	/// <remarks>
	/// If the PAT for the remote fork is invalid, flips to a secondary PAT and retries.
	/// </remarks>
	/// <returns>
	/// A new <see cref="GitSyncResponse"/> containing the PAT status and any
	/// rebuild/delete requests.
	/// </returns>
	Task<GitSyncResponse> SyncWithFork(GitSyncRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Service for synchronizing Git working directory with remote fork by calling
/// APIs from <see cref="IGitService"/>.
/// </summary>
public class GitSyncProcessor : IGitSyncProcessor
{
	private readonly WillowEnvironment willowEnvironment;
	private readonly IRepositoryProgress repositoryProgress;
	private readonly IRepositoryRules repositoryRules;
	private readonly ILogger<GitSyncProcessor> logger;
	private readonly IAuditLogger<GitSyncProcessor> auditLogger;
	private readonly IGitService gitService;
	private readonly IOptions<GitSyncOptions> gitOptions;
	private readonly HealthCheckGitSync healthCheck;
	private readonly ExponentialBackOff exponentialBackOff;
	private readonly IFileService fileService;

	private static DateTimeOffset nextRetry = DateTimeOffset.UtcNow;

	/// <summary>
	/// Creates a new <see cref="GitSyncProcessor"/>
	/// </summary>
	public GitSyncProcessor(
		WillowEnvironment willowEnvironment,
		IRepositoryProgress repositoryProgress,
		IRepositoryRules repositoryRules,
		IFileService fileService,
		IGitService gitService,
		IOptions<GitSyncOptions> gitOptions,
		HealthCheckGitSync healthCheck,
		ILogger<GitSyncProcessor> logger,
		IAuditLogger<GitSyncProcessor> auditLogger)
	{
		this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
		this.repositoryProgress = repositoryProgress ?? throw new ArgumentNullException(nameof(repositoryProgress));
		this.repositoryRules = repositoryRules ?? throw new ArgumentNullException(nameof(repositoryRules));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
		this.gitService = gitService ?? throw new ArgumentNullException(nameof(gitService));
		this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
		this.gitOptions = gitOptions ?? throw new ArgumentNullException(nameof(gitOptions));
		this.healthCheck = healthCheck ?? throw new ArgumentNullException(nameof(healthCheck));

		// 12 hr max backoff for retrying a failed git sync
		this.exponentialBackOff = new ExponentialBackOff(5000, 2, 12 * 60 * 60 * 1000);
	}

	/// <summary>
	/// Syncs the Git working directory with the remote fork. This consists of
	/// commiting changes (if any), pulling and resolving merge conflicts
	/// (if any), pushing changes (if any), and creating rebuild/delete requests
	/// if requested to do so.
	/// </summary>
	/// <remarks>
	/// If the PAT for the remote fork is invalid, flips to a secondary PAT and retries.
	/// </remarks>
	/// <returns>
	/// A new <see cref="GitSyncResponse"/> containing the PAT status and any
	/// rebuild/delete requests.
	/// </returns>
	public async Task<GitSyncResponse>
		SyncWithFork(GitSyncRequest request, CancellationToken cancellationToken)
	{
		if (!CheckGitSyncConfiguration())
		{
			return new GitSyncResponse { Skipped = true };
		}

		var response = await Policy
			.HandleResult<GitSyncResponse>(r => r.InvalidPat && !r.Skipped)
			.RetryAsync(onRetry: (result, retryCount) =>
			{
				// Before retrying one more time, flips the PAT that is currently being used
				gitService.SwitchPAT();
			}
			).ExecuteAsync(() => Sync(request, cancellationToken));

		// If PAT is still invalid, git sync has failed for both primary/secondary PATs.
		// So, we bump the timer for the next git sync retry using exponential backoff.
		if (response.InvalidPat && !response.Skipped)
		{
			nextRetry = DateTimeOffset.UtcNow.AddMilliseconds(exponentialBackOff.GetNextDelay());
			logger.LogError("Git sync failed because a valid PAT could not be found. Total Retries {count}", exponentialBackOff.Count());
			healthCheck.Current = HealthCheckGitSync.AuthorizationFailure;
		}

		return response;
	}

	/// <summary>
	/// Syncs the Git working directory with the remote fork. This consists of
	/// commiting changes (if any), pulling and resolving merge conflicts
	/// (if any), pushing changes (if any), and creating rebuild/delete requests
	/// if requested to do so.
	/// </summary>
	/// <returns>
	/// A new <see cref="GitSyncResponse"/> containing the PAT status and any
	/// rebuild/delete requests.
	/// </returns>
	private async Task<GitSyncResponse>
		Sync(GitSyncRequest request, CancellationToken cancellationToken)
	{
		IList<RuleExecutionRequest> rebuildRequests = new List<RuleExecutionRequest>();
		IList<RuleExecutionRequest> deleteRequests = new List<RuleExecutionRequest>();

		bool invalidPat = false;

		// Skipping git sync request if next retry timer is not up yet
		bool skipped = nextRetry > DateTimeOffset.UtcNow;

		GitSyncResponse response = new GitSyncResponse
		{
			Skipped = skipped,
			InvalidPat = invalidPat,
			RebuildRequests = rebuildRequests,
			DeleteRequests = deleteRequests
		};

		ProgressTracker progressTracker = null;

		if (skipped || !CheckGitSyncConfiguration() || !CheckRequestFlags(request))
		{
			if (skipped)
			{
				logger.LogWarning("Skipped git sync because time for next retry is not up yet");
			}

			// Create progress tracker on admin page to display failure
			progressTracker = new ProgressTracker(repositoryProgress, Progress.GitSyncId,
				ProgressType.GitSync, request.Id, request.RequestedBy, request.RequestedDate,
				request.RuleId, logger);

			await progressTracker.Failed();

			return response;
		}

		bool changedRule = request.RuleId != string.Empty;

		using var timing = logger.TimeOperation(TimeSpan.FromMinutes(1),
			"Git sync");

		try
		{
			bool cloned = await InitOrUpdateWorkingDirectory(request, changedRule);

			var cloneResult = new ProcessResult();

			string rulesDirectory = gitService.GetRulesDirectory(request.RequestedBy);

			if (cloned)
			{
				cloneResult = await fileService.UploadRulesFromDirectory(rulesDirectory, request.RequestedBy);
			}
			else if (!changedRule)
			{
				// If a clone didn't happen, upload any rules that may exist in the working directory
				// that somehow didn't get imported into the db. 99% of the time this is redundant
				var result = await fileService.UploadRulesFromDirectory(rulesDirectory, request.RequestedBy, overwrite: false);

				if (result.SaveCount > 0)
				{
					logger.LogWarning("Uploaded {count} out-of-sync entities in database", result.SaveCount);
				}
			}

			GitFileChangesResult stagedChanges = new(Array.Empty<string>(), Array.Empty<string>());

			if (!request.CloneOnly)
			{
				stagedChanges = await gitService.Stage(request.RequestedBy);
			}

			bool localChangesExist = stagedChanges.FileChanges.Any() || stagedChanges.FileDeletes.Any();
			bool remoteChangesExist = await RemoteChangesExist(request);

			localChangesExist &= !request.CloneOnly;
			remoteChangesExist &= !request.CloneOnly;

			if (ShouldIgnoreGitSync(request, localChangesExist, remoteChangesExist, cloned))
			{
				return response;
			}

			logger.LogInformation("Git sync: found changes. local={local}, remote={remote}, cloned={cloned}", localChangesExist, remoteChangesExist, cloned);

			//dont save to db just get the objects
			var stagedResult = await fileService.UploadRules(stagedChanges.FileChanges, request.RequestedBy, save: false);

			// Create progress tracker on admin page only at this point
			progressTracker = new ProgressTracker(repositoryProgress, Progress.GitSyncId,
				ProgressType.GitSync, request.Id, request.RequestedBy, request.RequestedDate,
				request.RuleId, logger);

			await progressTracker.Start();

			// Commit only if there are local changes
			if (localChangesExist)
			{
				await gitService.Commit(stagedChanges.CommitMessage, request.RequestedBy, request.UserEmail);

				auditLogger.LogInformation(
					request.UserEmail,
					new Dictionary<string, object> { ["CommitMessage"] = stagedChanges.CommitMessage },
					"Git commit {message}",
					stagedChanges.CommitMessage);
			}

			var uploadResult = new ProcessResult();

			// Pull from rules library fork only if there are remote changes
			if (remoteChangesExist)
			{
				var pullChanges = await gitService.Pull(request.RequestedBy, request.UserEmail);

				uploadResult = await fileService.UploadEntities(pullChanges.FileChanges, pullChanges.FileDeletes, request.RequestedBy);

				logger.LogInformation("Git pull: {summary}", uploadResult.Summary);
			}

			var syncedRules = uploadResult.Rules;
			var deletedRules = uploadResult.DeletedRules;
			bool hasGlobalChanges = uploadResult.Globals.Any() || uploadResult.DeletedGlobals.Any();

			bool rebuildAll = false;
			// Create rebuild requests for synced rules only if requested to do so
			if (request.RebuildSyncedRules)
			{
				(rebuildRequests, rebuildAll) =
					await CreateRebuildRequests(syncedRules, request.RequestedBy);
			}

			// Create rebuild requests for uploaded rules only if requested to do so
			if (request.RebuildUploadedRules)
			{
				(var uploadRebuildRequests, bool rebuildAllUpload) =
					await CreateRebuildRequests(stagedResult.Rules, request.RequestedBy);

				// Adding uploadRebuildRequests to rebuildRequests to store all rebuild
				// requests together before sending them over. Will only add requests
				// if a rebuild-all has not already been requested
				if (!rebuildAll)
				{
					foreach (var req in uploadRebuildRequests)
					{
						rebuildRequests.Add(req);
					}
				}

				rebuildAll |= rebuildAllUpload;
			}

			// Always create delete requests
			deleteRequests = await CreateDeleteRequests(deletedRules, request.RequestedBy);

			// Render all values on progress tracker
			await RenderProgressValues(request,
				progressTracker,
				changedRule,
				rebuildAll,
				uploadResult.ChangeCount,
				cloneResult.UniqueCount,
				uploadResult.DeleteCount,
				stagedResult.UniqueCount,
				rebuildRequests.Count);

			// Push to rules library fork only if there were local changes
			if (localChangesExist)
			{
				await gitService.Push(request.RequestedBy);
				logger.LogInformation("Git push: {summary}", uploadResult.Summary);
			}

			// Reset exponential backoff and nextPatRetry on success
			nextRetry = DateTimeOffset.UtcNow;
			exponentialBackOff.Reset();

			healthCheck.Current = HealthCheckGitSync.UpToDate;

			await progressTracker.Completed();
		}
		catch (GitService.InvalidPATException e)
		{
			invalidPat = true;

			if (exponentialBackOff.Count() > 2)
			{
				logger.LogError(e, "Encountered invalid PAT {rule}. Total Retries {count}", request.RuleId, exponentialBackOff.Count());
			}
			else
			{
				logger.LogWarning("Encountered invalid PAT {rule}. Total Retries {count}", request.RuleId, exponentialBackOff.Count());
			}

			if (progressTracker != null)
			{
				await progressTracker.Failed();
			}
		}
		catch (OperationCanceledException e)
		{
			logger.LogError(e, "Git sync cancelled {rule}", request.RuleId);

			if (progressTracker != null)
			{
				await progressTracker.Cancelled();
			}
		}
		catch (Exception e)
		{
			logger.LogError(e, "Failed to sync {rule}", request.RuleId);

			if (progressTracker != null)
			{
				await progressTracker.Failed();
			}
		}

		response.InvalidPat = invalidPat;
		response.RebuildRequests = rebuildRequests;
		response.DeleteRequests = deleteRequests;

		return response;
	}

	/// <summary>
	/// Creates rebuild requests for the specified rules. If more than 20% of the rules
	/// need to be rebuilt, requests to rebuild all rules.
	/// </summary>
	/// <returns>
	/// The requests, and whether a "rebuild all" was requested.
	/// </returns>
	public async Task<(IList<RuleExecutionRequest> requests, bool rebuildAll)>
		CreateRebuildRequests(IEnumerable<Rule> rules, string username)
	{
		IList<RuleExecutionRequest> requests = new List<RuleExecutionRequest>();
		bool rebuildAll = false;

		if (!rules.Any())
		{
			return (requests, rebuildAll);
		}

		int totalRuleCount = await repositoryRules.Count(x => true);

		// If more than 20% of the rules need to be rebuilt, request to rebuild them all
		if (totalRuleCount == 0 || (double)rules.Count() / totalRuleCount > 0.2)
		{
			logger.LogInformation("Requesting processor to rebuild all rules");
			var request = RuleExecutionRequest.CreateRuleExpansionRequest(
				willowEnvironment.Id, force: true, requestedBy: username);

			requests.Add(request);
			rebuildAll = true;
		}
		else // Otherwise, request to rebuild them individually
		{
			foreach (var rule in rules)
			{
				logger.LogInformation("Requesting processor to rebuild rule \"{rule}\"",
					rule.Name);
				var request = RuleExecutionRequest.CreateRuleExpansionRequest(
					willowEnvironment.Id, force: true, requestedBy: username, ruleId: rule.Id);

				requests.Add(request);
			}
		}

		return (requests, rebuildAll);
	}

	/// <summary>
	/// Creates delete requests for the specified rules.
	/// </summary>
	public async Task<IList<RuleExecutionRequest>>
		CreateDeleteRequests(IEnumerable<Rule> rules, string username)
	{
		IList<RuleExecutionRequest> requests = new List<RuleExecutionRequest>();

		if (!rules.Any())
		{
			return requests;
		}

		int totalRuleCount = await repositoryRules.Count(x => true);

		// Obviously don't want to request processor to delete all rules, so requesting
		// processor to delete rules individually
		foreach (var rule in rules)
		{
			logger.LogInformation("Requesting processor to delete rule \"{rule}\"",
				rule.Name);
			var request = RuleExecutionRequest.CreateDeleteRuleRequest(willowEnvironment.Id,
				rule.Id, username);

			requests.Add(request);
		}

		return requests;
	}

	/// <summary>
	/// Determines whether remote changes (modified or deleted rules) exist.
	/// </summary>
	/// <remarks>
	/// This is a cheap operation (git fetch).
	/// </remarks>
	/// <returns>Whether remote chages exist.</returns>
	private async Task<bool> RemoteChangesExist(GitSyncRequest request)
	{
		if (!CheckGitSyncConfiguration())
		{
			return false;
		}

		return await gitService.Fetch(request.RequestedBy);
	}

	/// <summary>
	/// Renders all progress values of the specified git sync request.
	/// </summary>
	private async Task RenderProgressValues(GitSyncRequest request,
		ProgressTracker progressTracker, bool changedRule, bool rebuildAll,
		int syncedCount, int clonedCount, int deletedCount, int stagedCount,
		int rebuildRequestCount)
	{
		int newRuleCount = syncedCount + clonedCount;
		if (newRuleCount > 0)
		{
			await progressTracker.SetValues("Synced Updates", newRuleCount,
				newRuleCount, isIgnored: false, force: true);
		}

		if (deletedCount > 0)
		{
			await progressTracker.SetValues("Synced Deletions", deletedCount,
				deletedCount, isIgnored: false, force: true);
		}

		if (stagedCount > 0)
		{
			await progressTracker.SetValues("Uploaded", stagedCount,
				stagedCount, isIgnored: false, force: true);
		}

		if (changedRule)
		{
			string valueName = request.DeleteRule ? "Deleted" : "Updated";
			await progressTracker.SetValues(valueName, 1, 1, isIgnored: false,
				force: true);
		}

		if (rebuildRequestCount > 0)
		{
			if (rebuildAll)
			{
				await progressTracker.SetValues("Queued Rebuild-All Request",
					rebuildRequestCount, rebuildRequestCount, isIgnored: false,
					force: true);
			}
			else
			{
				await progressTracker.SetValues("Queued Rebuild Requests",
					rebuildRequestCount, syncedCount +
					stagedCount, isIgnored: true, force: true);
			}
		}
	}

	/// <summary>
	/// Returns whether the specified git sync request should be ignored.
	/// Currently, a request is NOT ignored only if a) rules were changed locally
	/// (modified, deleted, or uploaded OR b) remote changes were found (fetched)
	/// OR c) a clone took place.
	/// </summary>
	private bool ShouldIgnoreGitSync(GitSyncRequest request,
		bool localChangesExist, bool remoteChangesExist, bool cloned)
	{
		if (!localChangesExist && !remoteChangesExist && !cloned)
		{
			healthCheck.Current = HealthCheckGitSync.UpToDate;

			return true;
		}

		return false;
	}

	/// <summary>
	/// Either initialize (clone from remote) or update the working directory
	/// based on the rules that were updated in the database.
	/// </summary>
	/// <returns>The number of rules that were cloned (and imported).
	/// If no clone occured, this is zero.
	/// </returns>
	private async Task<bool> InitOrUpdateWorkingDirectory(GitSyncRequest request,
		bool changedRule)
	{
		// First, clone (if not already cloned) and import
		// As of now, not queueing freshly cloned rules for rebuild,
		// just importing them into the database
		bool cloned = await gitService.Clone(request.RequestedBy);

		string rulesDirectory = gitService.GetRulesDirectory(request.RequestedBy);

		if (changedRule)
		{
			string directory;

			if (request.DeleteRule)
			{
				directory = Path.Combine(rulesDirectory, request.SyncFolder);
				var result = await fileService.DeleteIdFromDisk(request.RuleId, directory, deleteEmptyDirectories: true);
				logger.LogInformation("Deleted file {summary}", result.Summary);
			}

			//The file location might have changed
			if (!string.IsNullOrWhiteSpace(request.OldSyncFolder))
			{
				directory = Path.Combine(rulesDirectory, request.OldSyncFolder);
				await fileService.DeleteIdFromDisk(request.RuleId, directory, deleteEmptyDirectories: true);
				logger.LogInformation("File {ruleId} deleted from {oldSyncFolder} and will move to {syncFolder}", request.RuleId, request.OldSyncFolder, request.SyncFolder.TrimModelId());
			}
		}

		// Overwriting all rules in working directory
		await fileService.WriteAllEntitiesToDisk(rulesDirectory);

		return cloned;
	}

	/// <summary>
	/// Checks git sync configuration, initializing any unconfigured options
	/// </summary>
	/// <returns>Whether the configuration is sufficient for git sync to run.</returns>
	private bool CheckGitSyncConfiguration()
	{
		if (gitOptions.Value == null)
		{
			healthCheck.Current = HealthCheckGitSync.NotConfigured;
			logger.LogWarning("Fatal: GitSync options are not configured");

			return false;
		}

		bool enabled = true;

		if (string.IsNullOrEmpty(gitOptions.Value.GithubURI))
		{
			healthCheck.Current = HealthCheckGitSync.NotConfigured;
			logger.LogWarning("Remote Github URI is not configured");
			enabled = false;
		}

		if (string.IsNullOrEmpty(gitOptions.Value.PAT))
		{
			//switch pat will set status
			enabled &= gitService.SwitchPAT();
		}

		if (gitOptions.Value.StandardRulesPath is null)
		{
			healthCheck.Current = HealthCheckGitSync.NotConfigured;
			logger.LogWarning("Relative path for standard rules is not configured, defaulting to root directory");
			gitOptions.Value.StandardRulesPath ??= string.Empty;
		}

		if (!enabled)
		{
			logger.LogWarning("Git sync aborted: {status}", healthCheck.Current.Description);
		}

		return enabled;
	}

	/// <summary>
	/// Checks the git sync request flags for validity. For example, a git sync request is
	/// illegal if it requests a simultaneous deletion and upload.
	/// </summary>
	/// <returns>Whether the request flags are valid.</returns>
	private bool CheckRequestFlags(GitSyncRequest request)
	{
		bool valid = true;

		if (request.UploadedRules && request.RuleId != string.Empty)
		{
			logger.LogWarning("Rules cannot be simulatenously uploaded and modified");
			valid = false;
		}

		if (request.UploadedRules && request.DeleteRule)
		{
			logger.LogWarning("Rules cannot be simultaneously uploaded and deleted");
			valid = false;
		}

		if (!valid)
		{
			healthCheck.Current = HealthCheckGitSync.IllegalRequest;
			logger.LogWarning("Git sync aborted: illegal request");
		}

		return valid;
	}
}

/// <summary>
/// Response for a git sync operation.
/// </summary>
public class GitSyncResponse
{
	/// <summary>
	/// An indicator for whether the git sync request was skipped.
	/// A git sync request is currently only skipped if it is requested
	/// before the next git sync retry time.
	/// </summary>
	public bool Skipped { get; set; }

	/// <summary>
	/// An indicator for whether the Github PAT was invalid.
	/// </summary>
	public bool InvalidPat { get; set; }

	/// <summary>
	/// The rebuild requests for any rules that were updated.
	/// </summary>
	public IList<RuleExecutionRequest> RebuildRequests { get; set; }

	/// <summary>
	/// The delete requests for any rules that were deleted.
	/// </summary>
	public IList<RuleExecutionRequest> DeleteRequests { get; set; }
}
