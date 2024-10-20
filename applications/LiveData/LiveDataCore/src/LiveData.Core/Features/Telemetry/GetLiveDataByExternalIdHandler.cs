namespace Willow.LiveData.Core.Features.Telemetry;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Willow.LiveData.Core.Domain;
using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData;

internal class GetLiveDataByExternalIdHandler
{
    public static async Task<Results<Ok<IEnumerable<TimeSeriesAnalogData>>, UnauthorizedHttpResult, NotFound>> HandleAsync(
        [FromRoute] Guid connectorId,
        [FromRoute] string externalId,
        [FromQuery(Name = "clientId")] Guid? clientId,
        [FromQuery(Name = "startUtc"), BindRequired] DateTime startUtc,
        [FromQuery(Name = "endUtc"), BindRequired] DateTime endUtc,
        [FromQuery(Name = "interval")] string selectedInterval,
        IAdxLiveDataService telemetryService)
    {
        var interval = string.IsNullOrWhiteSpace(selectedInterval) ||
                       !TimeSpan.TryParse(selectedInterval, out var parsedInterval)
            ? (TimeSpan?)null
            : parsedInterval;

        var result = await telemetryService.GetTimeSeriesDataByExternalIdAsync(
            connectorId,
            externalId,
            clientId,
            startUtc,
            endUtc,
            interval);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }
}
