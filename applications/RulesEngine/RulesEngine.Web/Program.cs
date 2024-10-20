using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Willow.Rules.Web
{
    /// <summary>
    /// The web application
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point
        /// </summary>
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            // very detailed logging: using AzureEventSourceListener listener = AzureEventSourceListener.CreateConsoleLogger();
            host.Run();
        }

        /// <summary>
        /// Creates the host builder
        /// </summary>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    var env = hostContext.HostingEnvironment.EnvironmentName;

                    config.AddJsonFile("appsettings.json", optional: false)
                        .AddJsonFile($"appsettings.{env}.json", optional: true)
                        .AddJsonFile($"appsettings.user.json", optional: true)
                        .AddEnvironmentVariables("rules_");

                    //to retrieve the keyvault uri from settings, we have to build config first
                    var builtConfig = config.Build();

                    var keyVaultUri = builtConfig["Rules:KeyVaultUri"];

                    if (!string.IsNullOrEmpty(keyVaultUri))
                    {
                        //keyvault settings for the following paths
                        //AzureApplication--ClientSecret
                        //AzureAdB2C--ClientSecret
                        config.AddAzureKeyVault(new Uri(keyVaultUri), Startup.AzureCredentials(hostContext.HostingEnvironment), new AzureKeyVaultConfigurationOptions()
                        {
                            //could be used for key-rotations etc
                            //ReloadInterval
                        });
                    }
                })
                .ConfigureLogging((hostingContext, builder) =>
                {
                    builder.ClearProviders();

                    builder.AddSimpleConsole(opts =>
                    {
                        opts.IncludeScopes = false;
                        opts.SingleLine = true;
                        opts.TimestampFormat = "HH:mm:ss ";
                    });
                    // app insights is registered during open telemetry setup using services.AddWillowContext(Configuration) in startup
                    // builder.AddApplicationInsights();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseWebRoot("ClientApp");
                    webBuilder.UseStartup<Startup>();
                    // The new way MSFT wants you to do this
                    // webBuilder.UseSetting(WebHostDefaults.HostingStartupAssembliesKey, "Microsoft.AspNetCore.SpaProxy");
                });
    }
}
