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

internal class GetLastTrendlogsHandler
{
    public static async Task<Results<Ok<IEnumerable<TelemetryRawData>>, UnauthorizedHttpResult, NotFound>>
        HandleAsync([FromQuery(Name = "clientId")] Guid? clientId,
            [FromQuery(Name = "twinId"), BindRequired]
            string[] twinIds,
            ITelemetryService telemetryService)
    {
        var validTwinIds = twinIds.Where(twinId => !string.IsNullOrWhiteSpace(twinId));
        var result = await telemetryService.GetLastTelemetryRawDataAsync(clientId, validTwinIds);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }

    [Obsolete("Use HandleAsync instead")]
    public static async Task<Results<Ok<IEnumerable<TelemetryRawData>>, UnauthorizedHttpResult, NotFound>>
        HandleWithSiteIdAsync([FromQuery(Name = "clientId")] Guid? clientId,
            Guid siteId,
            [FromQuery(Name = "twinId"), BindRequired]
            string[] twinIds,
            ITelemetryService telemetryService)
    {
        var result =
            await telemetryService.GetLastTelemetryRawDataBySiteIdAsync(clientId,
                siteId,
                twinIds.Any() ? twinIds : null);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }
}
