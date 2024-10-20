namespace Willow.LiveData.Core.Features.TimeSeries;

internal static class Endpoints
{
    public static RouteGroupBuilder MapTimeseries(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("time-series")
            .WithTags("Time Series")
            .WithSummary("Read and write time series data.");

        group.MapGet("{twinId}", GetTimeseriesHandler.HandleAsync)
            .WithSummary("Retrieves time series data for given twin ID.");

        group.MapPost("ids", PostTimeseriesHandler.HandleAsync)
            .WithSummary("Retrieves time series data for the given twin IDs.");

        group.MapGet("{twinId}/latest", GetLatestHandler.HandleAsync)
            .WithSummary("Retrieves the latest time series data for the given twin ID.");

        group.MapPost("ids/latest", PostLatestHandler.HandleAsync)
            .WithSummary("Retrieves the latest time series data for the given twin IDs.");

        group.MapPost(string.Empty, PostTelemetryHandler.HandleAsync)
            .WithSummary("Send telemetry to Willow");

        return group;
    }
}
