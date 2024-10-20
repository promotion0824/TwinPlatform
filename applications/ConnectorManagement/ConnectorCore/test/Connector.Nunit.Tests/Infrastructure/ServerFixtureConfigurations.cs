namespace Connector.Nunit.Tests.Infrastructure
{
    using System.Collections.Generic;
    using Connector.Nunit.Tests.Infrastructure.Abstractions;
    using ConnectorCore;
    using ConnectorCore.Database;
    using ConnectorCore.Services;
    using DbUp;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class ServerFixtureConfigurations
    {
        public static readonly ServerFixtureConfiguration InMemoryDb = new ServerFixtureConfiguration
        {
            EnableTestAuthentication = true,
            MainServicePostConfigureServices = services =>
            {
                services.AddTransient<IDatabaseUpdater, InMemoryDbUpdater>();
                services.AddTransient<IDatabaseEraser, InMemoryDbEraser>();
            },
            MainServicePostAppConfiguration = (builder) =>
            {
                var config = new Dictionary<string, string>
                {
                    ["EnableSwagger"] = "true"
                };
                builder.AddInMemoryCollection(config);
            },
        };

        public static readonly ServerFixtureConfiguration SqlServer = new ServerFixtureConfiguration
        {
            EnableTestAuthentication = true,
            MainServicePostConfigureServices = services =>
            {
                services.AddTransient<IDatabaseUpdater, DatabaseUpdaterInitializer>();
                services.AddTransient<IDatabaseEraser, SqlDatabaseEraser>();

                services.AddTransient<IIotRegistrationService, TestIotRegistrationService>();
                services.AddTransient<IEventNotificationService, TestEventNotificationService>();
                services.AddTransient<IDigitalTwinService, TestDigitalTwinService>();
            },
            MainServicePostAppConfiguration = (builder) =>
            {
                var config = new Dictionary<string, string>
                {
                    ["EnableSwagger"] = "true"
                };
                builder.AddInMemoryCollection(config);
            },
        };
    }

    public class InMemoryDbUpdater : IDatabaseUpdater
    {
        public void DeployDatabaseChanges(ILoggerFactory loggerFactory, bool isDevEnvironment)
        {
        }
    }

    public class InMemoryDbEraser : IDatabaseEraser
    {
        public void EraseDb()
        {
        }
    }

    public class SqlDatabaseEraser : IDatabaseEraser
    {
        private readonly IDbConnectionStringProvider connectionStringProvider;

        public SqlDatabaseEraser(IDbConnectionStringProvider connectionStringProvider)
        {
            this.connectionStringProvider = connectionStringProvider;
        }

        public void EraseDb()
        {
            DropDatabase.For.SqlDatabase(connectionStringProvider.GetConnectionString());
        }
    }
}
