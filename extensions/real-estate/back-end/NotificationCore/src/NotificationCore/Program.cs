using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Willow.Common.Configuration;

namespace NotificationCore;

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
                    .ConfigureAppConfiguration((_, config) => config.ConfigureWillowAppConfiguration())
                    .UseAzureAppServices()
                    .UseStartup<Startup>();
            });
}
