using System;
using System.Collections.Generic;
using Willow.Infrastructure.MultiRegion;

namespace Willow.Tests.Infrastructure.MockServices
{
    public class MockMultiRegionSettings : IMultiRegionSettings
    {
        private readonly ServerFixtureConfiguration _serverFixtureConfiguration;

        public IEnumerable<string> RegionIds => _serverFixtureConfiguration.RegionIds ?? new string[0];

        public IEnumerable<RegionSettings> Regions => throw new NotImplementedException();

        public MockMultiRegionSettings(ServerFixtureConfiguration serverFixtureConfiguration)
        {
            _serverFixtureConfiguration = serverFixtureConfiguration;
        }
    }
}