using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RulesEngine.Processor.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Willow.Expressions;
using Willow.Rules.Configuration;
using Willow.Rules.Configuration.Customer;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using Willow.Rules.Sources;
using Willow.ServiceBus;
using WillowRules.Extensions;
using WillowRules.Services;

namespace Willow.Rules.Processor;

/// <summary>
/// A rule execution processor
/// </summary>
public interface IRuleExecutionProcessor
{
	/// <summary>
	/// Execute rules for a date time range
	/// </summary>
	Task Execute(RuleExecutionRequest request, bool isRealtime, CancellationToken cancellationToken);
}


/// <summary>
/// Hosted processor for a single ADX instance within a single WillowEnvironment
/// </summary>
/// <remarks>
/// That ADX instance may serve more than one ADT instance, but will publish to a single
/// Insights database which may then call out to one (or more) Insights APIs to post the results
/// </remarks>
public partial class RuleExecutionProcessor : RuleProcessorBase, IRuleExecutionProcessor
{
	/// <summary>
	/// The Willow Environment
	/// </summary>
	public override WillowEnvironment WillowEnvironment => willowEnvironment;

	private readonly WillowEnvironment willowEnvironment;
	private readonly IADXService adxService;
	private readonly IRepositoryRuleExecutions repositoryRuleExecutions;
	private readonly IRepositoryProgress repositoryProgress;
	private readonly IRepositoryRules repositoryRules;
	private readonly IRepositoryLogEntry repositoryLogEntry;
	private readonly IRepositoryADTSummary repositoryADTSummary;
	private readonly RuleTemplateRegistry ruleTemplateRegistry;
	private readonly IMessageSenderBackEnd messageSender;
	private readonly IDataQualityService dataQualityService;
	private readonly IRulesManager rulesManager;
	private readonly IActorManager actorManager;
	private readonly IInsightsManager insightsManager;
	private readonly ICommandsManager commandsManager;
	private readonly ITimeSeriesManager timeSeriesManager;
	private readonly ILogger<RuleExecutionProcessor> logger;
	private readonly ILogger throttledErrorlogger;
	private readonly ITelemetryCollector telemetryCollector;
	private readonly IRuleOrchestrator ruleOrchestrator;
	private readonly IEventHubService eventHubService;
	private readonly ExecutionOption executionOption;
	private readonly IMemoryCache memoryCache;
	private readonly IMLService mlService;
	private readonly IModelService modelService;
	private readonly HealthCheckProcessor healthCheckProcessor;
	private readonly int thrashingDelay;//milliseconds

	/// <summary>
	/// Settling time for a calculation (when multiple values change at once)
	/// </summary>
	private const double CondenseMinutes = 0.5;

	/// <summary>
	/// Creates a new <see cref="RuleExecutionProcessor"/>
	/// </summary>
	public RuleExecutionProcessor(
		WillowEnvironment willowEnvironment,
		IADXService adxService,
		IRepositoryRuleExecutions repositoryRuleExecutions,
		IRepositoryProgress repositoryProgress,
		IRepositoryRules repositoryRules,
		IRepositoryLogEntry repositoryLogEntry,
		IRepositoryADTSummary repositoryADTSummary,
		RuleTemplateRegistry ruleTemplateRegistry,
		IMessageSenderBackEnd messageSender,
		IDataQualityService dataQualityService,
		IRulesManager rulesManager,
		IActorManager actorManager,
		IInsightsManager insightsManager,
		ICommandsManager commandsManager,
		ITimeSeriesManager timeSeriesManager,
		ITelemetryCollector telemetryCollector,
		IRuleOrchestrator ruleOrchestrator,
		IEventHubService eventHubService,
		IMLService mlService,
		IModelService modelService,
		IMemoryCache memoryCache,
		IOptions<CustomerOptions> options,
		HealthCheckProcessor healthCheckProcessor,
		ILogger<RuleExecutionProcessor> logger,
		int thrashingDelay = 30000)
	{
		this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
		this.adxService = adxService ?? throw new ArgumentNullException(nameof(adxService));
		this.repositoryRuleExecutions = repositoryRuleExecutions ?? throw new ArgumentNullException(nameof(repositoryRuleExecutions));
		this.repositoryProgress = repositoryProgress ?? throw new ArgumentNullException(nameof(repositoryProgress));
		this.repositoryRules = repositoryRules ?? throw new ArgumentNullException(nameof(repositoryRules));
		this.repositoryADTSummary = repositoryADTSummary ?? throw new ArgumentNullException(nameof(repositoryADTSummary));
		this.repositoryLogEntry = repositoryLogEntry ?? throw new ArgumentNullException(nameof(repositoryLogEntry));
		this.ruleTemplateRegistry = ruleTemplateRegistry ?? throw new ArgumentNullException(nameof(ruleTemplateRegistry));
		this.messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
		this.dataQualityService = dataQualityService ?? throw new ArgumentNullException(nameof(dataQualityService));
		this.rulesManager = rulesManager ?? throw new ArgumentNullException(nameof(rulesManager));
		this.actorManager = actorManager ?? throw new ArgumentNullException(nameof(actorManager));
		this.insightsManager = insightsManager ?? throw new ArgumentNullException(nameof(insightsManager));
		this.commandsManager = commandsManager ?? throw new ArgumentNullException(nameof(commandsManager));
		this.timeSeriesManager = timeSeriesManager ?? throw new ArgumentNullException(nameof(timeSeriesManager));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.telemetryCollector = telemetryCollector ?? throw new ArgumentNullException(nameof(telemetryCollector));
		this.ruleOrchestrator = ruleOrchestrator ?? throw new ArgumentNullException(nameof(ruleOrchestrator));
		this.eventHubService = eventHubService ?? throw new ArgumentNullException(nameof(eventHubService));
		this.mlService = mlService ?? throw new ArgumentNullException(nameof(mlService));
		this.modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
		this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
		this.healthCheckProcessor = healthCheckProcessor ?? throw new ArgumentNullException(nameof(healthCheckProcessor));
		this.executionOption = options?.Value?.Execution ?? throw new ArgumentNullException(nameof(options.Value.Execution));
		this.throttledErrorlogger = logger.Throttle(TimeSpan.FromSeconds(10));
		this.healthCheckProcessor.Current = HealthCheckProcessor.Starting;
		this.thrashingDelay = thrashingDelay;
	}

