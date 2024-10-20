using System.Collections.Generic;
using Willow.Tests.Infrastructure;
using MobileXL.Services.Apis;

namespace MobileXL.Test
{
    public class ServerFixtureConfigurations
    {
        public static readonly ServerFixtureConfiguration Default = new ServerFixtureConfiguration
        {
            EnableTestAuthentication = true,
            StartupType = typeof(Startup),
            MainServicePostAppConfiguration = (configuration, testContext) =>
            {
            },
            MainServicePostConfigureServices = (services) =>
            {
            },
            DependencyServices = new List<DependencyServiceConfiguration>
            {
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
                    Name = ApiServiceNames.ConnectorCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.LiveDataCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.MarketPlaceCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.InsightCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.WorkflowCore
                },
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DigitalTwinCore
                }
            }
        };

        public static readonly ServerFixtureConfiguration DefaultWithoutTestAuthentication = new ServerFixtureConfiguration
        {
            EnableTestAuthentication = false,
            StartupType = typeof(Startup),
            MainServicePostAppConfiguration = (configuration, testContext) =>
            {
            },
            MainServicePostConfigureServices = (services) =>
            {
            },
            DependencyServices = new List<DependencyServiceConfiguration>
            {
                new DependencyServiceConfiguration
                {
                    Name = ApiServiceNames.DirectoryCore
                }
            }
        };
    }
}
