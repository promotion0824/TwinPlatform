namespace ConnectorCore.Requests.Scan;

using ConnectorCore.Models;
using ConnectorCore.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

internal class PatchScanHandler
{
    public static async Task<Results<Ok, BadRequest<ProblemDetails>, NotFound>> HandleAsync([FromRoute] Guid connectorId, [FromRoute] Guid scanId, [AsParameters] PatchScanRequest request, [FromServices] IScanService scanService)
    {
        if (request.IsEmpty())
        {
            return TypedResults.BadRequest(new ProblemDetails { Title = "At least one parameter should be provided" });
        }

        await scanService.PatchAsync(connectorId, scanId, request.Status, request.ErrorMessage, request.ErrorCount, request.StartTime, request.EndTime);
        return TypedResults.Ok();
    }
}