	/// <summary>
	/// How any work items allowed in first queue
	/// </summary>
	public const int MaxQueueLength = 100;

	/// <summary>
	/// How any channels (threads) for executing rules
	/// </summary>
	private readonly int ParallelRuleExecutions = 1 + Environment.ProcessorCount * 2;   // mix of CPU and I/O bound channels

	private const string NO_TWIN = "NO_TWIN";

	/// <summary>
	/// Execute a rule execution request
	/// </summary>
	public override async Task Execute(RuleExecutionRequest request, bool isRealtime, CancellationToken cancellationToken)
	{
		using var disp = logger.BeginScope(new Dictionary<string, object>()["requestType"] = request.Command);

		if (request.Command == RuleExecutionCommandType.ProcessDateRange)
		{
			// Find any existing work item in the database for an overlapping range and merge it
			// and mark it as needing to be run
			logger.LogInformation("Upsert / merge command {command} {startDate} to {endDate}", request.Command, request.StartDate, request.TargetEndDate);
			var ruleExecution = await this.repositoryRuleExecutions.MergeWorkItem(request);

			// Reset its execution state
			ruleExecution.PercentageReported = 0.0;
			ruleExecution.Percentage = 0.0;

			try
			{
				await ProcessOneRuleExecution(request, ruleExecution, isRealtime, cancellationToken);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed in ProcessOneRuleExecution");
			}
		}
		else
		{
			logger.LogInformation("Don't know how to handle {command}", request.Command);
		}
	}

	record IncomingPoint(RawData line, TimeSeries timeSeries, int partition);
	record SingleProcessedResult(DateTimeOffset localTimeStamp, ActorState actor, Rule rule, RuleInstance ruleInstance, RuleTemplate template, Env env);

