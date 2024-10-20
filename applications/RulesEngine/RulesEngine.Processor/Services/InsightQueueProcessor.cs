using EFCore.BulkExtensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Processor;
using Willow.Rules.Repository;
using Willow.Rules.Services;

namespace RulesEngine.Processor.Services;

/// <summary>
/// Background service that processes insights and sync insights to command
/// </summary>
public class InsightBackgroundService : BackgroundService
{
	//I will discuss and we'll get back to you. My first stab at it for healthy, never faulted insights:
	//State: Inactive
	//Priority: 3 
	//OccurredDate: null (but we'll need to validate nothing breaks here)
	//DetectedDate: null (again need to test this)
	//OccurrenceCount: 0


	private readonly IRepositoryInsight repositoryInsight;
	private readonly ICommandInsightService commandInsightService;
	private readonly IInsightsManager insightsManager;
	private readonly ITelemetryCollector telemetryCollector;
	private readonly ILogger<InsightBackgroundService> logger;
	private readonly ILogger throttledLogger;
	private readonly Dictionary<string, Insight> condenser = new();
	private readonly int cleanupPeriod;//minutes
	private static readonly object condensorLock = new object();

	/// <summary>
	/// Creates a new <see cref="InsightBackgroundService" />
	/// </summary>
	public InsightBackgroundService(
		IRepositoryInsight repositoryInsight,
		ICommandInsightService commandInsightService,
		IInsightsManager insightsManager,
		ITelemetryCollector telemetryCollector,
		ILogger<InsightBackgroundService> logger,
		int cleanupPeriod = 15)
	{
		this.repositoryInsight = repositoryInsight ?? throw new ArgumentNullException(nameof(repositoryInsight));
		this.commandInsightService = commandInsightService ?? throw new ArgumentNullException(nameof(commandInsightService));
		this.insightsManager = insightsManager ?? throw new ArgumentNullException(nameof(insightsManager));
		this.telemetryCollector = telemetryCollector ?? throw new ArgumentNullException(nameof(telemetryCollector));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.cleanupPeriod = cleanupPeriod;

		throttledLogger = logger.Throttle(TimeSpan.FromSeconds(60));
	}

	/// <summary>
	/// Main loop for background service
	/// </summary>
	/// <param name="stoppingToken"></param>
	/// <returns></returns>
	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// See https://github.com/dotnet/runtime/issues/36063#issuecomment-518913079
		Task.Run(async () =>
		{
			logger.LogInformation("Insight background service starting");

			await ProcessInsights(stoppingToken);
		}, stoppingToken);

