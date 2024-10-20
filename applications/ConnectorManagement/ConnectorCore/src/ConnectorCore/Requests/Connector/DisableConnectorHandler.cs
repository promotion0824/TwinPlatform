namespace ConnectorCore.Requests.Connector;

using ConnectorCore.Data;
using ConnectorCore.Entities;
using ConnectorCore.Services;
using LazyCache;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

internal class DisableConnectorHandler
{
    public static async Task<Results<Ok, BadRequest<ProblemDetails>, NotFound>> HandleAsync([FromRoute] Guid connectorId, [FromServices] IConnectorCoreDbContext dbContext, [FromServices] IAppCache appCache, [FromServices] IConnectorsService connectorsService)
    {
        var connector = await dbContext.Connectors.FirstOrDefaultAsync(c => c.Id == connectorId);
        if (connector == null)
        {
            return TypedResults.NotFound();
        }

        if (connector.IsArchived == true)
        {
            TypedResults.UnprocessableEntity(new ValidationProblemDetails
            {
                Title = "Data validation error",
                Detail = "Cannot disable an archived connector.",
            });
        }

        connector.IsEnabled = false;
        await dbContext.SaveChangesAsync();

        // Remove the entry from cache to force it to refresh
        appCache.Remove(string.Format(ConnectorConst.SingleConnectorCacheKey, connectorId.ToString()));
        appCache.Remove(string.Format(ConnectorConst.AllConnectorsCacheKey, Guid.Empty.ToString()));

        var connectorEntity = connector.ToConnectorEntity();
        await connectorsService.NotifyStateEventAsync(connectorEntity);
        await connectorsService.PublishToServiceBusAsync(connectorEntity, connector.IsEnabled ? ConnectorUpdateStatus.Enable : ConnectorUpdateStatus.Disable);

        return TypedResults.Ok();
    }
}