	private async Task ProcessOneRuleExecution(RuleExecutionRequest request,
		RuleExecution ruleExecution,
		bool isRealtime,
		CancellationToken stoppingToken)
	{
		// Nothing do do yet, spin wait
		// Previously we also checked rule count, but calc points don't have rules
		if (!(await rulesManager.HasRuleInstances()))
		{
			logger.LogInformation("No rule instances to process");
			healthCheckProcessor.Current = HealthCheckProcessor.NoRuleInstances;
			await Task.Delay(thrashingDelay, stoppingToken);  // prevent thrashing
			SendDefaultMetrics();
			return;
		}

		healthCheckProcessor.Current = HealthCheckProcessor.Healthy;

		logger.LogInformation("Processing from {startDate} to {targetEndDate}", ruleExecution.StartDate, ruleExecution.TargetEndDate);
		var earliest = ruleExecution.StartDate.UtcDateTime;
		var latest = ruleExecution.TargetEndDate.UtcDateTime;
		var progressId = request.ProgressId;  // Passed back in messages as progress type?
		var correlationId = request.CorrelationId;

		DateTimeOffset startedRun = DateTimeOffset.UtcNow; // for ETA
		DateTimeOffset lastReport = DateTimeOffset.UtcNow;
		DateTime minDateUtc = DateTime.MaxValue;
		DateTime maxDateUtc = earliest;
		DateTimeOffset startDate = DateTimeOffset.UtcNow.AddYears(-100).Date; // for reporting progress

		logger.LogInformation("Running '{ruleToRun}' from {earliest} to {latest}", ruleExecution.RuleId, earliest, latest);

		Stopwatch stopWatch = Stopwatch.StartNew();
		long ticksSpentPreparing = 0;
		long ticksSpentExecuting = 0;
		long ticksSpentInActor = 0;
		long ticksSpentSendingProgress = 0;
		long linesRead = 0;
		long ruleInstancesProcessed = 0;
		int bufferCount = 0;
		double speed = 0.0;
		double rawLineRatio = 0.0;
		bool singleRuleExecution = !string.IsNullOrEmpty(request.RuleId);

		var progressTracker = new ProgressTrackerForRuleExecution(request.RuleId,
			request.ProgressId, ProgressType.RuleExecution,
			request.CorrelationId,
			repositoryProgress, request.RequestedBy, request.RequestedDate, logger);

		try
		{
			await progressTracker.Start();

			// BUG BUG: Why is this being called in-parallel?
			// A second operation was started on this context instance before a previous operation completed. This is usually caused by different threads concurrently using the same instance of DbContext. For more information on how to avoid threading issues with DbContext, see https://go.microsoft.com/fwlink/?linkid=2097913.
			var ruleTemplateFactory = new RuleTemplateFactory(repositoryRules, ruleTemplateRegistry);
			await ruleTemplateFactory.Initialize();

			var summary = await repositoryADTSummary.GetLatest();

			// Load all the rule instances ahead of time, querying is too slow

			var rules = await rulesManager.GetRulesLookup(request);

			var ruleInstanceLookup = await rulesManager.GetRuleInstanceLookup(request, progressTracker, ruleTemplateFactory, summary.SystemSummary, NO_TWIN);

			await timeSeriesManager.LoadTimeSeriesBuffers(ruleInstanceLookup, ruleTemplateFactory, earliest);

			// Note: dictionary uses case insensitive keys because Guids might not be consistently cased between ADT and ADX

			if (!ruleInstanceLookup.Any())
			{
				logger.LogInformation("No rule instances to process. Rule instance lookup is empty");
				await progressTracker.Failed("No rule instances to process");
				this.healthCheckProcessor.Current = HealthCheckProcessor.NoRuleInstances;
				await Task.Delay(thrashingDelay, stoppingToken);  // prevent thrashing
				SendDefaultMetrics();
				return;
			}

			var textValueModelIds = modelService.GetModelIdsForTextBasedTelemetry().ToHashSet();

			//preload ml models which are found in the rule instances
			var mlModels = await mlService.ScanForModels(ruleInstanceLookup.SelectMany(v => v.Value).Distinct());

			//increment for batch runs
			bool incrementVerions = !isRealtime;

			await rulesManager.LoadMetadata(request, incrementVerions);

			ConcurrentDictionary<string, ActorState> actors = new();

			//only load existing actors for realtime. Batches should be clean slate
			if (isRealtime)
			{
				actors = await actorManager.LoadActorState(ruleInstanceLookup, earliest, progressTracker);
			}

			logger.LogInformation("Startup took {elapsed}", stopWatch.Elapsed);

			var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(10));
			var throttledErrorLogger = logger.Throttle(TimeSpan.FromSeconds(10));

			// Maps a time series to a group of time series that share a rule instance
			ConcurrentDictionary<string, int> partitionTable = new();

			// Map using the twin Id as that's what the mapping is based on. time series ID which will always be not null even if we don't know
			// the twin id. If we don't have one, use hashcode to ensure it goes down same channel

			int getPartition(TimeSeries timeSeries) =>
				string.IsNullOrEmpty(timeSeries.DtId) ? timeSeries.Id.GetHashCode() & 0x7fffffff :
				partitionTable.TryGetValue(timeSeries.DtId, out int partition) ? partition :
				timeSeries.Id.GetHashCode() & 0x7fffffff;

			// Mapping phase 1: assign a numeric partition to every used capablity twinId

			int c = 0;
			bool changed = true;
			int loopCount = 0;
			while (changed && loopCount++ < 1000000)
			{
				changed = false;
				foreach (var ruleInstance in ruleInstanceLookup.SelectMany(x => x.Value).Distinct())
				{
					int lowestPartition = int.MaxValue;

					var twinIds = ruleInstance.PointEntityIds.Select(x => x.Id).ToList();

					// Some calc points don't necessarily have point ids, add them to the "no twin" partition which is sent a dummy point every 15 minutes
					if (!twinIds.Any())
					{
						twinIds = new List<string>()
						{
							NO_TWIN
						};
					}

					// Calculated points have an output twin - if another rule is listening to that it needs to be in the same partition
					// so the OUTPUT twin ID is added to the twinIds field
					if (ruleInstance.RuleTemplate == RuleTemplateCalculatedPoint.ID)
					{
						var outputTimeSeries = await timeSeriesManager.GetOrAdd(ruleInstance.OutputTrendId, EventHubSettings.RulesEngineConnectorId, ruleInstance.OutputExternalId);
						if (!string.IsNullOrEmpty(outputTimeSeries?.DtId))
						{
							twinIds.Add(outputTimeSeries.DtId);
						}
					}

					foreach (var twinId in twinIds)
					{
						if (partitionTable.TryGetValue(twinId, out int partition))
						{
							lowestPartition = Math.Min(lowestPartition, partition);
						}
					}

					if (lowestPartition == int.MaxValue)
					{
						// none of the points have been seen before, allocate a new partition
						lowestPartition = c++;
					}

					// now mark all of the points with this lowest partition number
					foreach (var twinId in twinIds)
					{
						if (partitionTable.TryGetValue(twinId, out int partition))
						{
							if (partition != lowestPartition)
							{
								partitionTable[twinId] = lowestPartition;
								changed = true;
							}
						}
						else
						{
							partitionTable[twinId] = lowestPartition;
						}
					}

				}
				logger.LogInformation("Partition loop {loopCount} {changed}", loopCount, changed ? "changed" : "unchanged");
			}

			// At this point the mapping returns a partition number and we know that for any two
			// different partition values we are allowed to execute in parallel but for any two
			// same parition numbers there is a risk that the values are used in more than one
			// rule execution

			int countTotalGroups = partitionTable.Values.Distinct().Count();
			logger.LogInformation("Total partitions created {countMappings} average size {averageSize:0.0}", countTotalGroups, (double)partitionTable.Count / countTotalGroups);

			// We can now create a lock table for locking against this set
			// or we can further reduce it with a hash to control how many locks we create
			// or better yet we can just shove each down a channel which executes the
			// timeseries update and then the rule trigger and then the insight update synchronously

			/*
			/ ----------------------------------------------------------------------------------------------
			*/

			async Task<SingleProcessedResult> processLine(DateTime now, RuleInstance ruleInstance)
			{
				// Note: The RuleInstance determines the timezone for the calculation process and any insights generated
				// A rule can pull data points from many different systems even in different timezones
				// but the rule instance is anchored to a single timezone

				//ADX is UTC
				var tz = TimeZoneInfoHelper.From(ruleInstance.TimeZone);
				var nowAtRuleInstanceLocation = now.ConvertToDateTimeOffset(tz);
				var result = await processRuleInstance(nowAtRuleInstanceLocation, ruleInstance);
				return result;
			}

			/*
			/ ----------------------------------------------------------------------------------------------
			*/

#if DEBUG
			int activePartitions = 0;
			var partitionLogger = logger.Throttle(TimeSpan.FromSeconds(5));
			var pointConcurrencyCheck = new ConcurrentDictionary<string, int>();
#endif

			// TODO: Have three separate messages come here:
			// (i) an incoming point
			// (ii) a nothing-happened trigger for a rule instance
			// (iii) a shutdown now please message which flushes any internal buffer state
			//
			// the latter will allow this step to coallesce writes to insights
			// buffering them up in a concurrent dictionary which is then flushed
			// at the end

			// <summary>
			// Transform an input stream of points within a partition to an output stream of the date processed
			// </summary>
			// <remarks>
			// Within this channel everything is synchronous which prevents any simultaneous updates to the same
			// TimeSeries or Insight
			// </remarks>
			ChannelReader<DateTime> ProcessPointsAsync(ChannelReader<IncomingPoint> inputChannel, CancellationToken cancellationToken = default)
			{
				var boundedChannelOptions = new BoundedChannelOptions(20);
				var output = Channel.CreateBounded<DateTime>(boundedChannelOptions);

				// unobserved task - don't crash!
				Task.Run(async () =>
				{
					var condenser = new ConcurrentDictionary<string, IncomingPoint>();
					var queue = new Queue<(DateTime releaseTime, IncomingPoint point, RuleInstance ruleInstance)>();
					DateTime current = DateTime.MinValue;
					DateTime hwm = DateTime.MinValue;

					// Remembers which partition we are handling in this step
					int? partition = null;

					async Task FlushQueueDoWorkUpTo(DateTime now)
					{
						if (queue.Count == 0) return;

						if (logger.IsEnabled(LogLevel.Trace))
						{
							logger.LogTrace("P{partition:00}:   Flush queue {count} items", partition, queue.Count);
						}
						while (queue.TryPeek(out var top) && top.releaseTime <= now)
						{
							var q = queue.Dequeue();  // take it
							if (condenser.TryGetValue(q.ruleInstance.Id, out var consolidated))
							{
								if (consolidated.line.SourceTimestamp == q.point.line.SourceTimestamp)
								{
									// The latest item matches the queue item, process it, otherwise wait for it to happen again later in the queue
									condenser.TryRemove(q.ruleInstance.Id, out var _);
									if (logger.IsEnabled(LogLevel.Trace))
									{
										logger.LogTrace("P{partition:00}:   Processing timestamp {t} = {v} with {rule}", partition, consolidated.line.SourceTimestamp, consolidated.line.Value, top.ruleInstance.Id);
									}
									var r = await processLine(consolidated.line.SourceTimestamp, q.ruleInstance);

									// re-add all related Rule instances for the calc point back into queue for the next flush
									// the isvalid check is important as it will avoid circular referenced cp's
									// i.e if 2 cp's references each other, both actors stay invalid because neither have sufficient data
									if (r.template.Id == RuleTemplateCalculatedPoint.ID && r.actor.IsValid)
									{
										var ts = await timeSeriesManager.GetOrAdd(q.ruleInstance.OutputTrendId, EventHubSettings.RulesEngineConnectorId, q.ruleInstance.OutputExternalId);

										if (!string.IsNullOrEmpty(ts?.DtId))
										{
											// CHECKING THAT PARTITION IS CORRECT START
											if (partitionTable.TryGetValue(ts.DtId, out int newPartition))
											{
												newPartition = newPartition % ParallelRuleExecutions;
												if (partition is not null && partition != newPartition)
												{
													throttledLogger.LogWarning("PARTITION ERROR: Twin {id} should map to partition {partition} not {newpartition}", ts.DtId, partition, newPartition);
												}
											}
											else
											{
												throttledLogger.LogWarning("PARTITION ERROR: Calculated point {id} not found in partition table", ts.DtId);
											}
											// CHECKING THAT PARTITION IS CORRECT END

											if (ruleInstanceLookup.TryGetValue(ts.DtId, out var ruleInstancesTriggeredByCalcPoint))
											{
												foreach (var ruleInstance in ruleInstancesTriggeredByCalcPoint)
												{
													// only add if ri is not in condenser as it will run anyway either this iteration or the next
													// is it safe to add to the current queue? It is afterall in the same partition
													if (!condenser.ContainsKey(ruleInstance.Id))
													{
														condenser.AddOrUpdate(ruleInstance.Id, (k) => consolidated, (k, o) => consolidated);
														queue.Enqueue((q.releaseTime.AddMinutes(CondenseMinutes), consolidated, ruleInstance));
													}
												}
											}
										}
									}

									if (logger.IsEnabled(LogLevel.Trace))
									{
										if (r.env is not null) logger.LogTrace("-> {env}", r.env.DebugDump);
									}
									if (consolidated.line.SourceTimestamp > hwm) hwm = consolidated.line.SourceTimestamp;
									Interlocked.Increment(ref ruleInstancesProcessed);

									if (hwm != current)
									{
										current = hwm;
										await output.Writer.WriteAsync(current, cancellationToken).ConfigureAwait(false);
									}
								}
								else
								{
									if (logger.IsEnabled(LogLevel.Trace))
									{
										logger.LogTrace("Skipping this queue item {v1}, there's a later one {v2}", top.point.line.Value, consolidated.line.Value);
									}
								}
							}
						}

					}

					try
					{
						await foreach (IncomingPoint point in inputChannel.ReadAllAsync(cancellationToken).ConfigureAwait(false))
						{
							partition = partition ?? point.partition % ParallelRuleExecutions;

#if DEBUG
							Interlocked.Increment(ref activePartitions);
							int check = pointConcurrencyCheck.AddOrUpdate(point.timeSeries.Id, (key) => 1, (key, old) => old + 1);
							if (partition != point.partition % ParallelRuleExecutions) throw new Exception("Partitioning error");
							partitionLogger.LogInformation("Partition {partition}, Active partitions {countPartitions}/{totalPartitions} and check={check}", partition, activePartitions, ParallelRuleExecutions, check);
#endif

							// At this point we are in an isolated channel that handles time series that are used by
							// a partition of all the rule instances that has no overlaps with any other channel's instances

							// We can proceed synchronously in the secure knowledge that nobody else is updating
							// our time series, our actors, or our insights

							// We could even run this partition on a different server entirely

							var buffer = point.timeSeries;
							var line = point.line;

							if (line.SourceTimestamp > DateTimeOffset.Now.UtcDateTime.AddMinutes(1))
							{
								logger.LogWarning("Timestamp time is ahead of real-time");
							}

							try
							{
								// Flush the consolidation queue BEFORE adding this point to the time series buffers
								await FlushQueueDoWorkUpTo(line.SourceTimestamp);

								// UTC Time because we don't know what timezone an individual point is in
								// but a rule for an equipment item does have a timezone
								DateTimeOffset now = new(line.SourceTimestamp.Ticks, TimeSpan.Zero);

								bool hasTwin = !string.IsNullOrEmpty(buffer.DtId);
								//avoid compression state for no twin buffers to save mmeory. We should only be keeping 2 points anyway
								bool applyCompression = hasTwin;
								bool enableValidation = hasTwin;

								if (hasTwin)
								{
									//Set the latency
									//ignore for no tiwn buffers. Try to keep buffer slim to save on memory
									buffer.SetLatencyEstimate(line.EnqueuedTimestamp.Subtract(line.SourceTimestamp));
								}

								// Always add the point to a buffer, whether or not it's involved in a rule
								if (!string.IsNullOrEmpty(line.TextValue) && textValueModelIds.Contains(buffer.ModelId))
								{
									buffer.AddPoint(new TimedValue(now, line.Value, line.TextValue), applyCompression: applyCompression, includeDataQualityCheck: enableValidation);
								}
								else
								{
									buffer.AddPoint(new TimedValue(now, line.Value), applyCompression: applyCompression, includeDataQualityCheck: enableValidation);
								}
								
								// Keep the buffer trimmed
								timeSeriesManager.ApplyLimits(buffer, line.SourceTimestamp);

								//run status updates during execution so that templates can react to invalid ones
								buffer.SetStatus(now);

								logger.LogTrace("P{partition:00}: Ready to process timestamp {t}", partition, line.SourceTimestamp);

								// RELEASE ANY POINTS THAT ARE DUE TO COME OUT
								// BEFORE ADDING THIS ONE TO THE SET, DO THIS EVEN IF NO RULE WILL EXECUTE

								if (hasTwin)
								{
									if (ruleInstanceLookup.TryGetValue(buffer.DtId, out var ruleInstancesTriggeredByPoint))
									{
										buffer.SetUsedByRule();  // keep more than three and enable validation objects

										// PUT IT INTO THE CONDENSER, WILL GET PULLED OUT NEXT ITERATION NOT NOW
										foreach (var ruleInstance in ruleInstancesTriggeredByPoint)
										{
											logger.LogTrace("P{partition:00}:   Enqueuing timestamp {t}={v}", partition, line.SourceTimestamp, line.Value);
											condenser.AddOrUpdate(ruleInstance.Id, (k) => point, (k, o) => point);
											queue.Enqueue((line.SourceTimestamp.AddMinutes(CondenseMinutes), point, ruleInstance));
										}

										if (hwm != current)
										{
											current = hwm;
											await output.Writer.WriteAsync(current, cancellationToken).ConfigureAwait(false);
										}

									}
									else
									{
										// point is not used by any rule
										// TODO: Track coverage as a percentage of points covered by at least one rule
									}
								}
							}
							catch (Exception ex)
							{
								throttledErrorLogger.LogError(ex, "Failed to process point");
							}

#if DEBUG
							Interlocked.Decrement(ref activePartitions);
							pointConcurrencyCheck.AddOrUpdate(point.timeSeries.Id, (key) => 0, (key, old) => old - 1);
#endif
						}

						throttledLogger.LogInformation("P{partition}: Completed reading data", partition);

						// And flush the condensed values
						await FlushQueueDoWorkUpTo(DateTime.MaxValue);

						// -----------------------------------------------------------------------------------

						// And now trigger any rule instances that have not been triggered in over __ minutes
						// so they can detect missing data and go orange

						// BUG BUG: If this task never received ANY data it will not know what it's partition is

						if (partition is not null)
						{
							throttledLogger.TimeOperation("P{partition}: Triggering idle rule instances", partition);

							int countNotTriggered = 0;

							foreach (var ri in ruleInstanceLookup.SelectMany(x => x.Value).Distinct())
							{
								if (!ri.PointEntityIds.Any()) continue;

								if (!partitionTable.TryGetValue(ri.PointEntityIds.First().Id, out int rulePartition)) continue;

								if (partition != rulePartition % ParallelRuleExecutions) continue;

								if (!ri.Status.HasFlag(RuleInstanceStatus.Valid)) continue;

								if (ri.Disabled) continue;

								// Calculated points will also get a chance to run again here, although likely already
								// treiggered by the sentinel

								// Triggered within the last hour = good enough
								if (actors.TryGetValue(ri.Id, out var actorForTick) &&
									actorForTick.Timestamp.AddHours(1) > maxDateUtc) continue;

								var r = await processLine(maxDateUtc, ri);

								if (logger.IsEnabled(LogLevel.Trace))
								{
									if (r.env is not null) logger.LogTrace("-> {env}", r.env.DebugDump);
								}

								Interlocked.Increment(ref ruleInstancesProcessed);
								throttledLogger.LogInformation("Triggering idle rule {ruleId} on {equipmentId}", ri.RuleId, ri.EquipmentId);

								countNotTriggered++;
							}

							if (countNotTriggered > 0)
							{
								throttledLogger.LogInformation("Idle rule instances triggered post-run: {count} on parition {partition}", countNotTriggered, partition);
							}
						}
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "Failed to process line instances");
					}
					finally
					{
						output.Writer.Complete();
						if (partition is not null)
						{
							throttledLogger.LogInformation("P{partition}: Completed channel", partition);
						}
					}
				}, cancellationToken);

				return output;
			}


