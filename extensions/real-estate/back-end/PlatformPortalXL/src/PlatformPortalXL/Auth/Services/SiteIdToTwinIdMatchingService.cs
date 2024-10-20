using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Extensions;
using PlatformPortalXL.Services.CognitiveSearch;

namespace PlatformPortalXL.Auth.Services;

/// <summary>
/// Converts site Ids to twin Ids.
/// </summary>
/// <remarks>
/// User Management scopes are configured with twin Ids whereas auth code is called to evaluate against a site Id.
/// </remarks>
public class SiteIdToTwinIdMatchingService : ISiteIdToTwinIdMatchingService
{
    private static string CacheKeyForSiteIdToTwin(Guid siteId) => $"{siteId}-to-twin-match";
    private readonly TwinSearchClient _searchClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SiteIdToTwinIdMatchingService> _logger;

    public SiteIdToTwinIdMatchingService(
        TwinSearchClient searchClient,
        IMemoryCache cache,
        ILogger<SiteIdToTwinIdMatchingService> logger)
    {
        _searchClient = searchClient;
        _cache = cache;
        _logger = logger;
    }

    private static readonly IReadOnlyList<string> SpatialModelIds =
    [
        "dtmi:com:willowinc:Building;1",
        "dtmi:com:willowinc:Substructure;1",
        "dtmi:com:willowinc:SubBuilding;1",
        "dtmi:com:willowinc:OutdoorArea;1"
    ];

    /// <summary>
    /// Finds the most significant spatial twin for a site id and caches the result.
    /// </summary>
    /// <remarks>
    /// To find the most significant spatial twin we search twins by PrimaryModelId, traversing the SpatialModelIds
    /// collection in order. First looking for Building, then Substructure, etc. Put another way, a Building is more
    /// significant than a Substructure, which is more significant than a SubBuilding, etc.
    /// </remarks>
    public async Task<string> FindMatchToMostSignificantSpatialTwin(Guid siteId)
    {
        var key = CacheKeyForSiteIdToTwin(siteId);

        var twin = await _cache.GetOrCreateLockedAsync(key, async entry =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromMinutes(120));

            _logger.LogDebug("==== SiteIdToTwinIdMatchingService: CACHE MISS {key} ====", key);

            var spatialModelFilter = CreateFilter(SpatialModelIds, siteId);

            var (spatialModels, _) = await _searchClient.Search(spatialModelFilter, null);

            var twin = FindMostSignificantSpatialTwin(spatialModels, siteId);

            if (twin is not null)
            {
                CacheSiteIdMapping(siteId, twin);
            }

            return twin;
        });

        return twin?.TwinId;
    }

    /// <summary>
    /// Update a cache of site id to twin.
    /// </summary>
    /// <remarks>
    /// Load all spatial models, for each site Id find the most significant spatial twin and cache the mapping of
    /// site Id to twin. Called on an interval by a background service.
    /// </remarks>
    public async Task UpdateSiteIdsToTwinMappings()
    {
        _logger.LogDebug("Updating site id to twin id mappings");

        var spatialModelFilter = CreateFilter(SpatialModelIds);

        var (spatialModels, _) = await _searchClient.Search(spatialModelFilter, null);

        var siteIds = spatialModels
                        .Select(d => (Parsed: Guid.TryParse(d.SiteId, out var siteId), SiteId: siteId))
                        .Where(r => r.Parsed)
                        .Select(r => r.SiteId)
                        .Distinct();

        foreach (var siteId in siteIds)
        {
            try
            {
                var twin = FindMostSignificantSpatialTwin(spatialModels, siteId);

                if (twin is not null)
                {
                    CacheSiteIdMapping(siteId, twin);
                    continue;
                }

                _logger.LogWarning("Site {SiteId} is not mapped to a twin", siteId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping site {SiteId} to a twin", siteId);
            }
        }
    }

    /// <summary>
    /// Given a set of matches having a spatial model return the most significant twin matching the site Id.
    /// </summary>
    private static TwinWithAncestors FindMostSignificantSpatialTwin(List<SearchDocumentDto> matches, Guid siteId)
    {
        foreach (var spatialModelId in SpatialModelIds)
        {
            var match = matches.FirstOrDefault(i => i.SiteId == siteId.ToString() && string.Equals(i.PrimaryModelId, spatialModelId));

            if (match is null || string.IsNullOrEmpty(match.SiteId))
            {
                continue;
            }

            return new TwinWithAncestors(match.Id, match.Location is not null ? [.. match.Location] : []);
        }

        return null;
    }

    private static readonly MemoryCacheEntryOptions CacheOptions = new()
    {
        Priority = TwinCaching.Priority,
        AbsoluteExpiration = TwinCaching.GetAbsoluteExpiration
    };

    private void CacheSiteIdMapping(Guid siteId, TwinWithAncestors twin)
    {
        _cache.Set(CacheKeyForSiteIdToTwin(siteId), twin.TwinId, CacheOptions);
        _cache.Set(TwinCaching.CacheKeyForAncestorsLookup(twin.TwinId), twin, CacheOptions);

        _logger.LogDebug("SiteIdToTwinIdMatchingService: Mapped site '{siteId}' to twin '{twinId}'", siteId, twin.TwinId);
    }

    /// <summary>
    /// Create and return a filter to find twins that match the given model ids, and optionally the siteId.
    /// </summary>
    private static string CreateFilter(IEnumerable<string> modelIds, Guid? siteId = null)
    {
        List<string> clauses = ["Type eq 'twin'"];

        if (siteId.HasValue)
        {
            clauses.Add($"SiteId eq '{siteId}'");
        }

        var models = string.Join(",", modelIds.Select(modelId => SearchText.Escape(modelId, LexicalAnalyzerName.Values.Simple)));
        clauses.Add($"(ModelIds/any(m: search.in(m, '{models}')) or search.in(PrimaryModelId, '{models}'))");

        return string.Join(" and ", clauses);
    }
}