		return ReadFromQueue(stoppingToken);
	}

	private async Task ReadFromQueue(CancellationToken stoppingToken)
	{
		try
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				await ReadFromChannelAndEnqueue(stoppingToken);
			}
		}
		catch(Exception e)
		{
			logger.LogError(e, "Insight background service error reading from manager queue");
		}
		finally
		{
			CompleteInnerQueue();
		}

		logger.LogInformation("Insight background service closing");
	}

	/// <summary>
	/// Complete the inner queue
	/// </summary>
	public void CompleteInnerQueue()
	{
		insightsManager.ProcessingChannel.Writer.Complete();
	}

	/// <summary>
	/// Read from channel queue and write to inner queue
	/// </summary>
	/// <param name="stoppingToken"></param>
	/// <returns></returns>
	public async Task ReadFromChannelAndEnqueue(CancellationToken stoppingToken)
	{
		await foreach (var queuedInsight in insightsManager.ReaderQueue.ReadAllAsync(stoppingToken))
		{
			try
			{
				throttledLogger.LogInformation("Insight background service - Received incoming insight");

				bool queueInsight = true;

				lock (condensorLock)
				{
					if (condenser.TryGetValue(queuedInsight.Id, out var existingInsight))
					{
						if (existingInsight.Occurrences.Any() && queuedInsight.Occurrences.Any())
						{
							var hasOlderOccurrences = existingInsight.Occurrences.Min(v => v.Started) < queuedInsight.Occurrences.Min(v => v.Started);

							//leave the existing insight in the queue if there are older occurrences that has to get synced.
							//This can happen during a batch sync when realtime replaces them before the sync
							if (hasOlderOccurrences)
							{
								continue;
							}
						}

						queueInsight = false;
					}

					condenser[queuedInsight.Id] = queuedInsight;
				}

				if (queueInsight)
				{
					await insightsManager.ProcessingChannel.Writer.WriteAsync(queuedInsight, stoppingToken);
				}
			}
			catch(Exception ex)
			{
				logger.LogError(ex, "Insight background service - error writing to internal queue");
			}
		}
	}

	/// <summary>
	/// Process Insights and update Command if necessary
	/// </summary>
	/// <param name="stoppingToken"></param>
	public async Task ProcessInsights(CancellationToken stoppingToken)
	{
		long processedCount = 0;
		long syncCount = 0;
		var lastDelete = DateTime.Now;
		//exclude user updatable fields, but only for updates, inserts must still write because it gets its initial value from the rule instance
		var config = new BulkConfig()
		{
			PropertiesToExcludeOnUpdate = new List<string>()
			{
				nameof(Insight.CommandEnabled)
			}
		};

		await insightsManager.InsightCleanup();

		var insightSyncWatch = new Stopwatch();

		var reader = insightsManager.ProcessingChannel.Reader;

		await foreach (var queuedInsight in reader.ReadAllAsync(stoppingToken))
		{
			if (stoppingToken.IsCancellationRequested)
			{
				logger.LogWarning("Insight background service - Cancelling Insight Queue");
				break;
			}

			try
			{
				//This is only going to happen when processing insights??
				if ((DateTime.Now - lastDelete).TotalMinutes > cleanupPeriod)
				{
					await DeleteInsights(stoppingToken);

					lastDelete = DateTime.Now;

					//taking advantage of the 15 minute interval here :)
					telemetryCollector.TrackInsightsQueued(reader.Count);

					if (syncCount > 0)
					{
						var speed = Math.Round((double)insightSyncWatch.ElapsedMilliseconds / syncCount, 2);

						telemetryCollector.TrackInsightsSyncSpeed(speed);

						logger.LogInformation("Insight background service avg sync speed {speed}/s", speed);
					}
				}

				var insight = queuedInsight;

				lock (condensorLock)
				{
					//remove from queue
					if (condenser.Remove(insight.Id, out var removedInsight) && removedInsight != insight)
					{
						throttledLogger.LogInformation("Insight background service Found later version of insight {id}", insight.Id);

						//there is a later version of the insight in the queue, use that one
						insight = removedInsight;
					}
				}

				if (insight.ShouldSync())
				{
					syncCount++;

					insightSyncWatch.Start();

					try
					{
						await commandInsightService.UpsertInsightToCommand(insight);
					}
					finally
					{
						insightSyncWatch.Stop();
					}

#if DEBUG
					//set last sync date here too which is useful for local dev to confirm when a sync occurred
					insight.LastSyncDateUTC = DateTime.UtcNow;
#endif
				}

				insight.LastUpdatedUTC = DateTime.UtcNow;

				//only prune after insight dto sent to insight core
				if (insight.Occurrences.Any())
				{
					//only timebased pruning enabled for now to monitor
					//if (insight.Occurrences.Count > insightsManager.MaxOccurenceCount)
					//{
					//	insight.Occurrences = insight.Occurrences.TakeLast(insightsManager.MaxOccurenceCount).ToArray();
					//}

					var oldest = DateTimeOffset.UtcNow - insightsManager.MaxOccurenceLiftime;

					if (insight.Occurrences.First().Ended < oldest)
					{
						insight.Occurrences = insight.Occurrences.Where(v => v.Ended >= oldest).ToArray();
					}
				}

				//keep batchSize smallish, batch runs will contain many occurrences and therefor many bulk inserts
				await repositoryInsight.QueueWrite(insight, queueSize: 400, batchSize: 400, updateCache: false, config: config);

				processedCount++;
			}
			catch (Exception ex)
			{
				throttledLogger.LogError(ex, "Insight background service process insights failed");
			}
			finally
			{
				throttledLogger.LogInformation("Insight background service - Processed {processedCount} | Remaining {queueCount} insights", processedCount, reader.Count);

				//Only flush when all messages are processed, else QueueWrite will manage batches
				if (reader.Count == 0)
				{
					logger.LogInformation("Insight background service - Flushing queue. Processed {processedCount}", processedCount);
					processedCount = 0;
					await repositoryInsight.FlushQueue(updateCache: false, config: config);
				}
			}
		}
	}

	/// <summary>
	/// Delete Insights with no Rule Instance associated and delete from Command
	/// </summary>
	/// <param name="stoppingToken"></param>
	private async Task DeleteInsights(CancellationToken stoppingToken)
	{
		int deleteCount = 0;
		int syncDeleteCount = 0;

		var insightsToDelete = repositoryInsight.GetOrphanedInsights();

		await foreach (var insight in insightsToDelete)
		{
			if (stoppingToken.IsCancellationRequested)
				break;

			try
			{
				if (insight.CommandEnabled && insight.CommandInsightId != Guid.Empty)
				{
					if (insight.Status != InsightStatus.InProgress && insight.Status != InsightStatus.Resolved)
					{
						var deleteStatus = await commandInsightService.DeleteInsightFromCommand(insight);
						if (deleteStatus == System.Net.HttpStatusCode.OK || deleteStatus == System.Net.HttpStatusCode.NotFound)
						{
							syncDeleteCount++;
							await repositoryInsight.DeleteOne(insight, updateCache: false);
							deleteCount++;
						}
					}
					else
					{
						throttledLogger.LogWarning("Command enabled insight {insightId} has a status of {status} but no rule instance", insight.Id, insight.Status);
					}
				}
				else
				{
					await repositoryInsight.DeleteOne(insight, updateCache: false);
					deleteCount++;
				}
			}
			catch (Exception ex)
			{
				throttledLogger.LogError(ex, "Insight background service delete insight {insightId} failed", insight.Id);
			}
		}

		if (deleteCount > 0)
		{
			telemetryCollector.TrackCommandInsights(0, syncDeleteCount);
			logger.LogInformation("Insight background service Deletes: Insight Deletes {deleteCount}, Synced Insight Deletes {syncDeleteCount}", deleteCount, syncDeleteCount);
		}

		await insightsManager.InsightCleanup();
	}
}
