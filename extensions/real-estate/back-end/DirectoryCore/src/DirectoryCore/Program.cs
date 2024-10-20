using System.Threading.Tasks;
using DirectoryCore.Database;
using DirectoryCore.Services.UserSetupService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Willow.Common.Configuration;

namespace DirectoryCore
{
    public static class Program
    {
        public static async Task Main()
        {
            var host = CreateWebHostBuilder(null).Build();

            using (var scope = host.Services.CreateScope())
            {
                var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                var dbUpgradeChecker =
                    scope.ServiceProvider.GetRequiredService<IDbUpgradeChecker>();
                dbUpgradeChecker.EnsureDatabaseUpToDate(env);

                var userSetupService =
                    scope.ServiceProvider.GetRequiredService<ISingleTenantSetupService>();
                await userSetupService.SetupSingleTenantData();
            }

            host.Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureAppConfiguration(
                            (_, config) => config.ConfigureWillowAppConfiguration()
                        )
                        .UseAzureAppServices()
                        .UseStartup<Startup>();
                });
    }
}
