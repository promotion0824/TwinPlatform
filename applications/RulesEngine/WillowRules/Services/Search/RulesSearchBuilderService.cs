using Abodit.Graph;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.CognitiveSearch;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Services;

namespace Willow.Rules.Search;

/// <summary>
/// Services for searching across Rules, Insights, Twins and Models
/// </summary>
public interface IRulesSearchBuilderService //: ISearchBuilderService
{
	/// <summary>
	/// Sets the search service health status
	/// </summary>
	Task<HealthCheckResult> CheckHealth(CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes the index
	/// </summary>
	Task DeleteIndex(CancellationToken cancellationToken = default);

	/// <summary>
	/// Create or update the overall index
	/// </summary>
	Task CreateOrUpdateIndex(CancellationToken cancellationToken = default);

	/// <summary>
	/// Add rules to the search index
	/// </summary>
	Task AddRulesToSearchIndex(DateTimeOffset now, PendingDocsQueue pendingDocsQueue, ProgressTracker progressTracker, CancellationToken cancellationToken = default);

	/// <summary>
	/// Add insights to the search index
	/// </summary>
	Task AddInsightsToSearchIndex(DateTimeOffset now, PendingDocsQueue pendingDocsQueue, ProgressTracker progressTracker, CancellationToken cancellationToken = default);

	/// <summary>
	/// Add rule instances to the search index
	/// </summary>
	Task AddRuleInstancesToSearchIndex(DateTimeOffset now, PendingDocsQueue pendingDocsQueue, ProgressTracker progressTracker, CancellationToken cancellationToken = default);

	/// <summary>
	/// Recreate all the data in the index
	/// </summary>
	Task AddEverythingToSearchIndex(DateTimeOffset now, ProgressTracker progressTracker, CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates just models and twins
	/// </summary>
	Task AddModelsAndTwinsToSearchIndex(DateTimeOffset now, ProgressTracker progressTracker2, CancellationToken cancellationToken);
}

/// <summary>
/// A service for building the search indexes
/// </summary>
public class RulesSearchBuilderService : SearchBuilderService, IRulesSearchBuilderService
{
	private readonly ITwinService twinService;
	private readonly ITwinGraphService twinGraphService;
	private readonly IModelService modelService;
	private readonly IMetaGraphService metagraphservice;
	private readonly IRepositoryRules rulesRepository;
	private readonly IRepositoryRuleInstances ruleInstancesRepository;
	private readonly IRepositoryInsight insightsRepository;
	private readonly IRepositoryTimeSeriesBuffer timeSeriesBufferRepository;
	private readonly ILogger throttledLogger;
	private string indexName;

	/// <summary>
	/// Creates a new <see cref="SearchBuilderService" />
	/// </summary>
	public RulesSearchBuilderService(
		ITwinService twinService,
		ITwinGraphService twinGraphService,
		IModelService modelService,
		IMetaGraphService metagraphservice,
		IRepositoryRules rulesRepository,
		IRepositoryRuleInstances ruleInstancesRepository,
		IRepositoryInsight insightsRepository,
		IRepositoryTimeSeriesBuffer timeSeriesBufferRepository,
		ILogger<SearchBuilderService> logger,
		HealthCheckSearch healthCheckSearch,
		DefaultAzureCredential defaultAzureCredential,
		IOptions<AISearchSettings> searchSettings) : base(
			searchSettings.Value,
			logger,
			healthCheckSearch,
			defaultAzureCredential)
	{
		this.indexName = searchSettings.Value.UnifiedIndexName;
		this.twinService = twinService ?? throw new ArgumentNullException(nameof(twinService));
		this.twinGraphService = twinGraphService ?? throw new ArgumentNullException(nameof(twinGraphService));
		this.modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
		this.metagraphservice = metagraphservice ?? throw new ArgumentNullException(nameof(metagraphservice));
		this.rulesRepository = rulesRepository ?? throw new ArgumentNullException(nameof(rulesRepository));
		this.ruleInstancesRepository = ruleInstancesRepository ?? throw new ArgumentNullException(nameof(ruleInstancesRepository));
		this.insightsRepository = insightsRepository ?? throw new ArgumentNullException(nameof(insightsRepository));
		this.timeSeriesBufferRepository = timeSeriesBufferRepository ?? throw new ArgumentNullException(nameof(timeSeriesBufferRepository));
		this.throttledLogger = logger.Throttle(TimeSpan.FromSeconds(20));
	}

	/// <remarks>
	/// Make sure you CreateOrUpdate index before calling this
	/// </remarks>
	public async Task AddEverythingToSearchIndex(DateTimeOffset now, ProgressTracker progressTracker, CancellationToken cancellationToken = default)
	{
		using var timedLogger = logger.TimeOperation("Add everything to search index");

		try
		{
			if (!CheckAndWarnIfNoConfiguration()) return;
			var client = this.searchClientLazy.Value;

			var pendingDocsQueue = new PendingDocsQueue(healthCheckSearch, logger);

			await this.UploadModels(now, pendingDocsQueue, progressTracker, cancellationToken);
			await this.UploadTwins(now, pendingDocsQueue, client, progressTracker, cancellationToken);
			await this.UploadCapabilities(now, pendingDocsQueue, progressTracker, cancellationToken);
			await this.AddInsightsToSearchIndex(now, pendingDocsQueue, progressTracker, cancellationToken);
			await this.AddRuleInstancesToSearchIndex(now, pendingDocsQueue, progressTracker, cancellationToken);
			await this.AddRulesToSearchIndex(now, pendingDocsQueue, progressTracker, cancellationToken);

			await pendingDocsQueue.Flush(client, cancellationToken);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to add all documents to search index");
		}
	}

	public async Task AddModelsAndTwinsToSearchIndex(DateTimeOffset now, ProgressTracker progressTracker, CancellationToken cancellationToken = default)
	{
		using var timedLogger = logger.TimeOperation("Add twins and models to search index");

		try
		{
			if (!CheckAndWarnIfNoConfiguration()) return;
			var client = this.searchClientLazy.Value;

			var pendingDocsQueue = new PendingDocsQueue(healthCheckSearch, logger);

			var tasks = new[]
			{
				this.UploadTwins(now, pendingDocsQueue, client, progressTracker, cancellationToken),
				this.UploadModels(now, pendingDocsQueue, progressTracker, cancellationToken),
			};

			await Task.WhenAll(tasks).ConfigureAwait(false);
			await pendingDocsQueue.Flush(client, cancellationToken);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to add models and twins to search index");
		}
	}

	public async Task AddRulesToSearchIndex(DateTimeOffset now, PendingDocsQueue pendingDocsQueue, ProgressTracker progressTracker, CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested) return;
		if (!CheckAndWarnIfNoConfiguration()) return;

		using (var timedLogger = logger.TimeOperation("Search: Add Rules"))
		{
			var client = this.searchClientLazy.Value;

			int total = await rulesRepository.Count(x => true);
			int c = 0;

			await progressTracker.SetValues("Rules", 0, total, isIgnored: false, force: true);

			await foreach (var rule in rulesRepository.GetAll())
			{
				throttledLogger.LogInformation("Adding rule {ruleId} to search index", rule.Id);
				try
				{
					var doc = new UnifiedItemDto("rule",
						rule.Id,
						new Names(rule.Name),
						new Names(rule.Description, rule.Recommendations),
						new Ids(rule.Id),
						"",
						"",
						new LocationAncestorIds(),
						new LocationNames(),
						new FedByAncestorIds(),
						new FeedsAncestorIds(),
						new TenantAncestorIds(),
						rule.PrimaryModelId,
						new ModelIds(rule.PrimaryModelId, ModelWithoutFluff(rule.PrimaryModelId)!),
						new ModelNames(),
						new Tags((rule.Tags ?? Array.Empty<string>()).Concat(new[] { "rule" })),
						rule.Category ?? "NO CATEGORY",
						50);

					doc.IndexedDate = now;

					await pendingDocsQueue.Upload(client, doc, cancellationToken);
					await progressTracker.SetValues("Rules", c++, total, isIgnored: false);
				}
				catch (Exception ex)
				{
					logger.LogWarning(ex, "Failed to add rule {ruleId} to search index", rule.Id);
				}
			}

			await DeleteDocumentsBefore("rule", now, c, pendingDocsQueue, client, cancellationToken);

			await progressTracker.SetValues("Rules", total, total, isIgnored: false, force: true);
		}
	}

	public async Task AddInsightsToSearchIndex(DateTimeOffset now, PendingDocsQueue pendingDocsQueue, ProgressTracker progressTracker, CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested) return;
		if (!CheckAndWarnIfNoConfiguration()) return;

		using (var timedLogger = logger.TimeOperation("Search: Add Insights"))
		{
			var client = this.searchClientLazy.Value;
			await progressTracker.SetValues("Insights", 0, 10000, isIgnored: false, force: true);

			int total = await insightsRepository.Count(x => true);
			int c = 0;

			await progressTracker.SetValues("Insights", 0, total, isIgnored: false, force: true);

			await foreach (var insight in insightsRepository.GetAll())
			{
				try
				{
					var doc = new UnifiedItemDto("insight",
						insight.Id,  // prefix because insights share ID with ruleInstances
						new Names(insight.RuleName),
						new Names(insight.RuleDescription, insight.RuleRecomendations),
						new Ids(insight.Id, insight.EquipmentId,
							insight.CommandInsightId == Guid.Empty ? null! :
							insight.CommandInsightId.ToString()),
							"",
							"",
						new LocationAncestorIds(insight.TwinLocations?.Select(l => l.Id).ToArray() ?? []),
						new LocationNames(insight.TwinLocations?.Select(l => l.Name).ToArray() ?? []),
						new FedByAncestorIds(insight.FedBy.ToArray()),
						new FeedsAncestorIds(insight.Feeds.ToArray()),
						new TenantAncestorIds(),
						"",
						new ModelIds(insight.PrimaryModelId, ModelWithoutFluff(insight.PrimaryModelId)!),
						new ModelNames(),
						new Tags("insight", insight.RuleCategory),
						insight.RuleCategory,
						40);

					doc.IndexedDate = now;

					await pendingDocsQueue.Upload(client, doc, cancellationToken);
					await progressTracker.SetValues("Insights", c++, total, isIgnored: false);
				}
				catch (Azure.RequestFailedException ex)
				{
					throttledLogger.LogError("{message}", ex.Message + " " + JsonConvert.SerializeObject(insight));
				}
				catch (Exception ex)
				{
					throttledLogger.LogError(ex, "Could not add insight {insight}", insight.Id);
				}
			}

			await DeleteDocumentsBefore("insight", now, c, pendingDocsQueue, client, cancellationToken);

			await progressTracker.SetValues("Insights", total, total, isIgnored: false, force: true);
		}
	}

