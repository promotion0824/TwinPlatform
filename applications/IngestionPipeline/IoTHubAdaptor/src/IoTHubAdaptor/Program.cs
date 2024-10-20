using System.Reflection;
using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Willow.Hosting.Worker;
using Willow.LiveData.IoTHubAdaptor;
using Willow.LiveData.IoTHubAdaptor.Infrastructure.KeyVault;
using Willow.LiveData.IoTHubAdaptor.Models;
using Willow.LiveData.IoTHubAdaptor.Services;

WorkerStart.Run(args, "IoTHubTelemetryAdaptor", ConfigureServices, configureBuilder: ConfigureHostBuilder);

static void ConfigureServices(IWebHostEnvironment env, IConfiguration configuration, IServiceCollection services)
{
    services.AddIotHubListener<UnifiedTelemetryMessage, TelemetryProcessor>(config => configuration.Bind("EventHub", config));
    services.AddEventHubSender(config => configuration.Bind("EventHub", config));

    services.AddSingleton<ITransformService, TransformService>();
}

static void ConfigureHostBuilder(IHostBuilder builder)
{
    builder.ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddEnvironmentVariables();

                var settings = config.Build();
                var vaultName = settings["Azure:KeyVault:KeyVaultName"];

                if (context.HostingEnvironment.IsDevelopment() && string.IsNullOrWhiteSpace(vaultName))
                {
                    return;
                }
                else if (!context.HostingEnvironment.IsDevelopment() && string.IsNullOrWhiteSpace(vaultName))
                {
                    Console.WriteLine("Startup Variable 'Azure:KeyVault:KeyVaultName' not found. Exiting");
                    throw new InvalidOperationException("Startup Variable Azure:KeyVault:KeyVaultName not found.");
                }

                var vaultUri = $"https://{vaultName}.vault.azure.net/";
                var assemblyName = Assembly.GetEntryAssembly()?.GetName();

                //Clean assembly names, must only contain alphanumeric and dashes for KeyVault names.
                var prefix = string.Join(string.Empty, assemblyName?.Name?.Where(c => (char.IsLetterOrDigit(c) && c < 128) || c == '-') ?? string.Empty);
                config.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential(), new PrefixKeyVaultSecretManager(prefix));
            });
}
