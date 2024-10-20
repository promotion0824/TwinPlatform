using DigitalTwinCore.Constants;
using DigitalTwinCore.Entities;
using DigitalTwinCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinCore.Services.AdtApi
{
    public class SiteAdtSettings
    {
        public Guid SiteId { get; set; }

        // Note: Site-specific models are currently not supported, so these do not vary:
        public string[] AssetModelIds { get; set; }
        public string[] DeviceModelIds { get; set; }
        public string[] PointModelIds { get; set; }
        public string[] SiteModelIds { get; set; }
        public string[] LevelModelIds { get; set; }
        public string[] DocumentModelIds { get; set; }
        public string[] SpaceModelIds { get; set; }
        public string[] BuildingComponentModelIds { get; set; }
        public string[] StructureModelIds { get; set; }
        public string[] CollectionModelIds { get; set; }
        public string[] ComponentModelIds { get; set; }
		public string[] AccountModelIds { get; set; }

		// These are loaded from the database:
		public AzureDigitalTwinsSettings InstanceSettings { get; set; }
        public string SiteCodeForModelId { get; set; }
        public string AdxDatabase { get; set; }

        public static SiteAdtSettings CreateInstance(Guid siteId, SiteSettingEntity entity = null, AzureDigitalTwinsSettings settings = null, string adxDatabase = null)
        {
            if (settings == null && entity == null)
            {
                throw new ArgumentNullException($"{nameof(settings)} and {nameof(entity)}", "AzureDigitalTwinsSettings or SiteSettingEntity must be provided.");
            }

            return new SiteAdtSettings
            {
                SiteId = siteId,
                AssetModelIds = new[] { WillowInc.AssetModelId },
                DeviceModelIds = new[] { WillowInc.DeviceModelId },
                LevelModelIds = new[] { WillowInc.LevelModelId },
                PointModelIds = new[] { WillowInc.PointModelId },
                SiteModelIds = new[] { WillowInc.SiteModelId },
                DocumentModelIds = new[] { WillowInc.DocumentModelId },
                SpaceModelIds = new[] { WillowInc.SpaceModelId },
                BuildingComponentModelIds = new[] { WillowInc.BuildingComponentModelId },
                StructureModelIds = new[] { WillowInc.StructureModelId },
                ComponentModelIds = new[] { WillowInc.ComponentModelId },
                CollectionModelIds = new[] { WillowInc.CollectionModelId },
                AccountModelIds = new[] { WillowInc.AccountModelId },
                SiteCodeForModelId = entity?.SiteCodeForModelId,
                AdxDatabase = entity?.AdxDatabase ?? adxDatabase,
                InstanceSettings = new AzureDigitalTwinsSettings
                {
                    ClientId = settings?.ClientId,
                    ClientSecret = settings?.ClientSecret ?? string.Empty,
                    TenantId = settings?.TenantId,
                    InstanceUri = !string.IsNullOrWhiteSpace(entity?.InstanceUri)
                        ? new Uri(entity.InstanceUri, UriKind.Absolute)
                        : settings?.InstanceUri
                }
            };
        }
    }

	public static class SiteAdtSettingsExtensions
	{
		public static IEnumerable<string> GetDatabases(this IEnumerable<SiteAdtSettings> siteSettings)
		{
			return siteSettings.Select(x => x.AdxDatabase).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
		}

		public static IEnumerable<Guid> GetSites(this IEnumerable<SiteAdtSettings> siteSettings)
		{
			return siteSettings.Select(x => x.SiteId).ToList();
		}
	}
}
