using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Willow.Rules.DTO;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Repository;
using Willow.Rules.Services;

namespace Willow.Rules.Processor;

/// <summary>
/// Manages insights during execution
/// </summary>
public interface IInsightsManager
{
	/// <summary>
	/// Gets the reader channel (used by the background service)
	/// </summary>
	ChannelReader<Insight> ReaderQueue { get; }

	/// <summary>
	/// Gets the reader channel (used by the background service) that contains the items to process
	/// </summary>
	Channel<Insight> ProcessingChannel { get; }

	/// <summary>
	/// Creates insights and enqueue for background service
	/// </summary>
	Task FlushInsights(
		IDictionary<string, ActorState> actors,
		IDictionary<string, List<RuleInstance>> ruleInstances,
		ProgressTrackerForRuleExecution progressTracker,
		RuleTemplateFactory ruleTemplateFactory,
		SystemSummary summary,
		bool isRealtime,
		string ruleId);

	/// <summary>
	/// Waits for all insights to complete processing/syncing
	/// </summary>
	Task WaitForEmptyQueue(ProgressTrackerForRuleExecution progressTracker);

	/// <summary>
	/// Cleanup insight data
	/// </summary>
	Task InsightCleanup();

	/// <summary>
	/// Max occurrences for an Insight
	/// </summary>
	int MaxOccurenceCount { get; }

	/// <summary>
	/// Max lifetime of an insight occurrence
	/// </summary>
	TimeSpan MaxOccurenceLiftime { get; }
}

/// <summary>
/// Manages insights during execution
/// </summary>
public class InsightsManager : IInsightsManager
{
	private readonly ILogger<InsightsManager> logger;
	private readonly IRepositoryInsight repositoryInsight;
	private readonly ITelemetryCollector telemetryCollector;
	private readonly Channel<Insight> messageQueue;
	private readonly Channel<Insight> processingChannel;
	private int maxOccurenceCount = 1000;
	private TimeSpan maxOccurenceLiftime;

	/// <summary>
	/// Constructor
	/// </summary>
	public InsightsManager(
		IRepositoryInsight repositoryInsight,
		ITelemetryCollector telemetryCollector,
		ILogger<InsightsManager> logger,
		TimeSpan? maxOccurenceLiftime = null)
	{
		this.repositoryInsight = repositoryInsight ?? throw new ArgumentNullException(nameof(repositoryInsight));
		this.telemetryCollector = telemetryCollector ?? throw new ArgumentNullException(nameof(telemetryCollector));
		this.messageQueue = Channel.CreateBounded<Insight>(new BoundedChannelOptions(500000));
		this.processingChannel = Channel.CreateBounded<Insight>(new BoundedChannelOptions(500000));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.maxOccurenceLiftime = maxOccurenceLiftime ?? TimeSpan.FromDays(31 * 6);//6 months
	}

	/// <inheritdoc/>
	public ChannelReader<Insight> ReaderQueue => messageQueue.Reader;

	/// <inheritdoc/>
	public Channel<Insight> ProcessingChannel => processingChannel;

	/// <inheritdoc/>
	public ChannelWriter<Insight> Writer => messageQueue.Writer;

	/// <summary>
	/// Max occurrences for an Insight
	/// </summary>
	public int MaxOccurenceCount => maxOccurenceCount;

	/// <summary>
	/// Max lifetime of an insight occurrence
	/// </summary>
	public TimeSpan MaxOccurenceLiftime => maxOccurenceLiftime;

	/// <summary>
	/// Creates insights and enqueue for background service
	/// </summary>
	public async Task FlushInsights(
		IDictionary<string, ActorState> actors,
		IDictionary<string, List<RuleInstance>> ruleInstances,
		ProgressTrackerForRuleExecution progressTracker,
		RuleTemplateFactory ruleTemplateFactory,
		SystemSummary summary,
		bool isRealtime,
		string ruleId)
	{
		var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(15));
		var now = DateTimeOffset.UtcNow;

