namespace ConnectorCore.Requests.ConnectorTypes;

using ConnectorCore.Data;
using ConnectorCore.Entities;
using ConnectorCore.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

internal class GetConnectorTypesByIdHandler
{
    internal static async Task<Results<Ok<ConnectorTypeEntity>, BadRequest<ProblemDetails>, NotFound>> HandleAsync([FromServices] IConnectorCoreDbContext dbContext,
        [FromRoute] Guid connectorTypeId,
        CancellationToken cancellationToken = default)
    {
        var result = await dbContext.ConnectorTypes.Where(x => x.Id == connectorTypeId)
            .Select(
            x => new ConnectorTypeEntity
            {
                Id = x.Id,
                Name = x.Name,
                DeviceMetadataSchemaId = x.DeviceMetadataSchemaId,
                ConnectorConfigurationSchemaId = x.ConnectorConfigurationSchemaId,
                PointMetadataSchemaId = x.PointMetadataSchemaId,
                ScanConfigurationSchemaId = x.ScanConfigurationSchemaId,
            }).FirstOrDefaultAsync(cancellationToken);
        if (result is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }
}
