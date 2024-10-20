using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Willow.Common.Configuration;

namespace ImageHub
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
	                    .ConfigureAppConfiguration((_, config) => config.ConfigureWillowAppConfiguration())
                        .UseAzureAppServices()
                        .UseStartup<Startup>();
                }).UseSerilog((_, services, logger) =>
                {
                    logger.Enrich.FromLogContext()
                        .MinimumLevel.Override("System", LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .WriteTo.Console();
                });
    }
}
