namespace Willow.CognitiveSearch.Indexer;

using Azure.Search.Documents.Indexes.Models;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Skillset Helper Class.
/// </summary>
public static class SkillsetHelper
{
    /// <summary>
    /// Build Indexer Index Projection object for the skillset.
    /// </summary>
    /// <param name="selectors">Projection source to target field selectors.</param>
    /// <param name="includeParentDocuments">True to index parent document;false if not.</param>
    /// <returns>Instance of <see cref="SearchIndexerIndexProjection"/>.</returns>
    public static SearchIndexerIndexProjection BuildIndexerIndexProjections(this IEnumerable<IndexProjectionSelector> selectors, bool includeParentDocuments)
    {
        return new SearchIndexerIndexProjection(selectors.Select(s => s.ToSearchIndexProjectionSelector()).ToList())
        {
            Parameters = new SearchIndexerIndexProjectionsParameters()
            {
                ProjectionMode = includeParentDocuments ? IndexProjectionMode.IncludeIndexingParentDocuments : IndexProjectionMode.SkipIndexingParentDocuments,
            },
        };
    }

    /// <summary>
    /// Build Open AI Embedding Skill definition from Azure Open AI Service.
    /// </summary>
    /// <param name="request">Instance of <see cref="OpenAIEmbeddingSkillRequest"/> record.</param>
    /// <returns>Instance of <see cref="SearchIndexerSkill"/>.</returns>
    public static SearchIndexerSkill BuildOpenAIEmbeddingSkill(OpenAIEmbeddingSkillRequest request)
    {
        return new AzureOpenAIEmbeddingSkill(request.Inputs, request.Outputs)
        {
            Name = request.Name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? "Azure OpenAI Embedding skill to generate embeddings." : request.Description,
            Context = request.Context,
            ResourceUri = new Uri(request.ResourceUri ?? throw new ArgumentNullException("ResourceUri cannot be empty.")),
            DeploymentName = request.DeploymentId,
            ModelName = request.ModelName,
            ApiKey = request.ApiKey,
        };
    }

    /// <summary>
    /// Build Document Split skill for chunking blob documents.
    /// </summary>
    /// <param name="request">Instance of <see cref="TextSplitSkillRequest"/>.</param>
    /// <returns>Instance of <see cref="SearchIndexerSkill"/>.</returns>
    public static SearchIndexerSkill BuildDocumentSplitSkill(TextSplitSkillRequest request)
    {
        return new SplitSkill(request.Inputs, request.Outputs)
        {
            Name = request.Name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? "Split skill to chunk documents." : request.Description,
            Context = request.Context,
            DefaultLanguageCode = request.Language,
            TextSplitMode = request.SplitMode,
            MaximumPageLength = request.MaximumPageLength,
            PageOverlapLength = request.PageOverlapLength,
        };
    }
}
