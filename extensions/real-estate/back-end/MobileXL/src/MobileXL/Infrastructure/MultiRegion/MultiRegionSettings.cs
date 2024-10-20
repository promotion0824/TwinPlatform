using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MobileXL.Infrastructure;

namespace Willow.Infrastructure.MultiRegion
{
    public interface IMultiRegionSettings
    {
        IEnumerable<string> RegionIds { get; }
        IEnumerable<RegionSettings> Regions { get; }
    }

    public class MultiRegionSettings : IMultiRegionSettings
    {
        private readonly Dictionary<string, RegionSettings> _regions = new Dictionary<string, RegionSettings>();

        public IEnumerable<string> RegionIds => _regions.Keys;

        public IEnumerable<RegionSettings> Regions => _regions.Values;

        public MultiRegionSettings(IConfiguration configuration)
        {
            ReadSettings(configuration);
        }

        private void ReadSettings(IConfiguration configuration)
        {
            var regionsConfiguration = configuration.GetSection("Regions");
            if (regionsConfiguration == null)
            {
                return;
            }

            foreach (var regionConfiguration in regionsConfiguration.GetChildren())
            {
                var regionSettings = new RegionSettings
                {
                    Id = regionConfiguration.Key,
                    MachineToMachineAuthentication = regionConfiguration.GetConfigValue<MachineToMachineAuthentication>("MachineToMachineAuthentication")
                };
                _regions.Add(regionSettings.Id, regionSettings);
            }
        }
    }

    public class RegionSettings
    {
        public string Id { get; set; }
        public MachineToMachineAuthentication MachineToMachineAuthentication { get; set; }
    }

    public class MachineToMachineAuthentication
    {
        public string Domain { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Audience { get; set; }
    }
}
