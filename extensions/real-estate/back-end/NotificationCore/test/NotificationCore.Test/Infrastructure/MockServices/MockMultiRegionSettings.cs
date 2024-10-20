
using NotificationCore.Infrastructure.MultiRegion;

namespace NotificationCore.Test.Infrastructure.MockServices;
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
