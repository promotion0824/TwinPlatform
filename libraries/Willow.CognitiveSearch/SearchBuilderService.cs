namespace Willow.CognitiveSearch;

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Willow.Extensions.Logging;

/// <summary>
/// Base level services for searching.
/// </summary>
/// <remarks>
/// Inherit from this to create the search builder you need for your application
/// e.g. rules engine creates an IRulesSearchBuilderService.
/// </remarks>
public interface ISearchBuilderService
{
    /// <summary>
    /// Sets the search service health status.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<HealthCheckResult> CheckHealth(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create or update the overall index.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task CreateOrUpdateIndex(CancellationToken cancellationToken = default);
}

/// <summary>
/// The base service for building the search indexes.
/// </summary>
/// <remarks>
/// Doesn't actually do much, you need to implement add methods for each type of data you want to add to the index
/// It does include the code to ensure or rebuild the index.
/// </remarks>
public abstract class SearchBuilderService : ISearchBuilderService
{
    private readonly AISearchSettings searchSettings;

    /// <summary>
    /// An instance of the logger.
    /// </summary>
    protected readonly ILogger logger;

    /// <summary>
    /// The health check search instance.
    /// </summary>
    protected readonly HealthCheckSearch healthCheckSearch;
    private readonly DefaultAzureCredential defaultAzureCredential;

    /// <summary>
    /// The search client. (Lazy loaded)
    /// </summary>
    protected readonly Lazy<SearchClient> searchClientLazy;

    /// <summary>
    /// The search index client. (Lazy loaded)
    /// </summary>
    protected readonly Lazy<SearchIndexClient> searchIndexClientLazy;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchBuilderService"/> class.
    /// </summary>
    /// <param name="searchSettings">The search settings.</param>
    /// <param name="defaultAzureCredential">The default azure credential.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="healthCheckSearch">The health check search.</param>
    public SearchBuilderService(
        AISearchSettings searchSettings,
        ILogger logger,
        HealthCheckSearch healthCheckSearch,
        DefaultAzureCredential defaultAzureCredential)
    {
        this.searchClientLazy = new Lazy<SearchClient>(() => GetSearchClient());
        this.searchIndexClientLazy = new Lazy<SearchIndexClient>(() => GetSearchIndexClient());
        this.searchSettings = searchSettings ?? throw new ArgumentNullException(nameof(searchSettings));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.healthCheckSearch = healthCheckSearch ?? throw new ArgumentNullException(nameof(healthCheckSearch));
        this.defaultAzureCredential = defaultAzureCredential ?? throw new ArgumentNullException(nameof(defaultAzureCredential));
    }

    /// <summary>
    /// Gets the search client for the set Uri using the default azure credential.
    /// </summary>
    /// <returns>The search client.</returns>
    protected SearchClient GetSearchClient()
    {
        var endpoint = new Uri(searchSettings.Uri);
        return new SearchClient(endpoint, searchSettings.UnifiedIndexName, defaultAzureCredential);
    }

    /// <summary>
    /// Gets the search index client for the set Uri using the default azure credential.
    /// </summary>
    /// <returns>The search index client.</returns>
    protected SearchIndexClient GetSearchIndexClient()
    {
        var endpoint = new Uri(searchSettings.Uri);
        return new SearchIndexClient(endpoint, defaultAzureCredential);
    }

    /// <summary>
    /// Check and warn if the search service is not configured.
    /// </summary>
    /// <returns>True if configuration is correctly set up. False otherwise.</returns>
    /// <remarks>
    /// Weak attempt to make sure indexes only get created once per run of processor
    /// private static bool runonce = true;
    /// </remarks>
    protected bool CheckAndWarnIfNoConfiguration()
    {
        if (searchSettings is null || string.IsNullOrWhiteSpace(searchSettings.Uri))
        {
            logger.LogWarning("Search Api is not configured in options, search will not be available");
            healthCheckSearch.Current = HealthCheckSearch.NotConfigured;
            return false;
        }

        if (!Uri.IsWellFormedUriString(searchSettings.Uri, UriKind.Absolute))
        {
            logger.LogWarning("Search Api is misconfigured in options, search will not be available");
            healthCheckSearch.Current = HealthCheckSearch.NotConfigured;
            return false;
        }

        logger.LogInformation("Search Api is correctly configured");
        return true;
    }

    /// <summary>
    /// Checks the health of the search service by opening the index.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<HealthCheckResult> CheckHealth(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Search: Check can connect");

        if (!CheckAndWarnIfNoConfiguration())
        {
            return HealthCheckSearch.NotConfigured;
        }

