using DigitalTwinCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinCore.Dto
{
    [Serializable]
    public class AssetDto
    {
        public string ModelId { get; set; }
        public string TwinId { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool HasLiveData { get; set; }
        public List<TagDto> Tags { get; set; } = new List<TagDto>();
        public List<TagDto> PointTags { get; set; } = new List<TagDto>();
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Guid? FloorId { get; set; }
        public Dictionary<string, Property> Properties { get; set; }
        public Dictionary<string, List<TwinWithRelationshipsDto>> Relationships { get; set; }
        public List<double> Geometry { get; set; } = new List<double>();
        public string Identifier { get; set; }
        public string ForgeViewerModelId { get; set; }
        public List<PointDto> Points { get; set; }
        public Guid? ParentId { get; set; }
        public string ModuleTypeNamePath { get; set; }

        internal static AssetDto MapFrom(Asset asset) =>
            (asset == null) ? null : new AssetDto
            {
                Id = asset.Id,
                TwinId = asset.TwinId,
                Name = asset.Name,
                HasLiveData = asset.Points.Any(),
                CategoryId = asset.CategoryId,
                FloorId = asset.FloorId,
                Tags = TagDto.MapFrom(asset.Tags),
                PointTags = TagDto.MapFrom(asset.PointTags),
                Properties = asset.Properties,
                Geometry = asset.Geometry,
                Identifier = asset.Identifier,
                ForgeViewerModelId = asset.ForgeViewerModelId,
                Points = PointDto.MapFrom(asset.Points.ToList(), false, false),
                ParentId = asset.ParentId,
                CategoryName = asset.CategoryName,
                ModuleTypeNamePath = asset.ModuleTypeNamePath,
                Relationships = asset.DetailedRelationships.ToDictionary(r => r.Key, r => TwinWithRelationshipsDto.MapFrom(r.Value))
            };

        internal static List<AssetDto> MapFrom(IEnumerable<Asset> assets) =>
            assets?.Select(MapFrom).ToList() ?? new List<AssetDto>();
    }
}
