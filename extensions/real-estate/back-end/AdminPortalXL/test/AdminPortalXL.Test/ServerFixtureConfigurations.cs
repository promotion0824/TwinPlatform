using System.Collections.Generic;
using Willow.Tests.Infrastructure;
using AdminPortalXL;
using AdminPortalXL.Services;

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
                    Name = ApiServiceNames.RegionalDirectoryCore,
                    IsMultiRegion = true
                }
            },
            RegionIds = new string[] { "region1", "region2" }
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

        public static readonly ServerFixtureConfiguration MultiRegionsForTestRegionCode = new ServerFixtureConfiguration
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
                    Name = ApiServiceNames.RegionalDirectoryCore,
                    IsMultiRegion = true
                }
            },
            RegionIds = new string[] { "region1", "aue2", "eu21", "weu3" }
        };
    }
}