			DateTime maxValue = DateTime.MinValue;

			async Task<bool> reportProgress(DateTime now)
			{
				if (now > maxValue) maxValue = now;
				await sendProgress(maxValue, progressTracker);
				return true;
			}

			/*
			/ ----------------------------------------------------------------------------------------------
			*/

			async Task<SingleProcessedResult> processRuleInstance(DateTimeOffset timestamp, RuleInstance ruleInstance)
			{
				var startProcessing = stopWatch.ElapsedTicks;

				try
				{
					if (!actors.TryGetValue(ruleInstance.Id, out var actor))
					{
#if DEBUG
						if (actors.Count < 10)
						{
							logger.LogTrace("Adding actor {count} {id}", actors.Count, ruleInstance.Id);
						}
						else if (actors.Count == 10)
						{
							logger.LogTrace("Adding actor 10+ ... truncated");
						}
#endif
						int version = rulesManager.GetVersion(ruleInstance);

						actor = new ActorState(ruleInstance, timestamp, version);
						actors.TryAdd(ruleInstance.Id, actor);
					}

					// The actor state doesn't start early enough, create a whole new actor
					if (actor.EarliestSeen > timestamp)
					{
						throttledLogger.LogInformation("Resetting actor state at {time1} because time {time1} has gone too far backwards: {ruleId}", actor.EarliestSeen, timestamp, ruleInstance.RuleId);
						int version = rulesManager.GetVersion(ruleInstance);
						actor = new ActorState(ruleInstance, timestamp, version);
						actors[ruleInstance.Id] = actor;
					}

					//will be null for calculated points
					rules.TryGetValue(ruleInstance.RuleId, out var rule);

					var template = ruleTemplateFactory.GetRuleTemplateForRuleInstance(ruleInstance, logger);

					Interlocked.Add(ref ticksSpentPreparing, stopWatch.ElapsedTicks - startProcessing);
					startProcessing = stopWatch.ElapsedTicks;  // and reset the start point

					if (template is null)
					{
						throttledErrorLogger.LogError("Template or rule not found for {ruleInstanceId} `{ruleTemplate}`", ruleInstance.Id, ruleInstance.RuleTemplate);
						var result = new SingleProcessedResult(DateTimeOffset.MinValue, actor, rule, ruleInstance, null, null);
						return result;
					}

					long startMarker = stopWatch.ElapsedTicks;
					var env = Env.Empty.Push();

					// 74%-98% of the time is spent in this next call. Need to work on optimizing it.
					var previousBoolValue = actor.ValueBool;

					using var disp = logger.BeginScope(new Dictionary<string, object>() { ["ruleInstanceId"] = ruleInstance.Id });

					try
					{
						//for single rule execution only sync anything to ADX for the specified rule
						bool sendToAdx = singleRuleExecution ? ruleInstance.RuleId == request.RuleId : true;

						var timeSeriesReader = new RuleTemplateDependencies(ruleInstance, timeSeriesManager, eventHubService, mlModels, sendToAdx: sendToAdx);

						var currentTimeStamp = actor.Timestamp;

						actor = await template.Trigger(timestamp, env, ruleInstance, actor, timeSeriesReader, throttledErrorlogger);

						//every 24 hour but only if there was a change
						if (actor.Timestamp != currentTimeStamp && currentTimeStamp.Date != timestamp.Date)
						{
							actorManager.ApplyLimits(actor, ruleInstance, timestamp.DateTime);
						}

					}
					catch (Exception ex)
					{
						try
						{
							throttledErrorLogger.LogError(ex, "Failed to process trigger {ruleInstanceId} {equipmentId}", ruleInstance.Id, ruleInstance.EquipmentId);
						}
						catch (Exception ex2)
						{
							logger.LogError(ex2, $"And then exception in logging");
						}
						var result = new SingleProcessedResult(DateTimeOffset.MinValue, actor, rule, ruleInstance, template, env);
						return result;
					}

					// Update last changed state on state changed
					if (previousBoolValue != actor.ValueBool)
					{
						actor.UpdateLastChangedOutput(timestamp);
					}

					//if (!actors.ContainsKey(ruleInstance.Id)) logger.LogWarning("Internal error {id} != {id2}", ruleInstance.Id, actor.Id);
					// Update the actor in the state store

					Interlocked.Add(ref ticksSpentInActor, (stopWatch.ElapsedTicks - startMarker));

					// Rising edge on actor output state?
					bool fireEvent = actor.IsValid && (actor.ValueBool && !previousBoolValue);

					if (fireEvent)
					{
						// Will publish an Azure Event Grid or Service Bus message here
						// log: Publishing event for {ruleInstance.EquipmentId} {(newActorState.ValueBool ? "faulted" : "healthy")}"
					}

					// And update the local insights collection which MAY or MAY NOT update the external
					// insights API and any other connected ticketing systems

					// If insufficient data, or only a single positive outcome, do not create an insight
					// in fact, don't create an insight until we have at least one failure to report on
					{
						var result = new SingleProcessedResult(timestamp, actor, rule, ruleInstance, template, env);
						return result;
					}
				}
				catch (Exception ex)
				{
					throttledErrorLogger.LogError(ex, "Failed to process line {ruleInstanceId}", ruleInstance.Id);
					// Maybe stop if too many of these?
				}
				finally
				{
					ticksSpentExecuting += stopWatch.ElapsedTicks - startProcessing;
				}

				var resultFailed = new SingleProcessedResult(DateTimeOffset.MinValue, null, null, ruleInstance, null, null);
				return resultFailed;
			}

