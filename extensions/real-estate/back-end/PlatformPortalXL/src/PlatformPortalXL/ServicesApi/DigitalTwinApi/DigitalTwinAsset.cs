using PlatformPortalXL.Extensions;
using PlatformPortalXL.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace PlatformPortalXL.ServicesApi.DigitalTwinApi
{
    public class DigitalTwinAsset
    {
        public string ModelId { get; set; }
        public string TwinId { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool HasLiveData { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Tag> PointTags { get; set; }
        public Guid CategoryId { get; set; }
        public Guid? FloorId { get; set; }
        public Dictionary<string, DigitalTwinProperty> Properties { get; set; }
        public List<double> Geometry { get; set; }
        public string Identifier { get; set; }
        public string ForgeViewerModelId { get; set; }
        public List<DigitalTwinPoint> Points { get; set; }
        public string ModuleTypeNamePath { get; set; }

        public static Asset MapToModel(DigitalTwinAsset digitalTwinAsset)
        {
            return new Asset
            {
                ModelId = digitalTwinAsset.ModelId,
                CategoryId = digitalTwinAsset.CategoryId,
                Id = digitalTwinAsset.Id,
                TwinId = digitalTwinAsset.TwinId,
                EquipmentId = (digitalTwinAsset.HasLiveData) ? digitalTwinAsset.Id : (Guid?)null,
                FloorId = digitalTwinAsset.FloorId,
                Geometry = digitalTwinAsset.Geometry,
                HasLiveData = digitalTwinAsset.HasLiveData,
                Identifier = digitalTwinAsset.Identifier,
                Name = digitalTwinAsset.Name,
                PointTags = digitalTwinAsset.PointTags ?? new List<Tag>(),
                Tags = digitalTwinAsset.Tags ?? new List<Tag>(),
                Properties = digitalTwinAsset.Properties?.Values.SelectMany(ConvertToAssetProperty).ToList() ?? new List<AssetProperty>(),
                ForgeViewerModelId = digitalTwinAsset.ForgeViewerModelId,
                Points = AssetPoint.MapFrom(digitalTwinAsset.Points?.Select(p => p.MapToModel(Guid.Empty))),
                ModuleTypeNamePath = digitalTwinAsset.ModuleTypeNamePath
            };
        }

        private static IEnumerable<AssetProperty> ConvertToAssetProperty(DigitalTwinProperty digitalTwinProperty)
        {
            return digitalTwinProperty switch
            {
                { DisplayName : AdtConstants.CustomPropertiesProperty } => MapCustomProperties(digitalTwinProperty),
                { Kind : DigitalTwinPropertyKind.Component } => MapComponent(digitalTwinProperty),
                _ => MapProperty(digitalTwinProperty)
            };
        }

        public static List<Asset> MapToModels(IEnumerable<DigitalTwinAsset> digitalTwinAssets)
        {
            return digitalTwinAssets?.Select(MapToModel).ToList();
        }

        private static IEnumerable<AssetProperty> MapCustomProperties(DigitalTwinProperty twinProperty)
        {
            var output = new List<AssetProperty>();
            using (JsonDocument document = JsonDocument.Parse(twinProperty.Value.ToString()))
            {
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    output.Add(new AssetProperty
                    {
                        DisplayName = property.Name,
                        Value = property.Value.ValueKind != JsonValueKind.Object
                                                            ? property.Value.ToString() as object
                                                            : property.Value.EnumerateObject()
                                                                            .ToDictionary(kv => kv.Name, kv => kv.Value.ToString())
                    });
                }
            }
            return output;
        }

        private static IEnumerable<AssetProperty> MapComponent(DigitalTwinProperty twinProperty)
        {
            var output = new List<AssetProperty>();
            using (JsonDocument document = JsonDocument.Parse(twinProperty.Value.ToString()))
            {
                var properties = document.RootElement.EnumerateObject().Where(x => x.Name != AdtConstants.MetadataProperty);
                if (properties.Any())
                    output.Add(new AssetProperty { 
                                    DisplayName = twinProperty.DisplayName, 
                                    Value = properties.ToDictionary(kv => kv.Name, kv => kv.Value.ToString())});
            }
            return output;
        }

        private static IEnumerable<AssetProperty> MapProperty(DigitalTwinProperty twinProperty)
        {
            return new List<AssetProperty> { new AssetProperty { DisplayName = twinProperty.DisplayName, Value = twinProperty.Value.FromNewtonsoftJsonObject() } };
        }
    }
}
