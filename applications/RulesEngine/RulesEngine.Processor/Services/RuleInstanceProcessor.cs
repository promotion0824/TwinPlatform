using EFCore.BulkExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Willow.CognitiveSearch;
using Willow.Expressions;
using Willow.Rules.Cache;
using Willow.Rules.Configuration;
using Willow.Rules.Configuration.Customer;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using Willow.Rules.Sources;
namespace Willow.Rules.Processor;

/// <summary>
/// A rule instance processor
/// </summary>
public interface IRuleInstanceProcessor
{
	/// <summary>
	/// Cache all twins and models from ADT
	/// </summary>
	Task RebuildCache(RuleExecutionRequest rer, CancellationToken cancellationToken);

	/// <summary>
	/// Rebuild one or all of the rules
	/// </summary>
	Task RebuildRules(RuleExecutionRequest request, CancellationToken cancellationToken);

	/// <summary>
	/// Delete a rule and related metadata, instances and actors
	/// </summary>
	Task DeleteRule(RuleExecutionRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Processor for creating Rule Instances
/// </summary>
public class RuleInstanceProcessor : IRuleInstanceProcessor
{
	private readonly WillowEnvironment willowEnvironment;
	private readonly HealthCheckSearch healthCheckSearch;
	private readonly IRepositoryRules repositoryRules;
	private readonly IRepositoryRuleMetadata repositoryRuleMetadata;
	private readonly IRepositoryRuleInstances repositoryRuleInstances;
	private readonly IRuleInstancesService ruleInstancesService;
	private readonly IRepositoryRuleInstanceMetadata repositoryRuleInstanceMetadata;
	private readonly IRepositoryTimeSeriesMapping repositoryTimeSeriesMapping;
	private readonly IRepositoryRuleTimeSeriesMapping repositoryRuleTimeSeriesMapping;
	private readonly IRulesService rulesService;
	private readonly IRepositoryADTSummary repositoryADTSummary;
	private readonly ILoadMemoryGraphService graphService;
	private readonly IRepositoryProgress repositoryProgress;
	private readonly IDataCacheFactory diskCacheFactory;
	private readonly IADTCacheService adtCacheService;
	private readonly ITwinSystemService twinSystemService;
	private readonly ITwinService twinService;
	private readonly IRepositoryActorState repositoryActorState;
	private readonly IRepositoryCalculatedPoint repositoryCalculatedPoint;
	private readonly IRepositoryRuleExecutionRequest repositoryRuleExecutionRequest;
	private readonly ILogger<RuleInstanceProcessor> logger;
	private readonly ExecutionOption executionOption;

	/// <summary>
	/// Creates a new <see cref="RuleInstanceProcessor"/>
	/// </summary>
	public RuleInstanceProcessor(
		WillowEnvironment willowEnvironment,
		HealthCheckSearch healthCheckSearch,
		IRepositoryRuleInstances repositoryRuleInstances,
		IRuleInstancesService ruleInstancesService,
		IRepositoryRuleMetadata repositoryRuleMetadata,
		IRulesService rulesService,
		IRepositoryADTSummary repositoryADTSummary,
		ILoadMemoryGraphService graphService,
		IRepositoryProgress repositoryProgress,
		IDataCacheFactory diskCacheFactory,
		ITwinSystemService twinSystemService,
		ITwinService twinService,
		IADTCacheService adtCacheService,
		IRepositoryRules repositoryRules,
		IRepositoryRuleInstanceMetadata repositoryRuleInstanceMetadata,
		IRepositoryRuleTimeSeriesMapping repositoryRuleTimeSeriesMapping,
		IRepositoryTimeSeriesMapping repositoryTimeSeriesMapping,
		IRepositoryActorState repositoryActorState,
		IRepositoryCalculatedPoint repositoryCalculatedPoint,
		IRepositoryRuleExecutionRequest repositoryRuleExecutionRequest,
		IOptions<CustomerOptions> options,
		ILogger<RuleInstanceProcessor> logger)
	{
		this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
		this.healthCheckSearch = healthCheckSearch;
		this.repositoryRuleInstances = repositoryRuleInstances ?? throw new ArgumentNullException(nameof(repositoryRuleInstances));
		this.ruleInstancesService = ruleInstancesService ?? throw new ArgumentNullException(nameof(ruleInstancesService));
		this.repositoryRuleMetadata = repositoryRuleMetadata ?? throw new ArgumentNullException(nameof(repositoryRuleMetadata));
		this.rulesService = rulesService ?? throw new ArgumentNullException(nameof(rulesService));
		this.repositoryADTSummary = repositoryADTSummary ?? throw new ArgumentNullException(nameof(repositoryADTSummary));
		this.graphService = graphService ?? throw new ArgumentNullException(nameof(graphService));
		this.repositoryProgress = repositoryProgress ?? throw new ArgumentNullException(nameof(repositoryProgress));
		this.diskCacheFactory = diskCacheFactory ?? throw new ArgumentNullException(nameof(diskCacheFactory));
		this.adtCacheService = adtCacheService ?? throw new ArgumentNullException(nameof(adtCacheService));
		this.twinSystemService = twinSystemService ?? throw new ArgumentNullException(nameof(twinSystemService));
		this.twinService = twinService ?? throw new ArgumentNullException(nameof(twinService));
		this.repositoryRules = repositoryRules ?? throw new ArgumentNullException(nameof(repositoryRules));
		this.repositoryRuleInstanceMetadata = repositoryRuleInstanceMetadata ?? throw new ArgumentNullException(nameof(repositoryRuleInstanceMetadata));
		this.repositoryRuleTimeSeriesMapping = repositoryRuleTimeSeriesMapping ?? throw new ArgumentNullException(nameof(repositoryRuleTimeSeriesMapping));
		this.repositoryTimeSeriesMapping = repositoryTimeSeriesMapping ?? throw new ArgumentNullException(nameof(repositoryTimeSeriesMapping));
		this.repositoryActorState = repositoryActorState ?? throw new ArgumentNullException(nameof(repositoryActorState));
		this.repositoryCalculatedPoint = repositoryCalculatedPoint ?? throw new ArgumentNullException(nameof(repositoryCalculatedPoint));
		this.repositoryRuleExecutionRequest = repositoryRuleExecutionRequest ?? throw new ArgumentNullException(nameof(repositoryRuleExecutionRequest));
		this.executionOption = options?.Value?.Execution ?? throw new ArgumentNullException(nameof(options.Value.Execution));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	// Pipeline stages and the data at each step

	// First we find all the distinct Models used in rules and send them in
	private record Stage0(string modelId, IReadOnlyList<(Rule rule, RuleMetadata ruleMetadata)> rules);

	// Each model expands to the twins that we need to examine
	private record Stage1(BasicDigitalTwinPoco twin, IReadOnlyList<(Rule rule, RuleMetadata ruleMetadata)> rules);

	// Each twin gets augmented with a graph
	private record Stage2(TwinDataContext twincontext, IReadOnlyList<(Rule rule, RuleMetadata ruleMetadata)> rules);

	// Rule instances are then generated from each twin system graph
	private record Stage3(Rule rule, RuleMetadata metadata, RuleInstance instance);

	private record Stage4(Rule rule, RuleMetadata metadata, RuleInstance instance);

	// These are then aggregated by Rule

	/// <summary>
	/// Rebuild the rules
	/// </summary>
	public async Task RebuildRules(RuleExecutionRequest request, CancellationToken cancellationToken)
	{
		var expansionStartdate = DateTimeOffset.Now;

		logger.LogInformation("Rebuilding rules {ruleId}", request.RuleId);

		var tracker = new ProgressTrackerForRuleGeneration(repositoryProgress, request.CorrelationId, request.RequestedBy, request.RequestedDate, request.RuleId, logger);

		if (request.Command != RuleExecutionCommandType.BuildRule) return;

		await tracker.Start();

		try
		{
			string ruleId = request.RuleId;

			var globalEnv = await rulesService.AddGlobalsToEnv(Env.Empty.Push());

			globalEnv = await rulesService.AddMLModelsToEnv(globalEnv);

			var timeSeriesMappings = (await repositoryTimeSeriesMapping.Get(v => true)).ToDictionary(v => v.Id);

			int totalRuleTimeseriesMappings = 0;

			async Task addRuleTimeSeriesMapping(RuleInstance ruleInstance)
			{
				//these mappings are used for the adx table link during single rule execution
				foreach (var point in ruleInstance.PointEntityIds)
				{
					if (timeSeriesMappings.TryGetValue(point.Id, out var mapping))
					{
						Interlocked.Increment(ref totalRuleTimeseriesMappings);
						await repositoryRuleTimeSeriesMapping.QueueWrite(new RuleTimeSeriesMapping(ruleInstance, mapping, expansionStartdate), updateCache: false);
					}
				}
			}

			if (request.CalculatedPointsOnly)
			{
				var hasCpRules = await repositoryRules.Any(r => r.TemplateId == RuleTemplateCalculatedPoint.ID);

				await tracker.SetNoRuleInstanceProcessed();

				using (var timedlog = logger.TimeOperation(TimeSpan.FromMinutes(5), "Regenerating ONLY calculated points"))
				{
					foreach(var ruleInstance in  await rulesService.GenerateADTCalculatedPoints(tracker, globalEnv, cancellationToken))
					{
						await addRuleTimeSeriesMapping(ruleInstance);
					}

					await repositoryRuleTimeSeriesMapping.FlushQueue(updateCache: false);
				}

				if (!hasCpRules)
				{
					await tracker.Completed();
					return;
				}
			}
			else
			{
				// Only regen calculated points when no rule is specified
				if (string.IsNullOrEmpty(ruleId))
				{
					using var timedlog = logger.TimeOperation(TimeSpan.FromMinutes(5), "Regenerating calculated points");

					foreach (var ruleInstance in await rulesService.GenerateADTCalculatedPoints(tracker, globalEnv, cancellationToken))
					{
						await addRuleTimeSeriesMapping(ruleInstance);
					}

					await repositoryRuleTimeSeriesMapping.FlushQueue(updateCache: false);
				}
			}

			logger.LogInformation($"Regenerating {ruleId}");

			var sw = Stopwatch.StartNew();
			long swOverallStart = sw.ElapsedTicks;
			long step0ticks = 0;
			long step1Aticks = 0;
			long step1Bticks = 0;
			long step2ticks = 0;
			long step3ticks = 0;
			bool hasErrors = false;
			var throttledLoggerTicks = logger.Throttle(TimeSpan.FromSeconds(15));

			var rulesAndMetadata = (await ruleInstancesService.PrepareRulesAndMetadataForScan(
				request.Force, ruleId, request.CalculatedPointsOnly ? RuleTemplateCalculatedPoint.ID : null)).ToList();

			var rulesLookup = (await repositoryRules.Get(v => true)).ToDictionary(v => v.Id);

			var rulesAndMetadataLookup = rulesAndMetadata.ToDictionary(v => v.rule.Id);
			var totalRules = rulesAndMetadataLookup.Count;
			var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(15));
			//Exclude user editable fields
			var ruleInstanceBulkConfig = new BulkConfig()
			{
				PropertiesToExcludeOnUpdate = new List<string>()
				{
					nameof(RuleInstance.Disabled)
				}
			};

			// Use the Rules Engine producer consumer pattern

			// Control parallelism to ensure we don't swamp SQL
			int parallelism0 = 4;
			int parallelism1 = 16;  // graph build
			int parallelism2 = 16;  // instance generation. Dont increase too much, it just puts strain on the DB during cache get calls
			int parallelism3 = 1;   // avoid DB deadlocks keep to one thread
			int parallelism4 = 1;

			//this ratio can force expansion to use less threads
			//some envs, like Brookfield, has big CPU spikes that can reboot the container due to the large amount if ri's (+-400K)
			if (executionOption.ExpansionParallelismRatio < 1 && executionOption.ExpansionParallelismRatio > 0)
			{
				logger.LogInformation("Overriding ExpansionParallelismRatio with {val}", executionOption.ExpansionParallelismRatio);
				parallelism0 = Math.Max(1, (int)(parallelism0 * executionOption.ExpansionParallelismRatio));
				parallelism1 = Math.Max(1, (int)(parallelism1 * executionOption.ExpansionParallelismRatio));
				parallelism2 = Math.Max(1, (int)(parallelism2 * executionOption.ExpansionParallelismRatio));
			}

			// Step 1 Expand each model to the twins in it keeping the list of rules that we need to apply

			int modelCount = 0;
			int excludedCount = 0;
			int totalInstances = 0;  // volatile
			int countCompleted = 0;  // volatile

			async IAsyncEnumerable<Stage1> expandTwins(Stage0 group)
			{
				string modelId = group.modelId;
				int mc = ++modelCount;
				long startTick = sw.ElapsedTicks;  // step0ticks

				using (var timing = logger.TimeOperation("STEP 0: Expand twins for #{mc} {model}", mc, modelId))
				{
					List<BasicDigitalTwinPoco> twins;

					try
					{
						cancellationToken.ThrowIfCancellationRequested();

						twins = await twinService.GetTwinsByModelWithInheritance(modelId);

						// Each twin x each rule applied to it is about equal to the total instance count
						// minus any that fail

						Interlocked.Add(ref totalInstances, twins.Count * group.rules.Count());
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "STEP 0: Expand twins failed for #{mc} {model}", mc, modelId);
						hasErrors = true;
						yield break;
					}
					finally
					{
						step0ticks += sw.ElapsedTicks - startTick;
					}

					foreach (var twin in twins)
					{
						yield return new Stage1(twin, group.rules);
					}
				}
			}

			async Task<Stage2> graphForOneTwin(Stage1 stage1)
			{
				long startTick = sw.ElapsedTicks;  // step1ticks

				try
				{
					cancellationToken.ThrowIfCancellationRequested();

					var twin = stage1.twin;

					throttledLogger.LogInformation("STEP 1: Create graph {twinId}", stage1.twin.Id);
					var getGraphTimeout = new CancellationTokenSource(TimeSpan.FromMinutes(10)).Token;

					var graphTask = twinSystemService.GetTwinSystemGraph(new[] { twin.Id });

					var graph = await graphTask.WaitAsync(getGraphTimeout);

					step1Aticks += sw.ElapsedTicks - startTick;

					if (!graph.Nodes.Any()) // was TwinDataContext.IsValidGraph(twin, graph))
					{
						throttledLogger.LogWarning("Bad graph result for {twinId}", twin.Id);
						return null;
					}
					else
					{
						long startTickB = sw.ElapsedTicks;  // step1ticks
						var twinContext = TwinDataContext.Create(twin, graph);
						step1Bticks += sw.ElapsedTicks - startTickB;

						//2022/07/21: Certain customers do not want non-commissioned buildings to be included.
						if (twinContext.IsExcluded)
						{
							excludedCount++;
							throttledLogger.LogWarning("{count} twins excluded so far.", excludedCount);
							await tracker.SetTwinsExcluded(excludedCount);
							return null;
						}

						return new Stage2(twinContext, stage1.rules);
					}
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "STEP 1: Create graph failed for {twinId}", stage1.twin.Id);
					hasErrors = true;
					return null;
				}
			}

			async IAsyncEnumerable<Stage3> createRuleInstances(Stage2 stage2)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (stage2 is null) yield break;

				var graphLookup = new Dictionary<string, Abodit.Mutable.Graph<BasicDigitalTwinPoco, WillowRelation>>()
				{
					[stage2.twincontext.Twin.Id] = stage2.twincontext.Graph
				};

				// We have a single twin and a set of rules to apply to it
				foreach (var input in stage2.rules)
				{
					long startTicks = sw.ElapsedTicks;  // step2ticks
					RuleInstance instance;

					try
					{
						throttledLogger.LogInformation("STEP 2: Create instances {twinId} {ruleId}", stage2.twincontext.Twin.Id, input.rule.Id);

						var twinContext = stage2.twincontext;

						instance = await rulesService.ProcessOneTwin(input.rule, twinContext, globalEnv, rulesLookup, graphLookup: graphLookup);
					}
					catch (Exception ex)
					{
						hasErrors = true;
						logger.LogError(ex, "STEP 2: Create instances failed {twinId} {ruleId}", stage2.twincontext.Twin.Id, input.rule.Id);
						yield break;
					}
					finally
					{
						step2ticks += sw.ElapsedTicks - startTicks;
					}

					yield return new Stage3(input.rule, input.ruleMetadata, instance);
				};
			}

			async Task<Stage4> writeInstance(Stage3 stage3)
			{
				long startTick = sw.ElapsedTicks;
				try
				{
					cancellationToken.ThrowIfCancellationRequested();
					throttledLogger.LogInformation("STEP 3: Writing instance {instanceId}", stage3.instance.Id);

					await repositoryRuleInstances.QueueWrite(stage3.instance, updateCache: false, config: ruleInstanceBulkConfig, queueSize: 1000, batchSize: 1000);

					if(stage3.instance.Status.HasFlag(RuleInstanceStatus.Valid))
					{
						await addRuleTimeSeriesMapping(stage3.instance);
					}

					stage3.metadata.RuleInstanceStatus |= stage3.instance.Status;

					return new Stage4(stage3.rule, stage3.metadata, stage3.instance);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "STEP 3: Writing instance failed {instanceId}", stage3.instance.Id);
					hasErrors = true;
					return null;
				}
				finally
				{
					step3ticks += sw.ElapsedTicks - startTick;

					double elapsed = sw.ElapsedTicks - swOverallStart + 1;
					double step0percentage = step0ticks / elapsed / parallelism0;
					double step1Apercentage = step1Aticks / elapsed / parallelism1;
					double step1Bpercentage = step1Bticks / elapsed / parallelism1;
					double step2percentage = step2ticks / elapsed / parallelism2;
					double step3percentage = step3ticks / elapsed / parallelism3;
					throttledLoggerTicks.LogInformation("Rule instance progress s0:{step0:P0} --> s1A:{step1A:P0} s1B:{step1B:P0} --> s2:{step2:P0} --> s3:{step3:P0}. Completed {completeCount}/{totalCount}", step0percentage, step1Apercentage, step1Bpercentage, step2percentage, step3percentage, countCompleted, totalInstances);
				}
			}

