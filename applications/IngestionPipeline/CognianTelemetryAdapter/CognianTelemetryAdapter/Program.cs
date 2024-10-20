using Microsoft.AspNetCore.Hosting;
using Willow.CognianTelemetryAdapter;
using Willow.CognianTelemetryAdapter.Metrics;
using Willow.CognianTelemetryAdapter.Models;
using Willow.CognianTelemetryAdapter.Options;
using Willow.CognianTelemetryAdapter.Services;
using Willow.HealthChecks;
using Willow.Hosting.Worker;

WorkerStart.Run(args, "CognianTelemetryAdapter", ConfigureServices);

static void ConfigureServices(IWebHostEnvironment env, IConfiguration configuration, IServiceCollection services)
{
    // Filter out health checks from http logging
    services.AddApplicationInsightsTelemetryProcessor<HealthCheckTelemetryFilter>();
    services.AddMemoryCache();
    services.AddOptions<CognianAdapterOption>().Bind(configuration.GetSection(CognianAdapterOption.Section));
    services.AddSingleton<IMetricsCollector, MetricsCollector>();
    services.AddBatchEventHubListener<CognianTelemetryMessage, TelemetryProcessor>(config => configuration.Bind("EventHub", config));
    services.AddSingleton<ITransformService, TransformService>();
    services.AddEventHubSender(config => configuration.Bind("EventHub", config));
}