			/*
			/ ----------------------------------------------------------------------------------------------
			*/

			async Task sendProgress(DateTime timestamp, ProgressTrackerForRuleExecution progressTracker, bool force = false)
			{
				long startProcessing = stopWatch.ElapsedTicks;
				try
				{
					bool twentyFourHourBoundary = ruleExecution.CompletedEndDate.Date != timestamp.Date;

					startDate = timestamp;

					// always track the end-date closely
					ruleExecution.CompletedEndDate = timestamp; // UTC
					ruleExecution.Percentage = Math.Min(1.0, (timestamp - earliest).TotalHours /
						(ruleExecution.TargetEndDate - earliest).TotalHours);

					var now = DateTimeOffset.UtcNow;
					if (force ||
						(linesRead > 1000 && // no point reporting until we have enough data
						(lastReport.AddSeconds(15) < now)))
					{
						lastReport = now;

						var tickRealTime = (timestamp - minDateUtc).TotalMilliseconds;
						var proportionSpentPreparing = (double)(ticksSpentPreparing) / stopWatch.ElapsedTicks;
						var proportionSpentInActor = (double)(ticksSpentInActor) / stopWatch.ElapsedTicks;
						var proportionSpentExecuting = (double)(ticksSpentExecuting) / stopWatch.ElapsedTicks;
						var proportionSpentOnProgress = (double)(ticksSpentSendingProgress) / stopWatch.ElapsedTicks;
						var elapsedMilliseconds = stopWatch.ElapsedMilliseconds;
						speed = tickRealTime / (elapsedMilliseconds + 1);  // +1 avoid NaN

						// Code coverage for a building: % of points for which rules exist

						// no need to use throttled logger, already behind a 15s limit
						logger.LogInformation(
							"{linesRead:0.00}k lines@{rawLineRatio:0.00}/ms. " +
							"triggers: {ruleInstancesProcessed:0.00}k " +
							"{startDate:g}, sp={speed:0.0}x, " +
							"executionT:{proportionSpentExecuting:P1} " +
							"prepareT:{proportionSpentPreparing:P1} " +
							"actorT:{proportionSpentInActor:P1} " +
							"progressT:{proportionSpentProgress:P1} " +
							"inBuffer:{bufferCount}",
							linesRead / 1000.0,
							rawLineRatio,
							ruleInstancesProcessed / 1000.0,
							startDate, speed,
							proportionSpentExecuting,
							proportionSpentPreparing,
							proportionSpentInActor,
							proportionSpentOnProgress,
							bufferCount);

						ruleExecution.PercentageReported = ruleExecution.Percentage;

						await repositoryRuleExecutions.QueueWrite(ruleExecution, queueSize: 1, updateCache: false);

						await progressTracker.ReportProgress(earliest, maxDateUtc, latest, linesRead, speed);

						// Telemetry collectors do their own aggregation and send only when needed
						// but now only send them at the end of a run
						if (force || ruleExecution.Percentage > 0.1) // end of run
						{
							if (speed > 0)
							{
								telemetryCollector.TrackExecutionSpeed(speed);
							}
							telemetryCollector.TrackProportionSpentExecuting(proportionSpentExecuting);
							telemetryCollector.TrackProportionSpentInActor(proportionSpentInActor);
							if (rawLineRatio > 0) // may still be a zero value sneaking in here, affecting average?
							{
								telemetryCollector.TrackLineSpeed(rawLineRatio);
							}
						}
					}
				}
				catch (Exception ex)
				{
					throttledErrorLogger.LogError(ex, "Failed in sendProgress");
				}
				finally
				{
					Interlocked.Add(ref ticksSpentSendingProgress, stopWatch.ElapsedTicks - startProcessing);
				}
			}

