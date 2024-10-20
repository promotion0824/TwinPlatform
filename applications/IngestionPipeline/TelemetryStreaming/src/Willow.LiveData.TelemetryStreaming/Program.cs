using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Azure;
using MQTTnet.Extensions.ManagedClient;
using Willow.HealthChecks;
using Willow.Hosting.Worker;
using Willow.LiveData.TelemetryStreaming;
using Willow.LiveData.TelemetryStreaming.Infrastructure;
using Willow.LiveData.TelemetryStreaming.Metrics;
using Willow.LiveData.TelemetryStreaming.Models;
using Willow.LiveData.TelemetryStreaming.Services;

WorkerStart.Run(args, "TelemetryStreaming", Configure, ConfigureHealthChecks);

static void Configure(IWebHostEnvironment env, IConfiguration configuration, IServiceCollection services)
{
    services.AddSingleton<IMetricsCollector, MetricsCollector>();

    services.Configure<MqttConfig>(options => configuration.Bind("Mqtt", options));

    services.AddLazyCache();

    services.AddOptions<TableConfig>().BindConfiguration("Subscriptions");

    services.AddAzureClients(builder =>
    {
        Uri tableUri = configuration.GetSection("Subscriptions").GetValue<Uri>("StorageAccountUri") ??
            throw new InvalidOperationException("Unable to find a valid StorageAccountUri in configuration");

        builder.AddTableServiceClient(tableUri);
    });

    services.AddSingleton<MqttClientFactory>();

    DevMode devMode = new(false, false, false);

    // Only apply dev mode config if we are in development
    if (env.IsDevelopment())
    {
        configuration.Bind("DevMode", devMode);
    }

    _ = devMode.TestMqtt ?
        services.AddSingleton<IManagedMqttClient>(services => new TestMqttClient()) :
        services.AddSingleton(services => services.GetRequiredService<MqttClientFactory>().CreateManagedClient());

    _ = devMode.TestSubscriptions ?
        services.AddSingleton<ISubscriptionService, ConfigSubscriptionService>() :
        services.AddSingleton<ISubscriptionService, StorageTablesSubscriptionService>();

    _ = devMode.TestEventHub ?
        services.AddTestListener<TelemetryProcessor>() :
        services.AddBatchEventHubListener<TelemetryProcessor>(config => configuration.Bind("EventHub", config));
}

static void ConfigureHealthChecks(IHealthChecksBuilder builder)
{
    builder.AddSingletonCheck<MqttHealthCheck>("MQTT", null, ["healthz"]);
}
