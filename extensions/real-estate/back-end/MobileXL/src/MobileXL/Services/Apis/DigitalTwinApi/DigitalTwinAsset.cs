using System;
using System.Collections.Generic;
using System.Linq;
using MobileXL.Models;

namespace MobileXL.Services.Apis.DigitalTwinApi
{
    public class DigitalTwinAsset
    {
        public Guid Id { get; set; }
        public string TwinId { get; set; }
        public string Name { get; set; }
        public bool HasLiveData { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Tag> PointTags { get; set; }
        public Guid CategoryId { get; set; }
        public Guid? FloorId { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public List<double> Geometry { get; set; }
        public string Identifier { get; set; }
        public string ForgeViewerModelId { get; set; }
        public List<AssetPoint> Points { get; set; }

        public static Asset MapToModel(DigitalTwinAsset digitalTwinAsset)
        {
            return new Asset
            {
                CategoryId = digitalTwinAsset.CategoryId,
                Id = digitalTwinAsset.Id,
                EquipmentId = (digitalTwinAsset.HasLiveData) ? digitalTwinAsset.Id : (Guid?)null,
                FloorId = digitalTwinAsset.FloorId,
                Geometry = digitalTwinAsset.Geometry,
                HasLiveData = digitalTwinAsset.HasLiveData,
                Identifier = digitalTwinAsset.Identifier,
                Name = digitalTwinAsset.Name,
                PointTags = digitalTwinAsset.PointTags ?? new List<Tag>(),
                Tags = digitalTwinAsset.Tags ?? new List<Tag>(),
                Properties = digitalTwinAsset.Properties?.Select(p => new AssetProperty { DisplayName = p.Key, Value = p.Value }).ToList() ?? new List<AssetProperty>(),
                ForgeViewerModelId = digitalTwinAsset.ForgeViewerModelId,
                Points = digitalTwinAsset.Points
            };
        }

        public static List<Asset> MapToModels(IEnumerable<DigitalTwinAsset> digitalTwinAssets)
        {
            return digitalTwinAssets?.Select(MapToModel).ToList();
        }
    }
}
