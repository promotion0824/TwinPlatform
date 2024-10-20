using Azure.Search.Documents;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.CognitiveSearch;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Services;

namespace Willow.Rules.Search;

/// <summary>
/// Service to upload twins to Cognitive Search Index
/// </summary>
public class SearchBuilderTwins
{
	public SearchBuilderTwins(
		PendingDocsQueue pendingDocsQueue,
		ITwinService twinService,
		ITwinGraphService twinGraphService,
		IModelService modelService,
		IMetaGraphService metagraphService,
		ILogger logger)
	{
		this.pendingDocsQueue = pendingDocsQueue ?? throw new ArgumentNullException(nameof(pendingDocsQueue));
		this.twinService = twinService ?? throw new ArgumentNullException(nameof(twinService));
		this.twinGraphService = twinGraphService ?? throw new ArgumentNullException(nameof(twinGraphService));
		this.modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
		this.metagraphService = metagraphService ?? throw new ArgumentNullException(nameof(metagraphService));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		// Errors get their own throttled logger so they don't compete with regular logs
		this.throttledErrorLogger = logger.Throttle(TimeSpan.FromSeconds(1));
	}

	private readonly ILogger logger;
	private readonly ILogger throttledErrorLogger;
	private readonly PendingDocsQueue pendingDocsQueue;
	private readonly ITwinService twinService;
	private readonly ITwinGraphService twinGraphService;
	private readonly IModelService modelService;
	private readonly IMetaGraphService metagraphService;

	private static readonly string[] ancestorRelations = new[] { "isPartOf", "locatedIn", "isContainedIn", "isCapabilityOf" };
	private static readonly string[] feedsRelations = new[] { "isFedBy" };

	const int MAXPARALLELISM = 20;

	public async Task<int> UploadTwins(
		DateTimeOffset now,
		SearchClient client,
		ProgressTracker progressTracker,
		CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested) return 0;

		using var scopedLogger = logger.BeginScope("UploadTwins");
		using var timedLogger = logger.TimeOperation("Search: Uploading twins");

		int c = 0;

