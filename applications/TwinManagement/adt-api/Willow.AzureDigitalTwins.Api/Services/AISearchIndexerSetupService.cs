using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Messaging.Configuration;
using Willow.CognitiveSearch;
using Willow.CognitiveSearch.Index;
using Willow.CognitiveSearch.Indexer;

namespace Willow.AzureDigitalTwins.Api.Services;

/// <summary>
/// Configuration record for AI Search Indexer setup.
/// </summary>
public record AISearchIndexerSetupOption
{
    public const string Name = "AISearchIndexerSetup";
    public bool SetupEnabled { get; set; } = false;
    public bool IndexerEnabled { get; set; } = false;
}

/// <summary>
/// Configuration record for Azure Open AI Services.
/// </summary>
public record AzureOpenAIServiceOption
{
    public const string Name = "AzureOpenAIService";
    public string ResourceURI { get; set; }
    public string DeploymentId { get; set; }
    public string ApiKey { get; set; } = null;
}

public interface IAISearchIndexerSetupService
{
    Task Setup(CancellationToken cancellationToken = default);
}

public class AISearchIndexerSetupService : IAISearchIndexerSetupService
{
    private readonly ILogger<AISearchIndexerSetupService> _logger;
    private readonly DocumentStorageOptions _documentStorageOptions;
    private readonly DocumentStorageSettings _documentStorageSettings;
    private readonly AISearchIndexerSetupOption _aiSearchIndexerSetupOption;
    private readonly AzureOpenAIServiceOption _azureOpenAIServiceOption;
    private readonly AISearchSettings _searchSettings;
    private readonly IIndexerBuildService _indexerBuildService;
    private readonly IIndexBuildService _indexBuildService;

    const string SkillsetFileName = "DocumentIndex/Skillset.json";
    const string IndexerFileName = "DocumentIndex/Indexer.json";
    const string modelName = "text-embedding-ada-002";

    public AISearchIndexerSetupService(ILogger<AISearchIndexerSetupService> logger,
        IOptions<DocumentStorageOptions> documentOptions,
        IOptions<DocumentStorageSettings> documentSettings,
        IOptions<AISearchIndexerSetupOption> aiSearchIndexerSetupOption,
        IOptions<AzureOpenAIServiceOption> azureOpenAIServiceOption,
        IOptions<AISearchSettings> searchSettings,
        IIndexerBuildService indexerBuildService,
        IIndexBuildService indexBuildService)
    {
        _logger = logger;
        _documentStorageOptions = documentOptions.Value;
        _documentStorageSettings = documentSettings.Value;
        _aiSearchIndexerSetupOption = aiSearchIndexerSetupOption.Value;
        _azureOpenAIServiceOption = azureOpenAIServiceOption.Value;
        _searchSettings = searchSettings.Value;
        _indexerBuildService = indexerBuildService;
        _indexBuildService = indexBuildService;
    }

    public async Task Setup(CancellationToken cancellationToken = default)
    {
        try
        {

            if (!_aiSearchIndexerSetupOption.SetupEnabled)
            {
                _logger.LogCritical("AI Search Indexer Setup is not enabled. Skipping setup.");
                return;
            }

            _logger.LogInformation("AI Search: Setting up document pull indexer.");
            var indexName = await SetupIndex(_searchSettings.DocumentIndexName, cancellationToken);
            _logger.LogTrace("Document Search Index {IndexName} created/updated.", indexName);

            var dataSourceName = await SetupIndexerDatasource(indexName, cancellationToken);
            _logger.LogTrace("Document Search Index Datasource {DataSource} created/updated.", dataSourceName);

            var skillsetName = await SetupSkillset(indexName, cancellationToken);
            _logger.LogTrace("Document Search Indexer Skillset {Skillset} created/updated.", skillsetName);

            var indexerName = await SetupIndexer(indexName, dataSourceName, skillsetName, cancellationToken);
            _logger.LogTrace("Document Search Indexer {Indexer} created/updated.", indexerName);

            _logger.LogInformation("AI Search: Document pull indexer setup complete.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI Search: Error occurred while setting up document indexer");
        }
    }

