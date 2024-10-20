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

internal class GetConnectorByIdHandler
{
    private static async Task<Results<Ok<ConnectorEntity>, BadRequest<ProblemDetails>, NotFound>> HandleAsync(Guid? siteId, Guid connectorId, [FromServices] IAppCache appCache, [FromServices] IConnectorCoreDbContext dbContext, [FromServices] IOptions<CacheOptions> cacheOptions)
    {
        var connector = await appCache.GetOrAddAsync(string.Format(ConnectorConst.SingleConnectorCacheKey, connectorId.ToString()),
            async cache =>
            {
                cache.SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(cacheOptions.Value.ConnectorsCacheTimeoutInMinutes));
                return (await dbContext.Connectors.FirstOrDefaultAsync(x => x.Id == connectorId))?.ToConnectorEntity();
            });
        if (connector == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(connector);
    }

    public static Task<Results<Ok<ConnectorEntity>, BadRequest<ProblemDetails>, NotFound>> HandleWithSiteIdAsync([FromRoute] Guid siteId, [FromRoute] Guid connectorId, IAppCache appCache, IConnectorCoreDbContext dbContext, IOptions<CacheOptions> cacheOptions) => HandleAsync(siteId, connectorId, appCache, dbContext, cacheOptions);

    public static Task<Results<Ok<ConnectorEntity>, BadRequest<ProblemDetails>, NotFound>> HandleWithNoSiteIdAsync([FromRoute] Guid connectorId, IAppCache appCache, IConnectorCoreDbContext dbContext, IOptions<CacheOptions> cacheOptions) => HandleAsync(null, connectorId, appCache, dbContext, cacheOptions);
}
