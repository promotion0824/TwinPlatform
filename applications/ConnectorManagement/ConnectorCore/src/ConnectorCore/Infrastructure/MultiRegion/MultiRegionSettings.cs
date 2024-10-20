namespace Willow.Infrastructure.MultiRegion
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;

    internal class MultiRegionSettings : IMultiRegionSettings
    {
        private readonly Dictionary<string, RegionSettings> regions = new Dictionary<string, RegionSettings>();

        public MultiRegionSettings(IConfiguration configuration)
        {
            ReadSettings(configuration);
        }

        public IEnumerable<string> RegionIds => regions.Keys;

        public IEnumerable<RegionSettings> Regions => regions.Values;

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
                    MachineToMachineAuthentication = regionConfiguration.GetConfigValue<MachineToMachineAuthentication>("MachineToMachineAuthentication"),
                };
                regions.Add(regionSettings.Id, regionSettings);
            }
        }
    }

    internal class RegionSettings
    {
        public string Id { get; set; }

        public MachineToMachineAuthentication MachineToMachineAuthentication { get; set; }
    }

    internal class MachineToMachineAuthentication
    {
        public string Domain { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string Audience { get; set; }
    }
}
