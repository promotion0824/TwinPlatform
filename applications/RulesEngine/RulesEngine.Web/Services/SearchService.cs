
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading;
using Willow.CognitiveSearch;

namespace RulesEngine.Web;

/// <summary>
/// Wrapper for willow search
/// </summary>
public interface ISearchService
{
    /// <summary>
    ///  Search for an input.
    /// </summary>
    IAsyncEnumerable<SearchResult<UnifiedItemDto>> Search(string input, int? size = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Wrapper for willow search
/// </summary>
public class SearchService : SearchService<UnifiedItemDto>, ISearchService
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="searchSettings"></param>
    /// <param name="logger"></param>
    /// <param name="healthCheckSearch"></param>
    /// <param name="defaultAzureCredential"></param>
    public SearchService(
        IOptions<AISearchSettings> searchSettings,
        ILogger<SearchService<UnifiedItemDto>> logger,
        HealthCheckSearch healthCheckSearch,
        DefaultAzureCredential defaultAzureCredential)
        : base(searchSettings, logger, healthCheckSearch, defaultAzureCredential)
    {

    }
}
