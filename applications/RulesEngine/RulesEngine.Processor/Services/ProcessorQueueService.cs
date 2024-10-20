using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Processor;
using Willow.Rules.Repository;
using Willow.Rules.Search;
using Willow.Rules.Services;
using WillowRules.Extensions;

namespace RulesEngine.Processor.Services;

/// <summary>
/// A service to process incoming execution requests
/// </summary>
public class ProcessorQueueServiceBackgroundService : BackgroundService
{
	private readonly ILogger<ProcessorQueueServiceBackgroundService> logger;
	private readonly IRepositoryRuleExecutionRequest repositoryRuleExecutionRequest;
	private readonly IRuleInstanceProcessor ruleInstanceProcessor;
	private readonly IRuleOrchestrator ruleOrchestrator;
	private readonly IGitSyncOrchestrator gitSyncOrchestrator;
	private readonly IRepositoryProgress repositoryProgress;
	private readonly IDiagnosticsProcessor diagnosticsProcessor;
	private readonly IRulesSearchBuilderService searchBuilderService;
	private readonly ICalculatedPointsProcessor calculatedPointsProcessor;
	private readonly IMemoryCache memoryCache;
	private static TimeSpan interval = TimeSpan.FromSeconds(5);

	/// <summary>
	/// Service Constructor
	/// </summary>
	public ProcessorQueueServiceBackgroundService(
		IRepositoryRuleExecutionRequest repositoryRuleExecutionRequest,
		IRuleInstanceProcessor ruleInstanceProcessor,
		IRuleOrchestrator ruleOrchestrator,
		IGitSyncOrchestrator gitSyncOrchestrator,
		IDiagnosticsProcessor diagnosticsProcessor,
		IRulesSearchBuilderService searchBuilderService,
		ICalculatedPointsProcessor calculatedPointsProcessor,
		IRepositoryProgress repositoryProgress,
		IMemoryCache memoryCache,
		ILogger<ProcessorQueueServiceBackgroundService> logger)
	{
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.repositoryRuleExecutionRequest = repositoryRuleExecutionRequest ?? throw new ArgumentNullException(nameof(repositoryRuleExecutionRequest));
		this.ruleInstanceProcessor = ruleInstanceProcessor ?? throw new ArgumentNullException(nameof(ruleInstanceProcessor));
		this.ruleOrchestrator = ruleOrchestrator ?? throw new ArgumentNullException(nameof(ruleOrchestrator));
		this.gitSyncOrchestrator = gitSyncOrchestrator ?? throw new ArgumentNullException(nameof(gitSyncOrchestrator));
		this.diagnosticsProcessor = diagnosticsProcessor ?? throw new ArgumentNullException(nameof(diagnosticsProcessor));
		this.searchBuilderService = searchBuilderService ?? throw new ArgumentNullException(nameof(searchBuilderService));
		this.calculatedPointsProcessor = calculatedPointsProcessor ?? throw new ArgumentNullException(nameof(calculatedPointsProcessor));
		this.repositoryProgress = repositoryProgress ?? throw new ArgumentNullException(nameof(repositoryProgress));
		this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
	}

