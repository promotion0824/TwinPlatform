namespace Willow.LiveData.Core.Features.Telemetry;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Willow.LiveData.Core.Features.Telemetry.DTOs;
using Willow.LiveData.Core.Features.Telemetry.Interfaces;

internal class GetTrendlogsHandler
{
    public static async Task<Results<Ok<IEnumerable<TelemetryRawResultMultiple>>, UnauthorizedHttpResult, NotFound>>
        HandleAsync(
            [FromQuery(Name = "clientId")] Guid? clientId,
            [FromQuery(Name = "start"), BindRequired] DateTime start,
            [FromQuery(Name = "end"), BindRequired] DateTime end,
            [FromQuery(Name = "twinId"), BindRequired] string[] twinIds,
            ITelemetryService telemetryService)
    {
        var result = await telemetryService.GetTelemetryRawDataAsync(clientId, start, end, twinIds);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    [Obsolete("Use HandleAsync instead")]
    public static async Task<Results<Ok<IEnumerable<TelemetryRawResultMultiple>>, UnauthorizedHttpResult, NotFound>>
        HandleWithSiteIdAsync(
            [FromQuery(Name = "clientId")] Guid? clientId,
            Guid siteId,
            [FromQuery(Name = "start"), BindRequired] DateTime start,
            [FromQuery(Name = "end"), BindRequired] DateTime end,
            [FromQuery(Name = "twinId"), BindRequired] string[] twinIds,
            ITelemetryService telemetryService)
    {
        var result = await telemetryService.GetTelemetryRawDataBySiteIdAsync(clientId, siteId, start, end, twinIds);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }
}
