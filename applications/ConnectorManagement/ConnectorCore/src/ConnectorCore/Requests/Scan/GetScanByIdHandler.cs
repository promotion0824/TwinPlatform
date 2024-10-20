namespace ConnectorCore.Requests.Scan;

using ConnectorCore.Entities;
using ConnectorCore.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

internal class GetScanByIdHandler
{
    public static async Task<Results<Ok<ScanEntity>, BadRequest<ProblemDetails>, NotFound>> HandleAsync([FromRoute] Guid scanId, [FromServices] IScanService scanService)
    {
        var result = await scanService.GetById(scanId);
        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }
}
