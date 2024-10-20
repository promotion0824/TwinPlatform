using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RulesEngine.Processor;
using RulesEngine.Processor.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Configuration;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Sources;

namespace Willow.Rules.Processor;

/// <summary>
/// The real time service runs continuously executing chunks of real-time
/// executions interspersed with batch executions
/// </summary>
public class RealtimeExecutionBackgroundService : BackgroundService
{
	private readonly WillowEnvironment willowEnvironment;
	private readonly IRuleExecutionProcessor ruleExecutionProcessor;
	private readonly ICommandSyncProcessor commandSyncProcessor;
	private readonly IRuleInstanceProcessor ruleInstanceProcessor;
	private readonly IRuleOrchestrator ruleOrchestrator;
	private readonly IRepositoryRuleExecutionRequest repositoryRuleExecutionRequest;
	private readonly IRepositoryLogEntry repositoryLogEntry;
	private readonly ILogger<RealtimeExecutionBackgroundService> logger;
	private static TimeSpan interval = TimeSpan.FromSeconds(5);

	/// <summary>
	/// Creates the <see cref="RealtimeExecutionBackgroundService" />
	/// </summary>
	public RealtimeExecutionBackgroundService(
		WillowEnvironment willowEnvironment,
		IRuleExecutionProcessor ruleExecutionProcessor,
		ICommandSyncProcessor commandSyncProcessor,
		IRuleOrchestrator ruleOrchestrator,
		IRepositoryRuleExecutionRequest repositoryRuleExecutionRequest,
		IRuleInstanceProcessor ruleInstanceProcessor,
		IRepositoryLogEntry repositoryLogEntry,
		ILogger<RealtimeExecutionBackgroundService> logger)
	{
		this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
		this.ruleExecutionProcessor = ruleExecutionProcessor ?? throw new ArgumentNullException(nameof(ruleExecutionProcessor));
		this.commandSyncProcessor = commandSyncProcessor ?? throw new ArgumentNullException(nameof(commandSyncProcessor));
		this.ruleOrchestrator = ruleOrchestrator ?? throw new ArgumentNullException(nameof(ruleOrchestrator));
		this.repositoryRuleExecutionRequest = repositoryRuleExecutionRequest ?? throw new ArgumentNullException(nameof(repositoryRuleExecutionRequest));
		this.ruleInstanceProcessor = ruleInstanceProcessor ?? throw new ArgumentNullException(nameof(ruleInstanceProcessor));
		this.repositoryLogEntry = repositoryLogEntry ?? throw new ArgumentNullException(nameof(repositoryLogEntry));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Runs the background service until cancelled
	/// </summary>
	protected override Task ExecuteAsync(CancellationToken cancellationToken)
	{
		// See https://github.com/dotnet/runtime/issues/36063#issuecomment-518913079
		return Task.Run(async () =>
		{
			using var logScope = logger.BeginScope("Real-time execution service");

			await Task.Yield();

			logger.LogInformation("Real-time execution starting");
			await Task.Delay(TimeSpan.FromSeconds(10));  // give migrations etc. a chance

			try
			{
				logger.LogInformation("Real-time execution executing");

				// Main execution loop, don't crash it!
				while (!cancellationToken.IsCancellationRequested)
				{
					try
					{
						await DoWork(cancellationToken);
						LogAlive();
					}
					catch (TaskCanceledException)
					{
						logger.LogInformation("Real-time execution work cancelled by shutdown");
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "Real-time execution failure");
					}

					//         ruleOrchestrator.Listen.WaitToReadAsync()
					await Task.Delay(interval, cancellationToken);  // Should wait for a message on Channel
				}

				logger.LogInformation("Real-time execution complete");
			}
			catch (TaskCanceledException)
			{
				logger.LogInformation("Real-time execution loop closed by shutdown");
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Real-time execution failed to run");
			}
		});
	}