			var runFrequency = executionOption.RunFrequency;
			var settlingInterval = executionOption.SettlingInterval;

			DateTimeOffset lastCapabilityQualitySync = earliest;

			while (!stoppingToken.IsCancellationRequested)
			{
				// Start a new progress tracker
				progressTracker = new ProgressTrackerForRuleExecution(request.RuleId,
					request.ProgressId, ProgressType.RuleExecution,
					request.CorrelationId,
					repositoryProgress,
					request.RequestedBy,
					request.RequestedDate,
					logger);

				await progressTracker.Starting(earliest, latest);

				//Create a reader channel for incoming points
				var processingChannel = Channel.CreateBounded<IncomingPoint>(MaxQueueLength);

				ticksSpentExecuting = 0;
				ticksSpentInActor = 0;
				linesRead = 0;
				ruleInstancesProcessed = 0;
				rawLineRatio = 0.0;
				minDateUtc = DateTime.MaxValue;

				var applyLimitsTimeUtc = earliest;

				stopWatch.Restart();

				summary.SystemSummary.ClearRunningSummaries();

				// Start the clock now, we want to run every N minutes independent
				// from how long it takes to process this window of time series data
				var runEvery = Task.Delay(runFrequency, stoppingToken);

				// We need to allow time for 'settling' as not all connectors will deliver data
				// in perfect synchronization. Values are written to ADX out-of-order.
				// So although we'd like to run real-time we need to run at least ___ minutes behind real-time.
				latest = DateTimeOffset.Now.UtcDateTime.Add(-settlingInterval);

				logger.LogInformation("Initiate ADX query from {earliest} to {latest}", earliest, latest);
				//get all rule ids in lookup. It might include calc points the selected rule was dependent on
				IEnumerable<string> ruleIds = !singleRuleExecution ? null : ruleInstanceLookup.SelectMany(v => v.Value).Select(v => v.RuleId).Distinct();

				DateTime lastNoTwinSentinel = DateTime.MinValue;
				TimeSpan noTwinTimespan = TimeSpan.FromMinutes(15);
				bool hasNoTwins = partitionTable.TryGetValue(NO_TWIN, out int noTwinPartition);

				var (adxReader, queryResult) = adxService.RunRawQueryPaged(earliest, latest, ruleIds: ruleIds, cancellationToken: stoppingToken);

				var producer = Task.Run(async () =>
				{
					var lineReadStopwatch = Stopwatch.StartNew();

					logger.LogInformation("ADX: Producer starting for rule execution");

					using var watchdog = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
					var watchdogTimeout = TimeSpan.FromMinutes(30);
					watchdog.CancelAfter(watchdogTimeout);

					try
					{
						await foreach (var line in queryResult.ReadAllAsync(watchdog.Token))
						{
							stoppingToken.ThrowIfCancellationRequested();

							lineReadStopwatch.Stop();  // don't count any time waiting for the channel
							linesRead++;

							//rules engine should not read it's own generated points (e.g. calc points and impacts scores)
							if (string.Equals(line.ConnectorId, EventHubSettings.RulesEngineConnectorId, StringComparison.OrdinalIgnoreCase))
							{
								continue;
							}

							bufferCount = queryResult.Count;

							if (line.SourceTimestamp > maxDateUtc) maxDateUtc = line.SourceTimestamp;
							if (line.SourceTimestamp < minDateUtc) minDateUtc = line.SourceTimestamp;

							watchdog.CancelAfter(watchdogTimeout);  // reset watchdog

							var lineReadMilliseconds = lineReadStopwatch.ElapsedMilliseconds;

							rawLineRatio = lineReadMilliseconds > 0 ? (double)linesRead / lineReadMilliseconds : 0.0;

							// The incoming line can either be the trendid or the externalid/connectorid combo

							if (!string.IsNullOrEmpty(line.PointEntityId) ||
								(!string.IsNullOrEmpty(line.ExternalId)))  // or for Mapped, no connector Id
							{
								string connectorId = line.ConnectorId ?? "";

								// Get the buffer here but don't add the point yet, that's handled in the channel
								var buffer = await timeSeriesManager.GetOrAdd(line.PointEntityId, connectorId, line.ExternalId);

								if (buffer is not null)
								{
									//if single rule execution, skip points that don't belong to the rule
									//when the table link to rules engine is in all prod envs, this code can be removed, but for now
									//the feature can work without it but might be slower because it reads the whole ADX like batch would

									if (hasNoTwins && (maxDateUtc - noTwinTimespan) > lastNoTwinSentinel)
									{
										// run in a notwin sentinel to activate these ri's
										lastNoTwinSentinel = maxDateUtc;
										var sentinelBuffer = new TimeSeries(NO_TWIN, "")
										{
											DtId = NO_TWIN // Set the dtid otherwise the rule doesn't execute
										};
										var sentinelPoint = new IncomingPoint(line, sentinelBuffer, noTwinPartition);
										await processingChannel.Writer.WriteAsync(sentinelPoint, stoppingToken);
									}

									bool writePoint = true;

									if (singleRuleExecution)
									{
										if (string.IsNullOrEmpty(buffer.DtId) || !ruleInstanceLookup.ContainsKey(buffer.DtId))
										{
											writePoint = false;
										}
									}

									if (writePoint)
									{
										int partition = getPartition(buffer);
										var incomingPoint = new IncomingPoint(line, buffer, partition);
										await processingChannel.Writer.WriteAsync(incomingPoint, stoppingToken);
									}
								}
							}

							lineReadStopwatch.Start();
						}
					}
					catch (OperationCanceledException ex)
					{
						if (watchdog.IsCancellationRequested)
						{
							logger.LogWarning("ADX producer was cancelled and did not run to completion");
							// BUG BUG: This then seems to hang the pipeline, needs investigating
						}
						else
						{
							logger.LogError(ex, "ADX producer cancelled watchdog was healthy");
						}
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "Producer failed to read from ADX, terminating");
					}
					finally
					{
						logger.LogInformation("Completing processing channel");

						processingChannel.Writer.Complete();
					}
				}, stoppingToken);

