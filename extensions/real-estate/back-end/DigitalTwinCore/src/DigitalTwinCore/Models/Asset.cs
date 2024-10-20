using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinCore.Models
{
    public class Asset
    {
        public string ModelId { get; set; }
        public string TwinId { get; set; }
        public Guid Id { get; set; } // Willow assigned 
        public string Name { get; set; } //  not uniq
        public string CategoryName { get; set; } // dtmi
        public string Identifier { get; set; } // from 1 or 3 properties in the twin (extId...)

        public string ForgeViewerModelId { get; set; } // 2d/3d geometry
        public string ModuleTypeNamePath { get; set; }
        public List<double> Geometry { get; set; } = new List<double>();

        public Guid CategoryId { get; set; }
        public Guid? FloorId { get; set; } // SiteCore FloorId
        public List<Tag> Tags { get; set; } = new List<Tag>();
        public Dictionary<string, Property> Properties { get; set; } = new Dictionary<string, Property>();
        public IEnumerable<AssetRelationship> Relationships { get; set; } = new List<AssetRelationship>();
        public Dictionary<string, List<TwinWithRelationships>> DetailedRelationships { get; set; } = new Dictionary<string, List<TwinWithRelationships>>(); // Investa-specific
        public IEnumerable<Point> Points { get; set; } = new List<Point>();
        public List<Tag> PointTags =>
                            // Note: We can't just use Points.SelectMany(p => p.Tags).Distinct() here because it will use the default 
                            //   equality comparer (object.Equals) which is not overridden, so all duplicates will remain
                            Points.SelectMany(p => p.Tags).GroupBy(t => t.Name).Select(grp => grp.First()).ToList();
        public Guid? ParentId => Relationships.FirstOrDefault(r => r.Name == Constants.Relationships.IsPartOf)?.TargetId;
    }
}
