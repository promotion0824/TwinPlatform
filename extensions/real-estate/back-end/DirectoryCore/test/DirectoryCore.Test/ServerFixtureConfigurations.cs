using System;
using System.Collections.Generic;
using DirectoryCore.Database;
using DirectoryCore.Entities;
using DirectoryCore.Services;
using DirectoryCore.Services.Auth0;
using DirectoryCore.Services.AzureB2C;
using DirectoryCore.Test.MockServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Willow.Notifications.Interfaces;
using Willow.Tests.Infrastructure;

namespace DirectoryCore.Test
{
    public class ServerFixtureConfigurations
    {
        /// <summary>
        /// Return configuration values common across multiple fixtures. Add the
        /// connection string for DirectoryDb if it is passed in.
        /// </summary>
        private static Dictionary<string, string> GetConfigurationValues(string? connectionString)
        {
            var dict = new Dictionary<string, string>
            {
                ["Azure:KeyVault:KeyVaultName"] = "dummykeyvault",
            };
            if (connectionString != null)
            {
                dict["ConnectionStrings:DirectoryDb"] = connectionString;
            }
            return dict;
        }

        public static readonly ServerFixtureConfiguration SqlServer = new ServerFixtureConfiguration
        {
            EnableTestAuthentication = true,
            StartupType = typeof(Startup),
            MainServicePostAppConfiguration = (configuration, testContext) =>
            {
                var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                var connectionString = testContext.DatabaseFixture.GetConnectionString(
                    databaseName
                );
                var configurationValues = GetConfigurationValues(connectionString);
                configuration.AddInMemoryCollection(configurationValues);
            },
            MainServicePostConfigureServices = (services) =>
            {
                services.ReplaceSingleton<INotificationService, MockNotificationService>();
                services.ReplaceSingleton<IAuth0ManagementService, FakeAuth0ManagementService>();
                services.ReplaceSingleton<IAzureB2CService, FakeAzureB2CService>();
            }
        };

        public static readonly ServerFixtureConfiguration InMemoryDb =
            new ServerFixtureConfiguration
            {
                EnableTestAuthentication = true,
                StartupType = typeof(Startup),
                MainServicePostAppConfiguration = (configuration, testContext) =>
                {
                    if (TestEnvironment.UseInMemoryDatabase == false)
                    {
                        testContext.Output.WriteLine(
                            "Force to use database instead of in-memory database"
                        );
                        var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                        var connectionString = testContext.DatabaseFixture.GetConnectionString(
                            databaseName
                        );
                        var configurationValues = GetConfigurationValues(connectionString);
                        configuration.AddInMemoryCollection(configurationValues);
                    }
                    else
                    {
                        var configurationValues = GetConfigurationValues(null);
                        configuration.AddInMemoryCollection(configurationValues);
                    }
                },
                MainServicePostConfigureServices = (services) =>
                {
                    if (TestEnvironment.UseInMemoryDatabase)
                    {
                        var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                        services.ReplaceScoped(
                            GetInMemoryOptions<DirectoryDbContext>(databaseName)
                        );
                        services.ReplaceScoped<IDbUpgradeChecker>(
                            _ => new InMemoryDbUpgradeChecker()
                        );
                    }

                    services.ReplaceSingleton<INotificationService, MockNotificationService>();
                    services.ReplaceSingleton<
                        IAuth0ManagementService,
                        FakeAuth0ManagementService
                    >();
                    services.ReplaceSingleton<IAzureB2CService, FakeAzureB2CService>();
                },
                DependencyServices = new List<DependencyServiceConfiguration>
                {
                    new DependencyServiceConfiguration { Name = ApiServiceNames.Auth0 },
                    new DependencyServiceConfiguration { Name = ApiServiceNames.ImageHub },
                    new DependencyServiceConfiguration { Name = ApiServiceNames.SiteCore }
                }
            };

        public static readonly ServerFixtureConfiguration InMemoryDbWithoutMockAuth0 =
            new ServerFixtureConfiguration
            {
                EnableTestAuthentication = true,
                StartupType = typeof(Startup),
                MainServicePostAppConfiguration = (configuration, testContext) =>
                {
                    if (TestEnvironment.UseInMemoryDatabase == false)
                    {
                        testContext.Output.WriteLine(
                            "Force to use database instead of in-memory database"
                        );
                        var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                        var connectionString = testContext.DatabaseFixture.GetConnectionString(
                            databaseName
                        );
                        var configurationValues = GetConfigurationValues(connectionString);
                        configuration.AddInMemoryCollection(configurationValues);
                    }
                    else
                    {
                        var configurationValues = GetConfigurationValues(null);
                        configuration.AddInMemoryCollection(configurationValues);
                    }
                },
                MainServicePostConfigureServices = (services) =>
                {
                    if (TestEnvironment.UseInMemoryDatabase)
                    {
                        var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                        services.ReplaceScoped(
                            GetInMemoryOptions<DirectoryDbContext>(databaseName)
                        );
                        services.ReplaceScoped<IDbUpgradeChecker>(
                            _ => new InMemoryDbUpgradeChecker()
                        );
                    }

                    services.ReplaceSingleton<INotificationService, MockNotificationService>();
                },
                DependencyServices = new List<DependencyServiceConfiguration>
                {
                    new DependencyServiceConfiguration { Name = ApiServiceNames.Auth0 },
                    new DependencyServiceConfiguration { Name = ApiServiceNames.ImageHub }
                }
            };

        public static Func<IServiceProvider, DbContextOptions<T>> GetInMemoryOptions<T>(
            string dbName
        )
            where T : DbContext
        {
            return (_) =>
            {
                var builder = new DbContextOptionsBuilder<T>();
                builder.UseInMemoryDatabase(databaseName: dbName);
                builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
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
