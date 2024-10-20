namespace Willow.CognitiveSearch;

using Azure;
using Azure.Identity;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Indexer Build Service interface.
/// </summary>
public interface IIndexerBuildService
{
    /// <summary>
    /// Create or update storage container data source.
    /// </summary>
    /// <param name="request">Instance of <see cref="StorageContainerDataSource"/> record. </param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Task to await.</returns>
    Task CreateOrUpdateStorageContainerDataSourceAsync(StorageContainerDataSource request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create or update Skill set.
    /// </summary>
    /// <param name="request">Instance of <see cref="Skillset"/>.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Task to await.</returns>
    Task CreateOrUpdateSkillsetAsync(Skillset request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create or update Indexer.
    /// </summary>
    /// <param name="request">Instance of <see cref="IndexerRequest"/>.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Task to await.</returns>
    Task CreateOrUpdateIndexerAsync(IndexerRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disables the Search Service Indexer if exist.
    /// </summary>
    /// <param name="indexerName">Name of the Indexer.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>True = if the indexer exist and disabled; false = if indexer does not exist.</returns>
    public Task<bool> DisableIndexerIfExistAsync(string indexerName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset the Search Indexer. Resetting indexer will delete all change tracking.
    /// </summary>
    /// <param name="indexerName">Name of the Indexer.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public Task<bool> ResetIndexerAsync(string indexerName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Run the Search Indexer.
    /// </summary>
    /// <param name="indexerName">Name of the Indexer.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public Task<bool> RunIndexerAsync(string indexerName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Indexer Build Service.
/// </summary>
public class IndexerBuildService : IIndexerBuildService
{
    private readonly ILogger<IndexerBuildService> logger;
    private readonly HealthCheckSearch healthCheckSearch;
    private readonly Lazy<SearchIndexerClient> searchIndexerClientLazy;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexerBuildService"/> class.
    /// </summary>
    /// <param name="searchSettings">ACS Search Settings.</param>
    /// <param name="logger">Instance of ILogger.</param>
    /// <param name="healthCheckSearch">Health Check Search Instance.</param>
    /// <param name="defaultAzureCredential">Default Azure Credentials.</param>
    public IndexerBuildService(
    IOptions<AISearchSettings> searchSettings,
    ILogger<IndexerBuildService> logger,
    HealthCheckSearch healthCheckSearch,
    DefaultAzureCredential defaultAzureCredential)
    {
        ArgumentNullException.ThrowIfNull(searchSettings.Value, nameof(searchSettings.Value));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.healthCheckSearch = healthCheckSearch ?? throw new ArgumentNullException(nameof(healthCheckSearch));
        searchIndexerClientLazy = new Lazy<SearchIndexerClient>(() => new SearchIndexerClient(new Uri(searchSettings.Value.Uri), defaultAzureCredential));
    }

    /// <summary>
    /// Create or update storage container data source.
    /// </summary>
    /// <param name="request">Instance of <see cref="StorageContainerDataSource"/> record. </param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Task to await.</returns>
    /// <remarks>
    /// Datasource will be attempted to create with deletion detection policy: <see cref="SoftDeleteColumnDeletionDetectionPolicy"/>.
    /// This policy requires the blob storage account to set custom metadata say "IsDeleted=true" for deletion.
    /// </remarks>
    public async Task CreateOrUpdateStorageContainerDataSourceAsync(StorageContainerDataSource request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Attempting to create Azure AI Search DataSource:{DataSourceName}", request.Name);
        var storageDataContainer = new SearchIndexerDataContainer(request.ContainerName)
        {
            Query = request.ContainerQuery,
        };
        var dataSourceConnection = new SearchIndexerDataSourceConnection(request.Name, SearchIndexerDataSourceType.AzureBlob, request.StorageAccountConnection, storageDataContainer);

        // Skip Delete Detection Policy if SoftDeleteColumnName or SoftDeleteMarkerValue is empty
        if (!string.IsNullOrWhiteSpace(request.SoftDeleteColumnName) && !string.IsNullOrWhiteSpace(request.SoftDeleteMarkerValue))
        {
            dataSourceConnection.DataDeletionDetectionPolicy = new SoftDeleteColumnDeletionDetectionPolicy()
            {
                SoftDeleteColumnName = request.SoftDeleteColumnName,
                SoftDeleteMarkerValue = request.SoftDeleteMarkerValue
            };
        }

        var searchIndexerClient = searchIndexerClientLazy.Value;

        var response = await searchIndexerClient.CreateOrUpdateDataSourceConnectionAsync(dataSourceConnection, onlyIfUnchanged: false, cancellationToken);

        logger.LogInformation("Azure AI Search DataSource:{DataSourceName} created or updated successfully.", response.Value.Name);
    }

    /// <summary>
    /// Create or update Skill set.
    /// </summary>
    /// <param name="request">Instance of <see cref="Skillset"/>.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Task to await.</returns>
    public async Task CreateOrUpdateSkillsetAsync(Skillset request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Attempting to create Azure AI Search Skillset:{SkillsetName}", request.Name);

        var searchIndexerClient = searchIndexerClientLazy.Value;

        var skillset = new SearchIndexerSkillset(request.Name, request.Skills)
        {
            Description = request.Description,
            IndexProjection =  request.Projection
        };

        var response = await searchIndexerClient.CreateOrUpdateSkillsetAsync(skillset, onlyIfUnchanged: false, cancellationToken);

        logger.LogInformation("Azure AI Search Skillset:{SkillsetName} created successfully.", response.Value.Name);
    }

    /// <summary>
    /// Create or update Indexer.
    /// </summary>
    /// <param name="request">Instance of <see cref="IndexerRequest"/>.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>Task to await.</returns>
    public async Task CreateOrUpdateIndexerAsync(IndexerRequest request, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Attempting to create Azure AI Search Blob Indexer:{Indexer}", request.Name);

        var searchIndexerClient = searchIndexerClientLazy.Value;

        // Create the SkillSet
        var searchIndexer = new SearchIndexer(request.Name, request.DataSourceName, request.TargetIndexName)
        {
            SkillsetName = request.SkillsetName,
        };

        searchIndexer.IsDisabled = !request.Enabled;

        // Add Field Mappings to Search Indexer
        request.FieldMappings.ForEach((mapping) => searchIndexer.FieldMappings.Add(mapping.ToFieldMapping()));

        // Add Output Field Mappings to Search Indexer
        request.OutputFieldMappings.ForEach((mapping) => searchIndexer.OutputFieldMappings.Add(mapping.ToFieldMapping()));

        // Set the Schedule Interval
        if (request.ScheduleInterval != null)
        {
            searchIndexer.Schedule = new IndexingSchedule(request.ScheduleInterval.Value);
        }

        // Set Indexing Parameters
        if (request.IndexingParameters != null)
        {
            searchIndexer.Parameters = request.IndexingParameters.ToIndexingParameters();
        }

        var response = await searchIndexerClient.CreateOrUpdateIndexerAsync(searchIndexer, onlyIfUnchanged: false, cancellationToken);

        logger.LogInformation("Azure AI Search Indexer:{Indexer} created or updated successfully. Indexer will run every {Minutes} minutes.", response.Value.Name, request.ScheduleInterval?.TotalMinutes ?? 0);
    }

    /// <summary>
    /// Disables the Search Service Indexer if exist.
    /// </summary>
    /// <param name="indexerName">Name of the Indexer.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>True = if the indexer exist and disabled; false = if indexer does not exist.</returns>
    public async Task<bool> DisableIndexerIfExistAsync(string indexerName, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Attempting to get Indexer:{Indexer}", indexerName);

        var searchIndexerClient = searchIndexerClientLazy.Value;

        var existResponse = await searchIndexerClient.GetIndexerNamesAsync(cancellationToken);

        // return if the indexer does not exist
        if (existResponse.Value.Count == 0 || !existResponse.Value.Contains(indexerName))
        {
            logger.LogWarning("Indexer:{Indexer} not found for disable operation.", indexerName);
            return false;
        }

        // Get the Indexer
        var indexerResponse = await searchIndexerClient.GetIndexerAsync(indexerName, cancellationToken);

        var indexer = indexerResponse.Value;

        // Disable the Indexer
        if (indexer.IsDisabled == true)
        {
            logger.LogInformation("Indexer:{Indexer} is already disabled.", indexerName);
            return true;
        }
        else
        {
            indexer.IsDisabled = true;
        }

        // Send the update to the service.
        await searchIndexerClient.CreateOrUpdateIndexerAsync(indexer, onlyIfUnchanged: false, cancellationToken);
        logger.LogInformation("Indexer:{Indexer} is now disabled.", indexerName);

        return true;
    }

    /// <summary>
    /// Reset the Search Indexer. Resetting indexer will delete all change tracking.
    /// </summary>
    /// <param name="indexerName">Name of the Indexer.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public async Task<bool> ResetIndexerAsync(string indexerName, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get the client
            var searchIndexerClient = searchIndexerClientLazy.Value;

            // Reset the Indexer.
            var response = await searchIndexerClient.ResetIndexerAsync(indexerName, cancellationToken);

            if (response.IsError)
            {
                logger.LogError("Error resetting the indexer {IndexerName}. {Reason}.", indexerName, response.ReasonPhrase);
                return false;
            }

            return true;
        }
        catch (RequestFailedException ex)
        {
            logger.LogError("Error resetting the indexer {IndexerName}. {Reason}.", indexerName, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Run the Search Indexer.
    /// </summary>
    /// <param name="indexerName">Name of the Indexer.</param>
    /// <param name="cancellationToken">Cancellation Token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public async Task<bool> RunIndexerAsync(string indexerName, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get the client
            var searchIndexerClient = searchIndexerClientLazy.Value;

            // Reset the Indexer.
            var response = await searchIndexerClient.RunIndexerAsync(indexerName, cancellationToken);

            if (response.IsError)
            {
                logger.LogError("Error running the indexer {IndexerName}. {Reason}.", indexerName, response.ReasonPhrase);
                return false;
            }

            return true;
        }
        catch (RequestFailedException ex)
        {
            logger.LogError("Error running the indexer {IndexerName}. {Reason}.", indexerName, ex.Message);
            return false;
        }
    }
}
