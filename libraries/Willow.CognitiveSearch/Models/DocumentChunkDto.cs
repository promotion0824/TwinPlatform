namespace Willow.CognitiveSearch;

using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using System;
using System.Collections.Generic;

/// <summary>
/// DTO for search document Index.
/// </summary>
public partial class DocumentChunkDto
{
    /// <summary>
    /// Vector Search Profile name for vector field.
    /// Client building the index should define the profile containing the vector algorithm and vectorizer connection.
    /// </summary>
    public const string VectorProfileName = "Document-Vector-Profile";

    /// <summary>
    /// Dimension of the vector field. 1536 corresponds to the text-embedding-ada-002 embedding model.
    /// </summary>
    public const int VectorDimensions = 1536;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentChunkDto"/> class.
    /// </summary>
    public DocumentChunkDto() { }

    /// <summary>
    /// Gets or sets the portion of the chunked document text.
    /// </summary>
    [SearchableField(IsKey = false, IsFilterable = false, IsSortable = false, IsFacetable = false, AnalyzerName = LexicalAnalyzerName.Values.StandardLucene)]
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets type of Index document.
    /// </summary>
    [SimpleField(IsKey = false, IsFilterable = true, IsSortable = false, IsFacetable = false)]
    public string? ItemType { get; set; }

    /// <summary>
    /// Gets or sets the unique Id of the document chunk.
    /// </summary>
    [SearchableField(IsKey = true, IsFilterable = true, IsSortable = false, IsFacetable = false, AnalyzerName = LexicalAnalyzerName.Values.Keyword)]
    public string Id { get; set; } = null!;

    /// <summary>
    /// Gets or sets the unique Id of the parent document.
    /// </summary>
    [SearchableField(IsKey = false, IsFilterable = true, IsSortable = false, IsFacetable = false, AnalyzerName =LexicalAnalyzerName.Values.StandardLucene)]
    public string GroupId { get; set; } = null!;

    /// <summary>
    /// Gets or sets whether the document index is enabled for Copilot RAG retrieval.
    /// </summary>
    [SimpleField(IsKey = false, IsFilterable = true, IsSortable = false, IsFacetable = false)]
    public bool CopilotEnabled { get; set; }

    /// <summary>
    /// Gets or sets the File name of the document.
    /// </summary>
    [SearchableField(IsKey = false, IsFilterable = true, IsSortable = false, IsFacetable = false, AnalyzerName = LexicalAnalyzerName.Values.StandardLucene)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Absolute url of url of the document assigned by the storage account.
    /// </summary>
    [SimpleField(IsKey = false, IsFilterable = true, IsSortable = false, IsFacetable = false)]
    public string Uri { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Index Document last Modified date and time.
    /// </summary>
    [SimpleField(IsFilterable = true, IsSortable = false, IsFacetable = false)]
    public DateTimeOffset? IndexUpdateTime { get; set; }

    /// <summary>
    /// Gets or sets the Last Modified date and time of the document.
    /// </summary>
    [SimpleField(IsFilterable = true, IsSortable = false, IsFacetable = false)]
    public DateTimeOffset? DocLastUpdateTime { get; set; }

    /// <summary>
    /// Gets or sets the page number of the stored document chunk.
    /// </summary>
    [SimpleField]
    public int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the parse path.
    /// </summary>
    [SimpleField]
    public string ParsePath { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Vector Representation.
    /// </summary>
    [VectorSearchField(IsHidden = true, VectorSearchDimensions = VectorDimensions, VectorSearchProfileName = VectorProfileName)]
    public ICollection<float>? ContentVector { get; set; }

    /// <summary>
    /// Gets or sets the requested chunk size.
    /// </summary>
    [SimpleField]
    public int RequestedChunkSize { get; set; }

    /// <summary>
    /// Gets or sets the total number of chunks created for the parent document.
    /// </summary>
    [SimpleField]
    public int TotalDocumentNumChunks { get; set; }

    /// <summary>
    /// Gets or sets the length of the parent document.
    /// </summary>
    [SimpleField]
    public Int64 TotalDocumentLength { get; set; }

    /// <summary>
    /// Gets or sets the content-length of the parent document.
    /// </summary>
    [SimpleField]
    public Int64 ContentLength { get; set; }

    /// <summary>
    /// Gets or sets the content token count of the parent document.
    /// </summary>
    [SimpleField]
    public Int64 ContentTokenCount { get; set; }

    /// <summary>
    /// Gets or sets the document unstructured metadata.
    /// </summary>
    [SearchableField(IsFilterable = true)]
    public string DocUnstructuredMetadata { get; set; } = null!;

    /// <summary>
    /// Gets or set the indexer source the index document was created.
    /// </summary>
    [SimpleField(IsFilterable = true)]
    public string IndexerSource { get; set; } = null!;

    /// <summary>
    /// Gets or set the processing parameters.
    /// </summary>
    [SimpleField(IsFilterable = false)]
    public string ProcessingParameters { get; set; } = null!;
}
