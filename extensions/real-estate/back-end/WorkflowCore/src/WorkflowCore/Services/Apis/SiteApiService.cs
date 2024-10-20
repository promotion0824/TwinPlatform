using LazyCache;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using WorkflowCore.Models;

namespace WorkflowCore.Services.Apis;

public interface ISiteApiService
{
    Task<Site> GetExtendedSite(Guid siteId);
    Task<Site> GetCachedExtendedSite(Guid siteId);
}


public class SiteApiService : ISiteApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAppCache _appCache;

    public SiteApiService(IHttpClientFactory httpClientFactory, IAppCache appCache)
    {
        _httpClientFactory = httpClientFactory;
        _appCache = appCache;
    }

    /// <summary>
    /// Get site details with features object
    /// </summary>
    /// <param name="siteId"></param>
    /// <returns></returns>
    public async Task<Site> GetExtendedSite(Guid siteId)
    {
        using (var client = _httpClientFactory.CreateClient(ApiServiceNames.SiteCore))
        {
            var response = await client.GetAsync($"sites/{siteId}/extend");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<Site>();
        }
    }

    /// <summary>
    /// Get cached site details with features object
    /// Note: for now the cache is set to 5 minutes, but we should consider increase this time once we have a better understanding of
    /// how often the site configuration are updated
    /// </summary>
    /// <param name="siteId"></param>
    /// <returns></returns>
    public async Task<Site> GetCachedExtendedSite(Guid siteId)
    {
        return await _appCache.GetOrAddAsync($"{nameof(GetCachedExtendedSite)}-{siteId}", async (cache) =>
        {
            cache.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await GetExtendedSite(siteId);
        });
    }
}
