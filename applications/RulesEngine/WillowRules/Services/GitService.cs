using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RulesEngine.Processor.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.Configuration;
using Willow.Rules.Logging;

namespace Willow.Rules.Services;

/// <summary>
/// Service for pushing/pulling from standard rules library Github repository.
/// </summary>
public interface IGitService
{
	/// <summary>
	/// Checks if the local skills library repository exists, and if not, clones it.
	/// </summary>
	/// <returns>
	/// Whether a git clone took place.
	/// </returns>
	Task<bool> Clone(string username);

	/// <summary>
	/// Fetches changes from the remote skills library fork.
	/// </summary>
	/// <returns>
	/// Whether remote changes were found.
	/// </returns>
	Task<bool> Fetch(string username);

	/// <summary>
	/// Pushes to the remote skills library fork.
	/// </summary>
	/// <remarks>
	/// Will fail to push if remote is ahead in commits. Calling
	/// <see cref="Pull"/> directly before will ensure
	/// this cannot happen.
	/// </remarks>
	Task Push(string username);

	/// <summary>
	/// Returns the absolute path of the rules directory.
	/// </summary>
	string GetRulesDirectory(string username);

	/// <summary>
	/// Pulls all updated files from the remote skills library fork.
	/// </summary>
	/// <returns>
	/// All file changes as a <see cref="GitFileChangesResult"/>.
	/// </returns>
	Task<GitFileChangesResult> Pull(string username, string email);

	/// <summary>
	/// Stages all modified and deleted files in the working directory.
	/// </summary>
	/// <returns>
	/// All modified and deleted files as a <see cref="GitFileChangesResult"/>.
	/// </returns>
	Task<GitFileChangesResult> Stage(string username);

	/// <summary>
	/// Creates a new commit with the file(s) currently added to the staging area.
	/// </summary>
	Task Commit(string message, string username, string email);

	/// <summary>
	/// Switches the Github PAT that is currently being used by fetching it from
	/// the keyvault. If the primary PAT is currently being used, flips to
	/// the secondary, and vice-versa.
	/// </summary>
	/// <returns>Whether the PAT was successfully switched.</returns>
	bool SwitchPAT();
}

/// <summary>
/// Service for pushing/pulling from standard rules library Github repository.
/// </summary>
public class GitService : IGitService
{
	private readonly ILogger<GitService> logger;
	private readonly HealthCheckKeyVault healthCheckKeyVault;
	private readonly GitSyncOptions gitOptions;
	private readonly RulesOptions rulesOptions;

	private bool isUsingPrimaryPAT;

	/// <summary>
	/// Creates the <see cref="GitService"/>
	/// </summary>
	public GitService(ILogger<GitService> logger,
		HealthCheckKeyVault healthCheckKeyVault,
		IOptions<GitSyncOptions> gitOptions,
		IOptions<RulesOptions> rulesOptions)
	{
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.healthCheckKeyVault = healthCheckKeyVault ?? throw new ArgumentNullException(nameof(healthCheckKeyVault));
		this.gitOptions = gitOptions.Value ?? throw new ArgumentNullException(nameof(gitOptions));
		this.rulesOptions = rulesOptions?.Value ?? throw new ArgumentNullException(nameof(rulesOptions));
	}

	public Task<bool> Clone(string username)
	{
		string workingDirectory = GetWorkingDirectory();

		if (LibGit2Sharp.Repository.IsValid(workingDirectory))
		{
			//already cloned
			return Task.FromResult(false);
		}

		(_, bool cloned) = CloneAndGetRepo(username);

		return Task.FromResult(cloned);
	}

	public Task<bool> Fetch(string username)
	{
		bool changes = false;

		try
		{
			using (var repository = GetRepository(username))
			{
				var remote = repository.Network.Remotes["origin"];
				var refSpecs = remote.FetchRefSpecs.Select(refSpec => refSpec.Specification);

				FetchOptions fetchOptions = new FetchOptions
				{
					Prune = true,
					TagFetchMode = TagFetchMode.Auto,
					CredentialsProvider = (_url, _user, _cred) =>
						new UsernamePasswordCredentials
						{
							Username = "pat",
							Password = gitOptions.PAT
						}
				};

				Commands.Fetch(repository, remote.Name, refSpecs, fetchOptions, string.Empty);

				int? behindBy = repository.Head.TrackingDetails.BehindBy;
				if (behindBy != null)
				{
					if (behindBy > 0)
					{
						changes = true;
						logger.LogInformation("Fetched remote; {behindBy} commits behind",
							behindBy);
					}
				}
				else
				{
					logger.LogError("Upstream branch for {branch} does not exist",
						repository.Head.FriendlyName);
				}
			}
		}
		catch (LibGit2SharpException e)
		{
			throw new InvalidPATException("Invalid PAT encountered while fetching", e);
		}
		catch (Exception e)
		{
			logger.LogError(e, "Failed to fetch from {remote}. {error}",
				gitOptions.GithubURI, e.Message);
		}

		return Task.FromResult(changes);
	}

