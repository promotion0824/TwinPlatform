using Abodit.Mutable;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.Cache;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Sources;
using WillowRules.DTO;

namespace Willow.Rules.Services;

/// <summary>
/// A service for fetching system graphs containing one or more twins and the system they are part of
/// </summary>
public interface ITwinSystemService
{
	/// <summary>
	/// Get the graph around one or more twin ids
	/// </summary>
	Task<Graph<BasicDigitalTwinPoco, WillowRelation>> GetTwinSystemGraph(string[] twinIds);
}

/// <summary>
/// A service for fetching system graphs containing one or more twins and the system they are part of
/// </summary>
public class TwinSystemService : ITwinSystemService
{
	private readonly ITwinService twinService;
	private readonly WillowEnvironment willowEnvironment;
	private readonly ILogger<TwinSystemService> logger;
	private readonly ILogger throttledLogger;
	private readonly IDataCache<SerializableGraph<BasicDigitalTwinPoco, WillowRelation>> twinSystemGraphCache;

	/// <summary>
	/// Creates a new <see cref="TwinSystemService"/> for loading twin data from ADT
	/// </summary>
	public TwinSystemService(
		IDataCacheFactory diskCacheFactory,
		ITwinService twinService,
		WillowEnvironment willowEnvironment,
		ILogger<TwinSystemService> logger)
	{
		this.twinSystemGraphCache = diskCacheFactory?.TwinSystemGraphCache ?? throw new ArgumentNullException(nameof(diskCacheFactory));
		this.twinService = twinService ?? throw new ArgumentNullException(nameof(twinService));
		this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.throttledLogger = logger.Throttle(TimeSpan.FromSeconds(15));
	}

	/// <summary>
	/// Creates a subgraph including all of the nodes requested (cached on disk)
	/// </summary>
	public async Task<Graph<BasicDigitalTwinPoco, WillowRelation>> GetTwinSystemGraph(string[] twinIds)
	{
		string key = string.Join("_", twinIds);
		var graph = await twinSystemGraphCache.GetOrCreateAsync(willowEnvironment.Id, key, async () =>
		{
			using var timed = throttledLogger.TimeOperation(TimeSpan.FromSeconds(15), "Getting twin system graph non-cached {key}", key);

			var g = await GetTwinSystemGraphNonCached(twinIds);
			var ser = SerializableGraph<BasicDigitalTwinPoco, WillowRelation>.FromGraph(g, n => n.Id);
			return ser;
		});
		return graph.GetGraph(x => x.Id);
	}

