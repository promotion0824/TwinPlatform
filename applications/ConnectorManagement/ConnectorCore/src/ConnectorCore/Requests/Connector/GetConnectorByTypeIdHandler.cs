namespace ConnectorCore.Requests.Connector;

using ConnectorCore.Data;
using ConnectorCore.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

internal class GetConnectorByTypeIdHandler
{
    private static async Task<Results<Ok<ConnectorEntity>, BadRequest<ProblemDetails>, NotFound, Conflict<string>>> HandleAsync(Guid? siteId, Guid connectorTypeId, [FromServices] IConnectorCoreDbContext dbContext)
    {
        var connectors = await dbContext.Connectors.Where(c => c.ConnectorTypeId == connectorTypeId && (siteId == null || c.SiteId == siteId)).ToListAsync();
        if (!connectors.Any())
        {
            return TypedResults.NotFound();
        }

        if (connectors.Count > 1)
        {
            return TypedResults.Conflict($"There are more than one connector of specified type {connectorTypeId} on the site {siteId}");
        }

        return TypedResults.Ok(connectors.First().ToConnectorEntity());
    }

    public static Task<Results<Ok<ConnectorEntity>, BadRequest<ProblemDetails>, NotFound, Conflict<string>>> HandleWithSiteIdAsync([FromRoute] Guid siteId, [FromRoute] Guid connectorTypeId, IConnectorCoreDbContext dbContext) => HandleAsync(siteId, connectorTypeId, dbContext);

    public static Task<Results<Ok<ConnectorEntity>, BadRequest<ProblemDetails>, NotFound, Conflict<string>>> HandleWithNoSiteIdAsync([FromRoute] Guid connectorTypeId, IConnectorCoreDbContext dbContext) => HandleAsync(null, connectorTypeId, dbContext);
}