	public Task Push(string username)
	{
		using var timedLogger = logger.TimeOperation("Git push");
		using var scope = logger.BeginScope(new Dictionary<string, object>
		{
			["Username"] = username
		});

		try
		{
			using (var repository = GetRepository(username))
			{
				PushOptions pushOptions = new PushOptions
				{
					CredentialsProvider = (_url, _user, _cred) =>
					new UsernamePasswordCredentials
					{
						Username = "pat",
						Password = gitOptions.PAT
					}
				};

				try
				{
					repository.Network.Push(repository.Head, pushOptions);
					logger.LogInformation("Pushed to {remote} on branch {branch}",
						gitOptions.GithubURI, repository.Head.FriendlyName);
				}
				catch (LibGit2SharpException e)
				{
					logger.LogError(e, "Failed to push to {remote}. {error}",
						gitOptions.GithubURI, e.Message);
				}
			}
		}
		catch (LibGit2SharpException e)
		{
			throw new InvalidPATException("Invalid PAT encountered while pushing", e);
		}
		catch (Exception e)
		{
			logger.LogError(e, "Failed to push to {remote}. {e.Message}",
				gitOptions.GithubURI, e.Message);
		}

		return Task.CompletedTask;
	}

	public Task<GitFileChangesResult> Stage(string username)
	{
		var fileChanges = Array.Empty<string>();
		var fileDeletes = Array.Empty<string>();

		try
		{
			using (var repository = GetRepository(username))
			{
				var status = repository.RetrieveStatus(new StatusOptions());

				fileChanges = status.Where(v => v.State != FileStatus.DeletedFromWorkdir && v.State != FileStatus.DeletedFromIndex)
								.Select(v => GetFullPath(v.FilePath))
								.ToArray();

				fileDeletes = status.Where(v => v.State == FileStatus.DeletedFromWorkdir || v.State == FileStatus.DeletedFromIndex)
								.Select(v => GetFullPath(v.FilePath))
								.ToArray();

				Commands.Stage(repository, "*");
			}
		}
		catch (Exception e)
		{
			logger.LogError(e, "Failed to stage files in the working directory. {error}", e.Message);
		}

		return Task.FromResult(new GitFileChangesResult(fileChanges, fileDeletes));
	}

	public Task Commit(string message, string username, string email)
	{
		try
		{
			using (var repository = GetRepository(username))
			{
				Signature signature = GetSignature(username, email);
				try
				{
					Commit commit = repository.Commit(message, signature, signature);
					logger.LogInformation("{author} commited with message \"{message}\"",
						commit.Author.Name, commit.MessageShort);
				}
				catch (EmptyCommitException)
				{
					logger.LogInformation("No changes; nothing to commit");
				}
			}
		}
		catch (Exception e)
		{
			logger.LogError(e, "Failed to create new commit \"{message}\". {error}",
				message, e.Message);
		}

		return Task.CompletedTask;
	}

	public Task<GitFileChangesResult> Pull(string username, string email)
	{
		var fileUpdates = Array.Empty<string>();
		var fileDeletes = Array.Empty<string>();

		var changes = PullTreeChanges(username, email);

		if (changes is not null)
		{
			fileUpdates = changes
					.Where(v => v.Status == ChangeKind.Added ||
								v.Status == ChangeKind.Renamed ||
								v.Status == ChangeKind.Modified)
					.Where(v => IsRelativeFileInRulesPath(v.Path))
					.Select(v => GetFullPath(v.Path))
					.ToArray();

			fileDeletes = changes
								.Where(v => v.Status == ChangeKind.Deleted ||
											v.Status == ChangeKind.Renamed)
								.Where(v => IsRelativeFileInRulesPath(v.OldPath))
								.Select(v => GetFullPath(v.OldPath))
								.ToArray();
		}

		return Task.FromResult(new GitFileChangesResult(fileUpdates, fileDeletes));
	}