        try
        {
            var indexClient = this.searchIndexClientLazy.Value;
            await indexClient.GetIndexAsync(searchSettings.UnifiedIndexName, cancellationToken);
            return HealthCheckSearch.Healthy;
        }
        catch (Azure.RequestFailedException rfe) when (rfe.Status == (int)HttpStatusCode.NotFound)
        {
            return HealthCheckSearch.MissingIndex;
        }
        catch (Azure.RequestFailedException rfe) when (rfe.Status == (int)HttpStatusCode.Forbidden)
        {
            logger.LogWarning(rfe, "Forbidden, could not access search index");
            return HealthCheckSearch.Forbidden;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Search: Failed to get index name");
            return HealthCheckSearch.FailingCalls;
        }
    }

    /// <summary>
    /// Creates the Willow unified search index.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CreateOrUpdateIndex(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Search: Create or update index starting");

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (!CheckAndWarnIfNoConfiguration())
        {
            return;
        }

        //if (!runonce) return;
        //runonce = false;
        logger.LogInformation("Search: Create or update index getting client");
        var indexClient = this.searchIndexClientLazy.Value;

        logger.LogInformation("Search: creating synonym map");
        var twinNamesynonymMap = SearchSynonymMap.Willow;

        await indexClient.CreateOrUpdateSynonymMapAsync(twinNamesynonymMap, cancellationToken: cancellationToken);

        var searchIndex = new SearchIndex(searchSettings.UnifiedIndexName)
        {
            Fields =
            {
                new SearchField(nameof(UnifiedItemDto.Key), SearchFieldDataType.String)
                {
                    IsKey = true,
                },
                new SearchField(nameof(UnifiedItemDto.Type), SearchFieldDataType.String)
                {
                    IsFilterable = true,
                    IsSearchable = true,
                    AnalyzerName = LexicalAnalyzerName.Keyword,
                },
                new SearchField(nameof(UnifiedItemDto.Ids), SearchFieldDataType.Collection(SearchFieldDataType.String))
                {
                    IsFilterable = true,
                    IsSearchable = true,
                    AnalyzerName = CustomAnalyzers.LowerCaseKeywordCustomAnalyzerName,
                },
                new SearchField(nameof(UnifiedItemDto.Id), SearchFieldDataType.String) // boosted over the IDs collection
                {
                    IsFilterable = false,
                    IsSearchable = true,
                    AnalyzerName = CustomAnalyzers.LowerCaseKeywordCustomAnalyzerName,
                },
                new SearchField(nameof(UnifiedItemDto.Location), SearchFieldDataType.Collection(SearchFieldDataType.String))
                {
                    IsFilterable = true,
                    IsSearchable = true,
                    AnalyzerName = LexicalAnalyzerName.Keyword,
                },
                new SearchField(nameof(UnifiedItemDto.LocationNames), SearchFieldDataType.Collection(SearchFieldDataType.String))
                {
                    IsFilterable = true,
                    IsSearchable = true,
                    AnalyzerName = LexicalAnalyzerName.EnMicrosoft,
                },
                new SearchField(nameof(UnifiedItemDto.Feeds), SearchFieldDataType.Collection(SearchFieldDataType.String))
                {
                    IsFilterable = true,
                    IsSearchable = true,
                    AnalyzerName = LexicalAnalyzerName.Keyword,
                },
                new SearchField(nameof(UnifiedItemDto.FedBy), SearchFieldDataType.Collection(SearchFieldDataType.String))
                {
                    IsFilterable = true,
                    IsSearchable = true,
                    AnalyzerName = LexicalAnalyzerName.Keyword,
                },
                new SearchField(nameof(UnifiedItemDto.Tenant), SearchFieldDataType.Collection(SearchFieldDataType.String))
                {
                    IsFilterable = true,
                    IsSearchable = true,
                    AnalyzerName = LexicalAnalyzerName.Keyword,
                },
                new SearchField(nameof(UnifiedItemDto.ModelIds), SearchFieldDataType.Collection(SearchFieldDataType.String))
                {
                    IsFilterable = true,
                    IsSearchable = true,
                    AnalyzerName = CustomAnalyzers.LowerCaseKeywordCustomAnalyzerName,
                },
                new SearchField(nameof(UnifiedItemDto.ModelNames), SearchFieldDataType.Collection(SearchFieldDataType.String))
                {
                    IsFilterable = true,
                    IsSearchable = true,
                    AnalyzerName = LexicalAnalyzerName.EnMicrosoft,
                },
                new SearchField(nameof(UnifiedItemDto.PrimaryModelId), SearchFieldDataType.String)
                {
                    IsFilterable = true,
                    IsSearchable = true,
                    AnalyzerName = LexicalAnalyzerName.Keyword,
                },
                new SearchField(nameof(UnifiedItemDto.SiteId), SearchFieldDataType.String)
                {
                    IsFilterable = true,
                    IsSearchable = true,
                    AnalyzerName = LexicalAnalyzerName.Keyword,
                },
                new SearchField(nameof(UnifiedItemDto.ExternalId), SearchFieldDataType.String)
                {
                    IsFilterable = true,
                    IsSearchable = true,
                    AnalyzerName = LexicalAnalyzerName.Keyword,
                },
                new SearchField(nameof(UnifiedItemDto.Names), SearchFieldDataType.Collection(SearchFieldDataType.String))
                {
                    IsFilterable = true,
                    IsSearchable = true,
                    AnalyzerName = LexicalAnalyzerName.EnMicrosoft,
                    SynonymMapNames = { twinNamesynonymMap.Name },
                },
                new SearchField(nameof(UnifiedItemDto.SecondaryNames), SearchFieldDataType.Collection(SearchFieldDataType.String))
                {
                    IsFilterable = true,
                    IsSearchable = true,
                    AnalyzerName = LexicalAnalyzerName.EnMicrosoft,
                    SynonymMapNames = { twinNamesynonymMap.Name },
                },
                new SearchField(nameof(UnifiedItemDto.Tags), SearchFieldDataType.Collection(SearchFieldDataType.String))
                {
                    IsFilterable = true,
                    IsSearchable = true,
                    AnalyzerName = CustomAnalyzers.LowerCaseKeywordCustomAnalyzerName,
                },
                new SearchField(nameof(UnifiedItemDto.Earliest), SearchFieldDataType.DateTimeOffset)
                {
                    IsFilterable = true,
                    IsSearchable = false,

                    // no analyzer as not searcheable
                },
                new SearchField(nameof(UnifiedItemDto.Latest), SearchFieldDataType.DateTimeOffset)
                {
                    IsFilterable = true,
                    IsSearchable = false,

                    // no analyzer as not searcheable
                },
                new SearchField(nameof(UnifiedItemDto.IndexedDate), SearchFieldDataType.DateTimeOffset)
                {
                    IsFilterable = true,
                    IsSearchable = false,

                    // no analyzer as not searcheable
                },
                new SearchField(nameof(UnifiedItemDto.Category), SearchFieldDataType.String)
                {
                    IsFilterable = true,
                    IsSortable = true,
                    IsFacetable = true,
                    SynonymMapNames = { twinNamesynonymMap.Name },
                    AnalyzerName = LexicalAnalyzerName.Keyword,
                },
                new SearchField(nameof(UnifiedItemDto.Importance), SearchFieldDataType.Int32),
            },

            // Suggesters = {
            //  new SearchSuggester ("sg", nameof(SearchTwinDto.Category))
            // },
            ScoringProfiles =
            {
                new ScoringProfile("rules")
                {
                    TextWeights = new TextWeights(
                        new Dictionary<string, double>()
                        {
                            [nameof(UnifiedItemDto.Names)] = 9,
                            [nameof(UnifiedItemDto.SecondaryNames)] = 8,
                            [nameof(UnifiedItemDto.Id)] = 12, // primary ID is boosted over secondary ids
                            [nameof(UnifiedItemDto.Ids)] = 11,
                            [nameof(UnifiedItemDto.ModelNames)] = 8,
                            [nameof(UnifiedItemDto.ModelIds)] = 8,
                            [nameof(UnifiedItemDto.Tags)] = 7,
                            [nameof(UnifiedItemDto.Type)] = 7,
                            [nameof(UnifiedItemDto.Location)] = 5,
                            [nameof(UnifiedItemDto.LocationNames)] = 5,
                            [nameof(UnifiedItemDto.Feeds)] = 5,
                            [nameof(UnifiedItemDto.FedBy)] = 5,
                            [nameof(UnifiedItemDto.Category)] = 5,
                        }),
                    Functions =
                    {
                        new MagnitudeScoringFunction(
                            nameof(UnifiedItemDto.Importance),
                            1.5,  // Boost
                            new MagnitudeScoringParameters(0, 100)),      // Magnitude is 0-100
                    },
                    FunctionAggregation = ScoringFunctionAggregation.Sum,
                },
            },

            Suggesters =
            {
                new SearchSuggester("nameSuggester", new[] { nameof(UnifiedItemDto.Names) }),
            },
        };

        searchIndex.Analyzers.Add(CustomAnalyzers.LowerCaseKeywordCustomAnalyzer);

        try
        {
            try
            {
                healthCheckSearch.Current = HealthCheckSearch.Rebuilding;
                using (var timedLogger = logger.TimeOperation("Search: creating or updating index async"))
                {
                    await indexClient.CreateOrUpdateIndexAsync(searchIndex, cancellationToken: CancellationToken.None);
                }

                healthCheckSearch.Current = HealthCheckSearch.Healthy;
            }
            catch (RequestFailedException ex)
            {
                healthCheckSearch.Current = HealthCheckSearch.Rebuilding;

                logger.LogError(ex, "Index update failed, deleting and rebuilding it");
                await indexClient.DeleteIndexAsync(searchIndex.Name, cancellationToken);
                await indexClient.CreateOrUpdateIndexAsync(searchIndex, cancellationToken: CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Search index update failed");
            healthCheckSearch.Current = HealthCheckResult.Unhealthy(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Gets the model 'name' by removing the dtmi: preamble and trailing semicolon version.
    /// </summary>
    /// <param name="modelId">The model identifier.</param>
    /// <returns>The model name without the namespace.</returns>
    public static string? ModelWithoutFluff(string? modelId)
    {
        if (string.IsNullOrEmpty(modelId))
        {
            return null;
        }

        return modelId
            .Replace("dtmi:com:willowinc:", string.Empty)
            .Replace("dtmi:com:willowinc:airports:", string.Empty)
            .Replace(";1", string.Empty);
    }
}
