namespace Willow.LiveData.Core.Features.Telemetry;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData;
using Willow.LiveData.Core.Features.Telemetry.Interfaces;

internal class GetRawTelemetryHandler
{
    public static async Task<Results<Ok<GetTelemetryResult>, UnauthorizedHttpResult, NotFound>> HandleAsync(
        [FromQuery(Name = "clientId")] Guid? clientId,
        [FromQuery(Name = "start"), BindRequired]
        DateTime start,
        [FromQuery(Name = "end"), BindRequired]
        DateTime end,
        [FromQuery(Name = "twinIds"), BindRequired]
        string[] twinIds,
        [FromQuery(Name = "pageSize")] int pageSize,
        [FromQuery(Name = "continuationToken")]
        string continuationToken,
        ITelemetryService telemetryService)
    {
        var result = await telemetryService.GetTelemetryAsync(clientId,
            twinIds,
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
