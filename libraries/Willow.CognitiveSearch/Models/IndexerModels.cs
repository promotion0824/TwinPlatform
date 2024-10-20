namespace Willow.CognitiveSearch;

using System;
using System.Collections.Generic;
using Azure.Search.Documents.Indexes.Models;

/// <summary>
/// Blob Storage Container Data Source Request DTO
/// </summary>
public record StorageContainerDataSource
{
    /// <summary>
    /// Gets the name of the data source.
    /// </summary>
    required public string Name { get; init; }

    /// <summary>
    /// Gets the Storage Account Connection string; Connection string varies depending on the type of authentication.
    /// </summary>
    required public string StorageAccountConnection { get; init; }

    /// <summary>
    /// Gets the Blob Storage Account Container Name.
    /// </summary>
    required public string ContainerName { get; init; }

    /// <summary>
    /// Gets the Blob storage account container query to filer blobs. Optional.
    /// </summary>
    public string? ContainerQuery { get; init; }

    required public string SoftDeleteColumnName { get; init; }

    required public string SoftDeleteMarkerValue { get; init; }
}

/// <summary>
/// Skillset holding the collection of Skills.
/// </summary>
public record Skillset
{
    /// <summary>
    /// Gets or sets the name of the skillset.
    /// </summary>
    required public string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the skillset.
    /// </summary>
    required public string Description { get; set; }

    /// <summary>
    /// Gets or sets the list of Search Indexer skills.
    /// </summary>
    required public List<SearchIndexerSkill> Skills { get; set; }

    /// <summary>
    /// Gets or sets the list of Search Indexer index projection.
    /// </summary>
    required public SearchIndexerIndexProjection Projection { get; set; }

}

/// <summary>
/// Abstract class for defining Azure AI Search Skill DTO.
/// </summary>
public abstract record BaseSkill
{
    /// <summary>
    /// Gets the name of the skill.
    /// </summary>
    required public string Name { get; init; }

    /// <summary>
    /// Gets the name of the skill.
    /// </summary>
    required public string Description { get; init; }

    /// <summary>
    /// Gets the name of the context.
    /// </summary>
    required public string Context { get; init; }

    /// <summary>
    /// Gets the list input field mapping entry.
    /// </summary>
    required public List<InputFieldMappingEntry> Inputs { get; init; }

    /// <summary>
    /// Gets the list of output field mapping entry.
    /// </summary>
    required public List<OutputFieldMappingEntry> Outputs { get; init; }
}

/// <summary>
/// Azure Open AI Embedding Skill Request.
/// </summary>
public record OpenAIEmbeddingSkillRequest : BaseSkill
{
    /// <summary>
    /// Gets or sets the Azure resource uri for the Azure Open AI Service.
    /// </summary>
    public string? ResourceUri { get; set; }

    /// <summary>
    /// Gets or sets the model deployment Id from the Azure Open AI Service.
    /// </summary>
    public string? DeploymentId { get; set; }

    /// <summary>
    /// Deployment Model Name.
    /// </summary>
    public string? ModelName { get; set; } 

    /// <summary>
    /// Gets or sets the api key if you use to api key for the authentication with Azure Open AI Service.
    /// </summary>
    public string? ApiKey { get; set; } = null;
}

/// <summary>
/// Inbuilt Azure AI Search Document Split Skill request.
/// </summary>
public record TextSplitSkillRequest : BaseSkill
{
    /// <summary>
    /// Gets the language code for the text split skill.
    /// </summary>
    public SplitSkillLanguage? Language { get; init; } = SplitSkillLanguage.En;

    /// <summary>
    /// Gets the split mode for the document splitting. Allowed values [Pages, Sentence]. Default is pages.
    /// </summary>
    public TextSplitMode? SplitMode { get; init; } = TextSplitMode.Pages;

    /// <summary>
    /// Gets the maximum page length in characters to chunk per document. Applies only when text split model set to pages. Default is 10000.
    /// </summary>
    public int MaximumPageLength { get; init; } = 5000;

    /// <summary>
    /// Gets the maximum page overlap length in characters.
    /// </summary>
    public int PageOverlapLength { get; init; } = 200;
}

/// <summary>
/// Index Projection Selector
/// </summary>
public record IndexProjectionSelector
{
    /// <summary>
    /// Gets or sets the index name.
    /// </summary>
    public string? TargetIndexName { get; set; }

    /// <summary>
    /// Gets or sets the parent key field name on the index.
    /// </summary>
    required public string ParentKeyFieldName { get; set; }

    /// <summary>
    /// Gets or sets the source context expression.
    /// </summary>
    required public string SourceContext { get; set; }

    /// <summary>
    /// Gets or sets the list input field mapping entry.
    /// </summary>
    required public List<InputFieldMappingEntry> Mappings { get; set; }

