namespace Willow.LiveData.Core.Features.TimeSeries;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Willow.LiveData.Core.Features.TimeSeries.Extensions;
using Willow.LiveData.Core.Features.TimeSeries.Models;
using Willow.LiveData.Pipeline;

internal class PostTelemetryHandler
{
    public static async Task<Results<Accepted, UnauthorizedHttpResult, BadRequest>> HandleAsync([FromBody]IncomingTelemetry[] telemetry, [FromServices]ISender sender)
    {
        if (telemetry?.Length == 0)
        {
            return TypedResults.BadRequest();
        }

        // Filter out telemetry that does not have a digital twin ID or external ID.
        IEnumerable<Telemetry> unifiedTelemetry = telemetry.Where(t => !string.IsNullOrWhiteSpace(t.DtId) || !string.IsNullOrWhiteSpace(t.ExternalId)).Select(t => t.MapTo());

        await sender.SendAsync(unifiedTelemetry);

        return TypedResults.Accepted(null as string);
    }
}
