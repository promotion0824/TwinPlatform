using System;
using Azure.Identity;
using Azure.Search.Documents;

namespace PlatformPortalXL.Services.CognitiveSearch;

internal static class AzureSearchClientFactory
{
    private static readonly DefaultAzureCredential DefaultAzureCredential = new();

    internal static SearchClient GetSearchClient(AzureCognitiveSearchOptions cognitiveSearchOptions)
    {
        return GetSearchClient(cognitiveSearchOptions.Uri, cognitiveSearchOptions.IndexName);
    }

    private static SearchClient GetSearchClient(string cognitiveSearchUri, string cognitiveSearchIndex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cognitiveSearchUri);
        ArgumentException.ThrowIfNullOrWhiteSpace(cognitiveSearchIndex);

        return new SearchClient(new Uri(cognitiveSearchUri), cognitiveSearchIndex, DefaultAzureCredential);
    }
}
