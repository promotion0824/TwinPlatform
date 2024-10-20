using System;
using System.Collections.Generic;
using System.Linq;
using MobileXL.Models;

namespace MobileXL.Dto
{
public class AssetSimpleDto
    {
        public Guid Id { get; set; }
        public Guid? EquipmentId { get; set; }
        public string Name { get; set; }
        public bool HasLiveData { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<TagDto> PointTags { get; set; } = new List<TagDto>();
        public bool IsEquipmentOnly { get; set; }
        public string Identifier { get; set; }
        public string EquipmentName { get; set; }
        public string ForgeViewerModelId { get; set; }

        public static AssetSimpleDto MapFromModel(Asset asset)
        {
            if (asset == null)
            {
                return null;
            }

            return new AssetSimpleDto
            {
                Id = asset.Id,
                EquipmentId = asset.EquipmentId,
                Name = asset.Name,
                HasLiveData = asset.EquipmentId.HasValue,
                Tags = asset.Tags?.Select(t => t.Name).ToList(),
                PointTags = TagDto.MapFrom(asset.PointTags),
                IsEquipmentOnly = asset.EquipmentId.HasValue && asset.Id == asset.EquipmentId.Value,
                Identifier = asset.Identifier,
                EquipmentName = asset.EquipmentName,
                ForgeViewerModelId = asset.ForgeViewerModelId
            };
        }

        public static List<AssetSimpleDto> MapFromModels(IEnumerable<Asset> assets)
        {
            return assets?.Select(MapFromModel).ToList();
        }
    }
}
