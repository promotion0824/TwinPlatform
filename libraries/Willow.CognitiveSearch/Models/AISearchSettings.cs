namespace Willow.CognitiveSearch;

using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using System;

/// <summary>
/// Settings for Azure Cognitive Search.
/// </summary>
public class AISearchSettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AISearchSettings"/> class.
    /// </summary>
    public AISearchSettings() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AISearchSettings"/> class.
    /// </summary>
    /// <param name="unifiedIndexName">Unified Index Name.</param>
    /// <param name="documentIndexName">Document Index Name.</param>
    /// <param name="uri">Uri.</param>
    public AISearchSettings(string unifiedIndexName, string documentIndexName, string uri)
    {
        UnifiedIndexName = unifiedIndexName ?? throw new ArgumentNullException(nameof(unifiedIndexName));
        DocumentIndexName = documentIndexName;
        Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        if (!System.Uri.IsWellFormedUriString(uri, UriKind.Absolute))
        {
            throw new ArgumentException("Uri is not well formed", nameof(uri));
        }
    }

    /// <summary>
    /// Gets or sets the unified index name is typically the customer ID. Ex. twin-wil-dev.
    /// </summary>
    /// <remarks>
    /// Using CustomerID allows multiple indexes to exist on one cog search instance for testing and development
    /// In production we isolate to one per customer instance.
    /// </remarks>
    public string UnifiedIndexName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document index name. Ex. documents-wil-dev.
    /// </summary>
    public string DocumentIndexName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Uri for the cognitive search instance.
    /// </summary>
    public string Uri { get; set; } = string.Empty;

    /// <summary>
    /// Gets the target index name by search type.
    /// </summary>
    /// <typeparam name="T">Type of search.</typeparam>
    /// <returns>Name of the index and index default search options.</returns>
    /// <exception cref="NotImplementedException">If search type does not match any index.</exception>
    internal (string IndexName, SearchOptions DefaultOptions) GetIndexDefaults<T>()
    {
        if (typeof(T) == typeof(UnifiedItemDto))
        {
            return (UnifiedIndexName, new SearchOptions()
            {
                QueryType = SearchQueryType.Full,
                SearchMode = SearchMode.Any,
                ScoringProfile = "rules",
                IncludeTotalCount = true,
                Size = 50,
            });
        }
        else if (typeof(T) == typeof(DocumentChunkDto))
        {
            return (DocumentIndexName, new SearchOptions()
            {
                QueryType = SearchQueryType.Full,
                SearchMode = SearchMode.Any,
                IncludeTotalCount = true,
                Size = 50,
            });
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}
