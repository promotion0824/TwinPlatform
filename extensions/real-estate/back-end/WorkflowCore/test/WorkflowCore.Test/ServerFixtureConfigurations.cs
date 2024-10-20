using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Willow.Tests.Infrastructure;
using WorkflowCore.Database;
using WorkflowCore.Entities;
using WorkflowCore.Entities.Interceptors;
using WorkflowCore.Services;
using WorkflowCore.Services.Apis;

namespace WorkflowCore.Test
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
                    ["ConnectionStrings:WorkflowCore"] = testContext.DatabaseFixture.GetConnectionString(databaseName),
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
                configuration.AddJsonFile("appsettings.json");
                
                if (TestEnvironment.UseInMemoryDatabase == false)
                {
                    testContext.Output.WriteLine("Force to use database instead of in-memory database");
                    var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    var configurationValues = new Dictionary<string, string>
                    {
                        ["ConnectionStrings:WorkflowCore"] = testContext.DatabaseFixture.GetConnectionString(databaseName),
                        ["WorkflowOnScheduleUrl"] = "http://localhost/tickettemplates",
                    };
                    configuration.AddInMemoryCollection(configurationValues);
                }
                else
                {
                    var configurationValues = new Dictionary<string, string>
                    {
                        ["WorkflowOnScheduleUrl"] = "http://localhost/tickettemplates"
                    };
                    configuration.AddInMemoryCollection(configurationValues);
                }
            },
            MainServicePostConfigureServices = (services) =>
            {
                if (TestEnvironment.UseInMemoryDatabase)
                {
                    var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    services.ReplaceScoped(GetInMemoryOptions<WorkflowContext>(databaseName));
                    services.ReplaceScoped<IDbUpgradeChecker>(_ => new InMemoryDbUpgradeChecker());
                }
            },
            DependencyServices = new List<DependencyServiceConfiguration>
            {
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.ImageHub
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DirectoryCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.SiteCore
                },
                new DependencyServiceConfiguration
                {
	                Name = ApiServiceNames.DigitalTwinCore
                },
				new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DynamicsIntegrationForTicketCreation
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DynamicsIntegrationForTicketUpdate
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.InsightCore
                }
            }
        };

        public static readonly ServerFixtureConfiguration InMemoryDbWithInspectionHostedJobSetting = new ServerFixtureConfiguration
		{
			EnableTestAuthentication = true,
			StartupType = typeof(Startup),
			MainServicePostAppConfiguration = (configuration, testContext) =>
			{
				configuration.AddJsonFile("appsettings.json");
				var configurationValues = new Dictionary<string, string>
				{
                    ["BackgroundJobOptions:Inspection:EnableProcess"] = "true",
                    ["BackgroundJobOptions:Inspection:BatchSize"] = "2",
					["WorkflowOnScheduleUrl"] = "http://localhost/tickettemplates"

				};
				
				configuration.AddInMemoryCollection(configurationValues);
			},
			MainServicePostConfigureServices = (services) =>
			{
				if (TestEnvironment.UseInMemoryDatabase)
				{
					var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
					services.ReplaceScoped(GetInMemoryOptions<WorkflowContext>(databaseName));
					services.ReplaceScoped<IDbUpgradeChecker>(_ => new InMemoryDbUpgradeChecker());
				}
			},
			DependencyServices = new List<DependencyServiceConfiguration>
			{
				new DependencyServiceConfiguration
				{
					Name = ApiServiceNames.ImageHub
				},
				new DependencyServiceConfiguration
				{
					Name = ApiServiceNames.DirectoryCore
				},
				new DependencyServiceConfiguration
				{
					Name = ApiServiceNames.SiteCore
				},
				new DependencyServiceConfiguration
				{
					Name = ApiServiceNames.DynamicsIntegrationForTicketCreation
				},
				new DependencyServiceConfiguration
				{
					Name = ApiServiceNames.DynamicsIntegrationForTicketUpdate
				},
				new DependencyServiceConfiguration
				{
					Name = ApiServiceNames.DigitalTwinCore
				},
				new DependencyServiceConfiguration
				{
					Name = ApiServiceNames.SiteCore
				}
			}
		};

        public static readonly ServerFixtureConfiguration InMemoryDbWithTicketHostedJobSetting = new ServerFixtureConfiguration
        {
            EnableTestAuthentication = true,
            StartupType = typeof(Startup),
            MainServicePostAppConfiguration = (configuration, testContext) =>
            {
                configuration.AddJsonFile("appsettings.json");
                var configurationValues = new Dictionary<string, string>
                {
                    ["BackgroundJobOptions:Ticket:EnableProcess"] = "true",
                    ["BackgroundJobOptions:Ticket:BatchSize"] = "2",
                    ["WorkflowOnScheduleUrl"] = "http://localhost/tickettemplates"

                };

                configuration.AddInMemoryCollection(configurationValues);
            },
            MainServicePostConfigureServices = (services) =>
            {
                if (TestEnvironment.UseInMemoryDatabase)
                {
                    var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    services.ReplaceScoped(GetInMemoryOptions<WorkflowContext>(databaseName));
                    services.ReplaceScoped<IDbUpgradeChecker>(_ => new InMemoryDbUpgradeChecker());
                }
            },
            DependencyServices = new List<DependencyServiceConfiguration>
            {
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.ImageHub
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DirectoryCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.SiteCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DynamicsIntegrationForTicketCreation
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DynamicsIntegrationForTicketUpdate
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DigitalTwinCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.SiteCore
                }
            }
        };

        public static readonly ServerFixtureConfiguration InMemoryDbWithTicketTemplateHostedJobSetting = new ServerFixtureConfiguration
        {
            EnableTestAuthentication = true,
            StartupType = typeof(Startup),
            MainServicePostAppConfiguration = (configuration, testContext) =>
            {
                configuration.AddJsonFile("appsettings.json");
                var configurationValues = new Dictionary<string, string>
                {
                    ["BackgroundJobOptions:TicketTemplate:EnableProcess"] = "true",
                    ["BackgroundJobOptions:TicketTemplate:BatchSize"] = "2",
                    ["WorkflowOnScheduleUrl"] = "http://localhost/tickettemplates"

                };

                configuration.AddInMemoryCollection(configurationValues);
            },
            MainServicePostConfigureServices = (services) =>
            {
                if (TestEnvironment.UseInMemoryDatabase)
                {
                    var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    services.ReplaceScoped(GetInMemoryOptions<WorkflowContext>(databaseName));
                    services.ReplaceScoped<IDbUpgradeChecker>(_ => new InMemoryDbUpgradeChecker());
                }
            },
            DependencyServices = new List<DependencyServiceConfiguration>
            {
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.ImageHub
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DirectoryCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.SiteCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DynamicsIntegrationForTicketCreation
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DynamicsIntegrationForTicketUpdate
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DigitalTwinCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.SiteCore
                }
            }
        };


        public static readonly ServerFixtureConfiguration ApplicationSettings = new ServerFixtureConfiguration
        {
            StartupType = typeof(Startup),
            MainServicePostAppConfiguration = (configuration, testContext) =>
            {
                var configurationValues = new Dictionary<string, string>
                {
                    ["WorkflowOnScheduleUrl"] = "http://localhost/tickettemplates",
                };
                configuration.AddInMemoryCollection(configurationValues);
            }
        };

        public static Func<IServiceProvider, DbContextOptions<T>> GetInMemoryOptions<T>(string dbName) where T : DbContext
        {
			return (serviceProvider) =>
            {
                var builder = new DbContextOptionsBuilder<T>();
                builder.UseInMemoryDatabase(databaseName: dbName);
                builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
				builder.AddInterceptors(new AuditTrailInterceptor(serviceProvider.GetRequiredService<ISessionService>()));
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

        public static readonly ServerFixtureConfiguration InMemoryWithMappedIntegration = new ServerFixtureConfiguration
        {
            EnableTestAuthentication = true,
            StartupType = typeof(Startup),
            MainServicePostAppConfiguration = (configuration, testContext) =>
            {
                configuration.AddJsonFile("appsettings.json");
                var configurationValues = new Dictionary<string, string>
                {
                    ["MappedIntegrationConfiguration:IsEnabled"] = "true",
                    ["MappedIntegrationConfiguration:SourceName"] = "CMMS Name",
                };

                configuration.AddInMemoryCollection(configurationValues);
            },
            MainServicePostConfigureServices = (services) =>
            {
                if (TestEnvironment.UseInMemoryDatabase)
                {
                    var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    services.ReplaceScoped(GetInMemoryOptions<WorkflowContext>(databaseName));
                    services.ReplaceScoped<IDbUpgradeChecker>(_ => new InMemoryDbUpgradeChecker());
                    services.RemoveAll<IHostedService>();
                }
            },
            DependencyServices = new List<DependencyServiceConfiguration>
            {
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.ImageHub
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DirectoryCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.SiteCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DynamicsIntegrationForTicketCreation
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DynamicsIntegrationForTicketUpdate
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DigitalTwinCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.SiteCore
                },

                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.InsightCore
                }
            }
        };

        public static readonly ServerFixtureConfiguration InMemoryWithReadOnlyMappedIntegration = new ServerFixtureConfiguration
        {
            EnableTestAuthentication = true,
            StartupType = typeof(Startup),
            MainServicePostAppConfiguration = (configuration, testContext) =>
            {
                configuration.AddJsonFile("appsettings.json");
                var configurationValues = new Dictionary<string, string>
                {
                    ["MappedIntegrationConfiguration:IsEnabled"] = "true",
                    ["MappedIntegrationConfiguration:SourceName"] = "CMMS Name",
                    ["MappedIntegrationConfiguration:isReadOnly"] = "true",
                };

                configuration.AddInMemoryCollection(configurationValues);
            },
            MainServicePostConfigureServices = (services) =>
            {
                if (TestEnvironment.UseInMemoryDatabase)
                {
                    var databaseName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    services.ReplaceScoped(GetInMemoryOptions<WorkflowContext>(databaseName));
                    services.ReplaceScoped<IDbUpgradeChecker>(_ => new InMemoryDbUpgradeChecker());
                    services.RemoveAll<IHostedService>();
                }
            },
            DependencyServices = new List<DependencyServiceConfiguration>
            {
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.ImageHub
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DirectoryCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.SiteCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DynamicsIntegrationForTicketCreation
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DynamicsIntegrationForTicketUpdate
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DigitalTwinCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.SiteCore
                },

                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.InsightCore
                }
            }
        };
    }
}
