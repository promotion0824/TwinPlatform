using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;

namespace PlatformPortalXL.Models
{
    public class AssetPoint
    {
        public Guid Id { get; set; }

        public string TwinId { get; set; }

        public string Name { get; set; }

        public string Unit { get; set; }

        public PointType Type { get; set; }

        public string ExternalId { get; set; }

        public IList<Tag> Tags { get; set; }

        public decimal? DisplayPriority { get; set; }

        public string DisplayName { get; set; }
        public Dictionary<string, DigitalTwinProperty> Properties { get; set; }

        public static AssetPoint MapFrom(Point point) =>
            new AssetPoint
            {
                Id = point.EntityId,
                Name = point.Name,
                Unit = point.Unit,
                Type = point.Type,
                ExternalId = point.ExternalPointId,
                Tags = point.Tags,
                DisplayPriority = point.DisplayPriority,
                DisplayName = point.DisplayName,
                Properties = point.Properties,
                TwinId = point.TwinId
            };

        public static AssetPoint MapFromTwinPoint(DigitalTwinPoint point)
        {
            point.Properties.TryGetValue("unit", out var unitProperty);
            return new AssetPoint
            {
                Id = point.TrendId,
                Name = point.Name,
                Unit = point.Unit,
                Type = point.Type,
                ExternalId = point.ExternalId,
                Tags = point.Tags,
                Properties = point.Properties,
                TwinId = point.TwinId
            };
        }

        public static List<AssetPoint> MapFrom(IEnumerable<Point> equipmentPoints) =>
            equipmentPoints?.Select(MapFrom).ToList();
    }
}
