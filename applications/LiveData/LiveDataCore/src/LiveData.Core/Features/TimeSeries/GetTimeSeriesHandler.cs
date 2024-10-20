namespace Willow.LiveData.Core.Features.TimeSeries;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData;
using Willow.LiveData.Core.Features.Telemetry.Interfaces;

internal class GetTimeseriesHandler
{
    public static async Task<Results<Ok<GetTelemetryResult>, UnauthorizedHttpResult, NotFound>> HandleAsync(
        [FromQuery(Name = "start"), BindRequired]
        DateTime start,
        [FromQuery(Name = "end"), BindRequired]
        DateTime end,
        [FromRoute(Name = "twinId"), BindRequired]
        string twinId,
        [FromQuery(Name = "pageSize")] int pageSize,
        [FromQuery(Name = "continuationToken")]
        string continuationToken,
        ITelemetryService telemetryService)
    {
        var result = await telemetryService.GetTelemetryAsync(null,
            [twinId],
            start,
            end,
            pageSize,
            continuationToken);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }
}
