using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Willow.Common.Configuration;
using System;
using Microsoft.Extensions.Configuration;
using PlatformPortalXL.Infrastructure;

namespace PlatformPortalXL;

public static class Program
{
    private const long MaxRequestBodySize = 768 * 1024 * 2014;

    public static void Main()
    {
        try
        {
            Host.CreateDefaultBuilder(null)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureKestrel((_, options) =>
                        {
                            options.Limits.MaxRequestBodySize = MaxRequestBodySize;
                        })
                        .ConfigureAppConfiguration((context, config) =>
                        {
                            config.AddUserSecrets<Startup>();
                            config.ConfigureWillowAppConfiguration();
                            config.AddUserManagementConfiguration(context.HostingEnvironment.EnvironmentName);
                        })
                        .ConfigureServices(services =>
                        {
                            services.Configure<IISServerOptions>(options =>
                            {
                                options.MaxRequestBodySize = MaxRequestBodySize;
                            });
                        })
                        .UseAzureAppServices()
                        .UseStartup<Startup>();
                })
                .Build()
                .Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            Console.WriteLine(ex.StackTrace);
        }
    }
}
