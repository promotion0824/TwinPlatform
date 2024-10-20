namespace LibGit2SharpPOC;

using LibGit2Sharp;

public class POC
{
	public const string Remote = "https://github.com/andy-stanciu/POC.git";
	public const string RepoFolder = @"C:\Users\AndyStanciu\Documents\LibGit2SharpPOC\repo";
	public const string GithubUsername = "andy-stanciu";
	public const string GithubPAT = "XXXXXXXXXXXXX";

	public static void Main(string[] args)
	{
		// Clone repo from remote
		Repository repo = Clone(Remote, RepoFolder);

		DisplayLocalBranches(repo);
		DisplayRemoteBranches(repo);

		CheckoutBranch(repo, "main");

		CreateBranch(repo, "branch1");
		CheckoutBranch(repo, "branch1");
		Push(repo);

		DisplayLocalBranches(repo);
		DisplayRemoteBranches(repo);

		// Dummy author
		// Signature author = new Signature("First", "email@domain.net", DateTime.Now);

		// Show local branches
		// ShowLocalBranches(repo);

		// Show remote branches
		// ShowRemoteBranches(repo);

		// Creates a file, commits it, and pushes it to the remote repo
		// Signature commmitter = author;
		// CreateFileCommitAndPush(repo, commmitter);

		// Pull from remote repo (main branch)
		// Signature merger = author;
		// Pull(repo, merger);

		// Create branch named "branch1"
		// CreateBranch(repo, "branch1");

		// Checkout branch
		// CheckoutBranch(repo, "branch1");
	}

	/// <summary>
	/// Pulls from the specified repository from the configured upstream remote branch into the branch
	/// that is currently locally checked out. If there are merge conflicts, aborts (temporary).
	/// </summary>
	/// <param name="repository">The repository of interest</param>
	/// <param name="merger">Data of the user who is pulling (merging) this file</param>
	public static void Pull(Repository repository, Signature merger)
	{
		PullOptions options = new PullOptions();
		options.FetchOptions = new FetchOptions();
		options.FetchOptions.CredentialsProvider = (_url, _user, _cred) =>
		  new UsernamePasswordCredentials
		  {
			  Username = GithubUsername,
			  Password = GithubPAT
		  };

		MergeResult mergeResult = Commands.Pull(repository, merger, options);
		if (mergeResult.Commit != null)
		{
			Console.WriteLine("Pulled from " + repository.Network.Remotes.First().Url);
		}
		else
		{
			Console.WriteLine("Pull unsuccessful due to merge conflicts");
		}
		Console.WriteLine("Merge status: " + mergeResult.Status);
	}

	/// <summary>
	/// Pushes the currently selected branch to its tracked branch on the remote repository.
	/// Note: Currently throws if remote branch is ahead in commits.
	/// </summary>
	/// <param name="repository">The repository of interest</param>
	public static void Push(Repository repository)
	{
		PushOptions options = new PushOptions();
		options.CredentialsProvider = (_url, _user, _cred) =>
		  new UsernamePasswordCredentials
		  {
			  Username = GithubUsername,
			  Password = GithubPAT
		  };

		repository.Network.Push(repository.Head, options);
		Console.WriteLine("Pushed to " + repository.Network.Remotes.First().Url + " on branch " + repository.Head.FriendlyName);
	}

	/// <summary>
	/// Stages list of changed files and commits them to their tracked branch on the remote repository 
	/// with the specified message.
	/// </summary>
	/// <param name="repository">The repository of interest</param>
	/// <param name="author">Data of the user who authored this commit</param>
	/// <param name="committer">Data of the user who is committing (often the same as author) </param>
	/// <param name="commitMessage">Commit message to display</param>
	/// <param name="filesToCommit">List of files to commit</param>
	public static void StageAndCommit(Repository repository, Signature author, Signature committer,
									String commitMessage, params string[] filesToCommit)
	{
		foreach (string filename in filesToCommit)
		{
			repository.Index.Add(filename);
			Console.WriteLine("Added " + filename);
		}
		repository.Index.Write();

		try
		{
			Commit commit = repository.Commit(commitMessage, author, committer);
			Console.WriteLine(committer.Name + " commited with message " + commit.Message);
		}
		catch (EmptyCommitException)
		{
			Console.WriteLine("No changes; nothing to commit");
		}
	}

	/// <summary>
	/// Creates a new branch with the specified name and a remote branch with the same name,
	/// setting the local branch to track the remote branch.
	/// </summary>
	/// <param name="repository">The repository of interest</param>
	/// <param name="branchName">The name of the branch</param>
	public static void CreateBranch(Repository repository, string branchName)
	{
		Branch branch = repository.CreateBranch(branchName);
		Remote remote = repository.Network.Remotes.Add(branchName, repository.Network.Remotes.First().Url);
		repository.Branches.Update(branch, b =>
		  {
			  b.Remote = remote.Name;
			  b.UpstreamBranch = branch.CanonicalName;
		  });

		Console.WriteLine("Created new local branch \"" + branch.FriendlyName + "\" tracking remote branch \"" + remote.Name + "\"");
	}

