namespace Willow.LiveData.Core.Features.Telemetry;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Willow.LiveData.Core.Common;
using Willow.LiveData.Core.Features.Telemetry.DTOs;
using Willow.LiveData.Core.Features.Telemetry.Interfaces;

internal class GetBinaryDataHandler
{
    public static async Task<Results<Ok<IEnumerable<TelemetryBinaryResponseData>>, UnauthorizedHttpResult, NotFound>> HandleAsync(
        [FromQuery(Name = "clientId")] Guid? clientId,
        [FromRoute] string twinId,
        [FromQuery(Name = "start"), BindRequired]
        DateTime start,
        [FromQuery(Name = "end"), BindRequired]
        DateTime end,
        [FromQuery(Name = "interval")] string selectedInterval,
        ITelemetryService telemetryService)
    {
        var interval =
            string.IsNullOrWhiteSpace(selectedInterval) || !TimeSpan.TryParse(selectedInterval, out var parsedInterval)
                ? (TimeSpan?)null
                : parsedInterval;

        var result = await telemetryService.GetTelemetryDataByTwinIdAsync<TelemetryBinaryResponseData>(clientId,
            start,
            end,
            Constants.Binary,
            twinId,
            interval);

        if (result is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }
}
