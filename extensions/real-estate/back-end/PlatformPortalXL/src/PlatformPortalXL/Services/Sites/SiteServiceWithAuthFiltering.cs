using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlatformPortalXL.Auth.Services;
using PlatformPortalXL.Configs;
using PlatformPortalXL.Auth.Services;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Extensions;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.Features.Scopes;
using PlatformPortalXL.Features.SiteStructure.Requests;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.DirectoryApi.Responses;
using PlatformPortalXL.ServicesApi.SiteApi;
using Willow.Batch;
using Willow.Platform.Models;

namespace PlatformPortalXL.Services.Sites;

/// <summary>
/// Site service that filters sites based on user management permissions.
/// </summary>
/// <remarks>
/// Decorator for <see cref="SiteService"/> that filters sites based on user management permissions. Methods delegate
/// to the actual service, but the GetSites method filters the sites based on the user's permissions.
/// </remarks>
public class SiteServiceWithAuthFiltering : ISiteService
{
    private readonly SiteService _actual;
    private readonly ISiteApiService _siteApiService;
    private readonly IAccessControlService _accessControlService;
    private readonly IMemoryCache _cache;
    private readonly IDirectoryApiService _directoryApi;
    private readonly CustomerInstanceConfigurationOptions _customerInstanceConfigurationOptions;
    private readonly IAuthFeatureFlagService _featureFlagService;
    private readonly ILogger<SiteServiceWithAuthFiltering> _logger;

    /// <summary>
    /// Site service that filters sites based on user management permissions.
    /// </summary>
    /// <remarks>
    /// Decorator for <see cref="SiteService"/> that filters sites based on user management permissions. Methods delegate
    /// to the actual service, but the GetSites method filters the sites based on the user's permissions.
    /// </remarks>
    public SiteServiceWithAuthFiltering(SiteService actual,
        ISiteApiService siteApiService,
        IAccessControlService accessControlService,
        IMemoryCache cache,
        IDirectoryApiService directoryApi,
        IOptions<CustomerInstanceConfigurationOptions> customerInstanceConfigurationOptions,
        IAuthFeatureFlagService featureFlagService,
        ILogger<SiteServiceWithAuthFiltering> logger)
    {
        _actual = actual;
        _siteApiService = siteApiService;
        _accessControlService = accessControlService;
        _cache = cache;
        _directoryApi = directoryApi;
        _customerInstanceConfigurationOptions = customerInstanceConfigurationOptions.Value;
        _featureFlagService = featureFlagService;
        _logger = logger;
    }

    public Task Broadcast(Guid siteId, IEnumerable<string> urns)
        => _actual.Broadcast(siteId, urns);

    public async Task CheckScopePermission(Guid userId, string permissionId, string scopeId)
    {
        if (!_featureFlagService.IsFineGrainedAuthEnabled)
        {
            await _actual.CheckScopePermission(userId, permissionId, scopeId);
            return;
        }

        var sites = await GetAuthedSitesForUser(userId, permissionId);

        await _actual.CheckScopePermissionInternal(sites.ToList(), userId, scopeId);
    }

    public async Task<List<Guid>> GetAuthorizedSiteIds(Guid userId, string scopeId = null, IEnumerable<Guid> siteIds = null, Expression<Func<Site, bool>> predicate = null)
    {
        if (!_featureFlagService.IsFineGrainedAuthEnabled)
        {
            return await _actual.GetAuthorizedSiteIds(userId, scopeId, siteIds);
        }

        var sites = await GetAuthedSitesForUser(userId);

        var userSiteIds = await _actual.GetAuthorizedSiteIdsInternal(sites.ToList(), userId, scopeId, siteIds, predicate);

        return userSiteIds;
    }

    public Task<List<Guid>> GetAuthorizedSiteIds(Guid userId, List<string> scopeIds, Expression<Func<Site, bool>> predicate = null)
        => _actual.GetAuthorizedSiteIds  (userId, scopeIds, predicate);

    public Task<List<TwinDto>> GetUserSiteTwinsByScopeIdAsync(ScopeIdRequest request)
        => _actual.GetUserSiteTwinsByScopeIdAsync(request);

    public Task<string> MapToTwinId(Guid siteId)
        => _actual.MapToTwinId(siteId);

    public Task<Guid> GetCustomerId(Guid siteId)
        => _actual.GetCustomerId(siteId);

    public Task<Guid> GetCustomerId(IEnumerable<Guid> siteIds)
        => _actual.GetCustomerId(siteIds);

