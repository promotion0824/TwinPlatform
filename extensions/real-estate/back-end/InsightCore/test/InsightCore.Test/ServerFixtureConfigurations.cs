using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Willow.Tests.Infrastructure;
using Microsoft.Extensions.Configuration;
using InsightCore;
using InsightCore.Constants;
using InsightCore.Database;
using InsightCore.Entities;
using InsightCore.Services;
using InsightCore.Test.MockServices;

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
                    ["ConnectionStrings:InsightDb"] = testContext.DatabaseFixture.GetConnectionString(databaseName),
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
                var configurationValues = new Dictionary<string, string>
                {
                    ["InspectionOptions:RuleName"] = "Inspection Value Out of Range",
                    ["InspectionOptions:RuleId"] = "inspection-value-out-of-range-",
                    ["WillowActivateOptions:AppId"] = "aaf0a355-739d-4dfc-92b9-01da4aabe9e9",
                    ["WillowActivateOptions:AppName"] = "Willow Activate",
                    ["MappedIntegrationConfiguration:AppId"] = "aaf0a355-739d-4dfc-92b9-01da4aabe9e8",
                    ["MappedIntegrationConfiguration:AppName"] = "Mapped",
                    ["IsNotificationEnabled"] = "true"
                };
                if (TestEnvironment.UseInMemoryDatabase == false)
                {
                    testContext.Output.WriteLine("Force to use database instead of in-memory database");
                    var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    configurationValues.Add("ConnectionStrings:InsightDb", testContext.DatabaseFixture.GetConnectionString(databaseName));
                
                }
                configuration.AddInMemoryCollection(configurationValues);
            },
            MainServicePostConfigureServices = (services) =>
            {
                if (TestEnvironment.UseInMemoryDatabase)
                {
                    var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    services.ReplaceScoped(GetInMemoryOptions<InsightDbContext>(databaseName));
                    services.ReplaceScoped<IDbUpgradeChecker>(_ => new InMemoryDbUpgradeChecker());
                }
                services.ReplaceSingleton<IAnalyticsService, MockAnalyticsService>();
            },
            DependencyServices = new List<DependencyServiceConfiguration>
            {
                new DependencyServiceConfiguration
                {
                    Name = "TripPin"
                },
                new DependencyServiceConfiguration
                {
	                Name = ApiServiceNames.DigitalTwinCore
                },
                new DependencyServiceConfiguration
                {
	                Name = ApiServiceNames.WorkflowCore
                }
            }
        };
        public static readonly ServerFixtureConfiguration InMemoryDbWithHostedJobSetting = new ServerFixtureConfiguration
        {
            EnableTestAuthentication = true,
            StartupType = typeof(Startup),
            MainServicePostAppConfiguration = (configuration, testContext) =>
            {
                var configurationValues = new Dictionary<string, string>
                {
                    ["BackgroundJobOptions:BatchSize"] = "20",
                    ["WillowActivateOptions:AppId"] = "aaf0a355-739d-4dfc-92b9-01da4aabe9e9",
                    ["WillowActivateOptions:AppName"] = "Willow Activate",
                    ["MappedIntegrationConfiguration:AppId"] = "aaf0a355-739d-4dfc-92b9-01da4aabe9e8",
                    ["MappedIntegrationConfiguration:AppName"] = "Mapped"
                };
                if (TestEnvironment.UseInMemoryDatabase == false)
                {
                    testContext.Output.WriteLine("Force to use database instead of in-memory database");
                    var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    configurationValues.Add(
                        "ConnectionStrings:InsightDb", testContext.DatabaseFixture.GetConnectionString(databaseName));

                }
                configuration.AddInMemoryCollection(configurationValues);
            },
            MainServicePostConfigureServices = (services) =>
            {
                if (TestEnvironment.UseInMemoryDatabase)
                {
                    var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    services.ReplaceScoped(GetInMemoryOptions<InsightDbContext>(databaseName));
                    services.ReplaceScoped<IDbUpgradeChecker>(_ => new InMemoryDbUpgradeChecker());
                }
                services.ReplaceSingleton<IAnalyticsService, MockAnalyticsService>();
            },
            DependencyServices = new List<DependencyServiceConfiguration>
            {
                new DependencyServiceConfiguration
                {
                    Name = "TripPin"
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DigitalTwinCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.WorkflowCore
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
