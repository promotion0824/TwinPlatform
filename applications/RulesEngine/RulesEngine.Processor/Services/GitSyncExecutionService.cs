using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Willow.Extensions.Logging;
using Willow.Rules.Configuration;
using Willow.Rules.Model;
using Willow.Rules.Processor;
using Willow.Rules.Repository;
using Willow.Rules.Sources;

namespace RulesEngine.Processor.Services;

/// <summary>
/// Background service that runs git syncs at scheduled intervals.
/// </summary>
public class GitSyncExecutionService : BackgroundService
{
	private readonly ILogger<GitSyncExecutionService> logger;
	private readonly IRepositoryRuleExecutionRequest repositoryRuleExecutionRequest;
	private readonly WillowEnvironment willowEnvironment;
	private readonly IGitSyncProcessor gitSyncProcessor;
	private readonly IGitSyncOrchestrator gitSyncOrchestrator;
	private static readonly TimeSpan interval = TimeSpan.FromSeconds(5);
	private static DateTimeOffset nextAutoSync = DateTimeOffset.UtcNow;

	/// <summary>
	/// Creates a new <see cref="GitSyncExecutionService"/>.
	/// </summary>
	public GitSyncExecutionService(
		ILogger<GitSyncExecutionService> logger,
		IRepositoryRuleExecutionRequest repositoryRuleExecutionRequest,
		WillowEnvironment willowEnvironment,
		IGitSyncProcessor gitSyncProcessor,
		IGitSyncOrchestrator gitSyncOrchestrator)
	{
		this.logger = logger ??
			throw new ArgumentNullException(nameof(logger));
		this.repositoryRuleExecutionRequest = repositoryRuleExecutionRequest ??
			throw new ArgumentNullException(nameof(repositoryRuleExecutionRequest));
		this.willowEnvironment = willowEnvironment ??
			throw new ArgumentNullException(nameof(willowEnvironment));
		this.gitSyncProcessor = gitSyncProcessor ??
			throw new ArgumentNullException(nameof(gitSyncProcessor));
		this.gitSyncOrchestrator = gitSyncOrchestrator ??
			throw new ArgumentNullException(nameof(gitSyncOrchestrator));
	}

	/// <summary>
	/// Main loop for background service.
	/// </summary>
	protected override Task ExecuteAsync(CancellationToken cancellationToken)
	{
		// See https://github.com/dotnet/runtime/issues/36063#issuecomment-518913079
		return Task.Run(async () =>
		{
			await Task.Yield();

			var throttledLogger = logger.Throttle(TimeSpan.FromMinutes(5));

			logger.LogInformation("Git sync execution starting");
			await Task.Delay(TimeSpan.FromSeconds(10));  // give migrations etc. a chance

			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					try
					{
						// If nextAutoSync time is up, run an automatic git sync
						if (nextAutoSync < DateTimeOffset.UtcNow)
						{
							// Bump the auto sync timer by 15 minutes
							nextAutoSync = DateTimeOffset.UtcNow.AddMinutes(15);

							await DoGitSync(CreateGitSyncRequest(), CancellationToken.None);
						}

						await PollGitSync(cancellationToken);
					}
					catch (OperationCanceledException)
					{
						// ignore it, happens when cancellation token fires on shutdown
					}
					catch (Exception e)
					{
						logger.LogError(e, "Git sync execution failure");
					}

					// Wait for polling interval
					await Task.Delay(interval, cancellationToken);
				}

				logger.LogInformation("Git sync execution complete");
			}
			catch (OperationCanceledException)
			{
				// ignore it, happens when cancellation token fires on shutdown
			}
			catch (Exception e)
			{
				logger.LogError(e, "Git sync execution failed to run");
			}
		});
	}

	private async Task PollGitSync(CancellationToken cancellationToken)
	{
		bool incomingRequest = gitSyncOrchestrator.Listen.TryRead(out var request);

		if (incomingRequest)
		{
			// For now, once git sync requests reach this point they cannot be cancelled
			await DoGitSync(request, CancellationToken.None);
		}
	}

	/// <summary>
	/// Requests for the specified <see cref="GitSyncRequest"/> to run. If the request
	/// is successful and triggers delete/rebuild requests, forwards those requests
	/// to the rule execution processor.
	/// </summary>
	private async Task DoGitSync(GitSyncRequest request, CancellationToken cancellationToken)
	{
		using var scope = logger.BeginRequestScope(Progress.GitSyncId, request.Id, request.RequestedBy);

		var response = await gitSyncProcessor.SyncWithFork(request, cancellationToken);

		if (!response.InvalidPat && !response.Skipped)
		{
			foreach (var req in response.DeleteRequests)
			{
				await repositoryRuleExecutionRequest.UpsertOne(req, cancellationToken: CancellationToken.None);
			}

			foreach (var req in response.RebuildRequests)
			{
				await repositoryRuleExecutionRequest.UpsertOne(req, cancellationToken: cancellationToken);
			}
		}

		if (!cancellationToken.IsCancellationRequested)
		{
			await gitSyncOrchestrator.Completed(cancellationToken);
		}
	}

	/// <summary>
	/// Creates a new <see cref="GitSyncRequest"/> on behalf of the processor
	/// </summary>
	private GitSyncRequest CreateGitSyncRequest()
	{
		return new GitSyncRequest
		{
			Id = Guid.NewGuid().ToString(),
			CustomerEnvironmentId = willowEnvironment.Id,
			RequestedBy = RulesOptions.ProcessorCloudRoleName,
			RequestedDate = DateTimeOffset.UtcNow,
			UserEmail = "rulesprocessor@willowinc.com", // Fake email obviously-- Git will complain if this is empty
			RebuildSyncedRules = true,
			RebuildUploadedRules = true,
			StartDate = DateTime.UtcNow,
			TargetEndDate = DateTime.UtcNow
		};
	}
}