	public bool SwitchPAT()
	{
		using var loggerScope = logger.BeginScope(new Dictionary<string, object> { ["KeyVaultUri"] = rulesOptions.KeyVaultUri });

		bool success = false;

		// Fetch opposite PAT to what is currently being used
		string secretName = isUsingPrimaryPAT ?
			"rules-github-secondary-pat" : "rules-github-primary-pat";

		try
		{
			logger.LogInformation("Using {PAT} from key vault", secretName);

			var client = new SecretClient(vaultUri: new Uri(rulesOptions.KeyVaultUri),
					credential: new DefaultAzureCredential());

			var secret = client.GetSecret(secretName);

			gitOptions.PAT = secret.Value.Value;
			success = true;

			if (string.IsNullOrWhiteSpace(secret.Value.Value))
			{
				logger.LogWarning("Git sync PAT token is empty. {secretName}", secretName);
				healthCheckKeyVault.Current = HealthCheckKeyVault.MissingSecret;
				success = false;
			}
			else
			{
				healthCheckKeyVault.Current = HealthCheckKeyVault.Healthy;
			}
		}
		catch (System.UriFormatException e)
		{
			healthCheckKeyVault.Current = HealthCheckKeyVault.ConfigurationProblem(rulesOptions.KeyVaultUri);
			logger.LogError(e, "Failed to fetch {PAT} from key vault, bad Uri {uri}", secretName, rulesOptions.KeyVaultUri);
		}
		catch (Exception e)
		{
			// Need a better exception check for this one
			healthCheckKeyVault.Current = HealthCheckKeyVault.AuthorizationFailure;
			logger.LogError(e, "Failed to fetch {PAT} from key vault", secretName);
		}

		isUsingPrimaryPAT = !isUsingPrimaryPAT;

		return success;
	}

	private bool IsRelativeFileInRulesPath(string fileName)
	{
		// If the rules path is not configured, treating the entire working directory
		// as the rules path. So file is guaranteed to be within working directory.
		if (gitOptions.StandardRulesPath.IsNullOrEmpty())
		{
			return true;
		}

		string relativePath = Path.GetRelativePath(gitOptions.StandardRulesPath, fileName);

		// Somehow, there isnt a built in .NET method for this. If the relative path starts
		// with ../, this means that the specified path is outside of the rules path.
		return !relativePath.Replace('\\', '/').StartsWith("../");
	}

	/// <summary>
	/// Pulls from the remote skills library fork. Resolves all merge conflicts
	/// automatically by overwritting with local changes.
	/// </summary>
	/// <returns>
	/// All changes found as a result of the pull, or null if there were no changes or
	/// merge conflicts (should be impossible).
	/// </returns>
	private TreeChanges? PullTreeChanges(string username, string email)
	{
		TreeChanges? changes = null;

		try
		{
			using (var repository = GetRepository(username))
			{
				PullOptions pullOptions = new PullOptions
				{
					FetchOptions = new FetchOptions
					{
						CredentialsProvider = (_url, _user, _cred) =>
							new UsernamePasswordCredentials
							{
								Username = "pat",
								Password = gitOptions.PAT
							}
					},
					MergeOptions = new MergeOptions
					{
						MergeFileFavor = MergeFileFavor.Ours
					}
				};

				Signature signature = GetSignature(username, email);
				Commit lastCommit = repository.Head.Tip;

				MergeResult mergeResult = Commands.Pull(repository, signature, pullOptions);

				switch (mergeResult.Status)
				{
					case MergeStatus.UpToDate:
						{
							logger.LogInformation("Working directory is up to date with remote");
							break;
						}
					case MergeStatus.Conflicts:
						{
							logger.LogError("Encountered merge conflicts while pulling from {remote}",
								gitOptions.GithubURI);
							break;
						}
					default:
						{
							logger.LogInformation("Found changes when pulling from {remote}",
								gitOptions.GithubURI);

							changes = repository.Diff.Compare<TreeChanges>(
								lastCommit.Tree, repository.Head.Tip.Tree);

							foreach (TreeEntryChanges change in changes)
							{
								logger.LogInformation("{status} {path}",
									change.Status, change.Path);
							}
							break;
						}
				}
			}
		}
		catch (LibGit2SharpException e)
		{
			throw new InvalidPATException("Invalid PAT encountered while pulling", e);
		}
		catch (Exception e)
		{
			logger.LogError(e, "Failed to pull from {remote}. {error}",
				gitOptions.GithubURI, e.Message);
		}

		return changes;
	}

	/// <summary>
	/// Gets full path for the file
	/// </summary>
	private static string GetFullPath(string relativePath)
	{
		return Path.Combine(GetWorkingDirectory(), relativePath);
	}

	/// <summary>
	/// Gets the remote skills library fork. If it does not exist,
	/// clones it from Github.
	/// </summary>
	private LibGit2Sharp.Repository GetRepository(string username)
	{
		if (LibGit2Sharp.Repository.IsValid(GetWorkingDirectory()))
		{
			return new LibGit2Sharp.Repository(GetWorkingDirectory());
		}

		(var repository, _) = CloneAndGetRepo(username);

		return repository ?? new LibGit2Sharp.Repository(GetWorkingDirectory());
	}

