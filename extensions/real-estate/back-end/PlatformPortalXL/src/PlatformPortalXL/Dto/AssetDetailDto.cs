using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Platform.Localization;

namespace PlatformPortalXL.Dto
{
    public class AssetDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool HasLiveData { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<TagDto> PointTags { get; set; } = new List<TagDto>();
        public Guid? EquipmentId { get; set; }
        public string TwinId { get; set; }
        public Guid CategoryId { get; set; }
        public Guid? FloorId { get; set; }
        public IEnumerable<AssetPropertyDto> Properties { get; set; }
        public List<double> Geometry { get; set; } = new List<double>();
        public string Identifier { get; set; }
        public string ModuleTypeNamePath { get; set; }
        public string ForgeViewerModelId { get; set; }

        public static AssetDetailDto MapFromModel(Asset asset, IAssetLocalizer assetLocalizer)
        {
            return new AssetDetailDto
            {
                Id = asset.Id,
                Name = asset.Name,
                EquipmentId = asset.EquipmentId,
                TwinId = asset.TwinId,
                HasLiveData = asset.HasLiveData,
                CategoryId = asset.CategoryId,
                FloorId = asset.FloorId,
                Tags = asset.Tags.Select(s => s.Name).ToList(),
                PointTags = TagDto.MapFrom(asset.PointTags),
                Properties = AssetPropertyDto.MapFromModels(asset.Properties, assetLocalizer),
                Geometry = asset.Geometry,
                Identifier = asset.Identifier,
                ModuleTypeNamePath = asset.ModuleTypeNamePath,
                ForgeViewerModelId = asset.ForgeViewerModelId
            };
        }

        public static List<AssetDetailDto> MapFromModels(IEnumerable<Asset> assets, IAssetLocalizer assetLocalizer)
        {
            return assets.Select(a => MapFromModel(a, assetLocalizer)).ToList();
        }
    }
}
