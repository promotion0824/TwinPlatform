using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Options;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Twins;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services.CognitiveSearch.Extensions;
using static PlatformPortalXL.Features.Twins.TwinSearchResponse;

namespace PlatformPortalXL.Services.CognitiveSearch;

public interface ISearchService
{
    Task<(List<SearchTwin> twins, int nextPage)> Search(
        TwinCognitiveSearchRequest request
    );
}

/// <summary>
/// A service for searching twins
/// </summary>
public class SearchService : ISearchService
{
    private readonly AzureCognitiveSearchOptions _cognitiveSearchOptions;

    /// <summary>
    /// Creates a new <see cref="SearchService" />
    /// </summary>
    public SearchService(
        IOptions<AzureCognitiveSearchOptions> cognitiveSearchOptions
    )
    {
        _cognitiveSearchOptions = cognitiveSearchOptions.Value;
    }

    private SearchClient GetSearchClient() =>
        AzureSearchClientFactory.GetSearchClient(
            _cognitiveSearchOptions
        );

    /// <summary>
    /// Search for an input string in the index
    /// </summary>
    public async Task<(List<SearchTwin> twins, int nextPage)> Search(
        TwinCognitiveSearchRequest request
    )
    {
        var searchClient = GetSearchClient();
        const int pageSize = 100;
        var skip = (request.Page - 1) * pageSize;

        var escapedTerm = SearchText.Escape(request.Term, LexicalAnalyzerName.Values.Simple);

        // We want both synonym search and wildcard search.
        //
        // From the documentation: "If you need to do a single query that applies
        // synonym expansion and wildcard, regex, or fuzzy searches, you can combine
        // the queries using the OR syntax. For example, to combine synonyms with
        // wildcards for simple query syntax, the term would be <query> | <query>*."
        //
        //https://learn.microsoft.com/en-us/azure/search/search-synonyms#how-synonyms-are-used-during-query-execution
        var search = string.IsNullOrWhiteSpace(escapedTerm)
            ? string.Empty
            : $"{escapedTerm} | {escapedTerm}*";

        var searchOptions = new SearchOptions
        {
            QueryType = SearchQueryType.Simple,
            SearchMode = SearchMode.All,
            IncludeTotalCount = true,
            ScoringProfile = "rules",
            Filter = FilterExpressions(request)
        };

        searchOptions.SearchFields.Add("Id");
        searchOptions.SearchFields.Add("Ids");
        searchOptions.SearchFields.Add("ModelIds");
        searchOptions.SearchFields.Add("Names");
        searchOptions.SearchFields.Add("SecondaryNames");
        searchOptions.SearchFields.Add("Tags");
        searchOptions.SearchFields.Add("ModelNames");

        searchOptions.Select.Add("Id");
        searchOptions.Select.Add("Names");
        searchOptions.Select.Add("PrimaryModelId");
        searchOptions.Select.Add("SiteId");
        searchOptions.Select.Add("ExternalId");

        if (!request.Export)
        {
            searchOptions.Size = pageSize;
            searchOptions.Skip = skip;
        }

        //Search using wildcard as this search cannot be combined with fuzzy search for searching partial words like "bacnet"
        SearchResults<SearchDocumentDto> searchResults =
            await searchClient.SearchAsync<SearchDocumentDto>(search, searchOptions);
        IAsyncEnumerable<SearchDocumentDto> result = GetDocumentsAsync(searchResults);

        //Search again with fuzzy search in case the user has misspelled the word(ex. chiler instead of chiller)
        if (searchResults.TotalCount == 0)
        {
            var fuzzySearch = string.IsNullOrWhiteSpace(escapedTerm)
                ? string.Empty
                : $"{escapedTerm}~1";
            //Set the query type to the full Lucene syntax (queryType=full)
            //for specialized query forms: fuzzy search, proximity search, regular expressions.
            searchOptions.QueryType = SearchQueryType.Full;
            searchResults = await searchClient.SearchAsync<SearchDocumentDto>(
                fuzzySearch,
                searchOptions
            );
            result = GetDocumentsAsync(searchResults);
        }

        //Set the page to zero for the response to not send the next page url
        var nextPage = (searchResults.TotalCount > skip + pageSize) ? request.Page + 1 : 0;

        var documents = await result.ToListAsync();
        var searchTwins = documents.Select(d => d.AsSearchTwin()).ToList();

        return (searchTwins, nextPage);
    }

    private static async IAsyncEnumerable<SearchDocumentDto> GetDocumentsAsync(
        SearchResults<SearchDocumentDto> searchResults
    )
    {
        await foreach (var doc in searchResults.GetResultsAsync())
        {
            yield return doc.Document;
        }
    }

    private static string FilterExpressions(TwinCognitiveSearchRequest request)
    {
        var defaultModelIds = new List<string>
        {
            "dtmi:com:willowinc:Asset;1",
            "dtmi:com:willowinc:Space;1",
            "dtmi:com:willowinc:BuildingComponent;1",
            "dtmi:com:willowinc:Structure;1",
            "dtmi:com:willowinc:Component;1",
            "dtmi:com:willowinc:Collection;1",
            "dtmi:com:willowinc:Account;1"
        };

        List<string> siteFilters = new();
        List<string> fileTypeFilters = new();
        List<string> filterExpressions = new();
        var fileTypes = request.FileTypes;
        List<string> siteIds = request.SiteIds.Select(s => s.ToString()).ToList();

        filterExpressions.Add("Type eq 'twin'");

        foreach (var siteId in siteIds)
        {
            siteFilters.Add($"SiteId eq '{siteId}'");
        }

        if (siteFilters.Count > 0)
        {
            filterExpressions.Add($"({string.Join(" or ", siteFilters)})");
        }

        if (fileTypes?.Any() == true)
        {
            foreach (var fileType in request.FileTypes)
            {
                fileTypeFilters.Add(
                    $"search.ismatch('/.*{SearchText.Escape(fileType, LexicalAnalyzerName.Values.EnLucene)}/', 'Names', 'full', 'all')"
                );
            }
        }

        if (fileTypeFilters.Count > 0)
        {
            filterExpressions.Add($"({string.Join(" or ", fileTypeFilters)})");
        }

        var modelIds = !string.IsNullOrWhiteSpace(request.ModelId)
            ? new List<string>() { request.ModelId }
            : defaultModelIds;

        filterExpressions.Add(
            $"({string.Join(" or ", modelIds.Select(modelId =>
        {
            return $"(PrimaryModelId eq '{SearchText.Escape(modelId, LexicalAnalyzerName.Values.Simple)}' or " +
                    $"ModelIds/any(s: s eq '{SearchText.Escape(modelId, LexicalAnalyzerName.Values.Simple)}'))";
        }))})"
        );

        return (filterExpressions.Count > 0) ? string.Join(" and ", filterExpressions) : null;
    }
}