    public async Task<List<SiteDetailDto>> GetSites(Guid userId, bool includeWeather = false, bool includeDtIds = false)
    {
        if (!_featureFlagService.IsFineGrainedAuthEnabled)
        {
            return await _actual.GetSites(userId, includeWeather, includeDtIds);
        }

        var sites = await GetAuthedSitesForUser(userId);

        var customer = await _directoryApi.GetCustomer(Guid.Parse(_customerInstanceConfigurationOptions.Id));
        var isConnectivityViewEnabled = customer.Features.IsConnectivityViewEnabled;

        var getSiteTasks = sites.Select(s => _actual.GetSite(s,
                                                     () => _accessControlService.CanAccessSite(userId, Permissions.ManageFloors, s.Id),
                                                     isConnectivityViewEnabled,
                                                     includeWeather,
                                                     includeDtIds));

        var siteDetails = await Task.WhenAll(getSiteTasks);

        await _actual.AppendInsightsAndTicketStats(siteDetails);

        return siteDetails.ToList();
    }

    public async Task<BatchDto<SiteMiniDto>> GetSitesV2(Guid userId, BatchSitesRequest request)
    {
        var siteDtos = await _actual.GetSitesV2(userId, request);

        if (!_featureFlagService.IsFineGrainedAuthEnabled)
        {
            return siteDtos;
        }

        var sites = (await AuthFilterSites(userId, siteDtos.Items, Permissions.ViewSites)).ToList();

        return new BatchDto<SiteMiniDto> { Items = [.. sites], Total = sites.Count };
    }

    internal async Task<IEnumerable<Site>> GetAuthedSitesForUser(Guid userId, string permissionId = Permissions.ViewSites)
    {
        // Find all sites for the customer

        var customerId = Guid.Parse(_customerInstanceConfigurationOptions.Id);
        var customerSites = await _siteApiService.GetSitesByCustomerAsync(customerId);

        // Filter the customer sites that the user can access ...

        var authedUserSites = await AuthFilterSites(userId, customerSites, permissionId);

        return authedUserSites;
    }

    private async Task<IEnumerable<Site>> AuthFilterSites(Guid userId, List<Site> items, string permissionId)
    {
        var siteIds = await AuthFilterSiteIds(userId, items.Select(i => i.Id), permissionId);

        return items.Where(s => siteIds.Contains(s.Id));
    }

    private async Task<IEnumerable<SiteMiniDto>> AuthFilterSites(Guid userId, SiteMiniDto[] items, string permissionId)
    {
        var siteIds = await AuthFilterSiteIds(userId, items.Select(i => i.Id), permissionId);

        return items.Where(s => siteIds.Contains(s.Id));
    }

    /// <summary>
    /// Filters the site Ids based on the user's permissions.
    /// </summary>
    /// <returns>
    /// The sites that the user has the given permissions for. If an exception is encountered evaluating the permission
    /// for a site that site is excluded from the result.
    /// </returns>
    private async Task<IReadOnlyCollection<Guid>> AuthFilterSiteIds(Guid userId, IEnumerable<Guid> siteIds, string permissionId)
    {
        if (!_featureFlagService.IsFineGrainedAuthEnabled)
        {
            return siteIds.ToList();
        }

        var sw = Stopwatch.StartNew();

        // Short-lived caching is utilized here to reduce multiple callers performing the same eval multiple times
        // within a short time frame
        var evalKey = $"auth-siteIds-for-userId-[{userId}]-permId-[{permissionId}]";

        var sitesFiltered = await _cache.GetOrCreateLockedAsync(evalKey, async ci =>
        {
            ci.SetAbsoluteExpiration(TimeSpan.FromMinutes(1));

            var ids = new ConcurrentBag<Guid>();

            await Parallel.ForEachAsync(siteIds, new ParallelOptions { MaxDegreeOfParallelism = 10 }, async (siteId, _) =>
            {
                try
                {
                    if (await _accessControlService.CanAccessSite(userId, permissionId, siteId))
                    {
                        ids.Add(siteId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "AuthFilterSites error for site Id '{SiteId}'", siteId);
                }
            });

            return ids.ToList();
        });

        _logger.LogTrace("==== SiteServiceWithAuthFiltering: AuthFilterSiteIds EVAL: {Count:0} site(s) resolved for {EvalKey} took {Time} ({ElapsedMilliseconds:0}ms) ====", sitesFiltered.Count, evalKey, sw, sw.ElapsedMilliseconds);

        return sitesFiltered;
    }
}
