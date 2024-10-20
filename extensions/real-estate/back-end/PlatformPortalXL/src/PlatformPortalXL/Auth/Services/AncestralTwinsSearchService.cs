using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Services.CognitiveSearch;

namespace PlatformPortalXL.Auth.Services;

/// <summary>
/// Service for accessing ancestral twins, containing all ancestor locations.
/// </summary>
public interface IAncestralTwinsSearchService
{
    Task<IEnumerable<ITwinWithAncestors>> GetTwinsByModel(string model, int page, CancellationToken cancellationToken = default);

    Task<ITwinWithAncestors?> GetTwinById(string twinId, CancellationToken cancellationToken = default);
}

public class AncestralTwinsSearchService : IAncestralTwinsSearchService
{
    private readonly TwinSearchClient _searchClient;
    private readonly ILogger<AncestralTwinsSearchService> _logger;

    public AncestralTwinsSearchService(TwinSearchClient searchClient, ILogger<AncestralTwinsSearchService> logger)
    {
        _searchClient = searchClient;
        _logger = logger;
    }

    public Task<IEnumerable<ITwinWithAncestors>> GetTwinsByModel(string model, int page, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Executing twin ancestral query for '{Model}' page# {Page}", model, page);

        var escapedModel = SearchText.Escape(model, LexicalAnalyzerName.Values.Simple);
        var modelFilter = $"Type eq 'twin' and ModelIds/any(id: id eq '{escapedModel}')";

        return SearchTwins(modelFilter, new PageInfo(page, 1_000), cancellationToken);
    }

    public async Task<ITwinWithAncestors?> GetTwinById(string twinId, CancellationToken cancellationToken = default)
    {
        var twinMatch = await GetTwinsById([twinId], cancellationToken);

        return twinMatch.FirstOrDefault(i => i.TwinId == twinId);
    }

    private Task<IEnumerable<ITwinWithAncestors>> GetTwinsById(IEnumerable<string> twinIds, CancellationToken cancellationToken = default)
    {
        var idFilters = $"Ids/any(id: search.in(id, '{string.Join(
            ",",
            twinIds.Select(twinId => SearchText.Escape(twinId, LexicalAnalyzerName.Values.Simple)))}'))";

        var filter = $"Type eq 'twin' and {idFilters}";

        return SearchTwins(filter, null, cancellationToken);
    }

    private async Task<IEnumerable<ITwinWithAncestors>> SearchTwins(string filter, PageInfo pageInfo = null, CancellationToken cancellationToken = default)
    {
        var (results, _) = await _searchClient.Search(filter, pageInfo, cancellationToken);

        return results.Select(doc => new TwinWithAncestors(doc.Id, doc.Location is not null ? [..doc.Location] : []));
    }
}
