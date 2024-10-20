namespace ConnectorCore.Requests.Scan;

using ConnectorCore.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

internal class StopScanHandler
{
    public static async Task<Results<Ok, BadRequest<ProblemDetails>, NotFound>> HandleAsync([FromRoute] Guid connectorId, [FromRoute] Guid scanId, [FromServices] IScanService scanService)
    {
        await scanService.StopAsync(connectorId, scanId);
        return TypedResults.Ok();
    }
}
