using Microsoft.AspNetCore.Builder;
using Willow.HealthChecks;
using Willow.Hosting.Web;
using Willow.MappedTelemetryAdaptor;
using Willow.MappedTelemetryAdaptor.Models;
using Willow.MappedTelemetryAdaptor.Options;
using Willow.MappedTelemetryAdaptor.Services;
using Willow.Telemetry;

await WebApplicationStart.RunAsync(args, "MappedTelemetryAdaptor", Configure, ConfigureApp, null);

void Configure(WebApplicationBuilder builder)
{
    builder.Services.AddApplicationInsightsTelemetryProcessor<HealthCheckTelemetryFilter>();
    builder.Services.AddWillowAdxService(options => builder.Configuration.Bind("Adx", options));
    builder.Services.AddMemoryCache();
    builder.Services.AddMeterSingletonIfNotExists();
    builder.Services.AddSingleton<IIdMapCacheService, IdMapCacheService>();
    builder.Services.AddSingleton<IMetricsCollector, MetricsCollector>();
    builder.Services.AddProcessorFiltersFromAssembly<Version2ProcessorFilter>();
    builder.Services.AddOptions<IdMappingCacheOption>().Bind(builder.Configuration.GetSection(IdMappingCacheOption.Section));
    builder.Services.AddBatchEventHubListener<MappedInput, TelemetryProcessor>(config => builder.Configuration.Bind("EventHub", config));
    builder.Services.AddEventHubSender(config => builder.Configuration.Bind("EventHub", config));
    builder.Services.AddHostedService<IdMapBackgroundService>();
}

ValueTask ConfigureApp(WebApplication application)
{
    LoadConnectorCache(application.Services);

    return ValueTask.CompletedTask;
}

static void LoadConnectorCache(IServiceProvider services)
{
    var cacheService = services.GetRequiredService<IIdMapCacheService>();
    cacheService.LoadIdMapping().GetAwaiter().GetResult();
}
