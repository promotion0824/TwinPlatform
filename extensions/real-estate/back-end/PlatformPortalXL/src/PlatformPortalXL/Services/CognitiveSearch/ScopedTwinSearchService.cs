using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Auth;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Auth.Services;
using PlatformPortalXL.Features.Twins;
using PlatformPortalXL.Services.CognitiveSearch.Extensions;

namespace PlatformPortalXL.Services.CognitiveSearch;

/// <summary>
/// Represents a scoped twin search implementation.
/// </summary>
/// <remarks>
/// Builds a filter based on user permissions and the requested scope, model and file types, and runs the twin search.
/// </remarks>
public class ScopedTwinSearchService : ISearchService
{
    private readonly IAuthService _authService;
    private readonly ICurrentUser _currentUser;
    private readonly TwinSearchClient _twinSearchClient;
    private readonly ILogger<ScopedTwinSearchService> _logger;

    public ScopedTwinSearchService(
        IAuthService authService,
        TwinSearchClient twinSearchClient,
        ICurrentUser currentUser,
        ILogger<ScopedTwinSearchService> logger)
    {
        _authService = authService;
        _twinSearchClient = twinSearchClient;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Search twins observing the user's permissions.
    /// </summary>
    /// <param name="request">Requested scope, model and file types.</param>
    public async Task<(List<TwinSearchResponse.SearchTwin> twins, int nextPage)> Search(TwinCognitiveSearchRequest request)
    {
        var sw = Stopwatch.StartNew();

        var userPermissions = (await _authService.GetPermissions<CanViewSearchAndExplore>(_currentUser.Value)).ToList();
        if (userPermissions.Count == 0)
        {
            return ([], -1);
        }

        var filter = new TwinSearchFilterBuilder()
                            .AddPermissions(userPermissions)
                            .AddScope(request.ScopeId)
                            .AddModel(request.ModelId)
                            .AddFileTypes(request.FileTypes)
                            .Build();

        var (searchResults, nextPage) = await _twinSearchClient.Search(filter, request.Term, new PageInfo(request.Page, 100));

        var results = searchResults.Select(d => d.AsSearchTwin(_logger)).ToList();

        _logger.LogTrace("AzureSearchScopedTwinSearch: Query = '{QueryFilter}'", filter);
        _logger.LogTrace("AzureSearchScopedTwinSearch: Query returned {Count} result(s) took {Time} ({ElapsedMilliseconds:0}ms)", results.Count, sw, sw.ElapsedMilliseconds);

        return (results, nextPage.GetValueOrDefault());
    }
}