		try
		{
			//get latest command flag from db in case of user updates
			var existingInsights = (await repositoryInsight.GetCommandValues()).ToDictionary(v => v.id, v => v);
			int insightsCount = ruleInstances.SelectMany(v => v.Value).Distinct().Count();
			await progressTracker.ReportFlushingInsights(0, insightsCount);

			if (!isRealtime)
			{
				using (var timedLogger = logger.TimeOperation("Deleting all occurrences for batch, rule id {id}", ruleId))
				{
					await repositoryInsight.DeleteOccurrences(ruleId);
				}
			}

			using (var timedLogger = logger.TimeOperation("Queue {count} insights for background service processing", insightsCount))
			{
				int generatedCount = 0;
				int syncCount = 0;
				int faultyCount = 0;

				foreach (var ruleInstance in ruleInstances.SelectMany(v => v.Value).Distinct())
				{
					if (ruleInstance.RuleTemplate == RuleTemplateCalculatedPoint.ID)
					{
						continue;
					}

					if (actors.TryGetValue(ruleInstance.Id, out var actor))
					{
						var insight = new Insight(ruleInstance, actor);

						if (existingInsights.TryGetValue(insight.Id, out var existingInsight))
						{
							insight.CommandEnabled = existingInsight.enabled;
							insight.CommandInsightId = existingInsight.commandId;
							insight.Status = existingInsight.status;
							insight.LastSyncDateUTC = existingInsight.lastSyncDate;
						}

						//after a batch run, force a sync on all enabled insights so that batch run user can confirm changes in will app
						if (!isRealtime && insight.CommandEnabled)
						{
							insight.LastSyncDateUTC = DateTimeOffset.MinValue;
						}

						if (insight.HasOverlappingOccurrences())
						{
							logger.LogWarning("Insight {insightId} has overlapping occurrences.", insight.Id);
						}

						foreach (var dependency in insight.Dependencies)
						{
							if (existingInsights.TryGetValue(dependency.InsightId, out var insightDepencency))
							{
								dependency.CommandInsightId = insightDepencency.commandId;
							}
							else
							{
								dependency.CommandInsightId = Guid.Empty;
							}
						}

						SetNextAllowedSyncDate(insight, ruleInstance, ruleTemplateFactory, throttledLogger);

						await Writer.WriteAsync(insight);

						if (insight.CommandEnabled)
						{
							syncCount++;
						}

						if (insight.IsFaulty)
						{
							faultyCount++;
						}

						generatedCount++;

						//calling "insights.Values" on a concurrent dictionary is expensive memory wise
						//for more reading https://learn.microsoft.com/en-us/visualstudio/profiling/performance-insights-concurrentdictionary-values?view=vs-2022
						await progressTracker.ReportFlushingInsights(generatedCount, insightsCount);

						summary.AddToSummary(insight);
					}
				}

				telemetryCollector.TrackInsightsGeneratedCount(generatedCount);
				telemetryCollector.TrackCommandInsights(syncCount, 0);
				telemetryCollector.TrackFaultyInsights(faultyCount);

				// Report the final total
				await progressTracker.ReportFlushingInsights(insightsCount, insightsCount);
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to queue insights");
		}
	}

	/// <summary>
	/// When users change the state to resolved or ignored in command, don't immediately sync again, wait until the rule's window period has passed
	/// </summary>
	private static void SetNextAllowedSyncDate(Insight insight, RuleInstance ruleInstance, RuleTemplateFactory ruleTemplateFactory, ILogger logger)
	{
		insight.NextAllowedSyncDateUTC = DateTimeOffset.MinValue;

		if (insight.CanReOpen())
		{
			var template = ruleTemplateFactory.GetRuleTemplateForRuleInstance(ruleInstance, logger);

			if (template is not null)
			{
				var windowPeriod = template.GetWindowPeriod();

				insight.NextAllowedSyncDateUTC = insight.LastSyncDateUTC + windowPeriod;
			}
		}
	}

	/// <summary>
	/// Waits for all insights to complete processing/syncing
	/// </summary>
	public async Task WaitForEmptyQueue(ProgressTrackerForRuleExecution progressTracker)
	{
		var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(15));

		//better way to wait for a channel to be empty?
		//at least with the count check we can report back to the UI of how many is still left in the queue
		while (true)
		{
			int count = ProcessingChannel.Reader.Count;

			throttledLogger.LogInformation("Waiting for Insight sync to complete. {count} left", ProcessingChannel.Reader.Count);

			if (count > 0)
			{
				await progressTracker.ReportWaitingForSync(count);

				await Task.Delay(TimeSpan.FromMilliseconds(1000));
			}
			else
			{
				break;
			}
		}

		await InsightCleanup();

		logger.LogInformation("Insight sync queue now empty");

	}

	/// <summary>
	/// Cleanup insight data
	/// </summary>
	public async Task InsightCleanup()
	{
		try
		{
			await repositoryInsight.CheckValidInsights();

			await repositoryInsight.DeleteOldImpactScores();

			//only timebased pruning enabled for now to monitor
			//await repositoryInsight.DeleteOccurrences(MaxOccurenceCount);

			await repositoryInsight.DeleteOccurrencesBefore(DateTimeOffset.Now.Add(-MaxOccurenceLiftime));
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Insight cleanup failed");
		}
	}
}
