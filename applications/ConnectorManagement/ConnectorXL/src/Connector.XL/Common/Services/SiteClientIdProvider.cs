namespace Connector.XL.Common.Services;

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

internal class SiteClientIdProvider : ISiteClientIdProvider
{
    private readonly ISitesService sitesService;
    private readonly IMemoryCache memoryCache;

    public SiteClientIdProvider(ISitesService sitesService, IMemoryCache memoryCache)
    {
        this.sitesService = sitesService;
        this.memoryCache = memoryCache;
    }

    public async Task<Guid> GetClientIdForSiteAsync(Guid siteId)
    {
        return await memoryCache.GetOrCreateAsync($"client_id_for_site_{siteId}",
            async (entry) =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(4);
                var siteDto = await sitesService.GetSiteAsync(siteId);
                return siteDto.CustomerId;
            });
    }
}
