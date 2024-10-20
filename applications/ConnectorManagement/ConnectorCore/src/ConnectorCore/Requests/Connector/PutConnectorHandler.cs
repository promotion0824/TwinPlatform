namespace ConnectorCore.Requests.Connector;

using ConnectorCore.Data;
using ConnectorCore.Entities;
using ConnectorCore.Entities.Validators;
using ConnectorCore.Services;
using LazyCache;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

internal class PutConnectorHandler
{
    public static async Task<Results<Ok<ConnectorEntity>, UnprocessableEntity<ProblemDetails>, BadRequest<ProblemDetails>, NotFound>> HandleAsync([FromForm] ConnectorEntity connector, [FromServices] IConnectorCoreDbContext dbContext, [FromServices] IJsonSchemaValidator jsonSchemaValidator, [FromServices] IConnectorsService connectorsService, [FromServices] IAppCache appCache)
    {
        var existingConnector = await dbContext.Connectors.AsNoTracking().FirstOrDefaultAsync(c => c.Id == connector.Id);
        if (existingConnector == null)
        {
            return TypedResults.NotFound();
        }

        if (existingConnector.IsArchived == true)
        {
            return TypedResults.UnprocessableEntity(
                new ProblemDetails()
                {
                    Title = "Data validation error",
                    Detail = "Cannot update connector details if archived.",
                });
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

        dbContext.Connectors.Update(connector.ToConnector());
        await dbContext.SaveChangesAsync();

        // Remove the entry from cache to force it to refresh
        appCache.Remove(string.Format(ConnectorConst.SingleConnectorCacheKey, connector.Id.ToString()));
        appCache.Remove(string.Format(ConnectorConst.AllConnectorsCacheKey, Guid.Empty.ToString()));

        await connectorsService.NotifyStateEventAsync(connector);

        return TypedResults.Ok(connector);
    }
}