	private void LogAlive()
	{
		var assemblyInfo = System.Reflection.Assembly.GetEntryAssembly()?.GetName();
		string name = assemblyInfo.Name;
		var version = assemblyInfo?.Version;
		logger.LogDebug("{name} alive, version {version} working set {ws:0.0}GB", name, version, Environment.WorkingSet / 1024.0 / 1024.0 / 1024.0);
	}

	private async Task DoWork(CancellationToken cancellationToken)
	{
		bool incomingRequest = ruleOrchestrator.Listen.TryRead(out var request);

		if (incomingRequest)
		{
			// execution keeps its own "queue" so we have to manage the request that came in
			// and only delete when the batch starts, not when
			// the request is registered on the rule orchestator queue
			var existingRequestCount = await repositoryRuleExecutionRequest.Count(x => x.Id == request.Id);

			//it may have been cancelled, so just skip out
			if (existingRequestCount > 0)
			{
				//we can delete the queue request now
				logger.LogInformation("Deleting the requested rule execution by {by}", request.RequestedBy);
				await repositoryRuleExecutionRequest.DeleteOne(request);
			}
			else
			{
				logger.LogInformation("Batch request deleted/cancelled");
				return;
			}
		}

		request = request ?? RuleExecutionRequest.CreateRealtimeExecutionRequest(willowEnvironment.Id, RulesOptions.ProcessorCloudRoleName);

		using var disp = logger.BeginRequestScope(request);

		if (request.ProgressId == Progress.RealtimeExecutionId)
		{
			using (var timedLogger = logger.TimeOperation("Running real-time chunk"))
			{
				await this.ruleExecutionProcessor.Execute(request, isRealtime: true, cancellationToken);
			}
		}
		else
		{
			switch (request.Command)
			{
				case RuleExecutionCommandType.DeleteRule:
					{
						logger.LogInformation("{customerId}: Message received, {progressId}", request.CustomerEnvironmentId, request.ProgressId);

						await repositoryRuleExecutionRequest.DeleteOne(request);

						await ruleInstanceProcessor.DeleteRule(request, cancellationToken);

						if (!cancellationToken.IsCancellationRequested)
						{
							await ruleOrchestrator.RuleDeleteCompleted(cancellationToken);
						}

						break;
					}
				case RuleExecutionCommandType.ProcessDateRange:
					{
						using (var timedLogger = logger.TimeOperation("Running requested chunk {ruleId}", request.RuleId))
						{
							//register a cancellation token for batch runs so it can be cancelled by the user
							using (var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, ruleOrchestrator.GetCancellationToken(request.ProgressId)))
							{
								await this.ruleExecutionProcessor.Execute(request, isRealtime: false, tokenSource.Token);
								await ruleOrchestrator.ProcessDateRangeCompleted(tokenSource.Token);
							}
						}

						break;
					}
				case RuleExecutionCommandType.DeleteAllInsights:
					{
						await commandSyncProcessor.DeleteAllInsights(request, cancellationToken);

						break;
					}
				case RuleExecutionCommandType.DeleteAllMatchingInsights:
					{
						await commandSyncProcessor.DeleteAllMatchingInsights(request, cancellationToken);

						break;
					}
				case RuleExecutionCommandType.DeleteAllMatchingCommands:
					{
						await commandSyncProcessor.DeleteAllMatchingCommands(request, cancellationToken);

						break;
					}
				case RuleExecutionCommandType.DeleteCommandInsights:
					{
						await commandSyncProcessor.DeleteCommandInsightsNotSynced(request, cancellationToken);

						break;
					}
				case RuleExecutionCommandType.ReverseSyncInsights:
					{
						await commandSyncProcessor.ReverseSyncCommandInsights(request, cancellationToken);

						break;
					}
				case RuleExecutionCommandType.SyncCommandEnabled:
					{
						await commandSyncProcessor.SyncCommandEnabled(request, cancellationToken);

						break;
					}
				default:
					{
						logger.LogInformation("Real-time execution message received with unknown command {command}", request.Command);
						break;
					}
			}
		}

		await repositoryLogEntry.PruneLogs();
	}
}
