namespace ConnectorCore.Requests.Connector;

using ConnectorCore.Data;
using ConnectorCore.Entities;
using ConnectorCore.Services;
using LazyCache;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

internal class ArchiveConnectorHandler
{
    public static async Task<Results<Ok, BadRequest<ProblemDetails>, NotFound>> HandleAsync([FromRoute] Guid connectorId, bool archive, [FromServices] IConnectorCoreDbContext dbContext, [FromServices] IAppCache appCache, [FromServices]IConnectorsService connectorsService, [FromServices] IIotRegistrationService iotRegistrationService)
    {
        var connector = await dbContext.Connectors.FirstOrDefaultAsync(c => c.Id == connectorId);
        if (connector == null)
        {
            return TypedResults.NotFound();
        }

        connector.IsArchived = archive;
        connector.IsEnabled = false;
        await dbContext.SaveChangesAsync();

        // Remove the entry from cache to force it to refresh
        appCache.Remove(string.Format(ConnectorConst.SingleConnectorCacheKey, connectorId.ToString()));
        appCache.Remove(string.Format(ConnectorConst.AllConnectorsCacheKey, Guid.Empty.ToString()));
        var connectorEntity = connector.ToConnectorEntity();
        await connectorsService.NotifyStateEventAsync(connectorEntity);
        await connectorsService.PublishToServiceBusAsync(connectorEntity, ConnectorUpdateStatus.Archive);

        if (connectorEntity != null && !string.Equals(connectorEntity.ConnectionType, "iotedge"))
        {
            var connectionString = await iotRegistrationService.GetConnectionString(connectorEntity.ClientId);
            await iotRegistrationService.DeleteDevice(connectorEntity.RegistrationId, connectionString);
        }

        return TypedResults.Ok();
    }
}
