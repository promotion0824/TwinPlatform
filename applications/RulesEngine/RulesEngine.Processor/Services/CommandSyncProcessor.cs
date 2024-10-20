using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using Willow.Rules.Sources;
using Willow.ServiceBus;

namespace RulesEngine.Processor.Services;

/// <summary>
/// Various Command integration methods
/// </summary>
public interface ICommandSyncProcessor
{
	/// <summary>
	/// Deletes any insights marked not to sync but already pushed to Command.
	/// </summary>
	Task DeleteCommandInsightsNotSynced(RuleExecutionRequest request, CancellationToken cancellationToken = default);

	/// <summary>
	/// Reverse syncs the command insight ids back to the rules engine <see cref="Insight"/> using the ExternalId
	/// </summary>
	Task ReverseSyncCommandInsights(RuleExecutionRequest request, CancellationToken cancellationToken = default);

	/// <summary>
	///  Delete all insights from Rules Engine, optionally deleting from Command
	/// </summary>
	Task DeleteAllInsights(RuleExecutionRequest request, CancellationToken cancellationToken = default);

	/// <summary>
	///  Delete all matching insights for a ruleid from Rules Engine, optionally deleting from Command
	/// </summary>
	Task DeleteAllMatchingInsights(RuleExecutionRequest request, CancellationToken cancellationToken = default);

	/// <summary>
	///  Delete all matching commands for a ruleid from Rules Engine, optionally deleting from Command
	/// </summary>
	Task DeleteAllMatchingCommands(RuleExecutionRequest request, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sync CommandEnabled flag to rule instances and insights
	/// </summary>
	Task SyncCommandEnabled(RuleExecutionRequest request, CancellationToken cancellationToken = default);

}

/// <summary>
/// Various Command integration methods
/// </summary>
public class CommandSyncProcessor : ICommandSyncProcessor
{
	private readonly ILogger<CommandSyncProcessor> logger;
	private readonly IRepositoryInsight repositoryInsight;
	private readonly IRepositoryCommand repositoryCommand;
	private readonly IRepositoryActorState repositoryActorState;
	private readonly IRepositoryTimeSeriesBuffer repositoryTimeSeriesBuffer;
	private readonly IRepositoryRuleInstances repositoryRuleInstances;
	private readonly ICommandInsightService commandInsightService;
	private readonly ICommandService commandService;
	private readonly IRepositoryProgress repositoryProgress;
	private readonly IRepositoryRules repositoryRules;
	private readonly IMessageSenderBackEnd messageSender;
	private readonly WillowEnvironment willowEnvironment;

	/// <summary>
	/// Creates a new <see cref="CommandSyncProcessor"/>
	/// </summary>
	public CommandSyncProcessor(
		ILogger<CommandSyncProcessor> logger,
		IRepositoryInsight repositoryInsight,
		IRepositoryActorState repositoryActorState,
		IRepositoryTimeSeriesBuffer repositoryTimeSeriesBuffer,
		ICommandInsightService commandInsightService,
		ICommandService commandService,
		IRepositoryProgress repositoryProgress,
		IRepositoryCommand repositoryCommand,
		IRepositoryRules repositoryRules,
		IRepositoryRuleInstances repositoryRuleInstances,
		IMessageSenderBackEnd messageSender,
		WillowEnvironment willowEnvironment)
	{
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.repositoryInsight = repositoryInsight ?? throw new ArgumentNullException(nameof(repositoryInsight));
		this.repositoryActorState = repositoryActorState ?? throw new ArgumentNullException(nameof(repositoryActorState));
		this.repositoryTimeSeriesBuffer = repositoryTimeSeriesBuffer ?? throw new ArgumentNullException(nameof(repositoryActorState));
		this.commandInsightService = commandInsightService ?? throw new ArgumentNullException(nameof(commandInsightService));
		this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
		this.repositoryProgress = repositoryProgress ?? throw new ArgumentNullException(nameof(repositoryProgress));
		this.repositoryCommand = repositoryCommand ?? throw new ArgumentNullException(nameof(repositoryCommand));
		this.repositoryRules = repositoryRules ?? throw new ArgumentNullException(nameof(repositoryRules));
		this.repositoryRuleInstances = repositoryRuleInstances ?? throw new ArgumentNullException(nameof(repositoryRuleInstances));
		this.messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
		this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
	}

