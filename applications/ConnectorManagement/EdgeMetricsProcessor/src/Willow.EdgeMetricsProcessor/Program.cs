using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Willow.EdgeMetricsProcessor;
using Willow.EdgeMetricsProcessor.HealthChecks;
using Willow.EdgeMetricsProcessor.Metrics;
using Willow.EdgeMetricsProcessor.Models;
using Willow.HealthChecks;
using Willow.Hosting.Worker;

return WorkerStart.Run(args, "EdgeMetricsProcessor", ConfigureServices, AddHealthChecks);

static void ConfigureServices(IWebHostEnvironment env, IConfiguration configuration, IServiceCollection services)
{
    services.AddSingleton<HealthCheckEdgeMetricsProcessor>();

    // Filter out health checks from http logging
    services.AddApplicationInsightsTelemetryProcessor<HealthCheckTelemetryFilter>();
    services.AddSingleton<IMetricsCollector, MetricsCollector>();
    services.AddBatchEventHubListener<IoTHubMetric[], EdgeMetricsProcessor>(config => configuration.Bind("EventHub", config));
}

static void AddHealthChecks(IHealthChecksBuilder healthChecks)
{
    healthChecks.AddCheck<HealthCheckEdgeMetricsProcessor>("EdgeMetricsProcessor", tags: ["healthz"]);
}
