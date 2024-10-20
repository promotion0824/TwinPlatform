using System;
using System.Collections.Generic;

namespace PlatformPortalXL.Models
{
    public class Asset
    {
        public string ModelId { get; set; }
        public Guid Id { get; set; }
        public string TwinId { get; set; }
        public string Name { get; set; }
        public Guid? EquipmentId { get; set; }
        public Guid? FloorId { get; set; }
        public string Identifier { get; set; }
        public List<Tag> Tags { get; set; } = new List<Tag>();
        public List<Tag> PointTags { get; set; } = new List<Tag>();
        public string EquipmentName { get; set; }
        public string ForgeViewerModelId { get; set; }
        public Guid CategoryId { get; set; }
        public bool HasLiveData { get; set; }
        public List<AssetProperty> Properties { get; set; }
        public List<double> Geometry { get; set; }
        public List<AssetPoint> Points { get; set; }
        public string ModuleTypeNamePath { get; set; }
    }
}