	public async Task AddRuleInstancesToSearchIndex(DateTimeOffset now, PendingDocsQueue pendingDocsQueue, ProgressTracker progressTracker, CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested) return;
		if (!CheckAndWarnIfNoConfiguration()) return;

		using (var timedLogger = logger.TimeOperation("Search: Add RuleInstances"))
		{
			await progressTracker.SetValues("Skill Instances", 0, 10000, isIgnored: false, force: true);

			var client = this.searchClientLazy.Value;

			// SLOW

			logger.LogInformation("Adding Rule instances to search");
			int total = await this.ruleInstancesRepository.Count(RuleInstance.ExecutableInstanceFilter);
			int c = 0;

			await progressTracker.SetValues("Skill Instances", 0, total, isIgnored: false, force: true);

			await foreach (var instance in ruleInstancesRepository.GetRuleInstancesWithoutExpressions(RuleInstance.ExecutableInstanceFilter))
			{
				if (string.IsNullOrEmpty(instance.Id)) continue;   // Calculated point

				try
				{
					// Calculated points don't have a key
					if (string.IsNullOrEmpty(instance.EquipmentId)) continue;

					var doc = new UnifiedItemDto("ruleinstance",
						instance.Id,
						new Names(instance.RuleName ?? instance.RuleId),
						new Names($"{instance.EquipmentId} {instance.RuleName}"),
						new Ids(instance.Id, instance.EquipmentId),
						"",
						"",
						new LocationAncestorIds(instance.TwinLocations?.Select(l => l.Id).ToArray() ?? []),
						new LocationNames(instance.TwinLocations?.Select(l => l.Name).ToArray() ?? []),
						new FedByAncestorIds(instance.FedBy?.ToArray() ?? []),
						new FeedsAncestorIds(instance.Feeds?.ToArray() ?? []),
						new TenantAncestorIds(),
						"",
						new ModelIds(instance.PrimaryModelId, ModelWithoutFluff(instance.PrimaryModelId)!),
						new ModelNames(),
						new Tags(),
						instance.RuleCategory ?? "missing",
						10);

					doc.IndexedDate = now;

					await pendingDocsQueue.Upload(client, doc, cancellationToken);
					await progressTracker.SetValues("Skill Instances", c++, total, isIgnored: false);
				}
				catch (Azure.RequestFailedException ex)
				{
					throttledLogger.LogError("{message} {json}", ex.Message, JsonConvert.SerializeObject(instance));
				}
				catch (Exception ex)
				{
					throttledLogger.LogError(ex, "Could not add rules instance {instance}", instance.Id);
				}
			}

			await DeleteDocumentsBefore("ruleinstance", now, c, pendingDocsQueue, client, cancellationToken);

			await progressTracker.SetValues("Skill Instances", total, total, isIgnored: false, force: true);
		}
	}

	public async Task UploadModels(DateTimeOffset now, PendingDocsQueue pendingDocsQueue, ProgressTracker progressTracker, CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested) return;
		if (!CheckAndWarnIfNoConfiguration()) return;

		using (var timedLogger = logger.TimeOperation("Search: Uploading models"))
		{
			var client = this.searchClientLazy.Value;

			logger.LogInformation("Search: Fetching ontology");

			var ontology = await modelService.GetModelGraphCachedAsync(cancellationToken);

			if (ontology is null)
			{
				logger.LogError("Why did ontology load fail?");
				return;
			}

			int total = ontology.Nodes.Count();
			int c = 0;

			await progressTracker.SetValues("Models", 0, total, isIgnored: false, force: true);

			foreach (var model in ontology!.Nodes)
			{
				// TODO: var ancestors = ontology.GetAncestors(model.ModelId);

				if (cancellationToken.IsCancellationRequested) break;

				var ancestors = ontology.Successors<ModelData>(model,
					(s, r, e) => r.Name == "type")  // inherits
					.Distinct()
					.ToArray();

				var secondaryIds = ancestors
					.Select(a => a.Id!)
					.Concat(ancestors.Select(a => ModelWithoutFluff(a.Id!)!))
					.ToArray();

				var doc = new UnifiedItemDto("model",
					model.Id,  // This has to be the ID that will be used in the link
					new Names(model.LanguageDisplayNames.Select(x => x.Value).ToArray()),
					new Names(model.LanguageDescriptions.Select(x => x.Value).ToArray()),
					new Ids(model.Id, ModelWithoutFluff(model.Id!)!),
					"",
					"",
					new LocationAncestorIds(),
					new LocationNames(),
					new FedByAncestorIds(),
					new FeedsAncestorIds(),
					new TenantAncestorIds(),
					"",
					new ModelIds(secondaryIds),
					new ModelNames(model.Label),
					new Tags(),
					"Model",
					80
					);

				doc.IndexedDate = now;

				await pendingDocsQueue.Upload(client, doc, cancellationToken);
				await progressTracker.SetValues("Models", c++, total, isIgnored: false);
			}

			await DeleteDocumentsBefore("model", now, c, pendingDocsQueue, client, cancellationToken);

			await progressTracker.SetValues("Models", total, total, isIgnored: false, force: true);
		}
	}

	public async Task UploadTwins(DateTimeOffset now, PendingDocsQueue pendingDocsQueue, SearchClient client, ProgressTracker progressTracker, CancellationToken cancellationToken = default)
	{
		int count = await new SearchBuilderTwins(pendingDocsQueue, twinService, twinGraphService, modelService, metagraphservice, logger)
			.UploadTwins(now, client, progressTracker, cancellationToken);

		await DeleteDocumentsBefore("twin", now, count, pendingDocsQueue, client, cancellationToken);
	}

	public async Task UploadCapabilities(DateTimeOffset now, PendingDocsQueue pendingDocsQueue, ProgressTracker progressTracker, CancellationToken cancellationToken = default)
	{
		if (cancellationToken.IsCancellationRequested) return;
		if (!CheckAndWarnIfNoConfiguration()) return;

		using (var timedLogger = logger.TimeOperation("Search: Adding timeseries"))
		{
			var client = this.searchClientLazy.Value;
			
			logger.LogInformation("Adding timeSeries to search");
			var ontology = await metagraphservice.GetOntologyWithCountsCached("", new ProgressTrackerDummy());

			int c = 0;
			int total = await timeSeriesBufferRepository.GetAll().CountAsync(CancellationToken.None);

			await progressTracker.SetValues("TimeSeries", 0, total, isIgnored: false, force: true);

			await foreach (var timeSeries in timeSeriesBufferRepository.GetAll())
			{
				if (cancellationToken.IsCancellationRequested) break;

				string modelId = timeSeries.ModelId;
				var modelNode = ontology?.Nodes?.FirstOrDefault(x => x.ModelId.Equals(modelId));

				// Concat any language names to form the category == model type
				string modelName = modelNode is null ? modelId :
					string.Join(" ", modelNode.LanguageDescriptions.Values);

				int score = 40;

				var doc = new UnifiedItemDto("timeseries",
					string.IsNullOrEmpty(timeSeries.DtId) ? timeSeries.Id : timeSeries.DtId,  // link is to equipment page
					new Names(),
					new Names(),
					new Ids(timeSeries.DtId!, timeSeries.ExternalId, timeSeries.ConnectorId),
					"",
					"",
					new LocationAncestorIds(),
					new LocationNames(),
					new FedByAncestorIds(),
					new FeedsAncestorIds(),
					new TenantAncestorIds(),
					"",
					new ModelIds(timeSeries.ModelId, ModelWithoutFluff(timeSeries.ModelId)!),
					new ModelNames(),
					new Tags(timeSeries.UnitOfMeasure, "timeseries"),
					modelName ?? "TimeSeries",  // category
					score);

				doc.Earliest = timeSeries.EarliestSeen;
				// changes too frequently doc.Latest = timeSeries.LastSeen;

				doc.IndexedDate = now;

				await pendingDocsQueue.Upload(client, doc, cancellationToken);
				await progressTracker.SetValues("TimeSeries", c++, total, isIgnored: false);
			}

			await DeleteDocumentsBefore("timeseries", now, c, pendingDocsQueue, client, cancellationToken);

			await progressTracker.SetValues("TimeSeries", total, total, isIgnored: false, force: true);
		}
	}

	public Task DeleteIndex(CancellationToken cancellationToken = default)
	{
		logger.LogInformation("Deleting index {name} requested", indexName);

		var client = this.searchIndexClientLazy.Value;

		return client.DeleteIndexAsync(indexName, cancellationToken);
	}

	private async Task DeleteDocumentsBefore(string documentType, DateTimeOffset date, int expectedCount, PendingDocsQueue pendingDocsQueue, SearchClient client, CancellationToken cancellationToken = default)
	{
		int retries = 0;
		string countFilter = $"Type eq '{documentType}' and IndexedDate ge {date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}";

		await pendingDocsQueue.Flush(client, cancellationToken);

		logger.LogInformation("Search Update Waiting for {count} documents of type {type} to be synced before delete", expectedCount, documentType);

		while (true)
		{
			await Task.Delay(TimeSpan.FromSeconds(10));

			long actualCount = await GetCount(client, countFilter, cancellationToken);

			if (actualCount >= expectedCount)
			{
				break;
			}

			//stop trying after ten minutes. In msft it was showing one twin less during this query, not worth NOT doing deletes
			if(retries > 60)
			{
				logger.LogWarning("Search Update gave up on waiting for sync to complete for document type {type}. Last actual count was {count}", documentType, actualCount);
				break;
			}

			logger.LogInformation("Search Update still waiting for sync to complete for document type {type}. Last actual count was {count}", documentType, actualCount);

			retries++;
		}

		string query = $"Type eq '{documentType}' and (IndexedDate lt {date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")} or IndexedDate eq null)";

		int size = 500;
		int total = 0;

		while (true)
		{
			var soptions = new SearchOptions()
			{
				Filter = query,
				SearchMode = SearchMode.All,
				Size = size
			};

			SearchResults<UnifiedItemDto> response = await client.SearchAsync<UnifiedItemDto>("*", soptions);

			int subTotal = 0;

			await foreach (var doc in response.GetResultsAsync())
			{
				await pendingDocsQueue.Delete(client, doc.Document, cancellationToken);
				total++;
				subTotal++;
			}

			if(subTotal == 0)
			{
				break;
			}
		}

		if (total > 0)
		{
			logger.LogInformation("Search Update Deleted {count} documents of type {type}", total, documentType);
		}
	}

	private async Task<long> GetCount(SearchClient client, string filter, CancellationToken cancellationToken = default)
	{
		var options = new SearchOptions()
		{
			Filter = filter,
			SearchMode = SearchMode.All,
			IncludeTotalCount = true,
			Size = 1,
		};

		var result = await client.SearchAsync<UnifiedItemDto>("*", options, cancellationToken);

		return result.Value.TotalCount ?? 0;
	}
}
