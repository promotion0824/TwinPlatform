namespace ConnectorCore.Requests.Connector;

using ConnectorCore.Data;
using ConnectorCore.Entities;
using ConnectorCore.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

internal class ExportConnectorStateHandler
{
    public static async Task<Results<Ok, UnauthorizedHttpResult, BadRequest<ProblemDetails>, NotFound>> HandleAsync([FromRoute] Guid customerId, [FromServices] IConnectorCoreDbContext dbContext, [FromServices] IConnectorsService connectorsService)
    {
        var connectors = await dbContext.Connectors.Where(c => c.ClientId == customerId).ToListAsync();
        if (!connectors.Any())
        {
            return TypedResults.NotFound();
        }

        foreach (var connector in connectors)
        {
            await connectorsService.NotifyStateEventAsync(connector.ToConnectorEntity());
            await connectorsService.PublishToServiceBusAsync(connector.ToConnectorEntity(), ConnectorUpdateStatus.Export);
        }

        return TypedResults.Ok();
    }
}
