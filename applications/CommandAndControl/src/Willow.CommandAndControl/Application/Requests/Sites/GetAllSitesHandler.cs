namespace Willow.CommandAndControl.Application.Requests.Sites;

using Microsoft.Extensions.Caching.Memory;

internal static class GetAllSitesHandler
{
    public static async Task<Ok<IList<SiteDto>>> HandleAsync(IAdxService adxService, IMemoryCache memoryCache, CancellationToken cancellationToken)
    {
        var result = await memoryCache.GetOrCreateAsync<IList<SiteDto>>("All Sites", async entry =>
        {
            return (await adxService.QueryAsync<SiteDto>(@"
				ActiveTwins
				| project SiteId = tostring(Location.SiteId), SiteName = tostring(Location.SiteName)
				| distinct SiteId, SiteName",
                cancellationToken)).Where(x => x.SiteId != string.Empty).ToList();
        });
        return TypedResults.Ok(result);
    }
}
