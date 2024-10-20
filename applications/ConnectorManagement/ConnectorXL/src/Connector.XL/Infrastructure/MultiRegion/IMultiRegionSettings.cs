namespace Connector.XL.Infrastructure.MultiRegion;

internal interface IMultiRegionSettings
{
    IEnumerable<string> RegionIds { get; }

    IEnumerable<RegionSettings> Regions { get; }
}
