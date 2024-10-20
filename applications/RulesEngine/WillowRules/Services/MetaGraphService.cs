using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Abodit.Graph;
using Abodit.Mutable;
using Microsoft.Extensions.Logging;
using Willow.Rules.Cache;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Sources;
using WillowRules.DTO;
using WillowRules.Extensions;

namespace Willow.Rules.Services;

/// <summary>
/// Service for creating and caching meta graphs
/// </summary>
public interface IMetaGraphService
{
	/// <summary>
	/// Get the metagraph from the cached files
	/// </summary>
	Task<MetaGraphResult> ConstructMetaGraph(
		List<ModelData> modelData,
		IProgressTrackerForCache progressTracker);

	/// <summary>
	/// Get the metagraph for a particular modelId from the metagraph
	/// </summary>
	Graph<MetaGraphNode, MetaGraphRelation> ExtractSystemGraphForOneModel(Graph<MetaGraphNode, MetaGraphRelation> allGraph, string modelId);

	/// <summary>
	/// Gets the full meta graph using a disk cache and background refresh
	/// </summary>
	/// <returns></returns>
	Task<ModelSimpleGraphDto?> GetMetaGraphDtoCached(IProgressTrackerForCache progressTracker, CancellationToken cancellationToken = default);

	/// <summary>
	/// Get the system graph for a single model type
	/// </summary>
	Task<ModelSimpleGraphDto> GetModelSystemGraphCachedAsync(WillowEnvironment willowEnvironment, string modelId, IProgressTrackerForCache progressTracker);

	/// <summary>
	/// Gets the whole model ontology with counts added to each node
	/// </summary>
	/// <remarks>
	/// This needs a completed meta model graph first and then it can augment each model
	/// with a count of actuals and inherited
	/// </remarks>
	Task<ModelSimpleGraphDto?> GetOntologyWithCountsCached(string modelId, IProgressTrackerForCache progressTracker);

	/// <summary>
	/// Gets the whole model ontology with counts added to each node
	/// </summary>
	Task<ModelSimpleGraphDto?> GetOntologyWithCountsUncached(string modelId, IProgressTrackerForCache progressTracker);

