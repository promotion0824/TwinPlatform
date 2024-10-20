namespace Willow.LiveData.Core.Features.Telemetry;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Willow.LiveData.Core.Features.Telemetry.DTOs;
using Willow.LiveData.Core.Features.Telemetry.Interfaces;

internal class GetTrendLogHandler
{
    public static async Task<Results<Ok<TelemetryRawResult>, UnauthorizedHttpResult, NotFound>> HandleAsync(
        [FromQuery(Name = "clientId")] Guid? clientId,
        string twinId,
        [FromQuery(Name = "start"), BindRequired] DateTime start,
        [FromQuery(Name = "end"), BindRequired] DateTime end,
        [FromQuery(Name = "continuationToken")] string continuationToken,
        [FromQuery] int pageSize,
        ITelemetryService telemetryService)
    {
        var result = await telemetryService.GetTelemetryRawDataByTwinIdAsync(clientId,
            twinId,
            start,
            end,
            continuationToken,
            pageSize);

        if (result is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }
}
