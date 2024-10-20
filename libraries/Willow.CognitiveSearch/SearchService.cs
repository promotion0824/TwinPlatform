namespace Willow.CognitiveSearch;

using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Services for searching across Rules, Insights, Twins and Models.
/// </summary>
/// <typeparam name="T">Type of class model representing the index structure.</typeparam>
public interface ISearchService<T>
    where T : new()
{
    /// <summary>
    /// Search for an input.
    /// </summary>
    /// <param name="input">Search Expression.</param>
    /// <param name="size">Number of document to return in the response.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async Enumerator.</returns>
    IAsyncEnumerable<SearchResult<T>> Search(string input, int? size = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search for an input with search options controlling the search behavior.
    /// </summary>
    /// <param name="input">Search Expression.</param>
    /// <param name="searchOptions">Instance of <see cref="SearchOptions"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task for SearchResult of <see cref="UnifiedItemDto"/>.</returns>
    public Task<SearchResults<T>> Search(
        string input,
        SearchOptions searchOptions,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// A service for searching rules, insights, twins or models.
/// </summary>
/// <typeparam name="T">Type of class model representing the index structure.</typeparam>
public class SearchService<T> : ISearchService<T>
    where T : new()
{
    private readonly AISearchSettings searchSettings;
    private readonly ILogger<SearchService<T>> logger;
    private readonly HealthCheckSearch healthCheckSearch;
    private readonly DefaultAzureCredential defaultAzureCredential;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchService{T}"/> class.
    /// </summary>
    /// <param name="searchSettings">The search settings.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="healthCheckSearch">The health check search.</param>
    /// <param name="defaultAzureCredential">The default azure credential.</param>
    public SearchService(
        IOptions<AISearchSettings> searchSettings,
        ILogger<SearchService<T>> logger,
        HealthCheckSearch healthCheckSearch,
        DefaultAzureCredential defaultAzureCredential)
    {
        this.searchSettings = searchSettings.Value ?? throw new ArgumentNullException(nameof(searchSettings));
        this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        this.healthCheckSearch = healthCheckSearch ?? throw new ArgumentNullException(nameof(healthCheckSearch));
        this.defaultAzureCredential = defaultAzureCredential ?? throw new ArgumentNullException(nameof(defaultAzureCredential));
    }

    private SearchClient GetSearchClient(string indexName)
    {
        Uri endpoint = new (searchSettings.Uri);
        logger.LogInformation("Search service configured {endpoint}", endpoint);
        return new SearchClient(endpoint, indexName, defaultAzureCredential);
    }

    /// <summary>
    /// Search for an input.
    /// </summary>
    /// <param name="input">Search Expression.</param>
    /// <param name="size">Number of document to return in the response.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async Enumerator for T.</returns>
    public async IAsyncEnumerable<SearchResult<T>> Search(
        string input,
        int? size = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchSettings.Uri))
        {
            logger.LogWarning("Search Api is not configured in options, search will not be available");
            healthCheckSearch.Current = HealthCheckSearch.NotConfigured;
            var empty = new T();

            yield return new SearchResult<T>(empty, 0.0);
            yield break;
        }

        var response = await SearchInternal<T>(input, new SearchOptions() { Size = size }, cancellationToken);
        await foreach (var doc in response.GetResultsAsync())
        {
            yield return new SearchResult<T>(doc.Document, doc.Score ?? 0.0);
        }
    }

    /// <summary>
    /// Search for an input with search options controlling the search behavior.
    /// </summary>
    /// <param name="input">Search Expression.</param>
    /// <param name="searchOptions">Instance of <see cref="SearchOptions"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task for SearchResult.</returns>
    public Task<SearchResults<T>> Search(
        string input,
        SearchOptions searchOptions,
        CancellationToken cancellationToken = default)
    {
        return SearchInternal<T>(input, searchOptions, cancellationToken);
    }

    private async Task<SearchResults<T1>> SearchInternal<T1>(
        string input,
        SearchOptions searchOptions,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(input) && searchOptions?.VectorSearch is null)
            throw new ArgumentNullException("Either input or vector search query must be provided.");
        ArgumentNullException.ThrowIfNull(searchOptions);

        var indexDefaults = searchSettings.GetIndexDefaults<T1>();
        var client = GetSearchClient(indexDefaults.IndexName);
        SetSearchOptionDefaults(searchOptions, indexDefaults.DefaultOptions);
        SearchResults<T1>? response = null;
        try
        {
            response = await client.SearchAsync<T1>(input, searchOptions, cancellationToken);
        }
        catch (RequestFailedException rfe) when (rfe.Status == (int)HttpStatusCode.Forbidden)
        {
            logger.LogWarning(rfe, "Forbidden, could not access search index");
            healthCheckSearch.Current = HealthCheckSearch.Forbidden;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Search async call failed");
            healthCheckSearch.Current = HealthCheckSearch.FailingCalls;
            throw;
        }

        healthCheckSearch.Current = HealthCheckSearch.Healthy;

        return response ?? throw new NullReferenceException("Search returned null response");
    }

    private static SearchOptions SetSearchOptionDefaults(SearchOptions options, SearchOptions defaults)
    {
        options.QueryType ??= defaults.QueryType;
        options.SearchMode ??= defaults.SearchMode;
        options.ScoringProfile ??= defaults.ScoringProfile;
        options.IncludeTotalCount ??= defaults.IncludeTotalCount;
        options.Size ??= defaults.Size;
        return options;
    }
}
