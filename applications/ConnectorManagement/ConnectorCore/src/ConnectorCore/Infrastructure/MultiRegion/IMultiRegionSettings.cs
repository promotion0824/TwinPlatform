namespace Willow.Infrastructure.MultiRegion;

using System.Collections.Generic;

internal interface IMultiRegionSettings
{
    IEnumerable<string> RegionIds { get; }

    IEnumerable<RegionSettings> Regions { get; }
}
