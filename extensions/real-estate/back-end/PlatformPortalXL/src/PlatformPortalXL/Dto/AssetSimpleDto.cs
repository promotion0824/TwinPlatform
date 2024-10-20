using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.Dto
{
    public class AssetSimpleDto
    {
        public Guid Id { get; set; }
        public string TwinId { get; set; }
        public Guid? EquipmentId { get; set; }
        public string Name { get; set; }
        public bool HasLiveData { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<TagDto> PointTags { get; set; } = new List<TagDto>();
        public bool IsEquipmentOnly { get; set; }
        public string Identifier { get; set; }
        public string EquipmentName { get; set; }
        public string ForgeViewerModelId { get; set; }
        public string[] ModuleTypeNamePath { get; set; }
        public string FloorCode { get; set; }
        public List<AssetProperty> Properties { get; set; }
		public Guid? FloorId { get; set; }

		public static AssetSimpleDto MapFromModel(Asset asset)
        {
            if (asset == null)
            {
                return null;
            }

            return new AssetSimpleDto
            {
                Id = asset.Id,
                TwinId = asset.TwinId,
                EquipmentId = asset.EquipmentId,
                Name = asset.Name,
                HasLiveData = asset.EquipmentId.HasValue,
                Tags = asset.Tags?.Select(t => t.Name).ToList(),
                PointTags = TagDto.MapFrom(asset.PointTags),
                IsEquipmentOnly = asset.EquipmentId.HasValue && asset.Id == asset.EquipmentId.Value,
                Identifier = asset.Identifier,
                EquipmentName = asset.EquipmentName,
                ForgeViewerModelId = asset.ForgeViewerModelId,
                ModuleTypeNamePath = asset.ModuleTypeNamePath.SplitModulePaths(),
                Properties = asset.Properties,
				FloorId = asset.FloorId
            };
        }

        public static List<AssetSimpleDto> MapFromModels(IEnumerable<Asset> assets)
        {
            return assets?.Select(MapFromModel).ToList();
        }
    }
}
