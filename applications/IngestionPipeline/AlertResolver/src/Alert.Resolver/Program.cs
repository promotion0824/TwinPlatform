using System.Reflection;
using Ardalis.GuardClauses;
using Azure.Identity;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Willow.Alert.Resolver;
using Willow.Alert.Resolver.Helpers;
using Willow.Alert.Resolver.ResolutionHandlers.Implementation;
using Willow.Alert.Resolver.ResolutionHandlers.Implementation.DI;
using Willow.Alert.Resolver.ResolutionHandlers.Implementation.Handlers;
using Willow.Alert.Resolver.Services;
using Willow.Alert.Resolver.Transformers;
using Willow.Api.Authentication;
using Willow.Hosting.Worker;
using Willow.IoTService.Monitoring.Alerting;
using Willow.IoTService.Monitoring.Extensions;
using Willow.IoTService.Monitoring.Options;
using Willow.IoTService.Monitoring.Services.AppInsights;
using Willow.IoTService.Monitoring.Services.DeploymentDashboard;
using Willow.IoTService.Monitoring.Services.LiveDataCore;

const string alertResolverNotificationQueueName = "alertresolvernotification";

WorkerStart.Run(args, Assembly.GetAssembly(typeof(Program))!.GetName().Name!, Configure, ConfigureHealthCheck);

static void ConfigureHealthCheck(IHealthChecksBuilder healthChecks)
{
    healthChecks.AddCheck<HealthCheckServiceBus>("Service Bus", tags: ["healthz"]);
}

static void Configure(IWebHostEnvironment webHostEnvironment, IConfiguration configuration, IServiceCollection services)
{
    var configurationBuilder = new ConfigurationBuilder();
    configurationBuilder.AddConfiguration(configuration);
    configurationBuilder.AddKeyVaultConfiguration(configuration);
    services.AddSingleton(configuration);

    services.AddSingleton<HealthCheckServiceBus>();
    services.AddMassTransit(x =>
    {
        x.UsingAzureServiceBus((context, cfg) =>
        {
            var serviceBusConnectionString = Guard.Against.NullOrWhiteSpace(configuration.GetValue<string>("Azure:ServiceBus:ConnectionString"), "ServiceBusConnectionString",
                "Service Bus connection string is not defined.");
            cfg.Host(new Uri(serviceBusConnectionString),
                configurator => configurator.TokenCredential = new DefaultAzureCredential());
            cfg.UseServiceBusMessageScheduler();

            cfg.ReceiveEndpoint(configuration.GetValue("Azure:ServiceBus:QueueName", alertResolverNotificationQueueName)!, e =>
            {
                e.ConfigureConsumer<AlertResolverMessageConsumer>(context);
            });
        });

        x.AddConsumer<AlertResolverMessageConsumer>();
    });

    services.AddLogging();
    services.AddClientCredentialToken(configuration);
    services.Configure<DeviceMapping>(configuration.GetSection(nameof(DeviceMapping)));
    services.AddOptions<DeviceMapping>();
    services.AddOptions<M2MOptions>().Bind(configuration.GetSection(M2MOptions.SectionName));
    services.AddOptions<ServiceKeyOptions>().Bind(configuration.GetSection(ServiceKeyOptions.SectionName));
    services.AddTransient<IClientCredentialTokenService, ClientCredentialTokenService>();
    services.AddTransient<IDeviceMappingService, DeviceMappingService>();
    services.AddTransient<IDeviceService, DeviceService>();
    services.AddTransient<IModuleHelper, ModuleHelper>();
    services.AddTransient<IModuleNameTransformer, ModuleNameTransformer>();
    services.AddMemoryCache();
    services.AddSingleton<IConnectorStatsQueries, ConnectorStatsQueries>();
    services.AddTransient<IDeploymentDashboardApiService, DeploymentDashboardApiService>();
    services.AddHttpClients();
    services.AddApplicationInsightsTelemetry(configuration);

    services.AddMicrosoftTeamsAlerting(configuration);
    services.AddTransient<IMonitorEventTracker, MonitorEventTracker>();

    services.ConfigureResolutions<ResolutionRequest>()
        .AddHandler<CheckDeviceStatusHandler>()
        .AddHandler<CheckEdgeNetworkStatusHandler>()
        .AddHandler<RestartEdgeModuleHandler>()
        .AddHandler<VerifyTelemetryStatusHandler>()
        .AddHandler<SendFeedbackHandler>();
}
