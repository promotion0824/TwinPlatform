using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationCore.Database;
using NotificationCore.Entities;
using NotificationCore.Test.Infrastructure;

namespace NotificationCore.Test;

public class ServerFixtureConfigurations
{
    public static readonly ServerFixtureConfiguration SqlServer = new ServerFixtureConfiguration
    {
        EnableTestAuthentication = true,
        StartupType = typeof(Startup),
        MainServicePostAppConfiguration = (configuration, testContext) =>
        {
            var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            var configurationValues = new Dictionary<string, string>
            {
                ["ConnectionStrings:NotificationCoreDb"] = testContext.DatabaseFixture.GetConnectionString(databaseName),
            };
            configuration.AddInMemoryCollection(configurationValues);
        },
        MainServicePostConfigureServices = (services) =>
        {
        }
    };

    public static readonly ServerFixtureConfiguration InMemoryDb = new ServerFixtureConfiguration
    {
        EnableTestAuthentication = true,
        StartupType = typeof(Startup),
        MainServicePostAppConfiguration = (configuration, testContext) =>
        {
            var configurationValues = new Dictionary<string, string>()
            {
                ["AuthorizationAPI:BaseAddress"] = "http://authrz-api/",
                ["AuthorizationAPI:TokenAudience"] = "api://8c74e778-9a35-473b-bbf0-4b0e9cc6000e",
                ["AuthorizationAPI:APITimeoutMilliseconds"] = "25000",
                ["AuthorizationAPI:ExtensionName"] = "WillowApp",
                ["AuthorizationAPI:InstanceType"] = "nonprd",
            };

            if (TestEnvironment.UseInMemoryDatabase == false)
            {
                testContext.Output.WriteLine("Force to use database instead of in-memory database");
                var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                configurationValues.Add("ConnectionStrings:NotificationCoreDb", testContext.DatabaseFixture.GetConnectionString(databaseName));

            }
            configuration.AddInMemoryCollection(configurationValues);
        },
        MainServicePostConfigureServices = (services) =>
        {
            if (TestEnvironment.UseInMemoryDatabase)
            {
                var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                services.ReplaceScoped(GetInMemoryOptions<NotificationDbContext>(databaseName));
                services.ReplaceScoped<IDbUpgradeChecker>(_ => new InMemoryDbUpgradeChecker());
            }

        },
        DependencyServices = new List<DependencyServiceConfiguration>
            {
                new DependencyServiceConfiguration
                {
                    Name = "TripPin"
                }
            }
    };
    public static readonly ServerFixtureConfiguration InMemoryDbWithHostedJobSetting = new ServerFixtureConfiguration
    {
        EnableTestAuthentication = true,
        StartupType = typeof(Startup),
        MainServicePostAppConfiguration = (configuration, testContext) =>
        {
            var configurationValues = new Dictionary<string, string>();

            if (TestEnvironment.UseInMemoryDatabase == false)
            {
                testContext.Output.WriteLine("Force to use database instead of in-memory database");
                var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                configurationValues.Add(
                    "ConnectionStrings:NotificationCoreDb", testContext.DatabaseFixture.GetConnectionString(databaseName));

            }
            configuration.AddInMemoryCollection(configurationValues);
        },
        MainServicePostConfigureServices = (services) =>
        {
            if (TestEnvironment.UseInMemoryDatabase)
            {
                var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                services.ReplaceScoped(GetInMemoryOptions<NotificationDbContext>(databaseName));
                services.ReplaceScoped<IDbUpgradeChecker>(_ => new InMemoryDbUpgradeChecker());
            }

        },
        DependencyServices = new List<DependencyServiceConfiguration>
            {
                new DependencyServiceConfiguration
                {
                    Name = "TripPin"
                }
            }
    };
    public static Func<IServiceProvider, DbContextOptions<T>> GetInMemoryOptions<T>(string dbName) where T : DbContext
    {
        return (_) =>
        {
            var builder = new DbContextOptionsBuilder<T>();
            builder.UseInMemoryDatabase(databaseName: dbName);
            return builder.Options;
        };
    }

    public class InMemoryDbUpgradeChecker : IDbUpgradeChecker
    {
        public void EnsureDatabaseUpToDate(IWebHostEnvironment env)
        {
            // Do nothing
        }
    }

}
