namespace Willow.LiveData.Core.Features.TimeSeries;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Willow.LiveData.Core.Domain;
using Willow.LiveData.Core.Features.Telemetry.Interfaces;

internal class GetLatestHandler
{
    public static async Task<Results<Ok<IEnumerable<TelemetryData>>, UnauthorizedHttpResult, NotFound>>
        HandleAsync([FromRoute(Name = "twinId"), BindRequired]
            string twinId,
            ITelemetryService telemetryService)
    {
        var result = await telemetryService.GetLastTelemetryAsync([twinId]);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }
}
