using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.Dto
{
    public class PointSimpleDto
    {
        public Guid Id { get; set; }
        public Guid EntityId { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public PointType Type { get; set; }
        public Guid EquipmentId { get; set; }
        public string ExternalPointId { get; set; }
        public bool? HasFeaturedTags { get; set; }

        public static PointSimpleDto MapFrom(Point model, bool useDigitalTwinAsset)
        {
            return new PointSimpleDto
            {
                Id = model.Id,
                EntityId = model.EntityId,
                Name = model.Name,
                EquipmentId = model.EquipmentId,
                ExternalPointId = model.ExternalPointId,
                Unit = model.Unit,
                Type = model.Type,
                HasFeaturedTags = useDigitalTwinAsset ?
                                    model.DisplayPriority.HasValue && model.DisplayPriority.Value < AdtConstants.MaxDisplayPriority:
                                    model.Tags == null ? null : (bool?)model.Tags.Any(x => !string.IsNullOrEmpty(x.Feature))
            };
        }

        public static List<PointSimpleDto> MapFrom(IEnumerable<Point> points, bool useDigitalTwinAsset = false)
        {
            return points?.Select(p => MapFrom(p, useDigitalTwinAsset)).ToList();
        }
    }
}
