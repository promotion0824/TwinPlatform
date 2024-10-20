using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Options;
using PlatformPortalXL.Dto;

namespace PlatformPortalXL.Services.CognitiveSearch;
//https://learn.microsoft.com/en-us/azure/search/query-simple-syntax#query-size-limits
//https://learn.microsoft.com/en-us/azure/search/search-query-odata-search-in-function

public record PageInfo(int PageNumber, int PageSize);

/// <summary>
/// Represents an Azure AI Search service that searches for twins.
/// </summary>
/// <remarks>
/// Search filters should be escaped before being passed to search, escaping here could result in filters being double
/// escaped.
/// TODO #136383 - This service should internally escape the tokens.
/// </remarks>
public class TwinSearchClient
{
    private readonly SearchClient _client;

    public TwinSearchClient(IOptions<AzureCognitiveSearchOptions> options)
    {
        _client = AzureSearchClientFactory.GetSearchClient(options.Value);
    }

    public TwinSearchClient(SearchClient client)
    {
        _client = client;
    }

    public Task<(List<SearchDocumentDto> Results, int? NextPage)> Search(string searchFilter, PageInfo? pageInfo, CancellationToken cancellationToken = default)
    {
        return Search(searchFilter, null, pageInfo, cancellationToken);
    }

    public async Task<(List<SearchDocumentDto> Results, int? NextPage)> Search(string searchFilter, string searchTerm, PageInfo? pageInfo, CancellationToken cancellationToken = default)
    {
        var searchOptions = new SearchOptions
        {
            QueryType = SearchQueryType.Simple,
            SearchMode = SearchMode.All,
            IncludeTotalCount = true,
            ScoringProfile = "rules",
            Filter = searchFilter ?? "Type eq 'twin'"
        };

        if (pageInfo is not null)
        {
            searchOptions.Size = pageInfo.PageSize;
            searchOptions.Skip = (pageInfo.PageNumber - 1) * pageInfo.PageSize;
        }

        searchOptions.SearchFields.Add("Id");
        searchOptions.SearchFields.Add("Ids");
        searchOptions.SearchFields.Add("PrimaryModelId");
        searchOptions.SearchFields.Add("ModelIds");
        searchOptions.SearchFields.Add("Names");
        searchOptions.SearchFields.Add("SecondaryNames");
        searchOptions.SearchFields.Add("Tags");
        searchOptions.SearchFields.Add("ModelNames");
        searchOptions.SearchFields.Add("LocationNames");
        searchOptions.SearchFields.Add("ExternalId");

        searchOptions.Select.Add("Id");
        searchOptions.Select.Add("Names");
        searchOptions.Select.Add("Location");
        searchOptions.Select.Add("PrimaryModelId");
        searchOptions.Select.Add("ModelIds");
        searchOptions.Select.Add("SiteId");
        searchOptions.Select.Add("ExternalId");

        var searchText = string.IsNullOrWhiteSpace(searchTerm) ? "*" : searchTerm;

        SearchResults<SearchDocumentDto> results = await _client.SearchAsync<SearchDocumentDto>(searchText, searchOptions, cancellationToken);

        // Search again with fuzzy search in case the user has misspelled the word (e.g. chiler instead of chiller)
        if (results.TotalCount == 0 && !string.IsNullOrWhiteSpace(searchTerm))
        {
            var fuzzySearch = $"{searchText}~1";
            // Set the query type to the full Lucene syntax (queryType=full)
            // for specialized query forms: fuzzy search, proximity search, regular expressions.
            searchOptions.QueryType = SearchQueryType.Full;
            results = await _client.SearchAsync<SearchDocumentDto>(fuzzySearch, searchOptions, cancellationToken);
        }

        var documents = await GetDocumentsAsync(results).ToListAsync(cancellationToken: cancellationToken);

        var nextPage = GetNextPageNumber(searchOptions, results, pageInfo);

        return (documents, nextPage);
    }

    private static int? GetNextPageNumber(SearchOptions options, SearchResults<SearchDocumentDto> results, PageInfo? pageInfo)
    {
        if (pageInfo is null)
        {
            return null;
        }

        return (results.TotalCount > options.Skip + options.Size) ? pageInfo.PageNumber + 1 : null;
    }

    private static async IAsyncEnumerable<SearchDocumentDto> GetDocumentsAsync(SearchResults<SearchDocumentDto> searchResults)
    {
        await foreach (var doc in searchResults.GetResultsAsync())
        {
            yield return doc.Document;
        }
    }
}