	/// <summary>
	/// Gets the full metagraph for the entire system
	/// </summary>
	Task<SerializableGraph<MetaGraphNode, MetaGraphRelation>> GetSerializedMetaGraphCached(IProgressTrackerForCache progressTracker, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for creating and caching meta graphs
/// </summary>
public partial class MetaGraphService : IMetaGraphService
{
	private readonly WillowEnvironment willowEnvironment;
	private readonly IModelService modelService;
	private readonly ITwinService twinService;
	private readonly ITwinGraphService twinGraphService;
	private readonly IDataCacheFactory diskCacheFactory;
	private readonly ILogger<MetaGraphService> logger;
	private readonly ILogger throttledLogger;

	/// <summary>
	/// Creates a new <see cref="MetaGraphService"/> for loading twin data from ADT
	/// </summary>
	public MetaGraphService(
		WillowEnvironment willowEnvironment,
		IModelService modelService,
		ITwinService twinService,
		ITwinGraphService twinGraphService,
		IDataCacheFactory diskCacheFactory,
		ILogger<MetaGraphService> logger)
	{
		this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
		this.modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
		this.twinService = twinService ?? throw new ArgumentNullException(nameof(twinService));
		this.twinGraphService = twinGraphService ?? throw new ArgumentNullException(nameof(twinGraphService));
		this.diskCacheFactory = diskCacheFactory ?? throw new ArgumentNullException(nameof(diskCacheFactory));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.throttledLogger = logger.Throttle(TimeSpan.FromSeconds(15));
	}

	private const string metaGraphCacheKey = "metaGraph19";

	public async Task<SerializableGraph<MetaGraphNode, MetaGraphRelation>>
		GetSerializedMetaGraphCached(IProgressTrackerForCache progressTracker,
		CancellationToken cancellationToken = default)
	{
		logger.LogInformation("GetSerializedMetaGraphCached");
		var metagraphcache = this.diskCacheFactory.MetaModelGraph;

		var result = await metagraphcache.GetOrCreateAsync(willowEnvironment.Id, metaGraphCacheKey, async () =>
		{
			try
			{
				logger.LogInformation("GetSerializedMetaGraphCached (uncached)");
				var modelData = await this.modelService.GetModelsCachedAsync(cancellationToken);
				logger.LogInformation("Loaded modelData, {count} models", modelData.Count);

				var models = await this.ConstructMetaGraph(modelData, progressTracker);
				logger.LogInformation("Loaded metagraph {nodes} nodes, {edges} edges", models.Graph.Nodes.Count(), models.Graph.Edges.Count());

				var serializable = SerializableGraph<MetaGraphNode, MetaGraphRelation>.FromGraph(models.Graph, x => x.ModelId);

				return serializable;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Failed to construct metagraph");
				throw;
			}
		});

		// Don't keep it if there's no data
		if (!result.Nodes.Any())
		{
			logger.LogWarning("No caching empty result for meta graph");
			await metagraphcache.RemoveKey(willowEnvironment.Id, metaGraphCacheKey);
		}

		logger.LogInformation($"{nameof(GetSerializedMetaGraphCached)} got {result.Nodes.Count()}, {result.Edges.Count()}");

		return result;
	}

	/// <summary>
	/// Gets the full meta graph using a disk cache and background refresh
	/// </summary>
	/// <returns></returns>
	public async Task<ModelSimpleGraphDto?> GetMetaGraphDtoCached(IProgressTrackerForCache progressTracker,
		CancellationToken cancellationToken = default)
	{
		logger.LogInformation("GetMetaGraphCached");
		var serializedForm = await this.GetSerializedMetaGraphCached(progressTracker, cancellationToken);
		var metaGraph = serializedForm.GetGraph(x => x.ModelId);

		logger.LogInformation("GetMetaGraphDtoCached serialized form {nodes}, {edges}", serializedForm.Nodes.Count, serializedForm.Edges.Count);
		logger.LogInformation("GetMetaGraphDtoCached reconstituted form {nodes}, {edges}", metaGraph.Nodes.Count(), metaGraph.Edges.Count());

		var modelGraph = await this.modelService.GetModelGraphCachedAsync(cancellationToken);

		ModelSimpleGraphDto? result = new ModelSimpleGraphDto
		{
			Nodes = metaGraph.Nodes.Select(n => new ModelSimpleDto
			{
				Count = n.Count,
				CountInherited = n.CountWithInherited, // this will need the ontology descendants code
				Id = n.Id,
				Label = n.Name,
				LanguageDescriptions = n.LanguageDescriptions,
				LanguageDisplayNames = n.LanguageDisplayNames,
				ModelId = n.ModelId,
				Units = n.Units,
				Decommissioned = n.Decommissioned
			}).ToArray(),

			Relationships = metaGraph.Edges.Select(e => new ModelSimpleRelationshipDto
			{
				StartId = e.Start.Id,
				EndId = e.End.Id,
				Relationship = e.Predicate.Relation,
				Substance = e.Predicate.Substance
			}).ToArray()
		};

		if (!result.Nodes.Any()) return null; // which will not be cached

		return result;
	}

	/// <summary>
	/// Gets meta-graph of twins for a single model type following isFedBy and other relationships to
	/// get the system graph for this node
	/// </summary>
	public Graph<MetaGraphNode, MetaGraphRelation> ExtractSystemGraphForOneModel(Graph<MetaGraphNode, MetaGraphRelation> allGraph, string modelId)
	{
		var initialNode = allGraph.Nodes.FirstOrDefault(x => x.ModelId.Equals(modelId));

		var resultGraph = new Graph<MetaGraphNode, MetaGraphRelation>();
		if (initialNode is null) return resultGraph;

		// These edges get a second chance to come back if both ends are in the graph
		var rejectedGraph = new Graph<MetaGraphNode, MetaGraphRelation>();

		// Compute the system graph for this model like we do for the twin graph
		// So, follow feeds all the way to the end or start
		// Follow includedIn /partOf/... up the graph

		HashSet<string> seen = new();
		Queue<(string modelId, int distance, int direction, string following)> queue = new();

		queue.Enqueue((modelId, 0, 0, ""));

		while (queue.Count > 0)
		{
			var (current, distance, direction, following) = queue.Dequeue();
			if (seen.Contains(current)) continue;
			seen.Add(current);
			var node = allGraph.Nodes.FirstOrDefault(x => x.ModelId.Equals(current));
			if (node is null) continue;  // data error, ignore

			var forward = allGraph.Follow(node);
			var backward = allGraph.Back(node);

			foreach (var edge in forward)
			{
				// First step
				if (string.IsNullOrEmpty(following))
				{
					resultGraph.AddStatement(edge.Start, edge.Predicate, edge.End);
					if (edge.Predicate.Relation == "isCapabilityOf")
					{
						// This is a 'free' hop and we start again from the equipment item
						queue.Enqueue((edge.End.ModelId, distance, 1, ""));
					}
					else
					{
						queue.Enqueue((edge.End.ModelId, distance + 1, 1, edge.Predicate.Relation));
					}
					continue;
				}
				else if (direction == 1 && following.StartsWith("feeds", StringComparison.OrdinalIgnoreCase))
				{
					// Keep going forward on feeds
					if (edge.Predicate.Relation.StartsWith("feeds", StringComparison.OrdinalIgnoreCase))
					{
						resultGraph.AddStatement(edge.Start, edge.Predicate, edge.End);
						queue.Enqueue((edge.End.ModelId, distance + 1, 1, edge.Predicate.Relation));
						continue;
					}
				}

				if (edge.Predicate.Relation != "locatedIn")
				{
					// don't care about co-located objects, that's not a causal connection
					rejectedGraph.AddStatement(edge.Start, edge.Predicate, edge.End);
				}
			}

			foreach (var edge in backward)
			{
				if (string.IsNullOrEmpty(following))
				{
					resultGraph.AddStatement(edge.Start, edge.Predicate, edge.End);
					if (edge.Predicate.Relation == "isCapabilityOf")
					{
						// We don't want to bounce off an isCapabilityOf node to head back up the graph
						// Shouldn't happen anyway
					}
					else
					{
						queue.Enqueue((edge.Start.ModelId, distance + 1, -1, edge.Predicate.Relation));
					}
					continue;
				}
				else if (edge.Predicate.Relation == "isCapabilityOf")
				{
					// ignore it, only want capabilities of primary equipment item
					continue;
				}
				else if (direction == 1 &&
					following.StartsWith("feeds") &&
					edge.Predicate.Relation == "isPartOf" && edge.End.ModelId.Contains("HVACZone"))
				{
					// reflect off an HVAC Zone, came in forward, leave backward
					resultGraph.AddStatement(edge.Start, edge.Predicate, edge.End);
					queue.Enqueue((edge.Start.ModelId, distance + 1, -1, edge.Predicate.Relation));
					continue;
				}
				else if (direction == 1 &&
					following.StartsWith("isPartOf") &&
					edge.Predicate.Relation == "isPartOf" && edge.End.ModelId.Contains("Zone"))
				{
					// reflect off a Zone
					resultGraph.AddStatement(edge.Start, edge.Predicate, edge.End);
					queue.Enqueue((edge.Start.ModelId, distance + 1, -1, edge.Predicate.Relation));
					continue;
				}
				else if (direction == -1 && following == "isPartOf" && edge.Predicate.Relation == "isPartOf")
				{
					// Once we start going backwards looking for parts, keep going
					resultGraph.AddStatement(edge.Start, edge.Predicate, edge.End);
					queue.Enqueue((edge.Start.ModelId, distance + 1, -1, edge.Predicate.Relation));
					continue;
				}
				else if (direction == -1 && following.StartsWith("feeds", StringComparison.OrdinalIgnoreCase))
				{
					// keep going backward on feeds
					if (edge.Predicate.Relation.StartsWith("feeds", StringComparison.OrdinalIgnoreCase))
					{
						resultGraph.AddStatement(edge.Start, edge.Predicate, edge.End);
						queue.Enqueue((edge.Start.ModelId, distance + 1, -1, edge.Predicate.Relation));
						continue;
					}
				}

				if (edge.Predicate.Relation != "locatedIn")
				{
					// don't care about co-located objects, that's not a causal connection
					rejectedGraph.AddStatement(edge.Start, edge.Predicate, edge.End);
				}
			}
		}

		//Any edge that was rejected but which has both nodes in the result graph is also allowed
		foreach (var edge in rejectedGraph.Edges)
		{
			if (resultGraph.Nodes.Contains(edge.Start) && resultGraph.Nodes.Contains(edge.End))
			{
				resultGraph.AddStatement(edge.Start, edge.Predicate, edge.End);
			}
		}

		return resultGraph;
	}

	/// <summary>
	/// Gets meta-graph of twins summarized by model type starting from the graph of twins
	/// </summary>
	public async Task<MetaGraphResult> ConstructMetaGraph(
		List<ModelData> modelData,
		IProgressTrackerForCache progressTracker)
	{
		var twinCache = this.diskCacheFactory.TwinCache;
		var relationshipCache = this.diskCacheFactory.ExtendedRelationships;

		var twins = twinCache.GetAll(willowEnvironment.Id);
		var relationships = relationshipCache.GetAll(willowEnvironment.Id);

		// This is all of the models in the ontology, regardless of usage or not in the twin graph
		// It includes ancestry as relationships of type Relation.RDFSType
		//
		// We flatten the ancestors of a model into the metagraph node ancestors field
		// and sum up all the counts
		var modelGraph = await this.modelService.GetModelGraphCachedAsync();

		var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(15));

		Dictionary<string, MetaGraphNode> newNodes = new();

		MetaGraphNode? GetOrAdd(string modelId, string unit)
		{
			if (!newNodes.TryGetValue(modelId, out var newNode))
			{
				var model = modelGraph.Nodes.SingleOrDefault(x => x.DtdlModel.id == modelId);
				if (model is null)
				{
					// This should never happen but it has been seen in production
					throttledLogger.LogError("Model was not found in modelGraph Nodes for id {modelId}", modelId);
					return null;
				}
				else
				{
					newNode = new MetaGraphNode(model!);
					newNodes.Add(modelId, newNode);
				}
			}

			if (!string.IsNullOrWhiteSpace(unit) && !newNode.Units.Contains(unit))
			{
				newNode.Units.Add(unit);
			}

			return newNode;
		}

		// And finally run counts for all the new classes
		int twinCount = 0;
		(long totalTwins, long totalRelationships) = progressTracker.GetCounts();

		using (var timing = logger.TimeOperation(TimeSpan.FromMinutes(5), "Adding all nodes to metagraph"))
		{
			await foreach (var node in twins)
			{
				twinCount++;
				var modelNode = GetOrAdd(node.Metadata.ModelId, node.unit ?? "");
				if (modelNode is MetaGraphNode)
				{
					modelNode.Count++;
					await progressTracker.ReportGraphBuildCount("Twins", twinCount, (int)totalTwins);
					throttledLogger.LogInformation("Adding node #{count} to metagraph", twinCount);
				}
			}
		}

		using (var timing = logger.TimeOperation(TimeSpan.FromMinutes(5), "Adding edges to metagraph"))
		{

			Graph<MetaGraphNode, MetaGraphRelation> metaGraph = new();
			var relationshipIdentityMapper = new MetaGraphRelationshipIdentityMapper();

			var channel = Channel.CreateBounded<ExtendedRelationship>(100);

			Task producer = Task.Run(async () =>
			{
				var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(15));
				int count = 0;
				try
				{
					await foreach (var rel in relationships)
					{
						var relationshipName = rel.Name;

						if (relationshipName == "hasDocument") continue;  // EXCLUDE THESE FOR A MOMENT

						if (relationshipName == "installedBy") continue;  // EXCLUDE THESE FOR A MOMENT
																		  ////////if (relationshipName == "includedIn") continue;  // EXCLUDE THESE FOR A MOMENT

						////////if (relationshipName == "architectedBy") continue;  // EXCLUDE THESE FOR A MOMENT
						if (relationshipName == "manufacturedBy") continue;  // EXCLUDE THESE FOR A MOMENT
																			 ////////if (relationshipName == "operatedBy") continue;  // EXCLUDE THESE FOR A MOMENT
																			 ////////if (relationshipName == "ownedBy") continue;  // EXCLUDE THESE FOR A MOMENT
						if (relationshipName == "constructedBy") continue;  // EXCLUDE THESE FOR A MOMENT

						await channel.Writer.WriteAsync(rel);

						throttledLogger.LogInformation("Metagraph relationship {count:N0}", count++);
						await progressTracker.ReportGraphBuildCount("Relationships", twinCount, (int)totalTwins);
					}
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Failed to produce all relationships");
				}
				channel.Writer.Complete();
				logger.LogInformation("Completed producing relationships - completed channel");
			});

			// Lookup step (runs many in parallel)
			async Task<(ExtendedRelationship edge, bool ok, BasicDigitalTwinPoco startE, BasicDigitalTwinPoco endE)> LookupStep(ExtendedRelationship edge)
			{
				try
				{
					var startId = edge.SourceId;
					var endId = edge.TargetId;
					var relationshipName = edge.Name;
					var substance = edge.substance;

					var startE = await twinCache.TryGetValue(willowEnvironment.Id, startId).ConfigureAwait(false);
					var endE = await twinCache.TryGetValue(willowEnvironment.Id, endId).ConfigureAwait(false);

					return (edge, startE.ok && endE.ok, startE.result!, endE.result!);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Failed lookup step while reading all edges");
					return (edge, false, default!, default!);
				}
			}

			var multiplex = channel.Reader
				.Split(8, rel => rel.SourceId + rel.TargetId)
				.Select(c => c.TransformAsync(LookupStep))
				.Merge();


			int relationshipCount = 0;
			await foreach (var edge in multiplex.ReadAllAsync())
			{
				try
				{
					relationshipCount++;

					if (edge.ok)
					{
						var startE = edge.startE;
						var endE = edge.endE;
						var start = GetOrAdd(startE.Metadata.ModelId, startE.unit);
						var end = GetOrAdd(endE.Metadata.ModelId, endE.unit);

						if (start is MetaGraphNode && end is MetaGraphNode)
						{
							// Getting the identity mapped relation increments its count
							var rnew = relationshipIdentityMapper.Get(start, end, edge.edge.Name, edge.edge.substance);
							if (rnew.justAdded)
							{
								// Only need to add to the graph once, the count will keep increasing
								metaGraph.AddStatement(start, rnew.relation, end);
							}
						}
					}

					await progressTracker.ReportGraphBuildCount("MetaEdge", relationshipCount, (int)totalRelationships);
					throttledLogger.LogInformation("Added edge {edge:N0} to metagraph", relationshipCount);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Failed to add edge {edge} to metagraph", relationshipCount);
				}
			}

			await producer;

			timing.Complete();

			logger.LogInformation("Finished creating metagraph");

			var metaResult = new MetaGraphResult
			{
				Graph = metaGraph,
				TwinCount = twinCount,
				RelationshipCount = relationshipCount
			};

			return metaResult;
		}
	}