	/// <summary>
	///  Delete all insights from Rules Engine, optionally deleting from Command
	/// </summary>
	public async Task DeleteAllInsights(RuleExecutionRequest request, CancellationToken cancellationToken = default)
	{
		var progressTracker = new ProgressTracker(repositoryProgress, Progress.DeleteAllInsightsId, ProgressType.DeleteAllInsights, request.CorrelationId, request.RequestedBy, request.RequestedDate, request.RuleId, logger);

		IEnumerable<Insight> insightsForCommand = null;

		await progressTracker.Start();

		//only load up all insights if we have to go to command
		if (request.DeleteFromCommand)
		{
			insightsForCommand = await this.repositoryInsight.Get(v => v.CommandInsightId != Guid.Empty);
		}

		var totalCount = await this.repositoryInsight.Count(v => true);

		await progressTracker.SetValues("Insights", 0, totalCount, isIgnored: false, force: true);

		try
		{
			await repositoryInsight.RemoveAll();

			await progressTracker.SetValues("Insights", totalCount, totalCount, isIgnored: false, force: true);

			await messageSender.SendAllRuleMetadataUpdated(willowEnvironment);
		}
		catch (Exception ex)
		{
			await progressTracker.SetValues("Insights", 0, totalCount, isIgnored: false, force: true);

			logger.LogError(ex, "Failed deleting all insights");

			await progressTracker.Completed();

			//dont continue if insight delete failed
			return;
		}

		if (request.DeleteActors)
		{
			var actorCount = await this.repositoryActorState.Count(v => true);

			try
			{
				await repositoryActorState.RemoveAll();

				await progressTracker.SetValues("Actors", actorCount, actorCount, isIgnored: false, force: true);
			}
			catch (Exception ex)
			{
				await progressTracker.SetValues("Actors", 0, actorCount, isIgnored: false, force: true);

				logger.LogError(ex, "Failed deleting all actors");
			}
		}

		if (request.DeleteTimeSeries)
		{
			var timeSeriesCount = await this.repositoryTimeSeriesBuffer.Count(v => true);

			try
			{
				await repositoryTimeSeriesBuffer.RemoveAll();

				await progressTracker.SetValues("TimeSeries", timeSeriesCount, timeSeriesCount, isIgnored: false, force: true);
			}
			catch (Exception ex)
			{
				await progressTracker.SetValues("TimeSeries", 0, timeSeriesCount, isIgnored: false, force: true);

				logger.LogError(ex, "Failed deleting all time series");
			}
		}

		await messageSender.SendAllRuleMetadataUpdated(willowEnvironment);

		if (insightsForCommand is not null)
		{
			var throttled = logger.Throttle(TimeSpan.FromSeconds(5));
			var total = insightsForCommand.Count();

			logger.LogInformation("Deleting {count} form command.", total);

			int count = 0;

			foreach (var insight in insightsForCommand)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					break;
				}

				try
				{
					cancellationToken.ThrowIfCancellationRequested();
					await commandInsightService.DeleteInsightFromCommand(insight);
					count++;
					await Task.Delay(100);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Deleting insight {id} from command failed. Continuing", insight.CommandInsightId);
				}

				await progressTracker.SetValues("DeletedFromCommand", count, total, isIgnored: false, force: true);

				throttled.LogInformation("Deleted {count}/{total} form command.", count, total);
			}
		}

