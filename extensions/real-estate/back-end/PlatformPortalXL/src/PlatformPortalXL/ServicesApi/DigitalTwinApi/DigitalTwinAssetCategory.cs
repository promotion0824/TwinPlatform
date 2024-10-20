using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.ServicesApi.DigitalTwinApi
{
    public class DigitalTwinAssetCategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<DigitalTwinAssetCategory> Categories { get; set; }
        public List<DigitalTwinAsset> Assets { get; set; }

        public static AssetCategory MapToModel(DigitalTwinAssetCategory digitalTwinCategory)
        {
            return new AssetCategory
            {
                Id = digitalTwinCategory.Id,
                Name = digitalTwinCategory.Name,
                Categories = digitalTwinCategory.Categories?.Select(MapToModel).ToList(),
                Assets = DigitalTwinAsset.MapToModels(digitalTwinCategory.Assets)
            };
        }

        public static List<AssetCategory> MapToModels(IEnumerable<DigitalTwinAssetCategory> digitalTwinCategories)
        {
            return digitalTwinCategories?.Select(MapToModel).ToList();
        }
    }
}
