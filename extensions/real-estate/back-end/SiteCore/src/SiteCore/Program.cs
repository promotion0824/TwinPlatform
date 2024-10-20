using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SiteCore.Database;
using SiteCore.Services;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Willow.Common.Configuration;

namespace SiteCore
{
    public class Program
    {
        public static async Task Main()
        {
            var host = CreateWebHostBuilder(null).Build();

            using (var scope = host.Services.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                try
                {
                    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                    var dbUpgradeChecker =
                        scope.ServiceProvider.GetRequiredService<IDbUpgradeChecker>();
                    dbUpgradeChecker.EnsureDatabaseUpToDate(env);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to ensure database was up to date.");
                }

                try
                {
                    var scopePopulateService =
                        scope.ServiceProvider.GetRequiredService<ISitePreferencesScopePopulateService>();
                    await scopePopulateService.PopulateScopeIdToSitePreferences();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to populate the scopeId from DigitalTwinCore. {exceptionMessage}", ex.Message);
                }
            }

            host.Run();
        }

        private static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureAppConfiguration((_, config) =>
                        {
                            config.AddUserSecrets<Startup>();
                            config.ConfigureWillowAppConfiguration();
                        })
                        .UseAzureAppServices()
                        .UseStartup<Startup>();
                });
    }
}
