using EFCore.BulkExtensions;
using Microsoft.Extensions.Logging;
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
using Willow.Rules.Cache;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Sources;

namespace Willow.Rules.Services;

/// <summary>
/// Service for caching ADT
/// </summary>
public interface IADTCacheService
{
	/// <summary>
	/// Read ADT and cache the twins and relationships
	/// </summary>
	Task CacheTwin(WillowEnvironment willowEnvironment, IProgressTrackerForCache progressTracker, bool refreshTwins = true, bool refreshRelationships = true, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for caching ADT
/// </summary>
public class ADTCacheService : IADTCacheService
{
	private readonly ILogger<ADTCacheService> logger;
	private readonly ILogger throttledLogger;
	private readonly IModelService modelService;
	private readonly ITwinService twinService;
	private readonly IRepositoryCalculatedPoint repositoryCalculatedPoint;
	private readonly IDataCacheFactory diskCacheFactory;
	private readonly IRepositoryTimeSeriesMapping repositoryTimeSeriesMapping;
	private readonly ITwinGraphService twinGraphService;
	private readonly ADTInstance[] adtInstances;
	private readonly HealthCheckADT healthCheckADT;

	/// <summary>
	/// Creates a new ADT Cache Service
	/// </summary>
	public ADTCacheService(
		IADTService adtService,
		IModelService modelService,
		ITwinService twinService,
		IRepositoryCalculatedPoint repositoryCalculatedPoint,
		IDataCacheFactory diskCacheFactory,
		IRepositoryTimeSeriesMapping repositoryTimeSeriesMapping,
		ITwinGraphService twinGraphService,
		HealthCheckADT healthCheckADT,
		ILogger<ADTCacheService> logger
		)
	{
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.throttledLogger = logger.Throttle(TimeSpan.FromSeconds(15));
		this.modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
		this.twinService = twinService ?? throw new ArgumentNullException(nameof(twinService));
		this.repositoryCalculatedPoint = repositoryCalculatedPoint ?? throw new ArgumentNullException(nameof(repositoryCalculatedPoint));
		this.diskCacheFactory = diskCacheFactory ?? throw new ArgumentNullException(nameof(diskCacheFactory));
		this.repositoryTimeSeriesMapping = repositoryTimeSeriesMapping ?? throw new ArgumentNullException(nameof(repositoryTimeSeriesMapping));
		this.healthCheckADT = healthCheckADT ?? throw new ArgumentNullException(nameof(healthCheckADT));
		this.twinGraphService = twinGraphService ?? throw new ArgumentNullException(nameof(twinGraphService));
		this.adtInstances = adtService.AdtInstances;
	}

	/// <summary>
	/// Read ADT and cache the twins and relationships
	/// </summary>
	public async Task CacheTwin(
		WillowEnvironment willowEnvironment,
		IProgressTrackerForCache progressTracker,
		bool refreshTwins = true,
		bool refreshRelationships = true,
		CancellationToken cancellationToken = default)
	{
		using var scope = logger.BeginScope(new Dictionary<string, object>()
		{
			["Step"] = "CacheTwin"
		});

		try
		{
			DateTimeOffset startTime = DateTimeOffset.Now;

			int instanceCount = adtInstances.Length;

			using (var timing = logger.TimeOperation("FIRST STEP: GET ALL MODELS FOR {instanceCount} ADTs", instanceCount))
			{
				using var scope1 = logger.BeginScope("CacheTwin - FIRST STEP");
				var modelDataList = await modelService.ReplaceModelsInCacheAsync(progressTracker, cancellationToken);
			}

			if (refreshTwins)
			{
				using (var timing = logger.TimeOperation("SECOND STEP: GET ALL TWINS FOR {instanceCount} ADTs", instanceCount))
				{
					using var scope2 = logger.BeginScope("CacheTwin - SECOND STEP");
					await ReplaceTwinsInCache(willowEnvironment, progressTracker, cancellationToken);
					// this will clean out any that are older than the start date
				}
			}

			if (refreshRelationships)
			{
				// Get the relationships, but hopefully by now we already have all the twins we need
				using (var timing = logger.TimeOperation("THIRD STEP: FETCH RELATIONSHIPS FOR {instanceCount} ADTs", instanceCount))
				{
					using var scope3 = logger.BeginScope("CacheTwin - THIRD STEP");
					await ReplaceRelationshipsInCache(willowEnvironment, progressTracker, instanceCount, cancellationToken);
					// this will clean out any that are older than the start date and any related cache entries
				}
			}

			// Refresh twin location properties for UI and expansion
			//always refresh location properties becuase they are lost during twin updates
			using (var timing = logger.TimeOperation("FOURTH STEP: UPDATE TWIN LOCATIONS {instanceCount} ADTs", instanceCount))
			{
				using var scope4 = logger.BeginScope("CacheTwin - FOURTH STEP");
				await UpdateTwinLocations(willowEnvironment, progressTracker, cancellationToken);
			}

			// And now flush the cache for anything that depends on models or relationships
			logger.LogInformation("Removing old cached items");

			if (refreshTwins)
			{
				await diskCacheFactory.TwinCache.RemoveItems(willowEnvironment.Id, startTime);
			}

			if (refreshRelationships)
			{
				await diskCacheFactory.ExtendedRelationships.RemoveItems(willowEnvironment.Id, startTime);
				await diskCacheFactory.ForwardEdgeCache.RemoveItems(willowEnvironment.Id, startTime);
				await diskCacheFactory.BackEdgeCache.RemoveItems(willowEnvironment.Id, startTime);
				// The graph of a system of nodes from one or more starting points
				await diskCacheFactory.TwinSystemGraphCache.RemoveItems(willowEnvironment.Id, startTime);
			}

			//not twin specific
			await diskCacheFactory.OntologyCache.RemoveItems(willowEnvironment.Id, startTime);
			await diskCacheFactory.AllModelsCache.RemoveItems(willowEnvironment.Id, startTime);
			await diskCacheFactory.MetaModelGraph.RemoveItems(willowEnvironment.Id, startTime);
			await diskCacheFactory.DiskCacheTwinsByModelWithInheritance.RemoveItems(willowEnvironment.Id, startTime);
			await diskCacheFactory.AdtQueryResult.RemoveItems(willowEnvironment.Id, startTime);
			await diskCacheFactory.MetaSystemGraphs.RemoveItems(willowEnvironment.Id, startTime);
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Failed to cache ADT");
			throw;
		}
	}

	/// <summary>
	/// Update twin location properties for fast lookup during expansion
	/// </summary>
	private async Task UpdateTwinLocations(
		WillowEnvironment willowEnvironment,
		IProgressTrackerForCache progressTracker,
		CancellationToken cancellationToken)
	{
		var twinGraph = await twinGraphService.GetGraphCachedAsync(cancellationToken);

		Dictionary<string, MiniTwinDto> lookup = twinGraph.Nodes.ToDictionary(x => x.Id, x => x);

		int count = 0;
		int total = await diskCacheFactory.TwinCache.Count(willowEnvironment.Id);

		var calcPointConfig = new BulkConfig()
		{
			PropertiesToInclude = new List<string>()
			{
				nameof(CalculatedPoint.TwinLocations)
			}
		};

		var timeSeriesConfig = new BulkConfig()
		{
			PropertiesToInclude = new List<string>()
			{
				nameof(TimeSeriesMapping.TwinLocations)
			}
		};

		void updateTwinLocations(BasicDigitalTwinPoco twin)
		{
			if (lookup.TryGetValue(twin.Id, out var startNode))
			{
				var locations = twinGraph
						.Successors<MiniTwinDto>(startNode, (s, p, e) => (p.Name == "isPartOf" || p.Name == "locatedIn" || p.Name == "isCapabilityOf")).TopologicalSortApprox();

				twin.Locations =
						locations
						.Where(v => v != startNode)
						.Select(async t => await diskCacheFactory.TwinCache.TryGetValue(willowEnvironment.Id, t.Id))
						.Select(x => x.Result.result)
						.Where(x => x is not null)
						.Select(x => new TwinLocation(x!.Id, x!.name, x.ModelId()))
						.ToArray();
			}
		};

		await foreach (var twin in diskCacheFactory.TwinCache.GetAll(willowEnvironment.Id))
		{
			updateTwinLocations(twin);

			if (twin.Locations.Any())
			{
				count++;

				await diskCacheFactory.TwinCache.AddOrUpdate(willowEnvironment.Id, twin.Id, twin);

				await progressTracker.ReportTwinLocationUpdateCount(count, total);

				if(IsCalcPoint(twin))
				{
					var calcPoint = new CalculatedPoint()
					{
						Id = twin.Id,
						TwinLocations = twin.Locations
					};

					await repositoryCalculatedPoint.QueueWrite(calcPoint, updateCache: false, updateOnly: true, config: calcPointConfig);
				}

				if (HasTelemetry(twin))
				{
					var mapping = new TimeSeriesMapping()
					{
						Id = twin.Id,
						TwinLocations = twin.Locations
					};

					await repositoryTimeSeriesMapping.QueueWrite(mapping, updateCache: false, updateOnly: true, config: timeSeriesConfig);
				}

				throttledLogger.LogInformation("Twin Location updated: {count}/{total}", count, total);
			}
		}

		await repositoryCalculatedPoint.FlushQueue(updateOnly: true, updateCache: false, config: calcPointConfig);
		await repositoryTimeSeriesMapping.FlushQueue(updateOnly: true, updateCache: false, config: calcPointConfig);
	}

	/// <summary>
	/// Read all the relationships from ADT and store them in the cache
	/// </summary>
	/// <remarks>
	/// Also creates fast lookups for backward and forward edges from a node
	/// </remarks>
	private async Task ReplaceRelationshipsInCache(
		WillowEnvironment willowEnvironment,
		IProgressTrackerForCache progressTracker,
		int instanceCount,
		CancellationToken cancellationToken)
	{
		// These could be quite large
		ConcurrentDictionary<string, List<Edge>> forwardEdges = new();
		ConcurrentDictionary<string, List<Edge>> backwardEdges = new();

		var extendedRelationships = diskCacheFactory.ExtendedRelationships;
		var twinCache = diskCacheFactory.TwinCache;
		var forwardEdgeCache = diskCacheFactory.ForwardEdgeCache;
		var backwardEdgeCache = diskCacheFactory.BackEdgeCache;

		var startTime = DateTimeOffset.UtcNow;

		using (var timingBlockRelationships = logger.TimeOperation(TimeSpan.FromMinutes(10), "Fetch all relationships from all ADT instances"))
		{
			var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(30));
			await Parallel.ForEachAsync(adtInstances, cancellationToken, async (adtInstance, cancellationToken) =>
			{
				int relationshipCount = 0;

				try
				{
					int relationshipTotal = await adtInstance.ADTClient.GetRelationshipsCountAsync();
					await progressTracker.ReportRelationshipCount(adtInstance.Uri, 0, relationshipTotal);

					logger.LogInformation("Total relationships: {countRelationships:N0}", relationshipTotal);

					Stopwatch sw = Stopwatch.StartNew();
					long now = sw.ElapsedTicks;
					long start = now;

					var channel = Channel.CreateBounded<ExtendedRelationship>(new BoundedChannelOptions(100)
					{
						SingleWriter = true
					});

					Task producer = Task.Run(async () =>
					{
						try
						{
							await foreach (var rel in this.twinService.QueryAllRelationships(adtInstance.ADTClient))
							{
								if(!(await twinService.IsRelationshipAllowed(rel)))
								{
									continue;
								}

								await channel.Writer.WriteAsync(rel, cancellationToken);
							}
						}
						catch (OperationCanceledException)
						{
							throw;
						}
						catch (Exception ex)
						{
							logger.LogError(ex, "Failed to produce all relationships");
						}
						channel.Writer.Complete();
						logger.LogInformation("Completed producing relationships - completed channel");
					}, cancellationToken);

					var multiplex = channel.Reader.Split(8, x => x.Id, cancellationToken)
						.Select(c => c.TransformAsync(r =>
							ProcessOneRelationship(extendedRelationships, willowEnvironment, r, twinCache, forwardEdges, backwardEdges)));

					var merged = ChannelExtensions.Merge(multiplex.ToArray(), cancellationToken);

					await foreach (var result in merged.ReadAllAsync(cancellationToken))
					{
						// Back on a single thread
						relationshipCount++;

						try
						{
							double percentage = (double)relationshipCount / (relationshipTotal + 1);
							throttledLogger.LogInformation("{relationshipCount:N0}/{relationshipTotal:N0} relationships {percentage:P2}",
								relationshipCount, relationshipTotal, percentage);
							await progressTracker.ReportRelationshipCount(adtInstance.Uri, relationshipCount, relationshipTotal);
						}
						catch (Exception ex)
						{
							logger.LogError(ex, "Failed to update status scanning relationships");
						}
					}

					if (!producer.IsCompleted)
					{
						logger.LogInformation("Producer should be complete");
						await Task.WhenAny(producer, Task.Delay(TimeSpan.FromSeconds(30), cancellationToken));
						if (!producer.IsCompleted)
						{
							logger.LogInformation("Producer is still not complete (30s later)");
						}
					}

					// And send the final count so it looks right in the UI
					await progressTracker.ReportRelationshipCount(adtInstance.Uri, relationshipCount, relationshipCount);
				}
				catch (OperationCanceledException)
				{
					throw;
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Failed to read relationships for {adt}", adtInstance.Uri);
				}

				logger.LogInformation("Completed read of {relationshipCount:N0} relationships", relationshipCount);
			});
		} // Fetch all relationships from all ADT instances completed in 8.1min

		if (instanceCount > 1)
		{
			logger.LogInformation("Completed read of all relationships from all ADT instances");
		}

		logger.LogInformation("Start forward edges");

		// Write out all the forward and backward edges, cannot do this until we have read all the relationships
		Task fwdTask = Task.Run(async () =>
		{
			try
			{
				using (var timingBlockInternal = logger.TimeOperation(TimeSpan.FromMinutes(5), "Add forward edges"))
				{
					var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(30));
					// THIS SECTION IS TOO SLOW NOW, WAITS FOR EACH WRITE TO DISTRIBUTED CACHE?
					// PARALLELIZE IT

					int countForward = 0;
					int totalForward = forwardEdges.Count;

					foreach (var fwd in forwardEdges)
					{
						try
						{
							countForward++;
							cancellationToken.ThrowIfCancellationRequested();
							throttledLogger.LogDebug("Adding forward edge {key} {percentage:P1}", fwd.Key, countForward / (float)totalForward);
							await forwardEdgeCache.AddOrUpdate(willowEnvironment.Id, fwd.Key, new CollectionWrapper<Edge>(fwd.Value));
							await progressTracker.ReportForwardBuildCount(countForward, totalForward);
						}
						catch (OperationCanceledException)
						{
							throw;  // let outer try catch handle this one and break out of loop
						}
						catch (Exception ex)
						{
							logger.LogError(ex, "Failed to write forward edges for {twinid}", fwd.Key);
						}
					}

					logger.LogInformation("Completed write of forward edges");
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to cache forward edges");
			}
		});

		logger.LogInformation("Start backward edges");

		Task bwdTask = Task.Run(async () =>
		{
			try
			{
				using (var timingBlockInternal = logger.TimeOperation(TimeSpan.FromMinutes(5), "Add backward edges"))
				{
					var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(30));
					int countBackward = 0;
					int totalBackward = backwardEdges.Count;

					foreach (var bwd in backwardEdges)
					{
						try
						{
							countBackward++;
							throttledLogger.LogDebug("Adding backward edge {key} {percentage:P1}", bwd.Key, countBackward / (float)totalBackward);
							await backwardEdgeCache.AddOrUpdate(willowEnvironment.Id, bwd.Key, new CollectionWrapper<Edge>(bwd.Value));
							await progressTracker.ReportBackwardBuildCount(countBackward, totalBackward);
						}
						catch (OperationCanceledException)
						{
							throw;  // let outer try catch handle this one and break out of loop
						}
						catch (Exception ex)
						{
							logger.LogError(ex, "Failed to write backward edges for {twinid}", bwd.Key);
						}
					}
				}
				logger.LogInformation("Completed write of backward edges");
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to cache backward edges");
			}
		}, cancellationToken);

		logger.LogInformation("Wait for forward and backward edges");
		await Task.WhenAll(fwdTask, bwdTask);
	}