	/// <summary>
	/// Gets the whole model ontology with counts added to each node
	/// </summary>
	/// <remarks>
	/// This needs a completed meta model graph first and then it can augment each model
	/// with a count of actuals and inherited
	/// </remarks>
	public async Task<ModelSimpleGraphDto?> GetOntologyWithCountsCached(string modelId, IProgressTrackerForCache progressTracker)
	{
		//return await this.GetOntologyWithCountsUncached(progressTracker);

		var cache = this.diskCacheFactory.OntologyCache;

		var result = await cache.GetOrCreateAsync(willowEnvironment.Id, "ontology4_" + modelId, async () =>
		{
			return await GetOntologyWithCountsUncached(modelId, progressTracker);
		});

		return result;
	}

	public async Task<ModelSimpleGraphDto?> GetOntologyWithCountsUncached(string modelId, IProgressTrackerForCache progressTracker)
	{
		var metagraph = await this.GetMetaGraphDtoCached(progressTracker);
		if (metagraph is null) return null; /// could not load it
		var metanodeLookup = metagraph.Nodes.ToDictionary(x => x.ModelId, x => x);

		var ontology = await this.modelService.GetModelGraphCachedAsync();

		logger.LogInformation("Creating graph with aggregate counts from {nodes} nodes and {edges} edges", ontology.Nodes.Count(), ontology.Edges.Count());

		var resultGraph = new Graph<ModelSimpleDto, Relation>();

		int idGen = 0;

		ModelSimpleDto GetNode(ModelData n)
		{
			bool foundMetaNode = metanodeLookup!.TryGetValue(n.DtdlModel.id, out var metaNode);
			return new ModelSimpleDto
			{
				Count = foundMetaNode ? metaNode!.Count : 0,
				CountInherited = 0,
				Id = idGen++,
				Label = n.Label,
				LanguageDisplayNames = n.LanguageDisplayNames,
				LanguageDescriptions = n.LanguageDescriptions,
				ModelId = n.DtdlModel.id,
				Units = foundMetaNode ? metaNode!.Units : new List<string>(),
				IsCapability = modelService.IsCapability(n.DtdlModel.id)
			};
		}

		var newNodes = ontology.Nodes.ToDictionary(x => x.Id, n => GetNode(n));
		var resultnodeLookup = newNodes.Values.ToDictionary(x => x.ModelId, x => x);


		// Now copy the graph structure over

		foreach (var edge in ontology.Edges)
		{
			bool hasStart = newNodes.TryGetValue(edge.Start.Id, out var startE);
			bool hasEnd = newNodes.TryGetValue(edge.End.Id, out var endE);

			if (hasStart && hasEnd)  // should always be true
			{
				resultGraph.AddStatement(startE!, edge.Predicate, endE!);
			}
			else
			{
				logger.LogWarning("Missing StartE or EndE for {model}-{model2}", edge.Start.Id, edge.End.Id);
			}
		}

		// Sum up count of descendants by walking the modelgraph graph
		// and applying to the metagraph graph (!)
		foreach (ModelData modeldata in ontology.Nodes.OfType<ModelData>())
		{
			// TODO: Would it be quicker just to call ADT 1,000 times, mostly getting (0,0) back
			// and get the count and count inherited that way?
			// Then the metagraph would only be used in certain circumstances and web app
			// would be less dependent on processor to have run
			// meta graph takes about 5 minutes currently

			var descendants = ontology.Predecessors<ModelData>(modeldata, Relation.RDFSType);
			var ancestors = ontology.Successors<ModelData>(modeldata, Relation.RDFSType);
			//logger.LogInformation("{model} has {c1} descendants and {c2} ancestors", modeldata.Id, descendants.Count(), ancestors.Count());

			bool foundResultNode = resultnodeLookup.TryGetValue(modeldata.Id, out var resultnode);

			if (foundResultNode)
			{
				int sumInherited = 0;
				HashSet<string> unitsInherited = new(resultnode!.Units);
				foreach (var descendant in descendants)
				{
					if (descendant.Id == modeldata.Id) continue;  // ignore self
					if (metanodeLookup.TryGetValue(descendant.Id, out var descendantNode))
					{
						int c = descendantNode!.Count;
						//logger.LogInformation("{descendant} descendant of {parent} has {count}", descendant.Id, modeldata.Id, c);
						sumInherited += c;
						if (c > 0)
						{
							//logger.LogInformation("Adding {count} from {model1} to {model2}", c, descendant.Id, modeldata.Id);
						}

						unitsInherited.UnionWith(descendantNode!.Units);
					}
				}

				resultnode!.CountInherited = sumInherited;
				resultnode!.Units = unitsInherited.ToList();

				if (sumInherited > 0)
				{
					throttledLogger.LogTrace("Setting count inherited to {count} on {on}", resultnode.Count + sumInherited, modeldata.Id);
				}
			}
			else
			{
				logger.LogWarning("Not in dictionary {modelId}", modeldata.Id);
			}
		}

		// Filter the graph to only ancestors or descendants of the node selected
		if (!string.IsNullOrEmpty(modelId))
		{
			var foundStartNode = newNodes.TryGetValue(modelId, out var startNode);

			if (startNode is ModelSimpleDto)
			{
				var ancestors = resultGraph.Predecessors<ModelSimpleDto>(startNode, Relation.RDFSType);
				var descendants = resultGraph.Successors<ModelSimpleDto>(startNode, Relation.RDFSType);

				var merged = ancestors.Union(descendants);
				resultGraph = merged;
			}
		}

		return new ModelSimpleGraphDto
		{
			Nodes = resultGraph.Nodes.ToArray(),
			Relationships = resultGraph.Edges
				.Select(e =>
					new ModelSimpleRelationshipDto
					{
						StartId = e.Start.Id,
						EndId = e.End.Id,
						Relationship = e.Predicate.Name,
						Substance = ""  // extends relationship has no substance
					}).ToArray()
		};
	}