				// split incoming points by partition key into channels that run
				// synchronously with a guarantee that no two channels will read
				// or write to the same buffer, actor or insight
				var processed = processingChannel.Reader
					.Split(ParallelRuleExecutions, (v) => v.partition, stoppingToken)
					.Select(c => ProcessPointsAsync(c, stoppingToken))
					.Merge(stoppingToken)
					.Split(1, x => "X", stoppingToken)
					.Select(c => c.TransformAsync(x => reportProgress(x), stoppingToken))
					.Merge(stoppingToken)
					.ReadAllAsync(stoppingToken)
					.AllAsync(x => x, stoppingToken);

				// Observe all tasks to make sure they complete without an exception
				logger.LogInformation("Waiting for producer, adxreader and linereader to complete");
				await Task.WhenAll(producer, adxReader);

				logger.LogInformation("Waiting for processed to complete");
				await processed;

				//don't let db flushes occur for cancellations
				stoppingToken.ThrowIfCancellationRequested();

				logger.LogInformation("Flushing");

				await sendProgress(maxDateUtc, progressTracker, true);

				telemetryCollector.TrackMemoryCacheCount();

				using (var timedLogger2 = logger.TimeOperation("Waiting for all data tasks to complete"))
				{
					//flush insights first. The occurrence count can grow over the max for batches so we have to send
					//all of them to insight core first before pruning
					await insightsManager.FlushInsights(actors, ruleInstanceLookup, progressTracker, ruleTemplateFactory, summary.SystemSummary, isRealtime, request.RuleId);

					await actorManager.FlushActorsToDatabase(actors, ruleInstanceLookup, latest, progressTracker);

					await timeSeriesManager.FlushToDatabase(earliest, latest, summary.SystemSummary, progressTracker);

					await commandsManager.FlushToDatabase(latest, actors, ruleInstanceLookup, request.RuleId, progressTracker, summary.SystemSummary);

					await rulesManager.FlushMetadataToDatabase(rules, ruleInstanceLookup, actors);
				}

