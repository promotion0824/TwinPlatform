namespace Willow.LiveData.Core.Features.Telemetry;

using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Willow.LiveData.Core.Features.Telemetry.Interfaces;

internal class GetAggregateHandler
{
    public static async Task<Results<NotFound, UnauthorizedHttpResult, ProblemHttpResult, ContentHttpResult>>
        HandleAsync(
            [FromQuery(Name = "clientId")] Guid? clientId,
            [FromQuery(Name = "start"), BindRequired]
            DateTime start,
            [FromQuery(Name = "end"), BindRequired]
            DateTime end,
            [FromQuery(Name = "twinIds"), BindRequired]
            string[] twinIds,
            [FromQuery(Name = "twinTypes"), BindRequired]
            string[] twinTypes,
            [FromQuery(Name = "interval")] string selectedInterval,
            ITelemetryService telemetryService)
    {
        var interval =
            string.IsNullOrWhiteSpace(selectedInterval) || !TimeSpan.TryParse(selectedInterval, out var parsedInterval)
                ? (TimeSpan?)null
                : parsedInterval;
        if (twinTypes.Length != twinIds.Length)
        {
            return TypedResults.Problem($"Number of {nameof(twinIds)} should be the same as {nameof(twinTypes)}");
        }

        var result = await telemetryService.GetTelemetryDataByTwinIdAsync(clientId,
            start,
            end,
            twinIds,
            twinTypes,
            interval);

        if (result == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Text(JsonConvert.SerializeObject(result, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Include,
        }),
            MediaTypeNames.Application.Json,
            statusCode: StatusCodes.Status200OK);
    }
}