	/// <summary>
	/// Checks out (switches to) the specified branch.
	/// </summary>
	/// <param name="repository">The repository of interest</param>
	/// <param name="branchName">The name of the branch</param>
	/// <exception cref="UnbornBranchException">If the specified branch does not exist</exception>
	public static void CheckoutBranch(Repository repository, string branchName)
	{
		Branch branch = repository.Branches[branchName];
		if (branch == null)
		{
			throw new UnbornBranchException("Could not checkout branch \"" + branchName + "\" because it does not exist");
		}
		Commands.Checkout(repository, branch);
		Console.WriteLine("Switched to branch \"" + branchName + "\"");
	}

	/// <summary>
	/// Deletes the branch with the specified name and (if it exists) the remote branch 
	/// that is tracking it.
	/// </summary>
	/// <param name="repository">The repository of interest</param>
	/// <param name="branchName">The name of the branch</param>
	/// <exception cref="UnbornBranchException">If the specified branch does not exist</exception>
	public static void DeleteBranch(Repository repository, string branchName)
	{
		Branch branch = repository.Branches[branchName];
		if (branch == null)
		{
			throw new UnbornBranchException("Could not delete local branch \"" + branchName + "\" because it does not exist");
		}

		string remoteName = branch.RemoteName;
		repository.Branches.Remove(branch);
		Console.WriteLine("Deleted local branch \"" + branchName + "\"");

		if (remoteName != null)
		{
			repository.Network.Remotes.Remove(remoteName);
			Console.WriteLine("Deleted tracked remote branch \"" + remoteName + "\"");
		}
		else
		{
			Console.WriteLine("Warning: did not find any tracked remote branch to delete");
		}
	}

	/// <summary>
	/// Prints all local branches of the specified repository.
	/// </summary>
	/// <param name="repository">The repository of interest</param>
	public static void DisplayLocalBranches(Repository repository)
	{
		Console.WriteLine("List of local branches");
		DisplayBranches(repository, false);
	}

	/// <summary>
	/// Prints all remote branches of the specified repository.
	/// </summary>
	/// <param name="repository">The repository of interest</param>
	public static void DisplayRemoteBranches(Repository repository)
	{
		Console.WriteLine("List of remote branches");
		DisplayBranches(repository, true);
	}

	// necessary to implement?
	public static void MergeBranch(Repository repo, string branchName, Signature merger)
	{
		Branch branch = repo.Branches[branchName];
		if (branch == null)
		{
			throw new UnbornBranchException("Could not merge branch \"" + branchName + "\" because it does not exist");
		}

		MergeOptions mergeOptions = new MergeOptions();
		mergeOptions.FileConflictStrategy = CheckoutFileConflictStrategy.Theirs;
		repo.Merge(branch.Tip, merger, mergeOptions);
	}

	/// <summary>
	/// Clones a repository based on the specified url into the specified local folder.
	/// If a repository already exists at the specified local, will skip the cloning.
	/// </summary>
	/// <param name="url">The url of the repository of interest</param>
	/// <param name="repositoryFolder">The path to local folder to clone into</param>
	/// <returns>Either the cloned repository or the repository that already exists at the specified folder.</returns>
	public static Repository Clone(String url, String repositoryFolder)
	{
		CloneOptions options = new CloneOptions();
		options.CredentialsProvider = (_url, _user, _cred) =>
		  new UsernamePasswordCredentials
		  {
			  Username = GithubUsername,
			  Password = GithubPAT
		  };

		try
		{
			string repoPath = Repository.Clone(url, repositoryFolder, options);
			Console.WriteLine("Cloned " + url + " to " + repoPath);
		}
		catch (NameConflictException)
		{
			Console.WriteLine("Skipped cloning because repository already exists");
		}
		return new Repository(repositoryFolder);
	}

	/// <summary>
	/// Initializes an empty repository at the specified path.
	/// </summary>
	/// <param name="repositoryPath"></param>
	/// <returns>The initialized repository.</returns>
	public static Repository Init(String repositoryPath)
	{
		string path = Repository.Init(repositoryPath);
		Console.WriteLine("Initialized empty Git repository in " + path);
		return new Repository(repositoryPath);
	}

	/// <summary>
	/// Helper method to print either local or remote branches of the specified repository.
	/// </summary>
	/// <param name="repository">The repository of interest</param>
	/// <param name="isRemote">Whether to print local or remote branches</param>
	private static void DisplayBranches(Repository repository, bool isRemote)
	{
		foreach (Branch b in repository.Branches.Where(b => b.IsRemote == isRemote))
		{
			Console.WriteLine(string.Format("- {0}{1}", b.IsCurrentRepositoryHead ? "*" : " ", b.FriendlyName));
		}
	}

	/// <summary>
	/// Testing method to create a file and then stage, commit, and push it to the remote repository.
	/// </summary>
	/// <param name="repository">The repository of interest</param>
	/// <param name="committer">Data of the user who is commiting this file</param>
	private static void CreateFileCommitAndPush(Repository repository, Signature committer)
	{
		// Create new file
		File.WriteAllText(Path.Combine(repository.Info.WorkingDirectory, "foo.txt"), "foo");
		Console.WriteLine("Created file foo.txt");

		// Stages and commits file
		StageAndCommit(repository, committer, committer, "Write foo.txt", "foo.txt");

		// Pushes to remote repo on whatever branch is 
		Push(repository);
	}
}