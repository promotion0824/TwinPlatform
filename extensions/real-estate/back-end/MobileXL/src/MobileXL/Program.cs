using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using MobileXL.Infrastructure;
using Willow.Infrastructure.Azure;

namespace MobileXL
{
    public static class Program
    {
        public static void Main()
        {
            CreateHostBuilder(null).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
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

                            var vaultUri = new Uri($"https://{keyVaultConfig.KeyVaultName}.vault.azure.net/");

                            var keyVaultConfigBuilder = new ConfigurationBuilder();
                            var credential = new DefaultAzureCredential();
                            keyVaultConfigBuilder.AddAzureKeyVault(vaultUri,
                                                                   credential,
                                                                   new PrefixKeyVaultSecretManager(prefix));

                            config.AddConfiguration(keyVaultConfigBuilder.Build());
                        })
                        .UseAzureAppServices()
                        .UseStartup<Startup>();
                });
    }
}
