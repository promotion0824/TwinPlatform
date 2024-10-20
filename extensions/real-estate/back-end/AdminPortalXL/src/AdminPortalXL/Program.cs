using System.Reflection;
using Willow.Infrastructure.Azure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AdminPortalXL
{
    public static class Program
    {
        public static void Main()
        {
            CreateWebHostBuilder(null).Build().Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureServices(services =>
                        {
                            services.AddApplicationInsightsTelemetry();
                        })
                        .ConfigureAppConfiguration((context, config) =>
                        {
                            var keyVaultConfig = config.Build().GetConfigValue<KeyVaultConfig>("Azure:KeyVault");
                            if (keyVaultConfig == null || string.IsNullOrEmpty(keyVaultConfig.KeyVaultName))
                            {
                                return;
                            }

                            // The appVersion obtains the app version (1.0.0.0), which
                            // is set in the project file and obtained from the entry
                            // assembly. The versionPrefix holds the major version
                            // for the PrefixKeyVaultSecretManager.
                            var assemblyName = Assembly.GetEntryAssembly().GetName();
                            var prefix = $"{assemblyName.Name}--{assemblyName.Version.Major}";

                            var keyVaultConfigBuilder = new ConfigurationBuilder();

                            if (string.IsNullOrEmpty(keyVaultConfig.ClientId))
                            {
                                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                                var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                                keyVaultConfigBuilder.AddAzureKeyVault(
                                    $"https://{keyVaultConfig.KeyVaultName}.vault.azure.net/",
                                    keyVaultClient,
                                    new PrefixKeyVaultSecretManager(prefix));
                            }
                            else
                            {
                                keyVaultConfigBuilder.AddAzureKeyVault(
                                    $"https://{keyVaultConfig.KeyVaultName}.vault.azure.net/",
                                    keyVaultConfig.ClientId,
                                    keyVaultConfig.ClientSecret,
                                    new PrefixKeyVaultSecretManager(prefix));
                            }

                            config.AddConfiguration(keyVaultConfigBuilder.Build());
                        })
                        .UseSerilog((context, logger) =>
                        {
                            var applicationInsightsKey = context.Configuration.GetValue<string>("ApplicationInsights:InstrumentationKey");
                            logger.Enrich.FromLogContext()
                                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                                .WriteTo.ApplicationInsights(applicationInsightsKey, TelemetryConverter.Traces)
                                .WriteTo.Console();
                        })
                        .UseAzureAppServices()
                        .UseStartup<Startup>();
                });
    }
}
