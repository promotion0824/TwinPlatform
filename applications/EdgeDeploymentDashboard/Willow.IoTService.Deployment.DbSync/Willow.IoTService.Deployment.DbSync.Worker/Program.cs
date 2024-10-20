using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Willow.Hosting.Worker;
using Willow.IoTService.Deployment.DataAccess.DependencyInjection;
using Willow.IoTService.Deployment.DbSync.Application;
using Willow.IoTService.Deployment.DbSync.Application.HealthChecks;
using Willow.IoTService.Deployment.DbSync.Application.Infrastructure;

return WorkerStart.Run(args, "EdgeDeploymentDbSyncService", ConfigureServices, AddHealthChecks, ConfigureBuilder);

static void ConfigureServices(IWebHostEnvironment env, IConfiguration configuration, IServiceCollection services)
{
    services.AddTransient<IUpdateModuleService, UpdateModuleService>();
    services.AddValidatorsFromAssemblyContaining<ConnectorSyncConsumer>();
    services.AddMassTransit(config =>
    {
        config.AddConsumers(typeof(ConnectorSyncConsumer).Assembly);
        config.UsingAzureServiceBus((ctx, cfg) =>
        {
            var url = configuration.GetSection("AzureServiceBus:HostAddress").Value;
            var topicName = configuration.GetSection("AzureServiceBus:TopicName").Value;
            var subscriptionName = configuration.GetSection("AzureServiceBus:SubscriptionName").Value;

            if (url is null || topicName is null || subscriptionName is null)
            {
                throw new InvalidDataException("Missing AzureServiceBus configuration. HostAddress, TopicName and SubscriptionName are required.");
            }

            cfg.Host(new Uri(url));
            cfg.SubscriptionEndpoint(subscriptionName, topicName, e => { e.ConfigureConsumer<ConnectorSyncConsumer>(ctx); });
            cfg.ConfigureEndpoints(ctx);
        });
    });

    services.AddSingleton<HealthCheckServiceBus>();
    services.AddSingleton<HealthCheckSql>();
    services.AddHostedService<StartupHealthCheckService>();
    services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<HealthCheckServiceOptions>, StartupHealthCheckService.RemoveMasstransitHealthChecks>());

    services.ConfigureDataAccess(
                                 configuration.GetConnectionString("DeploymentDb")
                                 ?? throw new ArgumentNullException("ConnectionStrings:DeploymentDb"),
                                 "DbSyncService");
}

static void ConfigureBuilder(IHostBuilder builder)
{
    builder.ConfigureAppConfiguration(
                                      (_, builder) =>
                                      {
                                          builder.AddEnvironmentVariables();
                                          builder.AddUserSecrets(typeof(Program).Assembly);
                                      });
}

static void AddHealthChecks(IHealthChecksBuilder healthChecks)
{
    healthChecks.AddCheck<HealthCheckServiceBus>("Service Bus", tags: new[] { "healthz" });
    healthChecks.AddCheck<HealthCheckSql>("DeploymentDb SQL", tags: new[] { "healthz" });
}
