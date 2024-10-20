using System;
using System.Collections.Generic;
using System.Linq;
using MobileXL.Models;

namespace MobileXL.Dto
{
    public class AssetDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool HasLiveData { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<TagDto> PointTags { get; set; } = new List<TagDto>();
        public Guid? EquipmentId { get; set; }
        public Guid CategoryId { get; set; }
        public Guid? FloorId { get; set; }
        public IEnumerable<AssetPropertyDto> Properties { get; set; }
        public List<double> Geometry { get; set; } = new List<double>();
        public string Identifier { get; set; }

        public static AssetDetailDto MapFromModel(Asset asset)
        {
            return new AssetDetailDto
            {
                Id = asset.Id,
                Name = asset.Name,
                EquipmentId = asset.EquipmentId,
                HasLiveData = asset.HasLiveData,
                CategoryId = asset.CategoryId,
                FloorId = asset.FloorId,
                Tags = asset.Tags.Select(s => s.Name).ToList(),
                PointTags = TagDto.MapFrom(asset.PointTags),
                Properties = AssetPropertyDto.MapFromModels(asset.Properties),
                Geometry = asset.Geometry,
                Identifier = asset.Identifier,
            };
        }
    }
}