	/// <summary>
	/// Attempts to clone the remote skills library fork.
	/// If a repository already exists, will skip the cloning.
	/// </summary>
	/// <returns>
	/// Either the preexisting or newly cloned repository, and whether a clone took place.
	/// </returns>
	private (LibGit2Sharp.Repository? repository, bool cloned) CloneAndGetRepo(string username)
	{
		using var timedLogger = logger.TimeOperation("Git clone");
		using var scope = logger.BeginScope(new Dictionary<string, object>
		{
			["Username"] = username
		});

		LibGit2Sharp.Repository? repository = null;
		bool cloned = false;

		try
		{
			CloneOptions cloneOptions = new()
			{
				CredentialsProvider = (_url, _user, _cred) =>
				new UsernamePasswordCredentials
				{
					Username = "pat",
					Password = gitOptions.PAT
				}
			};

			try
			{
				string gitFolder = LibGit2Sharp.Repository.Clone(gitOptions.GithubURI, GetWorkingDirectory(), cloneOptions);

				cloned = true;
				logger.LogInformation("Cloned {remote} to {gitFolder}", gitOptions.GithubURI, gitFolder);
			}
			catch (NameConflictException)
			{
				logger.LogInformation("Skipped cloning because repository already exists");
			}

			repository = new LibGit2Sharp.Repository(GetWorkingDirectory());
		}
		catch (LibGit2SharpException e) when (e.Message.Contains("doesn't point at a valid Git repository"))
		{
			throw;
		}
		catch (LibGit2SharpException e)
		{
			throw new InvalidPATException("Invalid PAT or other error encountered while cloning", e);
		}
		catch (Exception e)
		{
			logger.LogError(e, "Failed to clone from {remote}. {error}",
				gitOptions.GithubURI, e.Message);
		}

		return (repository, cloned);
	}

	public string GetRulesDirectory(string username)
	{
		// In case the repository does not exist or was deleted
		GetRepository(username);

		string directory = Path.Combine(GetWorkingDirectory(), gitOptions.StandardRulesPath);

		if (!Directory.Exists(directory))
		{
			Directory.CreateDirectory(directory);
		}

		return directory;
	}

	private static Signature GetSignature(string username, string email)
	{
		return new Signature(username, email, DateTime.UtcNow);
	}

	private static string GetWorkingDirectory()
	{
		string path = Path.Combine(Path.GetTempPath(), "Willow", "Rules Library");

		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		return path;
	}

	/// <summary>
	/// An exception that occured when a Github PAT was invalid.
	/// </summary>
	public class InvalidPATException : Exception
	{
		/// <summary>
		/// Creates a new instance of the <see cref="InvalidPATException"/> class
		/// </summary>
		public InvalidPATException(string message) : base(message)
		{
		}

		/// <summary>
		/// Creates a new instance of the <see cref="InvalidPATException"/> class
		/// </summary>
		/// <param name="message"></param>
		public InvalidPATException(string message, Exception ex) : base(message, ex)
		{
		}
	}
}

/// <summary>
/// Represents changes during git stage, git pull, etc
/// </summary>
public class GitFileChangesResult
{
	/// <summary>
	/// Constructor for result
	/// </summary>
	/// <param name="fileChanges"></param>
	/// <param name="fileDeletes"></param>
	public GitFileChangesResult(string[] fileChanges, string[] fileDeletes)
	{
		FileChanges = fileChanges;
		FileDeletes = fileDeletes;
	}

	/// <summary>
	/// Files that Changed, ie add/modified
	/// </summary>
	public string[] FileChanges { get; init; }

	/// <summary>
	/// Delete files
	/// </summary>
	public string[] FileDeletes { get; init; }

	/// <summary>
	/// Generates a friendly commit message for git
	/// </summary>
	public string CommitMessage
	{
		get
		{
			string? result = null;
			bool changedFiles = false;

			if (FileChanges.Length > 0)
			{
				changedFiles = true;

				if (FileChanges.Length == 1)
				{
					result += $"Changed {Path.GetFileName(FileChanges[0])}";
				}
				else
				{
					result += $"Changed {FileChanges.Length} files";
				}
			}

			if (FileDeletes.Length > 0)
			{
				if (changedFiles)
				{
					result += ", deleted ";
				}
				else
				{
					result += "Deleted ";
				}

				if (FileDeletes.Length == 1)
				{
					result += $"{Path.GetFileName(FileDeletes[0])}";
				}
				else
				{
					result += $"{FileDeletes.Length} files";
				}
			}

			return result ?? "No Changes";
		}
	}
}