    /// <summary>
    /// Convert to <see cref="SearchIndexerIndexProjectionSelector"/> instance.
    /// </summary>
    /// <returns>Field Mapping.</returns>
    public SearchIndexerIndexProjectionSelector ToSearchIndexProjectionSelector()
    {
        return new SearchIndexerIndexProjectionSelector(TargetIndexName, ParentKeyFieldName, SourceContext, Mappings);
    }
}

/// <summary>
///  Indexer Field Mapping.
/// </summary>
public record IndexerFieldMapping
{
    /// <summary>
    /// Gets or sets the name of the field in the data source.
    /// </summary>
    required public string SourceFieldName { get; set; }

    /// <summary>
    /// Gets or sets the name of the target field in the index. Same as the source field name by default.
    /// </summary>
    required public string TargetFieldName { get; set; }

    /// <summary>
    /// Convert to <see cref="FieldMapping"/> instance.
    /// </summary>
    /// <returns>Field Mapping.</returns>
    public FieldMapping ToFieldMapping()
    {
        return new FieldMapping(SourceFieldName) { TargetFieldName = TargetFieldName };
    }

}

/// <summary>
/// Indexer Parameters Configurations.
/// </summary>
public record IndexerParametersConfiguration
{
    /// <summary>
    /// Gets or sets the type of data to extract for the blob indexer. Supported values ["allMetadata","storageMetadata","contentAndMetadata"].
    /// </summary>
    required public string DataToExtract { get; set; }

    /// <summary>
    /// Gets or sets the blob parsing mode. Supported values ["default","text","delimitedText","jsonArray","jsonLines"].
    /// </summary>
    public string? ParsingMode { get; set; }

    /// <summary>
    /// Gets or sets the PDF Text Rotation Algorithm; Default is None; Supported Values :[DetectAngles].
    /// </summary>
    public string? PdfTextRotationAlgorithm { get; set; }
}

/// <summary>
/// Indexer Parameters Property Record.
/// </summary>
public record IndexerParameters
{
    /// <summary>
    /// Gets or sets number of items that are read from the data source and indexed as a single batch in order to improve performance. The default depends on the data source type.
    /// </summary>
    public int? BatchSize { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of items that can fail indexing for indexer execution to still be considered successful. -1 means no limit. Default is 0.
    /// </summary>
    public int? MaxFailedItems { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of items in a single batch that can fail indexing for the batch to still be considered successful. -1 means no limit. Default is 0.
    /// </summary>
    public int? MaxFailedItemsPerBatch { get; set; }

    /// <summary>
    /// Gets or sets the Configuration Parameters.
    /// </summary>
    required public IndexerParametersConfiguration Configuration { get; set; }

    /// <summary>
    /// Convert to <see cref="IndexingParameters"/> instance.
    /// </summary>
    /// <returns>Indexing Parameters.</returns>
    public IndexingParameters ToIndexingParameters()
    {
        return new IndexingParameters()
        {
            BatchSize = BatchSize,
            MaxFailedItems = MaxFailedItems,
            MaxFailedItemsPerBatch = MaxFailedItemsPerBatch,
            IndexingParametersConfiguration = new IndexingParametersConfiguration()
            {
                DataToExtract = new BlobIndexerDataToExtract(Configuration.DataToExtract),
                ParsingMode = new BlobIndexerParsingMode(Configuration.ParsingMode),
                PdfTextRotationAlgorithm = new BlobIndexerPdfTextRotationAlgorithm(Configuration.PdfTextRotationAlgorithm),
                IndexStorageMetadataOnlyForOversizedDocuments = true,
            },
        };
    }
}

/// <summary>
/// Azure AI Search Indexer create or update request.
/// </summary>
public record IndexerRequest
{
    /// <summary>
    /// Gets the name of the Indexer.
    /// </summary>
    required public string Name { get; init; }

    /// <summary>
    /// Gets a value indicating whether the indexer should be enabled or disabled.
    /// </summary>
    required public bool Enabled { get; init; }

    /// <summary>
    /// Gets the name of data source to use by the indexer.
    /// </summary>
    required public string DataSourceName { get; init; }

    /// <summary>
    /// Gets the name of the existing target index.
    /// </summary>
    required public string TargetIndexName { get; init; }

    /// <summary>
    /// Gets the name of the skillset to be used by the index.
    /// </summary>
    required public string SkillsetName { get; init; }

    /// <summary>
    /// Gets the list of field mappings. Maps datasource fields with the index fields.
    /// </summary>
    public List<IndexerFieldMapping> FieldMappings { get; init; } = new List<IndexerFieldMapping>();

    /// <summary>
    /// Gets the list of output field mappings. Maps skills output field with the index fields.
    /// </summary>
    public List<IndexerFieldMapping> OutputFieldMappings { get; init; } = new List<IndexerFieldMapping>();

    /// <summary>
    /// Gets the schedule interval in timespan for the indexer to run.
    /// </summary>
    public TimeSpan? ScheduleInterval { get; init; } = null;

    /// <summary>
    /// Gets the Indexing Parameters for building the indexer.
    /// </summary>
    public IndexerParameters? IndexingParameters { get; init; }
}
