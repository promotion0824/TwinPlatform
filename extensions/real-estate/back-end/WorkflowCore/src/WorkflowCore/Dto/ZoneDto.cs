using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Dto
{
    public class ZoneDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid SiteId { get; set; }
        public bool IsArchived { get; set; }
        public ZoneStatistics Statistics { get; set; }

        public static ZoneDto MapFromModel(Zone model)
        {
            return new ZoneDto
            {
                Id = model.Id,
                SiteId = model.SiteId,
                Name = model.Name,
                IsArchived = model.IsArchived,
                Statistics = model.Statistics
            };
        }

        public static List<ZoneDto> MapFromModels(List<Zone> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }
}