	/// <summary>
	/// Start polling
	/// </summary>
	protected override Task ExecuteAsync(CancellationToken cancellationToken)
	{
		// See https://github.com/dotnet/runtime/issues/36063#issuecomment-518913079
		return Task.Run(async () =>
		{
			await Task.Yield();

			try
			{
				var throttledLogger = logger.Throttle(TimeSpan.FromMinutes(5));

				logger.LogInformation("Processor Queue service starting");
				await Task.Delay(TimeSpan.FromSeconds(10));  // give migrations etc. a chance

				var progress = await repositoryProgress.Get(v => true);

				//flag any "in progress" items as failed if service restarted during execution
				foreach (var item in progress)
				{
					if (item.Id != Progress.RealtimeExecutionId && item.Status == ProgressStatus.InProgress)
					{
						item.Status = ProgressStatus.Failed;
						item.FailedReason = "Service Restart";
						await repositoryProgress.UpsertOne(item);
					}
				}

				var requests = await repositoryRuleExecutionRequest.Get(v => true);

				//also delete "requested" requests (batch execution) otherwise it is orphaned
				foreach (var request in requests.Where(v => v.Requested))
				{
					await repositoryRuleExecutionRequest.DeleteOne(request);
				}

				//Polling loop, don't crash it!
				while (!cancellationToken.IsCancellationRequested)
				{
					try
					{
						requests = await repositoryRuleExecutionRequest.Get(v => true);

						throttledLogger.LogInformation("Processor Queue: {count} Items in queue", requests.Count());

						//get the next item in the queue, ignore ones that are being worked on by the real-time engine
						var request = requests.Where(v => !v.Requested).OrderBy(V => V.RequestedDate).FirstOrDefault();

						if (request is not null)
						{
							try
							{
								using (var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, ruleOrchestrator.GetCancellationToken(request.ProgressId)))
								{
									await ProcessRequest(request, tokenSource.Token);

									tokenSource.Token.ThrowIfCancellationRequested();
								}
							}
							catch (OperationCanceledException ex)
							{
								logger.LogError(ex, "Queue processor encountered cancellation for request {id} {progressId}", request.Id, request.ProgressId);

								try
								{
									//ensure item has been deleted in case any cancellations
									await repositoryRuleExecutionRequest.DeleteOne(request);
								}
								catch (Exception ex1)
								{
									logger.LogError(ex1, "Failed to delete queue request {id} {progressId}", request.Id, request.ProgressId);
								}
							}
							catch (Exception ex)
							{
								logger.LogError(ex, "Queue processor failed for request {id} {progressId}", request.Id, request.ProgressId);
							}
						}

						//polling interval
						await Task.Delay(interval, cancellationToken);
					}
					catch (TaskCanceledException)
					{
						// throw it away, shutting down
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "Queue processor failed");
						//delay a bit longer
						await Task.Delay(interval * 10, cancellationToken);
					}
				}
			}
			catch (TaskCanceledException)
			{
				// throw it away, shutting down
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Queue processor failed to start");
			}
		});
	}

	private async Task<bool> ProcessRequest(RuleExecutionRequest request, CancellationToken cancellationToken)
	{
		var timeoutToken = new CancellationTokenSource(6 * 60 * 60 * 1000).Token; //6 hours

		var cancelWhenCancelledOrAfterSixHours = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken).Token;

		switch (request.Command)
		{
			case RuleExecutionCommandType.DeleteAllInsights:
			case RuleExecutionCommandType.DeleteAllMatchingInsights:
			case RuleExecutionCommandType.DeleteCommandInsights:
			case RuleExecutionCommandType.ProcessDateRange:
			case RuleExecutionCommandType.ReverseSyncInsights:
			case RuleExecutionCommandType.DeleteRule:
			case RuleExecutionCommandType.SyncCommandEnabled:
				{
					//only flag as requested. The Rule Execution Service must delete the request when the execution starts
					request.Requested = true;
					await repositoryRuleExecutionRequest.UpsertOne(request);

					logger.LogInformation("{customerId}: Message received, Sending {progressId} to realtime queue. CorrelationId {correlationId}", request.CustomerEnvironmentId, request.ProgressId, request.CorrelationId);

					//Send a message to the real-time engine to do this work
					//Requests for the realtime queue have to wait here for them to get read
					//We cant do TryWrites here similar to when rebuild rules finishes
					//otherwise it drops them due to the queue only allowing 1 entry at a time
					await ruleOrchestrator.Send.WriteAsync(request);
					
					return true;
				}

		}

		using var disp = logger.BeginRequestScope(request);

		switch (request.Command)
		{
			// This will move to a new Container
			case RuleExecutionCommandType.UpdateCache:
				{
					logger.LogInformation("{customerId}: Message received, {progressId}", request.CustomerEnvironmentId, request.ProgressId);

					await repositoryRuleExecutionRequest.DeleteOne(request);

					await ruleInstanceProcessor.RebuildCache(request, cancellationToken);

					memoryCache.Compact();

					GC.Collect();

					if (!cancellationToken.IsCancellationRequested)
					{
						await ruleOrchestrator.UpdateCacheCompleted(cancellationToken);

						if (request.ExtendedData.RefreshSearchAfterwards)
						{
							var searchRequest = RuleExecutionRequest.CreateSearchIndexRefreshRequest(request.CustomerEnvironmentId, true, request.RequestedBy, onlyRefreshTwins: true);

							await repositoryRuleExecutionRequest.UpsertOne(searchRequest);
						}
					}

					break;
				}
			// This will move to a new Container
			case RuleExecutionCommandType.BuildRule:
				{
					logger.LogInformation("{customerId}: Message received, {progressId}", request.CustomerEnvironmentId, request.ProgressId);

					await repositoryRuleExecutionRequest.DeleteOne(request);

					await ruleInstanceProcessor.RebuildRules(request, cancellationToken);

					memoryCache.Compact();

					GC.Collect();

					if (!cancellationToken.IsCancellationRequested)
					{
						//To prevent Realtime from executing after every expansion, check expansions queued and only execute after last one
						var runRealtime = !await repositoryRuleExecutionRequest.Any(r => r.Command == RuleExecutionCommandType.BuildRule);

						await ruleOrchestrator.RebuildRulesCompleted(cancellationToken, runRealtime);
					}

					break;
				}
			// This will move to a new Container
			case RuleExecutionCommandType.RebuildSearchIndex:
				{
					logger.LogInformation("{customerId}: Message received, {progressId}", request.CustomerEnvironmentId, request.ProgressId);

					await repositoryRuleExecutionRequest.DeleteOne(request);

					var progressTracker = new ProgressTracker(repositoryProgress, Progress.SearchIndexRefreshId, ProgressType.SearchIndexRefresh, request.CorrelationId, request.RequestedBy, request.RequestedDate, request.RuleId, logger);

					await progressTracker.Start();

					try
					{
						if (request.RecreateIndex)
						{
							await searchBuilderService.DeleteIndex(cancelWhenCancelledOrAfterSixHours);
						}

						var upsertIndexTimeout = new CancellationTokenSource(TimeSpan.FromMinutes(10)).Token;

						try
						{
							var upsertTask = searchBuilderService.CreateOrUpdateIndex(upsertIndexTimeout);

							//the call to upsert the synonym map hangs intermittently in nau (even though we provide a cancellation token)
							//add a timeout to ensure it cancels out correctly
							await Task.WhenAny(upsertTask).WaitAsync(upsertIndexTimeout);
						}
						catch(OperationCanceledException e)
						{
							logger.LogError(e, "Upsert search index timeout");
							await progressTracker.Failed("Upsert search index timeout");
							break;
						}

						await progressTracker.SetValues("Index", 1, 1);

						if(request.ExtendedData.OnlyRefreshTwins)
						{
							await searchBuilderService.AddModelsAndTwinsToSearchIndex(DateTimeOffset.UtcNow, progressTracker, cancelWhenCancelledOrAfterSixHours);
						}
						else
						{
							await searchBuilderService.AddEverythingToSearchIndex(DateTimeOffset.UtcNow, progressTracker, cancelWhenCancelledOrAfterSixHours);
						}
					}
					catch (Azure.RequestFailedException rfe) when (rfe.Status == 403)
					{
						logger.LogError(rfe, "Failed to rebuild search index - forbidden, not authorized");
						throw;
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "Failed to rebuild search index");
						throw;
					}

					await progressTracker.Completed();

					if (!cancellationToken.IsCancellationRequested)
					{
						await ruleOrchestrator.RebuildSearchIndexCompleted(cancellationToken);
					}

					break;
				}
			case RuleExecutionCommandType.GitSync:
				{
					logger.LogInformation("{customerId}: Message received, {progressId}", request.CustomerEnvironmentId, request.ProgressId);

					await repositoryRuleExecutionRequest.DeleteOne(request);

					var gitSyncRequest = new GitSyncRequest
					{
						Id = request.Id,
						CustomerEnvironmentId = request.CustomerEnvironmentId,
						RequestedBy = request.RequestedBy,
						RequestedDate = request.RequestedDate,
						UserEmail = request.UserEmail,
						RuleId = request.RuleId,
						SyncFolder = request.SyncFolder,
						OldSyncFolder = request.OldSyncFolder,
						DeleteRule = request.DeleteRule,
						RebuildSyncedRules = request.RebuildSyncedRules,
						RebuildUploadedRules = request.RebuildUploadedRules,
						UploadedRules = request.UploadedRules,
						CloneOnly = request.CloneOnly,
						StartDate = request.StartDate,
						TargetEndDate = request.TargetEndDate
					};

					await gitSyncOrchestrator.Send.WriteAsync(gitSyncRequest, cancellationToken);

					if (!cancellationToken.IsCancellationRequested)
					{
						await ruleOrchestrator.GitSyncCompleted(cancellationToken);
					}

					break;
				}
			case RuleExecutionCommandType.ProcessCalculatedPoints:
				{
					logger.LogInformation("{customerId}: Message received, {progressId}", request.CustomerEnvironmentId, request.ProgressId);

					await repositoryRuleExecutionRequest.DeleteOne(request);

					await calculatedPointsProcessor.ProcessCalculatedPoints(request, cancellationToken);

					if (!cancellationToken.IsCancellationRequested)
					{
						await ruleOrchestrator.ProcessCalculatedPointsCompleted(cancellationToken);
					}

					break;
				}
			case RuleExecutionCommandType.RunDiagnostics:
				{
					logger.LogInformation("{customerId}: Message received, {progressId}", request.CustomerEnvironmentId, request.ProgressId);

					await repositoryRuleExecutionRequest.DeleteOne(request);

					await diagnosticsProcessor.RunDiagnostics(request, cancellationToken);

					break;
				}
			default:
				{
					logger.LogInformation("Message received with unknown command {command}", request.Command);
					await repositoryRuleExecutionRequest.DeleteOne(request);
					return false;
				}
		}

		return true;
	}
}
