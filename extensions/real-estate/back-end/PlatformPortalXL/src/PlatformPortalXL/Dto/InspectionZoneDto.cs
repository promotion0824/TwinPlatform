using System;
using System.Collections.Generic;
using System.Linq;

using Willow.Workflow;

namespace PlatformPortalXL.Dto
{
    public class InspectionZoneDto
    {
        public Guid Id { get; set; }
        public Guid SiteId { get; set; }
        public string Name { get; set; }
        public int? CheckCount { get; set; }
        public DateTime? LastUpdated { get; set; }
        public InspectionZoneStatistics Statistics { get; set; }
        public List<InspectionDto> Inspections { get; set; }
        public int? InspectionCount { get; set; }

        public static InspectionZoneDto MapFromModel(InspectionZone model)
        {
            return new InspectionZoneDto
            {
                Id = model.Id,
                SiteId = model.SiteId,
                Name = model.Name,
                CheckCount = model.Statistics?.CheckCount,
                LastUpdated = model.Statistics?.LastCheckSubmittedDate,
                Statistics = model.Statistics,
                InspectionCount = model.Statistics?.InspectionCount
            };
        }
        public static InspectionZoneDto MapFromModel(SimpleInspectionZone model)
        {
            return new InspectionZoneDto
            {
                Id = model.Id,
                SiteId = model.SiteId,
                Name = model.Name
            };
        }
        public static List<InspectionZoneDto> MapFromModels(IEnumerable<InspectionZone> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }
}