    private async Task<string> SetupIndex(string indexName, CancellationToken cancellationToken)
    {
        var vectorSearch = new VectorSearch();
        var algorithmName = "documents-vec-algorithm"; // string.Format("{0}-algorithm", indexName);
        var vectorizerName = "documents-vectorizer"; // string.Format("{0}-openAI-vectorizer", indexName);

        // For supported vector search algorithm refer to https://learn.microsoft.com/en-us/azure/search/vector-search-ranking
        vectorSearch.Algorithms.Add(new HnswAlgorithmConfiguration(algorithmName)
        {
            Parameters = new()
            {
                Metric = VectorSearchAlgorithmMetric.Cosine,
                M = 4,
                EfConstruction = 400,
                EfSearch = 500
            }
        });
        vectorSearch.Vectorizers.Add(new AzureOpenAIVectorizer(vectorizerName)
        {
            Parameters = new AzureOpenAIVectorizerParameters()
            {
                ResourceUri = new Uri(_azureOpenAIServiceOption.ResourceURI),
                DeploymentName = _azureOpenAIServiceOption.DeploymentId,
                ModelName = modelName,
                ApiKey = string.IsNullOrWhiteSpace(_azureOpenAIServiceOption.ApiKey) ? null : _azureOpenAIServiceOption.ApiKey
            }
        });
        vectorSearch.Profiles.Add(new VectorSearchProfile(DocumentChunkDto.VectorProfileName, algorithmName)
        {
            VectorizerName = vectorizerName
        });

        var fieldList = (new FieldBuilder()).Build(typeof(DocumentChunkDto));

        var documentSearchIndex = new SearchIndex(indexName)
        {
            Fields = fieldList,
            // BM25Similarity Algorithm is the default algo for Lucene analyzer. B and K1 values can be adjusted while using hybrid or keyword search
            Similarity = new BM25Similarity(),
            VectorSearch = vectorSearch
        };

        await _indexBuildService.CreateOrUpdateIndex(documentSearchIndex, tryRebuildOnFailure:true, cancellationToken);
        return documentSearchIndex.Name;
    }

    private async Task<string> SetupIndexerDatasource(string indexName, CancellationToken cancellationToken)
    {
        // Create Storage Account Datasource request
        var datasourceRequest = new StorageContainerDataSource()
        {
            Name = string.Format("{0}-datasource", indexName),
            ContainerName = _documentStorageSettings.DocumentsContainer,
            ContainerQuery = null,
            StorageAccountConnection = string.IsNullOrWhiteSpace(_documentStorageOptions.ResourceId) ?
                                                                _documentStorageOptions.ConnectionString :
                                                                _documentStorageOptions.ResourceId,
            SoftDeleteColumnName = "IsDeleted",
            SoftDeleteMarkerValue = "true"
        };

        await _indexerBuildService.CreateOrUpdateStorageContainerDataSourceAsync(datasourceRequest, cancellationToken);
        return datasourceRequest.Name;
    }