	/// <summary>
	/// Creates a subgraph including all of the nodes requested
	/// </summary>
	public async Task<Graph<BasicDigitalTwinPoco, WillowRelation>> GetTwinSystemGraphNonCached(string[] twinIds)
	{
		using var disp = logger.BeginScope(new Dictionary<string, object> { ["TwinIds"] = string.Join(",", twinIds) });

		Graph<BasicDigitalTwinPoco, WillowRelation> result = new();
		//this floods the logs
		//logger.LogInformation("Get Non Cached {id}", twinIds.First());

		// Identity map all the basic digital twin poco
		ConcurrentDictionary<string, BasicDigitalTwinPoco> memoized = new();
		BasicDigitalTwinPoco identityMap(BasicDigitalTwinPoco twin) => memoized.GetOrAdd(twin.Id, twin);

		// Replace "all" at the end of the twinids list with the top-level nodes
		if (twinIds.Length == 1 && twinIds.Contains("all"))
		{
			var topLevelTwins = await this.twinService.GetTopLevelEntities();
			twinIds =
				twinIds.Where(x => x != "all").Concat(topLevelTwins.Select(x => x.Id))
				 .ToArray();
			logger.LogDebug("Using all = {all}", string.Join(",", twinIds));
		}
		else
		{
			// Ignore all on twinids whenever any other twinid is present
			twinIds = twinIds.Where(x => x != "all").ToArray();
		}

		HashSet<string> seen = new();
		Queue<(string twinId, int distance, string following)> queue = new();
		foreach (var twinId in twinIds)
		{
			queue.Enqueue((twinId, 0, ""));
		}

		int MAX_LIMIT = 10000;
		int limit = MAX_LIMIT;
		var ancestorRelTypes = new string[] { "locatedIn", "isPartOf", "includedIn", "isHeldBy", "ownedBy", "hostedBy", "hasLocation", "servedBy" }.ToHashSet();

		while (queue.Count > 0)
		{
			if (limit-- < 0) { logger.LogWarning("Graph traversal hit traversals limit"); break; }

			if (limit % 1000 == 0) logger.LogWarning("Large graph traversal {traversals}", MAX_LIMIT - limit);

			(string id, int distance, string following) = queue.Dequeue();
			if (seen.Contains(id)) continue;
			seen.Add(id);

			bool isInitialNode = distance == 0;

			var twin = await twinService.GetCachedTwin(id);

			if (twin == null)
			{
				continue;
			}

			twin = identityMap(twin);

			switch (following)
			{
				case "feeds":
					{
						var relatedBackTwins = await twinService.GetCachedForwardRelatedTwins(twin.Id);
						foreach (var edge in relatedBackTwins.Where(x => x.RelationshipType == "isFedBy"))
						{
							var pred = WillowRelation.Get("feeds", edge.Substance);
							result.AddStatement(identityMap(edge.Destination), pred, twin);

							queue.Enqueue((edge.Destination.Id, distance + 1, "feeds"));
						}
						break;
					}
				case "isFedBy":
					{
						var relatedForwardTwins = await twinService.GetCachedBackwardRelatedTwins(twin.Id);
						foreach (var edge in relatedForwardTwins.Where(x => x.RelationshipType == "isFedBy"))
						{
							if (edge.RelationshipType != "isFedBy") continue;

							var pred = WillowRelation.Get("feeds", edge.Substance);
							result.AddStatement(twin, pred, identityMap(edge.Destination));

							queue.Enqueue((edge.Destination.Id, distance + 1, "isFedBy"));
						}
						break;
					}
				case "physicalGreater":
					{
						var relatedForwardTwins = await twinService.GetCachedForwardRelatedTwins(twin.Id);
						foreach (var edge in relatedForwardTwins)
						{
							if (!ancestorRelTypes.Contains(edge.RelationshipType)) continue;

							var pred = WillowRelation.Get(edge.RelationshipType, edge.Substance);
							result.AddStatement(twin, pred, identityMap(edge.Destination));

							queue.Enqueue((edge.Destination.Id, distance + 1, "physicalGreater"));
						}
						break;
					}
				case "isCapabilityOf":
				default:
					{
						//if (isInitialNode)
						{
							var relatedForwardTwins = (await twinService.GetCachedForwardRelatedTwins(twin.Id)).ToList();
							foreach (var edge in relatedForwardTwins)
							{
								//includedIn was added for walmart retail
								if (ancestorRelTypes.Contains(edge.RelationshipType))
								{
									// If the parent is an HVACZone we are interested in what feeds that zone
									// so push in on the queue with empty string to examine all of its links
									if (edge.Destination.Metadata.ModelId == "dtmi:com:willowinc:HVACZone;1")
									{
										queue.Enqueue((edge.Destination.Id, distance + 1, ""));
									}
									else if (edge.Destination.Metadata.ModelId == "dtmi:com:willowinc:OccupancyZone;1")
									{
										// These also act like a zone
										queue.Enqueue((edge.Destination.Id, distance + 1, ""));
									}
									else if (edge.Destination.Metadata.ModelId == "dtmi:com:willowinc:InferredOccupancySensor;1")
									{
										// These also act like a zone
										queue.Enqueue((edge.Destination.Id, distance + 1, ""));
									}
									else
									{
										queue.Enqueue((edge.Destination.Id, distance + 1, "physicalGreater"));
									}

									var pred = WillowRelation.Get(edge.RelationshipType, edge.Substance);
									result.AddStatement(twin, pred, identityMap(edge.Destination));
								}
								else if (edge.RelationshipType == "isFedBy")
								{
									queue.Enqueue((edge.Destination.Id, distance + 1, "feeds"));
									// flip the edge for the output
									var pred = WillowRelation.Get("feeds", edge.Substance);
									result.AddStatement(identityMap(edge.Destination), pred, twin);
								}
								else if (edge.RelationshipType == "isCapabilityOf")
								{
									if (edge.Destination.Metadata.ModelId == "dtmi:com:willowinc:Building;1")
									{
										// HACK HACK
										// If something isCapabilityOf a building then hack it to isMeasureOf
										// because it's also isCapabilityOf a Switchboard or similar
										// But don't queue it ... queue.Enqueue((edge.Destination.Id, distance + 1, "isMeasureOf"));
										var pred = WillowRelation.Get("isMeasureOf", edge.Substance);
										result.AddStatement(twin, pred, identityMap(edge.Destination));
									}
									else
									{
										queue.Enqueue((edge.Destination.Id, distance + 1, "isCapabilityOf"));
										var pred = WillowRelation.Get(edge.RelationshipType, edge.Substance);
										result.AddStatement(twin, pred, identityMap(edge.Destination));
									}
								}
								else if (edge.RelationshipType == "isHeldBy" || edge.RelationshipType == "ownedBy")
								{
									// Show these in the graph but don't keep following them
									// queue.Enqueue((edge.Destination.Id, distance + 1, "legallyGreater"));
									var pred = WillowRelation.Get(edge.RelationshipType, edge.Substance);
									result.AddStatement(twin, pred, identityMap(edge.Destination));
								}
								else if (following == "isCapabilityOf")
								{
									// If we came in on isCapabilityOf, don't leave on hostedBy
								}
							}

							var relatedBackTwins = (await twinService.GetCachedBackwardRelatedTwins(twin.Id)).ToList();
							foreach (var edge in relatedBackTwins)
							{
								if (edge.RelationshipType == "isFedBy")
								{
									// flip the edge for the output
									var pred = WillowRelation.Get("feeds", edge.Substance);
									result.AddStatement(twin, pred, identityMap(edge.Destination));

									// Bounce back down through an HVACZone
									if (edge.Destination.Metadata.ModelId == "dtmi:com:willowinc:HVACZone;1")
									{
										queue.Enqueue((edge.Destination.Id, distance + 1, ""));
									}
									else if (edge.Destination.Metadata.ModelId == "dtmi:com:willowinc:OccupancyZone;1")
									{
										// These also act like a zone
										queue.Enqueue((edge.Destination.Id, distance + 1, ""));
									}
									else if (edge.Destination.Metadata.ModelId == "dtmi:com:willowinc:InferredOccupancySensor;1")
									{
										// These also act like a zone
										queue.Enqueue((edge.Destination.Id, distance + 1, ""));
									}
									else
									{
										queue.Enqueue((edge.Destination.Id, distance + 1, "isFedBy"));
									}

								}
								else if (edge.RelationshipType == "isCapabilityOf")
								{
									if (twin.Metadata.ModelId == "dtmi:com:willowinc:Building;1")
									{
										// Skip this isCapabilityOf, we've already added isMeasureOf
										var pred = WillowRelation.Get("isMeasureOf", edge.Substance);
										result.AddStatement(identityMap(edge.Destination), pred, twin);
									}
									else
									{
										// If we go down to a capability, also look up from that capability
										// because of the double isCapabilityOf issue around HasInferredOccupancy and People Count
										queue.Enqueue((edge.Destination.Id, distance + 1, "isCapabilityOf"));
										// Mostly this will get straight back to the same node, but sometimes ...

										var pred = WillowRelation.Get(edge.RelationshipType, edge.Substance);
										result.AddStatement(identityMap(edge.Destination), pred, twin);
									}
								}
								else if (edge.RelationshipType == "isPartOf")
								{
									if (twin.Metadata.ModelId == "dtmi:com:willowinc:OccupancyZone;1")
									{
										// check it's an inferred occupancy
										queue.Enqueue((edge.Destination.Id, distance + 1, ""));
									}
									var pred = WillowRelation.Get(edge.RelationshipType, edge.Substance);
									result.AddStatement(identityMap(edge.Destination), pred, twin);
								}
								else
								{
									// Something else, add it to graph but don't follow it
									var pred = WillowRelation.Get(edge.RelationshipType, edge.Substance);
									result.AddStatement(identityMap(edge.Destination), pred, twin);
								}
							}
						}
						break;
					}
			}
		}

		return result;
	}
}
