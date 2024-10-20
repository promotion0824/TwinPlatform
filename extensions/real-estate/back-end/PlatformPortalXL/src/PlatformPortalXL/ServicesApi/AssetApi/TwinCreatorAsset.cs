using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.ServicesApi.AssetApi
{
    public class TwinCreatorAsset
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? EquipmentId { get; set; }
        public Guid? FloorId { get; set; }
        public string Identifier { get; set; }
        public string ForgeViewerModelId { get; set; }
        public Guid CategoryId { get; set; }
        public List<double> Geometry { get; set; } = new List<double>();
        public List<AssetProperty> AssetParameters { get; set; } = new List<AssetProperty>();
        public string ModuleTypeNamePath { get; set; }

        public static Asset MapToModel(TwinCreatorAsset twinCreatorAsset, Equipment equipment)
        {
            if (twinCreatorAsset == null)
            {
                return null;
            }

            return new Asset
            {
                Id = twinCreatorAsset.Id,
                Name = twinCreatorAsset.Name,
                EquipmentId = twinCreatorAsset.EquipmentId,
                HasLiveData = twinCreatorAsset.EquipmentId.HasValue,
                CategoryId = twinCreatorAsset.CategoryId,
                FloorId = twinCreatorAsset.FloorId,
                Tags = equipment.Tags,
                PointTags = equipment.PointTags.Where(pt => "2d".Equals(pt.Feature, StringComparison.InvariantCultureIgnoreCase)).ToList(),
                Points = AssetPoint.MapFrom(equipment.Points),
                Properties = twinCreatorAsset.AssetParameters,
                Geometry = twinCreatorAsset.Geometry,
                Identifier = twinCreatorAsset.Identifier,
                ForgeViewerModelId = twinCreatorAsset.ForgeViewerModelId,
                ModuleTypeNamePath = twinCreatorAsset.ModuleTypeNamePath
            };
        }

        public static Asset MapToModel(Equipment equipment)
        {
            if (equipment == null)
            {
                return null;
            }

            return new Asset
            {
                Id = equipment.Id,
                EquipmentId = equipment.Id,
                Name = equipment.Name,
                HasLiveData = true,
                Tags = equipment.Tags,
                PointTags = equipment.PointTags?.Where(pt => "2d".Equals(pt.Feature, StringComparison.InvariantCultureIgnoreCase)).ToList(),
                Points = AssetPoint.MapFrom(equipment.Points),
                FloorId = equipment.FloorId
            };
        }
    }
}