	private async Task<int> ProcessOneRelationship(IDataCache<ExtendedRelationship> extendedRelationships, WillowEnvironment willowEnvironment, ExtendedRelationship rel, IDataCache<BasicDigitalTwinPoco> twinCache, ConcurrentDictionary<string, List<Edge>> forwardEdges, ConcurrentDictionary<string, List<Edge>> backwardEdges)
	{
		try
		{
			await extendedRelationships.AddOrUpdate(willowEnvironment.Id, rel.Id, rel);

			(var targetok, var target) = await twinCache.TryGetValue(willowEnvironment.Id, rel.TargetId);
			(var sourceok, var source) = await twinCache.TryGetValue(willowEnvironment.Id, rel.SourceId);

			if (targetok)
			{
				var edge = new Edge { Destination = target, Substance = rel.substance, RelationshipType = rel.Name };
				forwardEdges.AddOrUpdate(rel.SourceId, edge);  // extension method
			}

			if (sourceok)
			{
				var edge = new Edge { Destination = source, Substance = rel.substance, RelationshipType = rel.Name };
				backwardEdges.AddOrUpdate(rel.TargetId, edge);
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to ProcessOneRelationship");
		}

		return 0;
	}


	private async Task ReplaceTwinsInCache(
		WillowEnvironment willowEnvironment,
		IProgressTrackerForCache progressTracker,
		CancellationToken cancellationToken = default)
	{
		//int instanceCount = willowEnvironment.ADTInstances.Count();
		logger.LogInformation("Replace twins in cache");

		// TODO: Wipe old twins that have been deleted after processing the cache

		var twinCache = diskCacheFactory.TwinCache;
		var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(15));
		var throttledLogger2 = logger.Throttle(TimeSpan.FromSeconds(15));
		var startTime = DateTimeOffset.UtcNow;
		var forwardEdgeCache = diskCacheFactory.ForwardEdgeCache;
		var backwardEdgeCache = diskCacheFactory.BackEdgeCache;

		await Parallel.ForEachAsync(adtInstances, cancellationToken, async (adtInstance, cancellationToken) =>
		{
			try
			{
				int c = 0;

				int twinCount = await adtInstance.ADTClient.GetTwinsCountAsync();
				int relationshipCount = await adtInstance.ADTClient.GetRelationshipsCountAsync();

				healthCheckADT.Current = HealthCheckADT.HealthyWithCounts(twinCount, relationshipCount);

				await progressTracker.ReportTwinCount(adtInstance.Uri, 0, twinCount);
				await progressTracker.ReportTwinLocationUpdateCount(0, twinCount);
				await progressTracker.ReportRelationshipCount(adtInstance.Uri, 0, relationshipCount);
				await progressTracker.ReportGraphBuildCount("Twins", 0, twinCount);
				await progressTracker.ReportGraphBuildCount("Relationships", 0, relationshipCount);
				await progressTracker.ReportGraphBuildCount("MetaEdge", 0, relationshipCount);

				// TODO: Move this to TwinService
				var twins = adtInstance.ADTClient.QueryAsync<BasicDigitalTwinPoco>("SELECT * FROM DIGITALTWINS", cancellationToken);

				logger.LogDebug("Processing ADT instance {adtInstance}", adtInstance.Uri);

				Stopwatch sw = Stopwatch.StartNew();
				long start = sw.ElapsedTicks;
				long now = start;

				long overheadCycles = 0;
				long diskCycles = 0;
				long reportCycles = 0;
				long otherCycles = 0;
				long calculatedPointCycles = 0;
				DateTimeOffset lastReport = DateTimeOffset.Now;

				var source = Channel.CreateBounded<BasicDigitalTwinPoco>(10);  // Small pre-buffer

				var producer = Task.Run(async () =>
					{
						using var timedLogger = logger.TimeOperation("Producer for twin caching");

						await foreach (var twin in twins)
						{
							await source.Writer.WriteAsync(twin, cancellationToken);
						}

						source.Writer.Complete();
					}, cancellationToken);

				const int parallelism = 8;

				async Task<bool> processOneTwin(BasicDigitalTwinPoco tw)
				{
					try
					{
						long nowLocal = sw.ElapsedTicks;

						tw.Contents = twinService.ConvertFromSystemTextJsonToRealObject(tw.Contents);
						twinService.ValidateTwin(tw);

						overheadCycles += sw.ElapsedTicks - nowLocal; nowLocal = sw.ElapsedTicks;

						(bool ok, var existingTwin) = await twinCache.TryGetValue(willowEnvironment.Id, tw.Id);

						if (ok && existingTwin!.Locations.Any())
						{
							//copy over existing locations so that UI can still show it during cache updates
							tw.Locations = existingTwin.Locations;
						}

						// replace it
						await twinCache.AddOrUpdate(willowEnvironment.Id, tw.Id, tw);

						// Create a mapping table from trendId or connectorId + externalId to DtdId
						if (HasTelemetry(tw))
						{
							var timeSeriesMapping = new TimeSeriesMapping
							{
								Id = tw.Id,  // unique Id
								DtId = tw.Id,
								ModelId = tw.Metadata.ModelId,
								Unit = tw.unit,
								TrendInterval = tw.trendInterval,
								TrendId = tw.trendID,
								ConnectorId = tw.connectorID,
								ExternalId = tw.externalID,
								LastUpdate = DateTimeOffset.Now
							};
							await repositoryTimeSeriesMapping.QueueWrite(timeSeriesMapping, updateCache: false);
						}

						diskCycles += sw.ElapsedTicks - nowLocal; nowLocal = sw.ElapsedTicks;

						otherCycles += sw.ElapsedTicks - nowLocal; nowLocal = sw.ElapsedTicks;

						// If the Twin has an expression we need to create a CalculatedPoint for it
						// This check will also prevent creation of Rules Engine created calculated points as we do not use TrendID and ValueExpression
						if (IsCalcPoint(tw))
						{
							if (!Guid.TryParse(tw.trendID, out var guid))
							{
								logger.LogWarning("Bad guid for calculated point {twindid}: {trendid}", tw.Id, tw.trendID);
							}
							else
							{
								if (!Guid.TryParse(tw.siteID, out var siteGUId))
								{
									logger.LogWarning("Missing site ID on calculated point {id}", tw.Id);
									// TBD, reject it? Command fails, we don't
								}

								var hasUnit = Unit.TryGetUnit(tw.unit, out var unit);
								var cpType = hasUnit && unit != null ? unit.OutputType : UnitOutputType.Undefined;

								var calculatedExpression = new CalculatedPoint()
								{
									Id = tw.Id,
									Name = tw.name,
									Description = tw.description,
									ValueExpression = tw.ValueExpression,
									TrendId = tw.trendID,
									LastUpdated = startTime,
									ModelId = tw.Metadata.ModelId,
									ExternalId = tw.externalID,
									TimeZone = tw.TimeZone?.Name,
									Unit = tw.unit,
									Source = CalculatedPointSource.ADT,
									ActionRequired = ADTActionRequired.None,
									ActionStatus = ADTActionStatus.TwinAvailable,
									TrendInterval = tw.trendInterval ?? 0,
									Type = cpType
								};
								await this.repositoryCalculatedPoint.QueueWrite(calculatedExpression);
							}
						}

						if (!Guid.TryParse(tw.siteID, out Guid _)) logger.LogWarning("Bad siteId on {twindId} {siteId}", tw.Id, tw.siteID);
						if (!Guid.TryParse(tw.uniqueID, out Guid _)) logger.LogWarning("Bad uniqueId on {twindId} {uniqueId}", tw.Id, tw.uniqueID);

						//force an update on empty forward and backward rel entries otherwise expansion will unnecessarily re-fetch empty lists
						//from ADT which slows down expansion 
						(ok, var backEdges) = await backwardEdgeCache.TryGetValue(willowEnvironment.Id, tw.Id);

						if (ok && backEdges!.Items.Count == 0)
						{
							await backwardEdgeCache.AddOrUpdate(willowEnvironment.Id, tw.Id, backEdges);
						}

						(ok, var fwdEdges) = await forwardEdgeCache.TryGetValue(willowEnvironment.Id, tw.Id);

						if (ok && fwdEdges!.Items.Count == 0)
						{
							await forwardEdgeCache.AddOrUpdate(willowEnvironment.Id, tw.Id, fwdEdges);
						}

						calculatedPointCycles += sw.ElapsedTicks - nowLocal; nowLocal = sw.ElapsedTicks;
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "Failed to cache {twin}", tw.Id);
					}

					// TODO return how many cycles instead
					return true;
				}

				async Task<bool> reportOneTwin(bool ok)
				{
					long nowLocal = sw.ElapsedTicks;

					c++;
					if (c % 100 == 0)
					{
						await progressTracker.ReportTwinCount(adtInstance.Uri, c, twinCount);

						long elapsed = sw.ElapsedTicks + 1;
						throttledLogger.LogInformation("Fetching Twins: {c:N0}/{twinCount:N0} Loading {percentage:P0} Overhead {overhead:P1} Disk {disk:P0} Report {report:P1} CalcPoints {calculatedPoints:P1} Other {other:P1} ",
							c, twinCount, (double)c / (twinCount + 1), (double)overheadCycles / elapsed, (double)diskCycles / elapsed, (double)reportCycles / elapsed, (double)calculatedPointCycles / elapsed, (double)otherCycles / elapsed);
					}

					reportCycles += sw.ElapsedTicks - nowLocal; nowLocal = sw.ElapsedTicks;
					return ok;
				}

				using (var timedLogger = logger.TimeOperation("Cache twins: parallel processing"))
				{
					var processed = await source.Reader
						// 8 parallel tasks to expand the system graph for each model Id
						.Split(parallelism, x => x.Id, cancellationToken)
						.Select(c => c.TransformAsync<BasicDigitalTwinPoco, bool>(x => processOneTwin(x), cancellationToken))
						.Merge(cancellationToken)
						.TransformAsync(c => reportOneTwin(c), cancellationToken)
						.ReadAllAsync(cancellationToken)
						.AllAsync(x => x, cancellationToken);
				}

				// Report the final count
				await progressTracker.ReportTwinCount(adtInstance.Uri, c, c);

				// And report how large the mapping table is
				int mappedCount = await repositoryTimeSeriesMapping.Count(x => true);
				await progressTracker.ReportMappedCount(adtInstance.Uri, mappedCount, mappedCount);
				logger.LogInformation("Cache twins: TimeSeriesMapping table has {mappedCount:N0} entries", mappedCount);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Cache twins: Failed to cache {uri}", adtInstance.Uri);
			}
		});

		// Do not delete twins before startTime if cancelled!
		cancellationToken.ThrowIfCancellationRequested();

		await repositoryTimeSeriesMapping.FlushQueue();

		await repositoryTimeSeriesMapping.DeleteBefore(startTime, CancellationToken.None);

		logger.LogInformation("Completed processing each ADT Instance");

		await this.repositoryCalculatedPoint.FlushQueue();

		await repositoryCalculatedPoint.DeleteBefore(startTime, CalculatedPointSource.ADT, CancellationToken.None);
	}

	private static bool HasTelemetry(BasicDigitalTwinPoco tw)
	{
		bool hasTelemtry = !string.IsNullOrEmpty(tw.trendID) ||
							(!string.IsNullOrEmpty(tw.connectorID) && !string.IsNullOrEmpty(tw.externalID)) ||
							// Mapped has no connector ID, they all have externalIDs but capabilities start with "PNT"
							(string.IsNullOrEmpty(tw.connectorID) && tw.externalID is not null && tw.externalID.StartsWith("PNT"));

		return hasTelemtry;
	}

	private static bool IsCalcPoint(BasicDigitalTwinPoco tw)
	{
		return !string.IsNullOrWhiteSpace(tw.ValueExpression);
	}
}
