namespace ConnectorCore.Requests.Scan;

using System;
using System.Threading.Tasks;
using ConnectorCore.Entities;
using ConnectorCore.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

internal class GetScansByConnectorIdHandler
{
    public static async Task<Results<Ok<List<ScanEntity>>, BadRequest<ProblemDetails>, NotFound>> HandleAsync([FromRoute] Guid connectorId, [FromServices] IScanService scanService)
    {
        var result = await scanService.GetByConnectorIdAsync(connectorId);
        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }
}