	/// <inheritdoc />
	public async Task<ModelSimpleGraphDto> GetModelSystemGraphCachedAsync(WillowEnvironment willowEnvironment, string modelId,
		IProgressTrackerForCache progressTracker)
	{
		var diskCache = diskCacheFactory.MetaSystemGraphs;

		logger.LogDebug("MetaGraphService:GetModelSystemGraphCachedAsync");

		var serializedSystemGraph = await diskCache.GetOrCreateAsync(willowEnvironment.Id, modelId, async () =>
		{
			logger.LogDebug("MetaGraphService:GetSerializedMetaGraphCached");

			var serializedMetaGraph = await GetSerializedMetaGraphCached(progressTracker);
			var metaGraph = serializedMetaGraph.GetGraph(x => x.ModelId);

			logger.LogDebug("MetaGraphService:Extract system graph");
			// Now extract just the systems piece we want to see
			var modelSystemGraph = this.ExtractSystemGraphForOneModel(metaGraph, modelId);

			return SerializableGraph<MetaGraphNode, MetaGraphRelation>.FromGraph(modelSystemGraph, x => x.ModelId);
		});

		var modelSystemGraph = serializedSystemGraph.GetGraph(x => x.ModelId);

		Dictionary<string, int> seen = new();

		// And update the count on each related entity
		foreach (var node in modelSystemGraph)
		{
			if (seen.TryGetValue(node.Name, out int count))
			{
				count++;
				node.Name = node.Name + $"{count + 1}";
				seen[node.Name] = count;
			}
			else
			{
				seen.Add(node.Name, 1);
			}
		}

		ModelSimpleGraphDto result = new ModelSimpleGraphDto
		{
			Nodes = modelSystemGraph.Nodes.Select(n => new ModelSimpleDto
			{
				Count = n.Count,
				CountInherited = n.CountWithInherited,
				Id = n.Id,
				Label = n.Name,
				ModelId = n.ModelId,
				Units = n.Units
			}).ToArray(),

			Relationships = modelSystemGraph.Edges.Select(e => new ModelSimpleRelationshipDto
			{
				StartId = e.Start.Id,
				EndId = e.End.Id,
				Relationship = e.Predicate.Relation,
				Substance = e.Predicate.Substance
			}).ToArray()
		};

		logger.LogDebug("MetaGraphService:GetModelSystemGraphCachedAsync completed");
		return result;
	}
}
