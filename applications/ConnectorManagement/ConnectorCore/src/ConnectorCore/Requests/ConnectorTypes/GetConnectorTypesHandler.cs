namespace ConnectorCore.Requests.ConnectorTypes;

using ConnectorCore.Data;
using ConnectorCore.Entities;
using ConnectorCore.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

internal class GetConnectorTypesHandler
{
    internal static async Task<Results<Ok<List<ConnectorTypeEntity>>, BadRequest<ProblemDetails>, NotFound>> HandleAsync([FromServices] IConnectorCoreDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        var result = await dbContext.ConnectorTypes.Select(
            x => new ConnectorTypeEntity
            {
                Id = x.Id,
                Name = x.Name,
                DeviceMetadataSchemaId = x.DeviceMetadataSchemaId,
                ConnectorConfigurationSchemaId = x.ConnectorConfigurationSchemaId,
                PointMetadataSchemaId = x.PointMetadataSchemaId,
                ScanConfigurationSchemaId = x.ScanConfigurationSchemaId,
            }).ToListAsync(cancellationToken);
        return TypedResults.Ok(result);
    }
}