			// Then we regroup by rule and write the metadata
			async Task<Stage4> updateMetata(Stage4 stage4)
			{
				if (stage4 == null)
				{
					return null;
				}

				try
				{
					cancellationToken.ThrowIfCancellationRequested();
					throttledLogger.LogInformation("STEP 4: Finishing up {ruleId}", stage4.rule.Id);

					Interlocked.Increment(ref countCompleted);
					await tracker.SetInstancesProcessed2(countCompleted, totalInstances);

					// Mark instance count immediately so that UI sees it
					stage4.metadata.RuleInstanceCount++;

					if (stage4.instance.Status.HasFlag(RuleInstanceStatus.Valid))
					{
						stage4.metadata.ValidInstanceCount++;
						stage4.metadata.CommandsGenerated += stage4.instance.RuleTriggersBound.Count;
					}

					//smaller queue size to get UI feedback
					await repositoryRuleMetadata.QueueWrite(stage4.metadata, updateCache: false, queueSize: 100);

					return stage4;
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "STEP 4: Finishing up failed {ruleId}", stage4.rule.Id);
					return null;
				}
			}

			var input = rulesAndMetadata
				.GroupBy(v => v.rule.TemplateId == RuleTemplateCalculatedPoint.ID ? v.rule.RelatedModelId ?? v.rule.PrimaryModelId : v.rule.PrimaryModelId)
				.Select(g => new Stage0(g.Key, g.ToList()));

			var source = Channel.CreateBounded<Stage0>(10);  // Small pre-buffer

			var producer = Task.Run(async () =>
			{
				int c = 0;
				using (var timedLogger = logger.TimeOperation("Iterating over all rule instances"))
				{
					foreach (var item in input)
					{
						c++;
						await source.Writer.WriteAsync(item, cancellationToken);
					}
					source.Writer.Complete();
				}
				logger.LogInformation("Completed producer for rule generation, {count} rules processed", c);
			});

			var processed2 = await source.Reader
				// 8 parallel tasks to expand the system graph for each model Id
				.Split(parallelism0, x => x.modelId, cancellationToken)
				.Select(c => c.TransformManyAsync<Stage0, Stage1>(x => expandTwins(x), cancellationToken))
				.Merge(cancellationToken)
				// Now expand the pipeline back out with 8 parallel tasks generating each rule
				// Could use any key here, they can all run in parallel, Id is well distributed
				// memory cache coherent
				.Split(parallelism1, x => x.twin.Id, cancellationToken)
				.Select(c => c.TransformAsync(x => graphForOneTwin(x), cancellationToken))
				.Merge(cancellationToken)
				.Where(x => x != null)
				// And once again split back out to 8 channels for instance generation
				.Split(parallelism2, x => x.twincontext.Twin.Id, cancellationToken)
				.Select(c => c.TransformManyAsync(x => createRuleInstances(x), cancellationToken))
				.Merge(cancellationToken)
				.Split(parallelism3, x => x.rule.Id, cancellationToken)
				.Select(c => c.TransformAsync(x => writeInstance(x), cancellationToken))
				.Merge(cancellationToken)
				.Split(parallelism4, x => x.rule.Id, cancellationToken)
				.Select(c => c.TransformAsync(x => updateMetata(x), cancellationToken))
				.Merge(cancellationToken)
				.ReadAllAsync(cancellationToken)
				.AllAsync(x => true, cancellationToken);

			// Finally write all the metadata
			foreach (var r in rulesAndMetadata)
			{
				var metadata = r.metadata;
				var rule = r.rule;

				metadata.ScanCompleted(rule);

				metadata.AddLog($"Skill Expansion finished. Total Instances: {metadata.RuleInstanceCount}. Valid Instances: {metadata.ValidInstanceCount}", request.RequestedBy);

				await repositoryRuleMetadata.QueueWrite(metadata, updateCache: false);
			}

			await repositoryRuleInstances.FlushQueue(updateCache: false, config: ruleInstanceBulkConfig);

			await repositoryRuleTimeSeriesMapping.FlushQueue(updateCache: false);

			await repositoryRuleMetadata.FlushQueue(updateCache: false);

			if (!hasErrors)
			{
				using (var timing = logger.TimeOperation(TimeSpan.FromMinutes(1), "Clearing Rule Instances {ruleId}", ruleId ?? ""))
				{
					using (var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
					{
						timeoutSource.CancelAfter(TimeSpan.FromMinutes(2));

						try
						{
							int count = await repositoryRuleInstances.DeleteInstancesBefore(expansionStartdate, ruleId, request.CalculatedPointsOnly, timeoutSource.Token);
							logger.LogInformation("Removed {count} rule instances before {before}", count, expansionStartdate);

							count = await repositoryRuleTimeSeriesMapping.DeleteBefore(expansionStartdate, ruleId, timeoutSource.Token);
							logger.LogInformation("Removed {count} rule time series mappings before {before}", count, expansionStartdate);
						}
						catch (OperationCanceledException ex)
						{
							logger.LogError(ex, $"Failed to remove old rules {ex.Message}");
						}
					}
				}
			}

			//Now we process the calculated points for applicable rules
			//This happens after clearing rule instances before expansionStartdate so calculated points for those are scheduled for deletion
			using (var timing = logger.TimeOperation(TimeSpan.FromMinutes(1), "Process calculated points {ruleId}", ruleId ?? ""))
			{
				try
				{
					cancellationToken.ThrowIfCancellationRequested();

					var calculatedPointRules = rulesAndMetadata.Select(rm => rm.rule).Where(r => r.TemplateId == RuleTemplateCalculatedPoint.ID);
					if (calculatedPointRules.Any())
					{
						await rulesService.ProcessCalculatedPoints(tracker, calculatedPointRules);

						//We only queue processing of calculated points in ADT for rules that is ADT Sync Enabled
						var adtEnabledCalculatedPointRules = calculatedPointRules.Where(r => r.ADTEnabled);
						if (adtEnabledCalculatedPointRules.Any())
						{
							var processCPsRequest = RuleExecutionRequest.CreateProcessCalculatedPointsRequest(
								request.CustomerEnvironmentId, requestedBy: request.RequestedBy, ruleId: request.RuleId ?? null);
							await repositoryRuleExecutionRequest.UpsertOne(processCPsRequest, cancellationToken: cancellationToken);
						}
					}
				}
				catch (OperationCanceledException ex)
				{
					logger.LogError(ex, $"Failed to process calculated points {ex.Message}. Process cancelled.");
				}
				catch (Exception ex)
				{
					logger.LogError(ex, $"Failed to process calculated points {ex.Message}.");
				}
			}

			await tracker.Completed();

			await Task.WhenAll(producer);  // observe any exception

			if (excludedCount > 0)
			{
				logger.LogWarning("{count} twins excluded as non-commissioned.", excludedCount);
			}

			logger.LogInformation("Regenerated rules {countCompleted:N0} instances for {willowEnvironment} took {totalMinutes:0.0} minutes. Total rule mappings {mappingsCount}", countCompleted, willowEnvironment.Id, sw.Elapsed.TotalMinutes, totalRuleTimeseriesMappings);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Rebuild Rules failed");
			await tracker.Failed();
		}

		if (cancellationToken.IsCancellationRequested)
		{
			await tracker.Cancelled();
		}
	}

	private class CachedTwinsAndRelationshipsFlag
	{
	}

	/// <summary>
	/// Rebuild the disk cache
	/// </summary>
	/// <remarks>
	/// Does not clear the cache before loading so cannot handle deletions yet
	/// </remarks>
	public async Task RebuildCache(RuleExecutionRequest rer, CancellationToken cancellationToken)
	{
		var progressTracker = new ProgressTrackerForCache(this.repositoryProgress, this.logger, rer.CorrelationId, rer.RequestedBy, rer.RequestedDate);

		await progressTracker.ReportStarting();

		try
		{
			DateTimeOffset startTime = DateTimeOffset.Now;

			using (var timedlogOuter = logger.TimeOperation("Rebuilding cache for {willowEnvironment}", this.willowEnvironment.Id))
			{
				logger.LogInformation($"Rebuilding cache for {this.willowEnvironment.Id}");

				// You cannot refresh from ADT more than once per ten minutes
				var diskCacheProgress = this.diskCacheFactory.Get<CachedTwinsAndRelationshipsFlag>("cacheProgress",
					TimeSpan.FromMinutes(10), CachePolicy.EagerReload, MemoryCachePolicy.NoMemoryCache);

				var alreadyThere = await diskCacheProgress.TryGetValue(willowEnvironment.Id, "cacheProgress");
				if (alreadyThere.ok)
				{
					logger.LogWarning("Skipping full fetch from ADT as it was fetched less than ten minutes ago");
				}

				cancellationToken.ThrowIfCancellationRequested();

				// TO SKIP THE FETCH ALL TWINS STEP
				//var progressForceMock = await diskCacheProgress.AddOrUpdate(willowEnvironment.Id, "cacheProgress", new CachedTwinsAndRelationshipsFlag { });
				// TO INCLUDE THE FETCH ALL STEPS
#if DEBUG
				await diskCacheProgress.RemoveKey(willowEnvironment.Id, "cacheProgress");
#endif
				using (var timedlog = logger.TimeOperation(TimeSpan.FromMinutes(2),
					"Cache Twin nodes and edges for {willowEnvironment}", this.willowEnvironment.Id))
				{
					var progress = await diskCacheProgress.GetOrCreateAsync(willowEnvironment.Id, "cacheProgress",
					async () =>
					{
						using (var timedlog = logger.TimeOperation(TimeSpan.FromMinutes(2), "Fetching all twins and relationships for {willowEnvironment}", this.willowEnvironment.Id))
						{
							try
							{
								await adtCacheService.CacheTwin(willowEnvironment, progressTracker, refreshTwins: rer.ExtendedData.OnlyRefreshTwins, refreshRelationships: rer.ExtendedData.OnlyRefreshRelationships, cancellationToken: cancellationToken);
							}
							catch (Exception ex)
							{
								logger.LogWarning(ex, "Failed to cache twins and relationships");
								throw;
							}
							return new CachedTwinsAndRelationshipsFlag { };
						}
					});
				}

				cancellationToken.ThrowIfCancellationRequested();

				// TODO: ProgressTracker on adding graph - it's slow too

				// Now generate the graphs
				var summary = await repositoryADTSummary.GetLatest();
				using (var timedlog = logger.TimeOperation(TimeSpan.FromMinutes(2),
					"Building the graphs for {willowEnvironment}", this.willowEnvironment.Id))
				{
					summary = await graphService.AddToSummary(summary, willowEnvironment, progressTracker, cancellationToken);
				}

				// Put a counts of twins, and models into the database
				using (var timedlog = logger.TimeOperation(TimeSpan.FromMilliseconds(100), "Putting the summary in DB"))
				{
					await this.repositoryADTSummary.UpdateADTRelatedSummary(summary);
				}
				logger.LogInformation("Updated summary in database");

				//dont do overall delete for partial updates
				if (rer.ExtendedData.OnlyRefreshTwins && rer.ExtendedData.OnlyRefreshRelationships)
				{
					try
					{
						using (var timedlog = logger.TimeOperation(TimeSpan.FromMinutes(2), "Clearing old cache entries"))
						{
							//clear any orphaned cache keys, but also arbitrary keys that could have been linked to older twin data
							//this keeps the cache table small.
							await diskCacheFactory.ClearCacheBefore(startTime);
						}
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "Failed to clear old/orphaned cache exntries");
					}
				}

				await progressTracker.ReportComplete();
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Cache rebuild failed");
			await progressTracker.Failed(ex.Message);
		}

		if (cancellationToken.IsCancellationRequested)
		{
			await progressTracker.Cancelled();
		}
	}

	/// <summary>
	/// Delete a rule and related metadata, instances and actors
	/// </summary>
	public async Task DeleteRule(RuleExecutionRequest request, CancellationToken cancellationToken)
	{
		var progressTracker = new ProgressTracker(repositoryProgress, Progress.DeleteRuleId, ProgressType.DeleteRule, request.CorrelationId, request.RequestedBy, request.RequestedDate, request.RuleId, logger);

		await progressTracker.Start();

		using (var timing = logger.TimeOperation(TimeSpan.FromMinutes(1), "Clearing Rule and data for {ruleId}", request.RuleId))
		{
			try
			{
				var ruleCount = await repositoryRules.DeleteRuleById(request.RuleId, cancellationToken);
				await progressTracker.SetValues("Skill", ruleCount > 0 ? ruleCount : 1, 1, isIgnored: false, force: true);

				var metaDataCount = await repositoryRuleMetadata.DeleteMetadataByRuleId(request.RuleId, cancellationToken);
				await progressTracker.SetValues("Metadata", metaDataCount, 1, isIgnored: false, force: true);

				//Instance metadata must be cleared before instances as the sql query uses a join between the tables
				var instanceMetadataCount = await repositoryRuleInstanceMetadata.DeleteMetadataByRuleId(request.RuleId, cancellationToken);
				await progressTracker.SetValues("Instance Metadata", instanceMetadataCount, instanceMetadataCount, isIgnored: false, force: true);

				var instanceCount = await repositoryRuleInstances.DeleteInstancesByRuleId(request.RuleId, cancellationToken);
				await progressTracker.SetValues("Instances", instanceCount, instanceCount, isIgnored: false, force: true);

				var actorsCount = await repositoryActorState.DeleteActorsByRuleId(request.RuleId, cancellationToken);
				await progressTracker.SetValues("Actors", actorsCount, actorsCount, isIgnored: false, force: true);

				var deleteInsightsRequest = RuleExecutionRequest.CreateDeleteAllMatchingInsightsRequest(request.CustomerEnvironmentId, request.RuleId, request.RequestedBy);

				await repositoryRuleExecutionRequest.UpsertOne(deleteInsightsRequest);

				var deleteCommandsRequest = RuleExecutionRequest.CreateDeleteAllMatchingCommandsRequest(request.CustomerEnvironmentId, request.RuleId, request.RequestedBy);

				await repositoryRuleExecutionRequest.UpsertOne(deleteCommandsRequest);

				//Need to set action required as delete for calculated points with this rule id
				if (await repositoryCalculatedPoint.Any(cp => cp.RuleId == request.RuleId))
				{
					var cpCount = await repositoryCalculatedPoint.ScheduleDeleteCalculatedPointsByRuleId(request.RuleId, cancellationToken);
					await progressTracker.SetValues("Calculated Points", cpCount, cpCount, isIgnored: false, force: true);

					var processCPsRequest =
						RuleExecutionRequest.CreateProcessCalculatedPointsRequest(willowEnvironment.Id, requestedBy: request.RequestedBy, ruleId: request.RuleId);
					await repositoryRuleExecutionRequest.UpsertOne(processCPsRequest, cancellationToken: cancellationToken);
				}

				await progressTracker.Completed();
			}
			catch (OperationCanceledException ex)
			{
				logger.LogError(ex, "Remove rule {ruleId} cancelled", request.RuleId);
				await progressTracker.Cancelled();
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to remove rule {ruleId}", request.RuleId);
				await progressTracker.Failed();
			}
		}
	}
}
