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

internal class GetAnalogDataBulkHandler
{
    public static async Task<Results<Ok<Dictionary<string, IEnumerable<TelemetryAnalogResponseData>>>, UnauthorizedHttpResult, NotFound>> HandleAsync(
        [FromQuery(Name = "clientId")] Guid? clientId,
        [FromQuery(Name = "start"), BindRequired] DateTime start,
        [FromQuery(Name = "end"), BindRequired] DateTime end,
        [FromQuery(Name = "twinId"), BindRequired] string[] twinIds,
        [FromQuery(Name = "interval")] string selectedInterval,
        ITelemetryService telemetryService)
    {
        var interval = string.IsNullOrWhiteSpace(selectedInterval) || !TimeSpan.TryParse(selectedInterval, out var parsedInterval) ? (TimeSpan?)null : parsedInterval;

        var result = await telemetryService.GetTelemetryDataByTwinIdAsync<TelemetryAnalogResponseData>(clientId,
            start,
            end,
            Constants.Analog,
            twinIds,
            interval);

        if (result is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }
}