		if (cancellationToken.IsCancellationRequested)
		{
			await progressTracker.Cancelled();
		}
		else
		{
			await progressTracker.Completed();
		}
	}

	/// <summary>
	///  Delete all matching insights for a ruleid from Rules Engine, optionally deleting from Command
	/// </summary>
	public async Task DeleteAllMatchingInsights(RuleExecutionRequest request, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Delete insights for rule {ruleId}", request.RuleId);

		var progressTracker = new ProgressTracker(repositoryProgress, Progress.DeleteAllMatchingInsightsId, ProgressType.DeleteAllMatchingInsights, request.CorrelationId, request.RequestedBy, request.RequestedDate, request.RuleId, logger);

		await progressTracker.Start();

		int countClosed = 0;
		int countRemoved = 0;
		await progressTracker.SetValues("CommandClosed", 0, countClosed);
		await progressTracker.SetValues("CommandRemoved", 0, countRemoved);

		async Task CloseAndDeleteInsight(ProgressTracker progressTracker, Insight insight)
		{
			var statusCodeClosed = await commandInsightService.CloseInsightInCommand(insight);
			if (statusCodeClosed == System.Net.HttpStatusCode.OK)
			{
				countClosed++;
				await progressTracker.SetValues("CommandClosed", countClosed, countClosed);
			}

			var statusCode = await commandInsightService.DeleteInsightFromCommand(insight);
			if (statusCode == System.Net.HttpStatusCode.OK)
			{
				countRemoved++;
				await progressTracker.SetValues("CommandRemoved", countClosed, countClosed);
			}
		}

		try
		{
			logger.LogInformation("Delete using insghts table");

			foreach (var insight in await repositoryInsight.GetInsightsForRule(request.RuleId))
			{
				// In case Command does not DELETE the insight, we need to close it
				if (insight.CommandInsightId != Guid.Empty)
				{
					await CloseAndDeleteInsight(progressTracker, insight);
				}

				cancellationToken.ThrowIfCancellationRequested();
			}

			//see if we can find insights in command that does not exist in rules engine anymore
			var sites = await repositoryRuleInstances.GetQueryable()
																.Where(v => v.RuleId == request.RuleId)
																.Select(v => new { v.Id, v.SiteId })
																.ToListAsync();

			var siteIds = sites.Select(v => v.SiteId).Where(v => v is not null).Distinct();

			logger.LogInformation("Reverse Delete Delete using rule instance table for {count} sites", siteIds.Count());

			foreach (var siteId in siteIds)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var commandInsights = await commandInsightService.GetInsightsForSiteId(siteId.Value);

				foreach (var insight in commandInsights.Where(v => v.RuleId == request.RuleId))
				{
					await CloseAndDeleteInsight(progressTracker, new Insight()
					{
						Id = insight.ExternalId,
						CommandInsightId = insight.Id,
						SiteId = siteId
					});
				}
			}
		}
		catch (Exception ex)
		{
			await progressTracker.Failed();

			logger.LogError(ex, "Failed deleting command insights for rule {ruleId}", request.RuleId);
		}

		if (cancellationToken.IsCancellationRequested)
		{
			await progressTracker.Cancelled();
			return;
		}

		int count = 0;

		try
		{

			count = await this.repositoryInsight.RemoveAllInsightsForRule(request.RuleId);
		}
		catch (Exception ex)
		{
			await progressTracker.Failed();

			logger.LogError(ex, "Failed to delete insights for rule {ruleId}", request.RuleId);
		}

		await progressTracker.SetValues("TotalRemoved", count, count, force: true);

		if (cancellationToken.IsCancellationRequested)
		{
			await progressTracker.Cancelled();
		}
		else
		{
			await progressTracker.Completed();
		}

		logger.LogInformation("Deleted insights {insight_count}, closed:{countClosed}=removed:{countRemoved} from Command", count, countClosed, countRemoved);
	}

	/// <summary>
	///  Delete all matching commands for a ruleid from Rules Engine
	/// </summary>
	public async Task DeleteAllMatchingCommands(RuleExecutionRequest request, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Delete insights for rule {ruleId}", request.RuleId);

		var progressTracker = new ProgressTracker(repositoryProgress, Progress.DeleteAllMatchingCommandsId, ProgressType.DeleteAllMatchingInsights, request.CorrelationId, request.RequestedBy, request.RequestedDate, request.RuleId, logger);

		await progressTracker.Start();

		int countClosed = 0;
		int countRemoved = 0;
		await progressTracker.SetValues("CommandClosed", 0, countClosed);
		await progressTracker.SetValues("CommandRemoved", 0, countRemoved);

		try
		{
			await foreach (var command in repositoryCommand.GetAll(v => v.RuleId == request.RuleId))
			{
				if (command.IsTriggered && command.CanSync())
				{
					//force an end date before sending
					command.EndTime = DateTimeOffset.UtcNow;

					await commandService.QueueCommand(command.CreateRequestedCommandDto());
				}

				cancellationToken.ThrowIfCancellationRequested();
			}
		}
		catch (Exception ex)
		{
			await progressTracker.Failed();

			logger.LogError(ex, "Failed deleting command for rule {ruleId}", request.RuleId);
		}

		if (cancellationToken.IsCancellationRequested)
		{
			await progressTracker.Cancelled();
			return;
		}

		int count = 0;

		try
		{
			count = await this.repositoryCommand.RemoveAllCommandsForRule(request.RuleId);
		}
		catch (Exception ex)
		{
			await progressTracker.Failed();

			logger.LogError(ex, "Failed to delete commands for rule {ruleId}", request.RuleId);
		}

		await progressTracker.SetValues("TotalRemoved", count, count, force: true);

		if (cancellationToken.IsCancellationRequested)
		{
			await progressTracker.Cancelled();
		}
		else
		{
			await progressTracker.Completed();
		}

		logger.LogInformation("Deleted commands {count}, closed:{countClosed}=removed:{countRemoved} from Command", count, countClosed, countRemoved);
	}

	/// <summary>
	/// Deletes any insights marked not to sync but already pushed to Command.
	/// </summary>
	public async Task DeleteCommandInsightsNotSynced(RuleExecutionRequest request, CancellationToken cancellationToken = default)
	{
		var totalCount = await this.repositoryInsight.Count(v => v.CommandEnabled == false && v.CommandInsightId != Guid.Empty);
		var insightsToDelete = await this.repositoryInsight.Get(v => v.CommandEnabled == false && v.CommandInsightId != Guid.Empty);

		int successCount = 0;
		int failedCount = 0;
		var startTime = DateTime.Now;
		var stopwatch = new Stopwatch();

		logger.LogInformation("Attempting to delete {count} insights from command", totalCount);

		var progressTracker = new ProgressTracker(repositoryProgress, Progress.DeleteCommandInsightsId, ProgressType.DeleteCommandInsights, request.CorrelationId, request.RequestedBy, request.RequestedDate, request.RuleId, logger);

		int totalToProcess = insightsToDelete.Count();

		foreach (var insight in insightsToDelete)
		{
			logger.LogInformation("Deleting insight {id} from command. {success}/{total} deleted. {failed} failed.", insight.CommandInsightId, successCount, totalCount, failedCount);

			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}

			try
			{
				stopwatch.Start();

				var status = await commandInsightService.DeleteInsightFromCommand(insight);

				if (status == System.Net.HttpStatusCode.OK || status == System.Net.HttpStatusCode.NotFound)
				{
					if (status == System.Net.HttpStatusCode.OK)
					{
						successCount++;
					}

					if (request.ClearCommandId)
					{
						insight.CommandInsightId = Guid.Empty;
						await repositoryInsight.UpsertOne(insight);
					}
				}
				else
				{
					failedCount++;
				}

				int totalProcessed = failedCount + successCount;

				await progressTracker.SetValues("Total", totalProcessed, totalCount, isIgnored: false);
				await progressTracker.SetValues("Succeeded", successCount, totalCount, isIgnored: true);
				await progressTracker.SetValues("Failed", failedCount, totalCount, isIgnored: true);

				await Task.Delay(100);

				stopwatch.Stop();
			}
			catch
			{
				failedCount++;
				logger.LogInformation("Deleting insight {id} from command failed. Continuing", insight.CommandInsightId);
			}
		}

		if (cancellationToken.IsCancellationRequested)
		{
			await progressTracker.Cancelled();
		}
		else
		{
			await progressTracker.Completed();  // force send at 100%
		}

		logger.LogInformation("Deleting insights processor finished. {success}/{total} deleted. {failed} failed.", successCount, totalCount, failedCount);
	}

	/// <summary>
	/// Reverse syncs the command insight ids back to the rules engine <see cref="Insight"/> using the ExternalId
	/// </summary>
	public async Task ReverseSyncCommandInsights(RuleExecutionRequest request, CancellationToken cancellationToken = default)
	{
		var startTime = DateTime.Now;
		var sitesAndCounts = await repositoryInsight.GetSiteIds();
		var stopwatch = new Stopwatch();
		var totalRulesEngineCount = sitesAndCounts.Sum(v => v.count);
		int totalCommandCount = 0;
		int currentCount = 0;

		logger.LogInformation("Reverse sync started. Found {totalCount} site ids", sitesAndCounts.Count());
		var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(10));

		var progressTracker = new ProgressTracker(repositoryProgress, Progress.ReverseSyncCommandInsightsId, ProgressType.ReverseSyncInsights, request.CorrelationId, request.RequestedBy, request.RequestedDate, request.RuleId, logger);

		await progressTracker.Start();

		foreach (var site in sitesAndCounts)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}

			var siteId = site.siteId;

			var commandInsights = await commandInsightService.GetInsightsForSiteId(siteId);

			logger.LogInformation("Reverse sync processing {count} command insights for site id {siteId}", commandInsights.Count(), siteId);

			foreach (var insight in commandInsights.Where(v => !string.IsNullOrEmpty(v.ExternalId)))
			{
				totalCommandCount++;
				await progressTracker.SetValues("Command", totalCommandCount, totalCommandCount);

				var result = await repositoryInsight.SetCommandInsightId(insight.ExternalId, insight.Id, insight.GetStatus());  // writes to log too

				if (result > 0)
				{
					currentCount++;
				}

				throttledLogger.LogInformation("Reverse sync {insightId} {state} {status} {sequenceId}", insight.Id, insight.State, insight.LastStatus, insight.SequenceNumber);

				await progressTracker.SetValues("Reverse sync", currentCount, totalRulesEngineCount);
			}
		}

		if (cancellationToken.IsCancellationRequested)
		{
			await progressTracker.Cancelled();
		}
		else
		{
			await progressTracker.Completed();  // force send at 100%
		}

		logger.LogInformation("Reverse sync finished. {totalProcessed}/{totalCount} synced.", currentCount, totalRulesEngineCount);
	}

	/// <summary>
	/// Sync CommandEnabled flag to rule instances and insights
	/// </summary>
	public async Task SyncCommandEnabled(RuleExecutionRequest request, CancellationToken cancellationToken = default)
	{
		var progressTracker = new ProgressTracker(repositoryProgress, Progress.SyncCommandEnabledId, ProgressType.SyncCommandEnabled, request.CorrelationId, request.RequestedBy, request.RequestedDate, request.RuleId, logger);

		await progressTracker.Start();

		try
		{
			var rule = await repositoryRules.GetOne(request.RuleId, updateCache: false);

			if (rule is null)
			{
				await progressTracker.Failed($"Rule {request.Id} not found");

				return;
			}

			logger.LogInformation("Sync command enabled flag for rule {id}, enabled: {flag}", request.RuleId, rule.CommandEnabled);

			int count = await repositoryRules.EnableSyncForRule(request.RuleId, rule.CommandEnabled);

			int total = await repositoryInsight.GetInsightCountForRule(request.RuleId);

			logger.LogInformation("Updated {Count} records while setting rule enabled flag", count);

			await progressTracker.SetValues("Insights updated", count, total);

			await progressTracker.Completed();
		}
		catch (Exception ex)
		{
			await progressTracker.Failed($"SyncCommandEnabled failed {ex.Message}");
			logger.LogError(ex, "SyncCommandEnabled failed");
		}

	}
}
