namespace Willow.LiveData.Core.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Willow.LiveData.Core.Features.Telemetry;
using Willow.LiveData.Core.Infrastructure.Filters;

internal static class Endpoints
{
    public static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder app)
    {
        var telemetryEndpoints = app.MapGroup("telemetry").WithTags("Telemetry");

        telemetryEndpoints.MapGet("point/analog/{twinId}", GetAnalogDataHandler.HandleAsync)
            .WithSummary("Retrieves analog telemetry data for given twinId.");
        telemetryEndpoints.MapGet("point/analog", GetAnalogDataBulkHandler.HandleAsync)
            .WithSummary("Retrieves analog telemetry data for given twinIds.");
        telemetryEndpoints.MapGet("point/binary/{twinId}", GetBinaryDataHandler.HandleAsync)
            .WithSummary("Retrieves binary telemetry data for given twinId.");
        telemetryEndpoints.MapGet("point/binary", GetBinaryDataBulkHandler.HandleAsync)
            .WithSummary("Retrieves binary telemetry data for given twinIds.");
        telemetryEndpoints.MapGet("point/multistate/{twinId}", GetMultiStateDataHandler.HandleAsync)
            .WithSummary("Retrieves multistate telemetry data for given twinId.");
        telemetryEndpoints.MapGet("point/multistate", GetMultiStateDataBulkHandler.HandleAsync)
            .WithSummary("Retrieves multistate telemetry data for given twinIds.");
        telemetryEndpoints.MapGet("point/sum/{twinId}", GetSumDataHandler.HandleAsync)
            .WithSummary("Retrieves sum telemetry data for given twinId.");
        telemetryEndpoints.MapGet("point/sum", GetSumDataBulkHandler.HandleAsync)
            .WithSummary("Retrieves sum data of all rows by twinIds");
        telemetryEndpoints.MapGet("point/aggregate", GetAggregateHandler.HandleAsync)
            .WithSummary("Retrieves summary by point types.");
        telemetryEndpoints.MapGet("points/{twinId}/trendlog", GetTrendLogHandler.HandleAsync)
            .WithSummary("Retrieves raw data of all rows by twinId.")
            .AddEndpointFilter<PageSizeValidationFilter>();

        telemetryEndpoints.MapGet("sites/{siteId:guid}/trendlogs", GetTrendlogsHandler.HandleWithSiteIdAsync)
            .WithSummary("Retrieves raw data of all rows by twinIds for given siteId.")
            .WithOpenApi(openApiOperation =>
            {
                openApiOperation.Deprecated = true;
                return openApiOperation;
            });
        telemetryEndpoints.MapGet("trendlogs", GetTrendlogsHandler.HandleAsync)
            .WithSummary("Retrieves raw data of all rows by twinIds.")
            .AddEndpointFilter<RequiredTwinIdValidationFilter>();

        telemetryEndpoints.MapGet("sites/{siteId:guid}/lastTrendlogs", GetLastTrendlogsHandler.HandleWithSiteIdAsync)
            .WithSummary("Retrieves latest data by TwinIds.")
            .WithOpenApi(openApiOperation =>
            {
                openApiOperation.Deprecated = true;
                return openApiOperation;
            });
        telemetryEndpoints.MapGet("lastTrendlogs", GetLastTrendlogsHandler.HandleAsync)
            .WithSummary("Retrieves latest data by TwinIds.")
            .AddEndpointFilter<RequiredTwinIdValidationFilter>();

        telemetryEndpoints.MapGet("raw", GetRawTelemetryHandler.HandleAsync)
            .WithSummary("Retrieves raw telemetry data for given twinIds.")
            .AddEndpointFilter<PageSizeValidationFilter>()
            .AddEndpointFilter<DateRangeValidationFilter>();

        telemetryEndpoints.MapGet("point/analog/{connectorId}/{externalId}", GetLiveDataByExternalIdHandler.HandleAsync)
            .WithSummary("Retrieves all analog rows by external id.");

        return telemetryEndpoints;
    }
}