				progressTracker.AddToSummary(summary.SystemSummary);

				summary.SystemSummary.AddToSummary(rules.Values);

				if (!singleRuleExecution)
				{
					await repositoryADTSummary.UpdateSystemRelatedSummary(summary);
				}

				ruleExecution.Percentage = 1.0;

				await this.repositoryRuleExecutions.UpsertOne(ruleExecution, updateCache: false, CancellationToken.None);

				logger.LogInformation("COMPLETED RUN\nMinDate {minDateUtc} (UTC)\nMaxDate {maxDateUtc} (UTC)\nTime period: {delta}", minDateUtc, maxDateUtc, maxDateUtc - minDateUtc);

				await messageSender.SendRuleMetadataUpdated();

				//batch runs should always sync becuase it's usually over 24 hours of data
				//and realtime syncs every 24 hours of data
				bool shouldSyncCapabilities = (maxDateUtc - lastCapabilityQualitySync).TotalHours > 24;

				if (shouldSyncCapabilities)
				{
					await dataQualityService.SendCapabilityStatusUpdate(timeSeriesManager.BufferList, stoppingToken);
					//reset to current time
					lastCapabilityQualitySync = maxDateUtc;
				}

				// Check to see if Orchestrator wants us to do something else
				if (ruleOrchestrator.Listen.TryPeek(out var queueWork))
				{
					logger.LogInformation("Work item pending in work queue {id}", queueWork.ProgressId);
					// And at this point we should suspend work and go see what manual request has been
					// queued up.
				}

				var somethingToRead = ruleOrchestrator.Listen.WaitToReadAsync(stoppingToken);

				var executionWait = (runFrequency.TotalMinutes - stopWatch.Elapsed.TotalMinutes) / runFrequency.TotalMinutes;

				telemetryCollector.TrackExecutionTotalWaitTime(executionWait);

				// if cache is not accessed, items arent expired so we kick it a bit
				memoryCache.Compact();

				await repositoryLogEntry.PruneLogs();

				// Wait for the fifteen minutes or cancellation or an incoming message
				logger.LogInformation("Waiting for next work item or elapsed interval");

				//Ensure once-off processing for manual batch request
				if (!isRealtime)
				{
					//First wait until all insights are flushed.
					await insightsManager.WaitForEmptyQueue(progressTracker);

					await progressTracker.ReportFinished(linesRead);

					break;
				}
				else
				{
					await progressTracker.ReportFinished(linesRead);
				}

				await Task.WhenAny(somethingToRead.AsTask(), runEvery);

				// Check again after the delay to be more responsive
				// TODO: The delay should end when a new value appears on the channel
				if (ruleOrchestrator.Listen.TryPeek(out var item2))
				{
					// There is a manual run waiting or some other message (TODO)
					logger.LogInformation("Pausing real-time execution to handle incoming message");

					//Wait until all insights are flushed.
					//for example if we are going to do deletes, don't let the insight queue still process insights while a delete is happening
					await insightsManager.WaitForEmptyQueue(progressTracker);

					await progressTracker.ReportFinished(linesRead);

					break;
				}

				// Advance to next window of time
				logger.LogInformation("Start date moving up to {maxDate} UTC", maxDateUtc);
				//earliest = maxDate.UtcDateTime;
				earliest = maxDateUtc;
			}
			// END WHILE

			if (stoppingToken.IsCancellationRequested)
			{
				await progressTracker.Cancelled();
			}

			return;
		}
		catch (OperationCanceledException)
		{
			// normal exception during shutdown of queue
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Rule execution failed");
			await progressTracker.Failed();
		}

		if (stoppingToken.IsCancellationRequested)
		{
			await progressTracker.Cancelled();
		}

		// FAILED STATUS - TODO: Better way to record this
		ruleExecution.Percentage = 1.0;

		// TODO: Getting an InvalidOperationException here occasionally: context is in use on a different thread
		await this.repositoryRuleExecutions.UpsertOne(ruleExecution, updateCache: false, CancellationToken.None);
	}

	private void SendDefaultMetrics()
	{
		telemetryCollector.TrackActors(0, 0, 0, 0);
		telemetryCollector.TrackCommandInsights(0, 0);
		telemetryCollector.TrackTimeSeries(0, 0);
		telemetryCollector.TrackLineSpeed(0);
		telemetryCollector.TrackMemoryCacheCount();
	}
}
