namespace ConnectorCore.Requests.Scan;

using ConnectorCore.Entities;
using ConnectorCore.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

internal class CreateScanHandler
{
    public static async Task<Results<Ok<ScanEntity>, BadRequest<ProblemDetails>, NotFound>> HandleAsync([FromRoute] Guid connectorId, [FromBody] ScanEntity scanEntity, [FromServices] IScanService scanService)
    {
        scanEntity.ConnectorId = connectorId;
        var result = await scanService.CreateAsync(scanEntity);
        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }
}
