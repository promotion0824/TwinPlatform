using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Willow.Tests.Infrastructure;
using Microsoft.Extensions.Configuration;
using SiteCore.Database;
using SiteCore.Entities;
using SiteCore.Services;

namespace SiteCore.Tests
{
    public class ServerFixtureConfigurations
    {
        public static readonly ServerFixtureConfiguration InMemoryDb = new ServerFixtureConfiguration
        {
            EnableTestAuthentication = true,
            StartupType = typeof(Startup),
            MainServicePostAppConfiguration = (configuration, testContext) =>
            {
                var configurationValues = new Dictionary<string, string>
                {
                    ["FloorModuleOptions:Modules2D:AllowedExtensions:0"] = ".png",
                    ["FloorModuleOptions:Modules2D:MaxSizeBytes"] = "2097152",
                    ["FloorModuleOptions:Modules2D:MaxWidth"] = "1024",
                    ["FloorModuleOptions:Modules2D:MaxHeight"] = "768",
                    ["FloorModuleOptions:Modules3D:MaxSizeBytes"] = "5242880",
                    ["ForgeOptions:TokenEndpoint"] = Test.Constants.Forge.TokenEndpoint,
                    ["ForgeOptions:Scope:0"] = "viewables:read"
                };

                if (TestEnvironment.UseInMemoryDatabase == false)
                {
                    testContext.Output.WriteLine("Force to use database instead of in-memory database");
                    var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    configurationValues["ConnectionStrings:SiteDb"] = testContext.DatabaseFixture.GetConnectionString(databaseName);
                }

                configuration.AddInMemoryCollection(configurationValues);
            },
            MainServicePostConfigureServices = (services) =>
            {
                if (TestEnvironment.UseInMemoryDatabase)
                {
                    var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    services.ReplaceScoped(GetInMemoryOptions<SiteDbContext>(databaseName));
                    services.ReplaceScoped<IDbUpgradeChecker>(_ => new InMemoryDbUpgradeChecker());
                }
            },
            DependencyServices = new List<DependencyServiceConfiguration>
            {
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.ImageHub
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
}
