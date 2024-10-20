using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Willow.Tests.Infrastructure;
using Microsoft.Extensions.Configuration;
using DigitalTwinCore;
using DigitalTwinCore.Constants;
using DigitalTwinCore.Database;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Services.AdtApi;
using DigitalTwinCore.Test.MockServices;
using DigitalTwinCore.Services;
using Moq;
using Willow.Tests.Infrastructure.MockServices;
using DigitalTwinCore.Services.Adx;
using System.Threading.Tasks;

namespace Workflow.Tests
{
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
                    ["ConnectionStrings:DigitalTwinDb"] = testContext.DatabaseFixture.GetConnectionString(databaseName),
                    ["EnableServiceKeyAuthMiddleware"] = bool.FalseString,
                    ["DisableScopeTreeUpdater"] = bool.TrueString
                };
                configuration.AddInMemoryCollection(configurationValues);
            },
            MainServicePostConfigureServices = (services) =>
            {
                services.ReplaceScoped<IDigitalTwinService, TestDigitalTwinService>();
                services.ReplaceSingleton<IAdtApiService, InMemoryAdtApiService>();
            }
        };

        public static readonly ServerFixtureConfiguration InMemoryDb = new ServerFixtureConfiguration
        {
            EnableTestAuthentication = true,
            StartupType = typeof(Startup),
            MainServicePostAppConfiguration = (configuration, testContext) =>
            {
                var configurationValues = new Dictionary<string, string>
                {
                    ["EnableServiceKeyAuthMiddleware"] = bool.FalseString,
                    ["DisableScopeTreeUpdater"] = bool.TrueString
                };
                if (TestEnvironment.UseInMemoryDatabase == false)
                {
                    testContext.Output.WriteLine("Force to use database instead of in-memory database");
                    var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    configurationValues.Add("ConnectionStrings:DigitalTwinDb", testContext.DatabaseFixture.GetConnectionString(databaseName));
                }
                configuration.AddInMemoryCollection(configurationValues);
            },
            MainServicePostConfigureServices = (services) =>
            {
                if (TestEnvironment.UseInMemoryDatabase)
                {
                    var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    services.ReplaceScoped(GetInMemoryOptions<DigitalTwinDbContext>(databaseName));
                    services.ReplaceScoped<IDbUpgradeChecker>(_ => new InMemoryDbUpgradeChecker());
                    services.ReplaceScoped<IDigitalTwinService, TestDigitalTwinService>();
                    services.ReplaceSingleton<IAdtApiService, InMemoryAdtApiService>();
                    services.ReplaceSingleton<IAssetService, MockCachelessAssetService>();
                    services.ReplaceSingleton<IAdxDatabaseInitializer, DummyAdxInitializer>();
                }
            },
            DependencyServices = new List<DependencyServiceConfiguration>
            {
                new()
                {
                    Name = ApiServiceNames.DirectoryCore
                }
            }
        };

        public static Func<IServiceProvider, DbContextOptions<T>> GetInMemoryOptions<T>(string dbName) where T : DbContext
        {
            return (_) =>
            {
                var builder = new DbContextOptionsBuilder<T>();
                builder.UseInMemoryDatabase(databaseName: dbName, b => b.EnableNullChecks(false));
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

        public class DummyAdxInitializer : IAdxDatabaseInitializer
        {
            public Task EnsureDatabaseObjectsExist()
            {
                return null;
            }
        }

    }
}
