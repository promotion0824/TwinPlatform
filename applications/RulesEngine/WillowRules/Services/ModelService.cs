using Abodit.Graph;
using Abodit.Mutable;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Cache;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Sources;

namespace Willow.Rules.Services;

/// <summary>
/// ModelService extensions
/// </summary>
public static class ModelServiceExtensions
{
	/// <summary>
	/// Indicates whether a model id inherits from capability
	/// </summary>
	public static bool IsCapability(this IModelService service, string modelId)
	{
		return service.InheritsFromOrEqualTo(modelId, "dtmi:com:willowinc:Capability;1");
	}

	/// <summary>
	/// Indicates whether a modelid inherits (or equal) from another modelid
	/// </summary>
	public static bool InheritsFromOrEqualTo(this IModelService service, string modelId, string parentModelId)
	{
		if (modelId == parentModelId)
		{
			return true;
		}

		return service.InheritsFrom(modelId, parentModelId);
	}

	private static string[] textBasedTelemetryIds = new string[]
	{
		"dtmi:com:willowinc:Event;1",
		"dtmi:com:willowinc:BilledActiveElectricalEnergy;1"
	};

	/// <summary>
	/// Gets a flat list of models ids allowed for text based telemetry
	/// </summary>
	public static bool IsTextBasedTelemetry(this IModelService service, string modelId)
	{
		foreach (string allowedModelId in textBasedTelemetryIds)
		{
			if (service.InheritsFromOrEqualTo(modelId, allowedModelId))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Gets a flat list of models ids allowed for text based telemetry
	/// </summary>
	public static IEnumerable<string> GetModelIdsForTextBasedTelemetry(this IModelService service)
	{
		foreach (string modelId in textBasedTelemetryIds)
		{
			foreach (var node in service.Predecessors(modelId, includeStartNode: true))
			{
				yield return node.Id;
			}
		}
	}
}

/// <summary>
/// Methods for getting models
/// </summary>
public interface IModelService
{
	/// <summary>
	/// Gets the cached graph, loaded once, non-async
	/// </summary>
	Graph<ModelData, Relation> CachedGraph { get; }

	/// <summary>
	/// Gets a cached model lookup for fast model lookup
	/// </summary>
	Dictionary<string, ModelData> ModelLookup { get; }

	/// <summary>
	/// Gets a single model from the cache of all models
	/// </summary>
	Task<ModelData?> GetSingleModelAsync(string modelId);

	/// <summary>
	/// Gets the model graph from the cache (call GetModelsUncached before this to refresh)
	/// </summary>
	/// <returns></returns>
	Task<Graph<ModelData, Relation>> GetModelGraphCachedAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Fetches models from ADT and updates the cache with the result
	/// </summary>
	Task<List<ModelData>> ReplaceModelsInCacheAsync(IProgressTrackerForCache progressTracker, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the models from the cache, or if not available from ADT
	/// </summary>
	Task<List<ModelData>> GetModelsCachedAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Indicates whether a modelid inherits from another modelid
	/// </summary>
	bool InheritsFrom(string modelId, string parentModelId);

	/// <summary>
	/// Gets Predecessors for a model id
	/// </summary>
	IEnumerable<ModelData> Predecessors(string modelId, bool includeStartNode = false);

	/// <summary>
	/// Gets Successors for a model id
	/// </summary>
	IEnumerable<ModelData> Successors(string modelId, bool includeStartNode = false);
}

/// <summary>
/// Loads and caches models
/// </summary>
public class ModelService : IModelService
{
	private readonly IDataCacheFactory diskCacheFactory;
	private readonly WillowEnvironment willowEnvironment;
	private readonly ADTInstance[] adtInstances;
	private const string CACHEKEY = "allmodels4";

	private readonly ILogger<ModelService> logger;

	// bad: sync over async, but happens only once
	public Graph<ModelData, Relation> CachedGraph => cachedGraphLazy.Value;
	public Dictionary<string, ModelData> ModelLookup => modelLookup.Value;
	private ConcurrentDictionary<string, HashSet<string>> modelLookupResults = new();

	private readonly Lazy<Graph<ModelData, Relation>> cachedGraphLazy;
	private readonly Lazy<Dictionary<string, ModelData>> modelLookup;

	public ModelService(IDataCacheFactory diskCacheFactory,
		IADTService adtService,
		WillowEnvironment willowEnvironment,
		ILogger<ModelService> logger)
	{
		this.diskCacheFactory = diskCacheFactory ?? throw new ArgumentNullException(nameof(diskCacheFactory));
		this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.adtInstances = adtService.AdtInstances;
		this.cachedGraphLazy = new(() =>
		{
			var graph = this.GetModelGraphCachedAsync(default).ConfigureAwait(false).GetAwaiter().GetResult();
			return graph;
		});
		this.modelLookup = new(() =>
		{
			return CachedGraph.Nodes.ToDictionary(v => v.Id);
		});
	}

	// TODO: This merges models across all configured ADT instances. Do we need to keep them separate?

	private async Task<List<ModelData>> FetchModelsAsync(WillowEnvironment willowEnvironment, CancellationToken cancellationToken = default)
	{
		using (var timed = logger.TimeOperation(TimeSpan.FromMinutes(1), "FetchModelsAsync from ADT"))
		{
			var list = new List<ModelData>();
			var startTime = DateTimeOffset.UtcNow;

			foreach (var adtClient in adtInstances.Select(x => x.ADTClient))
			{
				var asyncModels = adtClient.GetModelsAsync(new GetModelsOptions { IncludeModelDefinition = true }, cancellationToken: cancellationToken);

				await foreach (var f in asyncModels)
				{
					cancellationToken.ThrowIfCancellationRequested();
					// Skip all rec ontology models per Rick 6/18/2022
					if (f.Id.StartsWith("dtmi:digitaltwins:rec_3_3:")) continue;
					var serializableVersion = new ModelData(f);
					list.Add(serializableVersion);
					await diskCacheFactory.Models.AddOrUpdate(willowEnvironment.Id, f.Id, serializableVersion);
				}
			}

			await diskCacheFactory.Models.RemoveItems(willowEnvironment.Id, startTime);

			return list;
		}
	}

	public async Task<List<ModelData>> GetModelsCachedAsync(CancellationToken cancellationToken = default)
	{
		using (var timed = logger.TimeOperation(TimeSpan.FromMinutes(1), "GetModelsCachedAsync"))
		{
			var allmodelsCache = this.diskCacheFactory.AllModelsCache;
			var coll = await allmodelsCache.GetOrCreateAsync(willowEnvironment.Id, CACHEKEY,
			   async () => new CollectionWrapper<ModelData>(await FetchModelsAsync(willowEnvironment, cancellationToken)));

			return coll!.Items;
		}
	}

	public async Task<List<ModelData>> ReplaceModelsInCacheAsync(IProgressTrackerForCache progressTracker, CancellationToken cancellationToken = default)
	{
		var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(10)).Token;

		await progressTracker.ReportModelCount(0, 1000);  // get in list first place
		using (var timed = logger.TimeOperation(TimeSpan.FromMinutes(1), "ReplaceModelsInCacheAsync"))
		{
			try
			{
				var modelsTask = FetchModelsAsync(willowEnvironment, cancellationToken);

				var models = await modelsTask.WaitAsync(timeout);

				var allmodelsCache = this.diskCacheFactory.AllModelsCache;

				await allmodelsCache.AddOrUpdate(willowEnvironment.Id, CACHEKEY,
					new CollectionWrapper<ModelData>(models));

				await progressTracker.ReportModelCount(models.Count(), models.Count());

				return models;
			}
			catch(OperationCanceledException ex)
			{
				logger.LogError(ex, "ReplaceModelsInCacheAsync timed out or cancelled");
				throw;
			}
		}
	}

	public async Task<Graph<ModelData, Relation>> GetModelGraphCachedAsync(CancellationToken cancellationToken = default)
	{
		using var timelog = logger.TimeOperation(TimeSpan.FromMinutes(5), "GetModelGraphCachedAsync");

		List<ModelData> models = await this.GetModelsCachedAsync(cancellationToken);

		Graph<string, Relation> idGraph = new();
		ConcurrentDictionary<string, ModelData> dict = new();    // TODO: DUPLICATES WHEN MULTIPLE ADT INSTANCES!

		foreach (ModelData model in models)
		{
			if (model.Id.StartsWith("dtmi:digitaltwins:rec_3_3:")) continue;  // ignore rec
																			  // Just overwrite any duplicate models for now
			dict.AddOrUpdate(model.Id, x => model, (x, y) => model);

			if (!(model.DtdlModel.extends is null))
			{
				foreach (string parentId in model.DtdlModel.extends)
				{
					if (parentId.StartsWith("dtmi:digitaltwins:rec_3_3:")) continue;  // exlude rec ontology
					idGraph.AddStatement(model.Id, Relation.RDFSType, parentId);
				}
			}

			if (model.DtdlModel.contents is not null)
			{
				foreach (Content content in model.DtdlModel.contents)
				{
					if (!content.type.Contains("Relationship")) continue;

					string name = content.name;
					string targetId = content.target;

					if (targetId is null) continue; // throw new Exception("Target model ID for model " + model.Id + " is null");
					if (name is null) throw new Exception("Name of relationship between " + model.Id + " and " + targetId + " is null");

					try
					{
						lock (Relation.RDFSType)  // GetByName is not thread safe
						{
							var relation = Relation.GetByName(name);
							idGraph.AddStatement(model.Id, relation, targetId);
						}
					}
					catch (InvalidOperationException ex)
					{
						// Operations that change non-concurrent collections must have exclusive access.
						logger.LogWarning(ex, "Problem adding relation, not thread safe code?");
					}
					catch (ArgumentException ex)
					{
						// An item with the same key has already been added. Key: REGULATEDBY
						logger.LogWarning(ex, "Problem adding relation, not thread safe code?");
					}
				}
			}

			// Note that a model is not added to the graph if it has no relationships
		}

		Graph<ModelData, Relation> modelGraph = new();

		foreach (var edge in idGraph.Edges)
		{
			if (!dict.TryGetValue(edge.Start, out var modelStart))
			{
				logger.LogError("Model was not found with id: {startId}", edge.Start);
				continue;
			}
			if (!dict.TryGetValue(edge.End, out var modelEnd))
			{
				logger.LogError("Model was not found with id: {endId}", edge.End);
				continue;
			}
			modelGraph.AddStatement(modelStart, edge.Predicate, modelEnd);
		}

		return modelGraph;
	}

	private async Task<ModelData?> Fetch(string modelId)
	{
		if (modelId == "all") return null!;
		foreach (var adtinstance in adtInstances)
		{
			var adtResponse = await adtinstance.ADTClient.GetModelAsync(modelId);
			var modelData = new ModelData(adtResponse.Value);
			return modelData;
		}
		return null!;
	}

	public async Task<ModelData?> GetSingleModelAsync(string modelId)
	{
		// TODO: Model needs to be per ADT Instance not one for all of a customer environment
		// OR do we agree that all models in a customer environment must be additive (open world)
		var model = await diskCacheFactory.Models.GetOrCreateAsync(willowEnvironment.Id, modelId,
			() => Fetch(modelId));
		return model;
	}

	public IEnumerable<ModelData> Predecessors(string modelId, bool includeStartNode = false)
	{
		if (ModelLookup.TryGetValue(modelId, out var modelForNode))
		{
			var predecessors = (IEnumerable<ModelData>)CachedGraph.Predecessors<ModelData>(modelForNode, Relation.RDFSType);

			if (includeStartNode)
			{
				predecessors = predecessors.Prepend(modelForNode);
			}

			return predecessors;
		}

		return Array.Empty<ModelData>();
	}

	public IEnumerable<ModelData> Successors(string modelId, bool includeStartNode = false)
	{
		if (ModelLookup.TryGetValue(modelId, out var modelForNode))
		{
			var successors = (IEnumerable<ModelData>)CachedGraph.Successors<ModelData>(modelForNode, Relation.RDFSType);

			if (includeStartNode)
			{
				successors = successors.Prepend(modelForNode);
			}

			return successors;
		}

		return Array.Empty<ModelData>();
	}

	public bool InheritsFrom(string modelId, string parentModelId)
	{
		if (!modelLookupResults.TryGetValue(modelId, out var result))
		{
			if (ModelLookup.TryGetValue(modelId, out var modelForNode))
			{
				var ancestors = CachedGraph.Successors<ModelData>(modelForNode, Relation.RDFSType);

				result = ancestors.Where(v => v != modelForNode).Select(v => v.Id).ToHashSet();
			}
			else
			{
				result = new HashSet<string>(0);
			}

			modelLookupResults[modelId] = result;
		}

		return result.Contains(parentModelId);
	}
}
