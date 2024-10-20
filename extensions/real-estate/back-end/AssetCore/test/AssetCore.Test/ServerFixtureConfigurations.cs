using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Willow.Tests.Infrastructure;
using Microsoft.Extensions.Configuration;
using AssetCore;
using AssetCoreTwinCreator.Domain;
using Microsoft.Data.Sqlite;
using AssetCoreTwinCreator.MappingId;
using AssetCore.Database;

namespace Workflow.Tests
{
    public class ServerFixtureConfigurations
    {
        public static readonly ServerFixtureConfiguration Default = new ServerFixtureConfiguration
        {
            EnableTestAuthentication = true,
            StartupType = typeof(Startup),
            MainServicePostAppConfiguration = (configuration, testContext) =>
            {
                if (TestEnvironment.UseInMemoryDatabase == false)
                {
                    testContext.Output.WriteLine("Force to use database instead of in-memory database");
                    var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    var configurationValues = new Dictionary<string, string>
                    {
                        ["ConnectionStrings:AssetDb"] = testContext.DatabaseFixture.GetConnectionString(databaseName),
                    };
                    configuration.AddInMemoryCollection(configurationValues);
                }
            },
            MainServicePostConfigureServices = (services) =>
            {
                if (TestEnvironment.UseInMemoryDatabase)
                {
                    var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    services.ReplaceScoped(GetInMemoryOptions<AssetDbContext>(databaseName));
                }
            },
            DependencyServices = new List<DependencyServiceConfiguration>
            {

            }
        };

        public static readonly ServerFixtureConfiguration SqliteInMemoryDb = new ServerFixtureConfiguration
        {
            EnableTestAuthentication = true,
            StartupType = typeof(Startup),
            MainServicePostAppConfiguration = (configuration, testContext) =>
            {
                if (TestEnvironment.UseInMemoryDatabase == false)
                {
                    testContext.Output.WriteLine("Force to use database instead of in-memory database");
                    var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    var configurationValues = new Dictionary<string, string>
                    {
                        ["ConnectionStrings:AssetDb"] = testContext.DatabaseFixture.GetConnectionString(databaseName),
                    };
                    configuration.AddInMemoryCollection(configurationValues);
                }
            },
            MainServicePostConfigureServices = (services) =>
            {
                if (TestEnvironment.UseInMemoryDatabase)
                {
                    services.ReplaceScoped(GetSqliteInMemoryOptions<AssetDbContext>());
                    services.ReplaceScoped(GetSqliteInMemoryOptions<MappingDbContext>());
                    services.ReplaceSingleton<IDbUpgradeChecker, SqliteInMemoryDbUpgradeChecker>();
                }
            },
            DependencyServices = new List<DependencyServiceConfiguration>
            {

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

        public static Func<IServiceProvider, DbContextOptions<T>> GetSqliteInMemoryOptions<T>() where T : DbContext
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            return (_) =>
            {
                var builder = new DbContextOptionsBuilder<T>();
                builder.UseSqlite(connection);
                return builder.Options;
            };
        }
    }

    public class SqliteInMemoryDbUpgradeChecker : IDbUpgradeChecker
    {
        public void EnsureDatabaseUpToDate(IWebHostEnvironment env)
        {
            // Do nothing
        }
    }
}
