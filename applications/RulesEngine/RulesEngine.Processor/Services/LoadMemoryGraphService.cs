using Abodit.Graph;
using Abodit.Mutable;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Services;
using Willow.Rules.Sources;
using WillowRules.Extensions;

namespace Willow.Rules.Processor;

/// <summary>
/// Not real, just a POC for some of the rules we could run against a Twin graph to validate it
/// </summary>
public interface ILoadMemoryGraphService
{
	/// <summary>
	/// Creates a graph to load an in-memory graph of models or twins
	/// </summary>
	Task<ADTSummary> AddToSummary(ADTSummary summary, WillowEnvironment willowEnvironment, IProgressTrackerForCache trackerForCache, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public class LoadMemoryGraphService : ILoadMemoryGraphService
{
	private readonly IModelService modelService;
	private readonly ITwinService twinService;
	private readonly IMetaGraphService metaGraphService;
	private readonly ILogger<LoadMemoryGraphService> logger;

	/// <summary>
	/// Creates a new <see cref="LoadMemoryGraphService"/>
	/// </summary>
	public LoadMemoryGraphService(
		IModelService modelService,
		ITwinService twinService,
		IMetaGraphService metaGraphService,
		ILogger<LoadMemoryGraphService> logger)
	{
		this.modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
		this.twinService = twinService ?? throw new ArgumentNullException(nameof(twinService));
		this.metaGraphService = metaGraphService ?? throw new ArgumentNullException(nameof(metaGraphService));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <inheritdoc />
	public async Task<ADTSummary> AddToSummary(
		ADTSummary summary,
		WillowEnvironment willowEnvironment,
		IProgressTrackerForCache progressTracker,
		CancellationToken cancellationToken = default)
	{
		Stopwatch sw = Stopwatch.StartNew();

		var modeldata = await this.modelService.ReplaceModelsInCacheAsync(progressTracker, cancellationToken);
		var modelGraph = await this.modelService.GetModelGraphCachedAsync(cancellationToken);

		double t1 = sw.ElapsedMilliseconds;

		logger.LogInformation($"Scanned date: {DateTimeOffset.Now}");
		logger.LogInformation($"Willow Environment: {willowEnvironment.Id}");
		logger.LogInformation($"Model load took {t1 / 1000:0.0}s");

		// Reproject the model graph into meta nodes for the DOT graph
		var modelGraphForDot = new Abodit.Mutable.Graph<Model.MetaGraphNode, Relation>();
		var dotNode = modelGraph.Nodes
			.Select(n => new MetaGraphNode(n))
			.ToDictionary(n => n.ModelId, n => n);

		int c = 0;
		foreach (var edge in modelGraph.Edges)
		{
			c++;
			var start = dotNode[edge.Start.DtdlModel.id];
			var end = dotNode[edge.End.DtdlModel.id];
			modelGraphForDot.AddStatement(start, edge.Predicate, end);

			await progressTracker.ReportGraphBuildCount("Dot", 0, c);
		}

		// logger.LogInformation("Get the twin graph (uncached)");

		// var twinGraph = await this.twinGraphService.GetGraphUncachedAsync(summaryFile, errorFile, willowEnvironment);
		// double t2 = sw.ElapsedMilliseconds;

		MetaGraphResult metaGraphResult;
		using (var timed = logger.TimeOperation(TimeSpan.FromMinutes(2), "Construct metagraph"))
		{
			metaGraphResult = await this.metaGraphService.ConstructMetaGraph(modeldata, progressTracker);
		}

		int allTwinsCount = 0;
		int capabilitiesCount = 0;
		var modelSummary = new Dictionary<string, ModelSummary>();

		var ontology = await modelService.GetModelGraphCachedAsync();

		var modelLookup = ontology.ToDictionary(v => v.Id);

		using (var timed = logger.TimeOperation(TimeSpan.FromMinutes(2), "Recount cached twins"))
		{
			await foreach (var twin in twinService.GetAllCached())
			{
				allTwinsCount++;
				if (!string.IsNullOrEmpty(twin.trendID)
					// Mapped has no trendIds currently
					|| twin.Id.StartsWith("PNT"))
				{
					capabilitiesCount++;
				}

				if (allTwinsCount % 100000 == 0) logger.LogDebug("Recounting twins {allTwinsCount} and capabilities {capabilitiesCount}", allTwinsCount, capabilitiesCount);

				AddPropertyUsageToSummary(twin, ontology, modelLookup, modelSummary);
			}
		}

		summary.ADTInstanceId = willowEnvironment.Id;       // same for now
		summary.CustomerEnvironmentId = willowEnvironment.Id;
		summary.AsOfDate = DateTimeOffset.Now;
		summary.CountRelationships = metaGraphResult.RelationshipCount;
		summary.CountTwins = allTwinsCount;
		summary.CountCapabilities = capabilitiesCount;
		summary.CountTwinsNotInGraph = 0; // can't get this right now .. allTwinsCount - twinGraph.Nodes.Count(),
		summary.CountModels = modeldata.Count;
		summary.CountModelsInUse = metaGraphResult.Graph.Nodes.Count();
		summary.ExtensionData.ModelSummary = modelSummary.Values.ToList();

		return summary;
	}

	private static void AddPropertyUsageToSummary(
		BasicDigitalTwinPoco twin,
		Graph<ModelData, Relation> ontology,
		Dictionary<string, ModelData> modelLookup,
		Dictionary<string, ModelSummary> modelSummaryLookup)
	{
		foreach(var usage in twin.GetPropertyUsage(ontology, modelLookup))
		{
			var model = usage.model;
			var content = usage.property;
			bool used = usage.used;

			if (!modelSummaryLookup.TryGetValue(model.Id, out var modelSummary))
			{
				modelSummary = new ModelSummary()
				{
					ModelId = model.Id
				};

				modelSummaryLookup.Add(model.Id, modelSummary);
			}

			var property = content.name;

			var propertySummary = modelSummary.PropertyReferences.FirstOrDefault(v => v.PropertyName == property);

			if (propertySummary is null)
			{
				propertySummary = new PropertySummary()
				{
					PropertyName = property,
				};

				modelSummary.PropertyReferences.Add(propertySummary);
			}

			if(used)
			{
				propertySummary.TotalUsed++;
			}

			propertySummary.TotalDelcared++;
		}
	}
}
