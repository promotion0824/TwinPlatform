namespace Willow.PublicApi.Transforms;

using Willow.Telemetry;
using Yarp.ReverseProxy.Transforms.Builder;

/// <summary>
/// Counts the number of calls to a specific route.
/// </summary>
/// <remarks>
/// As a route counter, it ignores specific route values, e.g. calls to /twins/{id} will be counted together, regardless of the value of {id}.
/// </remarks>
internal static class CounterTransform
{
    public static TransformBuilderContext AddCounterTransform(this TransformBuilderContext context)
    {
        context.AddRequestTransform(transformContext =>
        {
            var metricsCollector = transformContext.HttpContext.RequestServices.GetRequiredService<IMetricsCollector>();
            var path = context.Route.Match.Path ?? string.Empty;

            var clientId = transformContext.HttpContext.GetClientIdFromToken() ?? transformContext.HttpContext.GetClientIdFromBody() ?? string.Empty;

            Dictionary<string, string> tags = new()
            {
                { "path", path },
                { "clientId", clientId },
            };

            metricsCollector.TrackMetric<long>(path, 1, MetricType.Counter, $"Number of requests to {path}", tags);

            return ValueTask.CompletedTask;
        });

        return context;
    }
}