		try
		{
			await progressTracker.SetValues("Twins", 0, 100000, isIgnored: false, force: true);

			var ontology = await metagraphService.GetOntologyWithCountsCached("", new ProgressTrackerDummy());

			logger.LogInformation("Search: Upload Twins: Fetching ontology model graph");

			var ontology2 = await modelService.GetModelGraphCachedAsync(cancellationToken);

			if (ontology2 is null)
			{
				logger.LogError("Search: Upload Twins: Why did ontology load fail?");
				return 0;
			}

			logger.LogInformation("Search: Upload Twins: Counting twins");

			int total = 1; // dummy
			try
			{
				// This count operation is failing for some sites, maybe a too slow SQL instance?
				// But we don't really need it, it's a nice-to-have for display purposes.
				total = await twinService.CountCached();
				logger.LogInformation("Search: Upload Twins: Counted {twinCount} twins", total);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Search: Upload Twins: Failed to count twins, fall back to dummy value");
			}

			await progressTracker.SetValues("Twins", 0, total, isIgnored: false, force: true);

			logger.LogInformation("Search: Upload Twins: Fetching twin graph");

			// Hang: this next line never returns when search build locks up?

			var twinGraph = await twinGraphService.GetGraphCachedAsync(cancellationToken);
			int errors = 0;

			logger.LogInformation("Search: Upload Twins: Start processing twins");

			(var channel, var producer) = twinService.GetAllCached()
				.CreateChannel(100, cancellationToken);

			async Task<bool> expandTwins(BasicDigitalTwinPoco twin, Dictionary<string, MiniTwinDto> lookup, CancellationToken cancellationToken)
			{
				using var scopedLogger2 = logger.BeginScope("Adding {twin}", twin.Id);

				try
				{
					if (cancellationToken.IsCancellationRequested) return false;
					if (twin.Id.Equals("#REF!")) return false; // eh? where did that come from?

					string modelId = twin.Metadata.ModelId;
					var modelNode = ontology?.Nodes?.FirstOrDefault(x => x.ModelId.Equals(modelId));

					string modelName = modelNode?.Label ?? modelId;

					List<string> tags = twin.tags?.Keys?.ToList() ?? new();
					if (!string.IsNullOrEmpty(twin.unit))
					{
						tags.Add(twin.unit);
						tags.Add(modelName);  // Boost the model name with another appearance
					}

					var contents = twinService.ConvertFromSystemTextJsonToRealObject(twin.Contents);

					// Find other interesting ids
					if (contents.TryGetValue("modelNumber", out var modelNumber))
					{
						tags.Add($"{modelNumber}");
					}

					// THESE ARE JUST EXAMPLES OF THE MANY ADDITIONAL FIELDS WE MIGHT CHOOSE TO INDEX

					if (contents.TryGetValue("customProperties", out var customProperties))
					{
						if (customProperties is Dictionary<string, object> cd && cd.TryGetValue("Warranty", out var warranty))
						{
							if (warranty is Dictionary<string, object> w && w.TryGetValue("provider", out var provider))
							{
								tags.Add($"{provider}");
							}
						}
					}

					if (contents.TryGetValue("serviceResponsibilityRef", out var serviceResponsibilityRef))
					{
						if (serviceResponsibilityRef is Dictionary<string, object> cd && cd.TryGetValue("name", out var serviceResponsibilityName))
						{
							tags.Add($"{serviceResponsibilityName}");
						}
					}

					int score = 80;
					// Floors rank higher than random sensors with the word floor in them somewhere
					// modelId.Contains("Portfolio;1") ? 5 :
					// modelId.Contains("Level;1") ? 5 :
					// modelId.Contains("Land;1") ? 4 :
					// modelId.Contains("Building;1") ? 4 :
					// modelId.Contains("Room;1") ? 4 :
					// modelId.Contains("Sensor;1") ? 3 :
					// 3;  // default for twins

					// TODO: Could also use Ontology counts to bias the result?

					bool startOk = lookup.TryGetValue(twin.Id, out var start);

					// No start node if node has no relationships, graph does not allow orphans

					var locationAncestorIds =
						start is null ? Array.Empty<string>() :
						 twinGraph.Successors<MiniTwinDto>(start,
						(s, r, e) => ancestorRelations.Any(x => x == r.Name))
						 .TopologicalSortApprox()
						.Distinct()
						.Except(new[] { start })  // exclude self
						.Select(x => x.Id)
						.ToArray();

					var locationAncestorNames =
						locationAncestorIds.Select(async id => await twinService.GetCachedTwin(id))
						.Select(x => x.Result)
						.Where(x => x is not null)
						.Select(x => x!.name)
						.ToArray();

					var feedsAncestorIds =
						start is null ? Array.Empty<string>() :
						twinGraph.Predecessors<MiniTwinDto>(start,
						(s, r, e) => feedsRelations.Any(x => x == r.Name))
						.Distinct()
						.Except(new[] { start })  // exclude self
						.Select(x => x.Id)
						.ToArray();

					var fedByAncestorIds =
						start is null ? Array.Empty<string>() :
						twinGraph.Successors<MiniTwinDto>(start,
						(s, r, e) => feedsRelations.Any(x => x == r.Name))
						.Distinct()
						.Except(new[] { start })  // exclude self
						.Select(x => x.Id)
						.ToArray();

					string name = twin.name ?? "No name";

					// Some sites have underscores in twin names, e.g. HPK AirHandlingUnit_SSI-1
					string[] secondaryNames = new[] { twin.description, name.Replace("_", " ") }
						.Except(new[] { name })
						.Distinct()
						.ToArray();

					// Identity map
					var startModel = ontology2.Nodes.FirstOrDefault(x => x.Id == twin.Metadata.ModelId);

					var ontologyAncestors = startModel is null ? Array.Empty<ModelData>() :
						ontology2.Successors<ModelData>(startModel, (s, r, e) => r.Name == "type")  // inherits
						.Distinct()
						.ToArray();

					var ontologyAncestorModelIds = ontologyAncestors
						.SelectMany(a => new[] { a.Id!, SearchBuilderService.ModelWithoutFluff(a.Id!), a.Label })
						.Concat(new[] { modelId, SearchBuilderService.ModelWithoutFluff(modelId), modelName })           // Change of plan, will include self
						.Where(x => !string.IsNullOrWhiteSpace(x))
						.Select(x => x!)
						.Distinct()
						.ToArray();

					var modelNames = ontologyAncestors
						.Select(a => a.Label!)
						.ToArray();

					var ids = new List<string>
					{
						twin.Id, twin.externalID, twin.connectorID, twin.trendID, twin.uniqueID
					};

					if (twin.Contents.TryGetValue("geometryViewerID", out object? geometryViewerId) && geometryViewerId is not null)
					{
						ids.Add(geometryViewerId.ToString()!);
					}

					var doc = new UnifiedItemDto("twin",
						twin.Id,
						new Names(name),  // Name can be null! PS-PSN-L03-B-ME-03-PAC-001
						new Names(secondaryNames),
						new Ids(ids.ToArray()),
						twin.siteID,
						twin.externalID,
						new LocationAncestorIds(locationAncestorIds),
						new LocationNames(locationAncestorNames),
						new FedByAncestorIds(fedByAncestorIds),
						new FeedsAncestorIds(feedsAncestorIds),
						new TenantAncestorIds(),
						modelId,  // primary model id
						new ModelIds(ontologyAncestorModelIds),
						new ModelNames(modelNames),
						new Tags(tags),
						modelName,
						score);

					doc.Latest = twin.LastUpdatedOn ?? twin.Metadata.LastUpdatedOn;

					doc.IndexedDate = now;

					await pendingDocsQueue.Upload(client, doc, cancellationToken);
				}
				catch (Exception ex)
				{
					throttledErrorLogger.LogWarning(ex, "Could not process twin {id}", twin.Id);
					errors++;
				}
				return true;
			}

			async Task<bool> reportTwins(bool ok)
			{
				Interlocked.Increment(ref c);  // should be only one thread, just in case
				if (total < c) total = c;  // Handle case where total is not known
				await progressTracker.SetValues("Twins", c, total, isIgnored: false);
				return ok;
			}

			// Improve speed mapping Ids to MiniTwinDtos
			Dictionary<string, MiniTwinDto> lookup = twinGraph.Nodes.ToDictionary(x => x.Id, x => x);

			var processor = channel.Reader
				.Split(MAXPARALLELISM, x => x.Id, cancellationToken)  // mostly cpu bound
				.Select(c => c.TransformAsync(x => expandTwins(x, lookup, cancellationToken), cancellationToken))
				.Merge(cancellationToken)
				.TransformAsync(x => reportTwins(x), cancellationToken)
				.ReadAllAsync(cancellationToken)
				.AllAsync(x => true, cancellationToken);

			await Task.WhenAll(producer, processor.AsTask()).ConfigureAwait(false);

			await progressTracker.SetValues("Twins", total, total, isIgnored: false, force: true);

			if (errors > 0)
			{
				c -= errors;

				await progressTracker.SetValues("Twin errors", errors, errors, isIgnored: false, force: true);
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to upload twins to Search index");
			throw;
		}

		return c;
	}
}
