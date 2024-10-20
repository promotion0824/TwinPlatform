namespace Willow.CognitiveSearch.Index;

using Azure;
using Azure.Identity;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Threading;
using System;
using Willow.Extensions.Logging;

/// <summary>
/// Index Build Service Contract.
/// </summary>
public interface IIndexBuildService
{
     /// <summary>
     /// Create or update the search service index based on the input definition.
     /// </summary>
     /// <param name="searchIndex">Definition of SearchIndex to create.</param>
     /// <param name="tryRebuildOnFailure">Delete and recreate the index if service throws an RequestFailedException if existing index can't be updated.</param>
     /// <param name="cancellationToken">Cancellation Token.</param>
     /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task CreateOrUpdateIndex(SearchIndex searchIndex, bool tryRebuildOnFailure = true, CancellationToken cancellationToken = default);
}

/// <summary>
/// Index Build Service Implementation.
/// </summary>
public class IndexBuildService : SearchBuilderService, IIndexBuildService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IndexBuildService"/> class.
    /// </summary>
    /// <param name="searchSettings">Search Settings options.</param>
    /// <param name="logger">ILogger Instance.</param>
    /// <param name="healthCheckSearch">Heath Check Search Instance.</param>
    /// <param name="defaultAzureCredential">Instance of Default Azure Credentials to get the access token.</param>
    public IndexBuildService(
    IOptions<AISearchSettings> searchSettings,
    ILogger<IndexBuildService> logger,
    HealthCheckSearch healthCheckSearch,
    DefaultAzureCredential defaultAzureCredential)
        : base(searchSettings.Value, logger, healthCheckSearch, defaultAzureCredential)
    {
    }

    /// <summary>
    /// Create or update the search service index based on the input definition.
    /// </summary>
    /// <param name="searchIndex">Definition of SearchIndex to create.</param>
    /// <param name="tryRebuildOnFailure">Delete and recreate the index if service throws an RequestFailedException if existing index can't be updated.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task CreateOrUpdateIndex(SearchIndex searchIndex, bool tryRebuildOnFailure = true, CancellationToken cancellationToken = default)
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

        logger.LogInformation("Search: Create or update index getting client");
        var indexClient = searchIndexClientLazy.Value;

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
                if (tryRebuildOnFailure)
                {
                    healthCheckSearch.Current = HealthCheckSearch.Rebuilding;
                    logger.LogError(ex, "Index update failed, deleting and rebuilding it");
                    await indexClient.DeleteIndexAsync(searchIndex.Name, cancellationToken);
                    await indexClient.CreateOrUpdateIndexAsync(searchIndex, cancellationToken: CancellationToken.None);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Search index update failed");
            healthCheckSearch.Current = HealthCheckResult.Unhealthy(ex.Message);
            throw;
        }
    }
}
