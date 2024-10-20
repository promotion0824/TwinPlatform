namespace ConnectorCore.Requests.Connector;

using ConnectorCore.Data;
using ConnectorCore.Entities;
using ConnectorCore.Entities.Validators;
using ConnectorCore.Services;
using LazyCache;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

internal class PostConnectorHandler
{
    public static async Task<Results<Created<ConnectorEntity>, BadRequest<ProblemDetails>, NotFound>> HandleAsync([FromForm] ConnectorEntity connector, [FromServices] IConnectorCoreDbContext dbContext, [FromServices] IJsonSchemaValidator jsonSchemaValidator, [FromServices] IConnectorsService connectorsService, [FromServices] IAppCache appCache)
    {
        if (connector?.ConnectorTypeId == null)
        {
            throw new ArgumentException("Check Connector under the scheme can not be made. ConnectorTypeId field is not filled.", nameof(ConnectorEntity.Configuration));
        }

        var connectorType = await dbContext.ConnectorTypes.FirstOrDefaultAsync(x => x.Id == connector.ConnectorTypeId);
        var columns = await dbContext.SchemaColumns
            .Where(x => x.SchemaId == connectorType.ConnectorConfigurationSchemaId)
            .Select(x => new SchemaColumnEntity
            {
                DataType = x.DataType,
                Id = x.Id,
                IsRequired = x.IsRequired,
                Name = x.Name,
                SchemaId = x.SchemaId,
                UnitOfMeasure = x.UnitOfMeasure,
            }).ToListAsync();

        if (!jsonSchemaValidator.IsValid(columns, connector.Configuration, out var errors))
        {
            return TypedResults.BadRequest(new ProblemDetails()
            {
                Title = "Data validation error",
                Detail =
                    "Connector's metadata should comply relevant schema: " + string.Join("\n", errors),
            });
        }

        if (connector.Id == Guid.Empty)
        {
            connector.Id = Guid.NewGuid();
        }

        if (!string.Equals(connector.ConnectionType, "iotedge", StringComparison.InvariantCultureIgnoreCase))
        {
            await connectorsService.RegisterDevice(connector, false);
        }

        dbContext.Connectors.Add(connector.ToConnector());
        await dbContext.SaveChangesAsync();

        if (connector.SiteId != Guid.Empty)
        {
            appCache.Remove(string.Format(ConnectorConst.AllConnectorsCacheKey, connector.SiteId.ToString()));
        }

        appCache.Remove(string.Format(ConnectorConst.SingleConnectorCacheKey, connector.Id.ToString()));
        appCache.Remove(string.Format(ConnectorConst.AllConnectorsCacheKey, Guid.Empty.ToString()));

        await connectorsService.UpsertConnectorApplication(connector);
        await connectorsService.NotifyStateEventAsync(connector);
        await connectorsService.PublishToServiceBusAsync(connector, ConnectorUpdateStatus.New);

        return TypedResults.Created(string.Empty, connector);
    }
}
