using Azure.Core;
using Azure.Identity;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Willow.Hosting.Worker;
using Willow.IoTService.Deployment.DataAccess.DependencyInjection;
using Willow.IoTService.Deployment.ManifestStorage.Hosting;
using Willow.IoTService.Deployment.Service.Application.BackgroundJobs;
using Willow.IoTService.Deployment.Service.Application.Deployments;
using Willow.IoTService.Deployment.Service.Application.HealthChecks;
using Willow.IoTService.Deployment.Service.Worker.Infrastructure;

WorkerStart.Run(args, "EdgeDeploymentService", ConfigureServices, AddHealthChecks, ConfigureBuilder);
return;

static void ConfigureServices(IWebHostEnvironment env, IConfiguration configuration, IServiceCollection services)
{
    services.AddSingleton<IDeploymentServiceFactory, DeploymentServiceFactory>();
    services.AddTransient<IBaseModuleTransformer, BaseModuleTransformer>();
    services.AddTransient<IDefaultTransformer, DefaultTransformer>();
    services.AddTransient<IDeploymentConfigurationCreator, DeploymentConfigurationCreator>();
    services.AddTransient<IEdgeConnectorEnvService, EdgeConnectorEnvService>();
    services.AddTransient<IEdgeConnectorTransformer, EdgeConnectorTransformer>();
    services.AddTransient<IKeyVaultService, KeyVaultService>();
    services.AddTransient<IModuleConfigContentService, ModuleConfigContentService>();
    services.AddTransient<IServiceBusAdminService, ServiceBusAdminService>();

    services.AddSingleton<HealthCheckServiceBus>();
    services.AddSingleton<HealthCheckSql>();
    services.AddHostedService<StartupHealthCheckService>();
    services.AddHostedService<AlertResolverBackgroundJob>();

    services.AddValidatorsFromAssemblyContaining<DeploymentConfiguration>();
    services.AddMassTransit(
                            config =>
                            {
                                config.SetKebabCaseEndpointNameFormatter();
                                config.AddConsumers(typeof(DeployModuleConsumer).Assembly);
                                config.UsingAzureServiceBus(
                                                            (cxt, cfg) =>
                                                            {
                                                                var url = configuration.GetSection("AzureServiceBus:HostAddress")
                                                                                 .Value;
                                                                cfg.Host(new Uri(url ?? throw new ArgumentNullException("AzureServiceBus:HostAddress")));
                                                                cfg.ConfigureEndpoints(cxt);
                                                            });
                            });
    services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<HealthCheckServiceOptions>, RemoveMasstransitHealthChecks>());

    services.UseManifestStorage(configuration);
    services.Configure<WillowContainerRegistry>(configuration.GetSection(nameof(WillowContainerRegistry)));
    services.AddOptions<WillowContainerRegistry>();
    services.ConfigureDataAccess(
                                 configuration.GetConnectionString("DeploymentDb")
                                 ?? throw new ArgumentNullException("ConnectionStrings:DeploymentDb"),
                                 "DeploymentService");
    services.AddMemoryCache();
}

static void ConfigureBuilder(IHostBuilder builder)
{
    builder.ConfigureAppConfiguration(
                                      (_, builder) =>
                                      {
                                          builder.AddEnvironmentVariables();
                                          builder.AddUserSecrets(typeof(Program).Assembly);
                                          var config = builder.Build();

                                          var urlString = config.GetValue<string>("AppSecretsUrl");
                                          builder.AddAzureKeyVault(
                                                                   new Uri(urlString ?? throw new ArgumentNullException("AppSecretsUrl")),
                                                                   new DefaultAzureCredential(),
                                                                   new PrefixKeyVaultSecretManager());
                                      });
}

static void AddHealthChecks(IHealthChecksBuilder healthChecks)
{
    healthChecks.AddCheck<HealthCheckServiceBus>("Service Bus", tags: ["healthz"]);
    healthChecks.AddCheck<HealthCheckSql>("DeploymentDb SQL", tags: ["healthz"]);
}