    private async Task<string> SetupSkillset(string indexName, CancellationToken cancellation)
    {
        // Read the Skillset file
        if (!File.Exists(SkillsetFileName))
            throw new ArgumentException($"Search Indexer Skillset definition file {SkillsetFileName} does not exist.");

        List<SearchIndexerSkill> searchIndexerSkills = [];

        var skillsetDocument = JsonDocument.Parse(File.ReadAllText(SkillsetFileName));

        var serializerOption = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
        
        // Build Split Text Skill
        if (skillsetDocument.RootElement.TryGetProperty(nameof(TextSplitSkillRequest), out JsonElement splitSkillJsonElement))
        {
            var request = splitSkillJsonElement.Deserialize<TextSplitSkillRequest>(serializerOption);
            var skillDefinition = SkillsetHelper.BuildDocumentSplitSkill(request);
            searchIndexerSkills.Add(skillDefinition);
            _logger.LogTrace("Search Indexer Skill {SkillName} is now added to the skillset.", request.Name);
        }

        // Build Open AI Embedding Skill
        if (skillsetDocument.RootElement.TryGetProperty(nameof(OpenAIEmbeddingSkillRequest), out JsonElement openAIJsonElement))
        {
            var request = openAIJsonElement.Deserialize<OpenAIEmbeddingSkillRequest>(serializerOption);

            // Override resource uri and auth parameters
            request.ModelName = modelName;
            request.ResourceUri = _azureOpenAIServiceOption.ResourceURI;
            request.DeploymentId = _azureOpenAIServiceOption.DeploymentId;
            request.ApiKey = string.IsNullOrWhiteSpace(_azureOpenAIServiceOption.ApiKey) ? null : _azureOpenAIServiceOption.ApiKey;
            var skillDefinition = SkillsetHelper.BuildOpenAIEmbeddingSkill(request);
            searchIndexerSkills.Add(skillDefinition);
            _logger.LogTrace("Search Indexer Skill {SkillName} is now added to the skillset.", request.Name);
        }

        // Build Indexer Projection
        SearchIndexerIndexProjection searchIndexerIndexProjection = null;
        if (skillsetDocument.RootElement.TryGetProperty(nameof(IndexProjectionSelector), out JsonElement projectionJsonElement))
        {
            var request = projectionJsonElement.Deserialize<IndexProjectionSelector>(serializerOption);
            request.TargetIndexName = indexName;
            searchIndexerIndexProjection = SkillsetHelper.BuildIndexerIndexProjections([request], false);
        }

        // Build the Skillset request
        var skillset = new Skillset()
        {
            Name = string.Format("{0}-skillset", indexName),
            Description = string.Format("Document vectorization skillset for the index: {0}.", indexName),
            Projection = searchIndexerIndexProjection,
            Skills = searchIndexerSkills
        };

        await _indexerBuildService.CreateOrUpdateSkillsetAsync(skillset, cancellation);
        return skillset.Name;
    }

    private async Task<string> SetupIndexer(string indexName, string dataSourceName, string skillsetName, CancellationToken cancellation)
    {
        string indexerName = string.Format("{0}-indexer", indexName);

        if (!_aiSearchIndexerSetupOption.IndexerEnabled)
        {
            _logger.LogTrace("Search Indexer is disabled. Attempting to disable Indexer:{Indexer}",indexerName);
            await _indexerBuildService.DisableIndexerIfExistAsync(indexerName, cancellation);
            return indexerName;
        }

        // Read the Index Definition File
        if (!File.Exists(IndexerFileName))
            throw new ArgumentException($"Document Search Indexer definition file {IndexerFileName} does not exist.");

        var indexerJsonDocument = JsonDocument.Parse(File.ReadAllText(IndexerFileName));
        var serializerOption = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

        // Build request
        var indexerRequest = new IndexerRequest()
        {
            Enabled = _aiSearchIndexerSetupOption.IndexerEnabled,
            Name = indexerName,
            TargetIndexName = indexName,
            DataSourceName = dataSourceName,
            SkillsetName = skillsetName,
            ScheduleInterval = indexerJsonDocument.RootElement.GetProperty("scheduleInterval").Deserialize<TimeSpan>(serializerOption),
            FieldMappings = indexerJsonDocument.RootElement.GetProperty("fieldMappings").Deserialize<List<IndexerFieldMapping>>(serializerOption),
            OutputFieldMappings = indexerJsonDocument.RootElement.GetProperty("outputFieldMappings").Deserialize<List<IndexerFieldMapping>>(serializerOption),
            IndexingParameters = indexerJsonDocument.RootElement.GetProperty("parameters").Deserialize<IndexerParameters>(serializerOption),
        };

        await _indexerBuildService.CreateOrUpdateIndexerAsync(indexerRequest, cancellation);
        return indexerName;
    }
}
