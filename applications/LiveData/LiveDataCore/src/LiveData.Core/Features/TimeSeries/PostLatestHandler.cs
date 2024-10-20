namespace Willow.LiveData.Core.Features.TimeSeries;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Willow.LiveData.Core.Domain;
using Willow.LiveData.Core.Features.Telemetry.Interfaces;

internal class PostLatestHandler
{
    public static async Task<Results<Ok<IEnumerable<TelemetryData>>, UnauthorizedHttpResult, NotFound>>
        HandleAsync([FromBody]string[] twinIds, ITelemetryService telemetryService)
    {
        var validTwinIds = twinIds.Where(twinId => !string.IsNullOrWhiteSpace(twinId));
        var result = await telemetryService.GetLastTelemetryAsync(twinIds);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(result);
    }
}
