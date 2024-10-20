using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Willow.HealthChecks;
using Willow.Hosting.Worker;

return WorkerStart.Run(args, Assembly.GetAssembly(typeof(Program))!.GetName().Name!, Configure, AddHealthChecks);

static void Configure(IWebHostEnvironment environment, IConfiguration configuration, IServiceCollection services)
{
    services.Configure<AdxQueryConfig>(options => configuration.Bind("AdxQueries", options));

    // Filter out health checks from http logging
    services.AddApplicationInsightsTelemetryProcessor<HealthCheckTelemetryFilter>();

    services.AddMemoryCache();
    services.AddOptions<ConnectorOverridesOption>().Bind(configuration.GetSection("ConnectorOverrides"));
    services.AddWillowAdxService(options => configuration.Bind("Adx", options));
    services.AddSingleton(new MetricsAttributesHelper(configuration));
    services.AddSingleton<IMetricsCollector, MetricsCollector>();
    services.AddSingleton<IAdxQueryExecutor, AdxQueryExecutor>();
    services.AddSingleton<IConnectorApplicationBuilder, ConnectorApplicationBuilder>();
    services.AddSingleton<IHealthMetricsRepository, HealthMetricsRepository>();
    services.AddHostedService<MetricsAggregatorWorker>();
}

static void AddHealthChecks(IHealthChecksBuilder healthChecks)
{
    healthChecks.AddCheck<AdxHealthCheck>("AdxConnectionHealthCheck", tags: ["healthz"]);
}
