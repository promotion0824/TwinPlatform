//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MSMConnectorApi
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Azure.Identity;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.SmartPlaces.Facilities.MSMConnectorApi.Modules;

    public class Program
    {
        /// <summary>
        /// Entry point into MSMConnectorApi
        /// </summary>
        /// <param name="args">An option for passing in configuration values, though not required</param>
        /// <exception cref="ArgumentNullException">Thrown when required configuration values are not present</exception>
        public static async Task Main(string[] args)
        {
            Console.WriteLine($"Hello from {Assembly.GetExecutingAssembly().FullName}!");

            using IHost host = Host
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    var settings = config.Build();

                    var vaultUri = settings["KeyVaultEndpoint"];

                    if (string.IsNullOrWhiteSpace(vaultUri))
                    {
                        Console.WriteLine("Startup Variable 'KeyVaultEndpoint' not found. Exiting");
                        throw new ArgumentNullException("KeyVaultEndpoint");
                    }

                    config.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential());
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Configure ILogger
                    services.AddLogging();

                    // Enable Controllers
                    services.AddControllers();
                    services.AddEndpointsApiExplorer();
                    
                    // Setup versioning to enable migration as functionality evolves
                    services.AddApiVersioning(options =>
                    {
                        options.AssumeDefaultVersionWhenUnspecified = false;
                        options.ReportApiVersions = true;
                        options.ApiVersionReader = new QueryStringApiVersionReader();
                    });

                    // Setup Swagger
                    services.AddVersionedApiExplorer(options =>
                    {
                        options.SubstituteApiVersionInUrl = true;
                    });
                    services.AddSwaggerGen();
                    services.ConfigureOptions<ConfigureSwaggerOptions>();

                    // Configure options for ApplicationInsights
                    services.AddApplicationInsightsTelemetry(options =>
                    {
                        options.ConnectionString = hostContext.Configuration["AppInsightsConnectionString"];
                        options.EnableAdaptiveSampling = false;
                    });

                    if (hostContext.HostingEnvironment.IsDevelopment())
                    {
                        Console.WriteLine("Hi Devs");
                    }
                })
                .Build();

            // Start the host
            await host.RunAsync();

            Console.WriteLine($"Goodbye from {Assembly.GetExecutingAssembly().FullName}!");
        }
    }
}
