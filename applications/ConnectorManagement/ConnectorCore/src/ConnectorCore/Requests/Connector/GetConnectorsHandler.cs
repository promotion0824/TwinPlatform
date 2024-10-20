namespace ConnectorCore.Requests.Connector;

using ConnectorCore.Data;
using ConnectorCore.Entities;
using ConnectorCore.Models;
using LazyCache;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

internal class GetConnectorsHandler
{
    private static async Task<Results<Ok<List<ConnectorEntity>>, BadRequest<ProblemDetails>, NotFound>> HandleAsync(Guid? siteId, bool? includePointsCount, [FromServices] IConnectorCoreDbContext dbContext, [FromServices] IAppCache appCache, [FromServices] IOptions<CacheOptions> cacheOptions)
    {
        var connectors = await appCache.GetOrAddAsync(string.Format(ConnectorConst.AllConnectorsCacheKey, siteId.ToString()),
            async cache =>
            {
                cache.SetAbsoluteExpiration(
                    DateTimeOffset.UtcNow.AddMinutes(cacheOptions.Value.ConnectorsCacheTimeoutInMinutes));
                return await dbContext.Connectors
                    .Where(c => siteId == null || c.SiteId == siteId)
                    .Select(x => x.ToConnectorEntity())
                    .ToListAsync();
            });
        if (!connectors.Any())
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(connectors);
    }

    public static Task<Results<Ok<List<ConnectorEntity>>, BadRequest<ProblemDetails>, NotFound>> HandleWithSiteIdAsync([FromRoute] Guid siteId, IConnectorCoreDbContext dbContext, IAppCache appCache, IOptions<CacheOptions> cacheOptions) => HandleAsync(siteId, null, dbContext, appCache, cacheOptions);

    public static Task<Results<Ok<List<ConnectorEntity>>, BadRequest<ProblemDetails>, NotFound>> HandleWithNoSiteIdAsync(IConnectorCoreDbContext dbContext, IAppCache appCache, IOptions<CacheOptions> cacheOptions) => HandleAsync(null, null, dbContext, appCache, cacheOptions);
}
