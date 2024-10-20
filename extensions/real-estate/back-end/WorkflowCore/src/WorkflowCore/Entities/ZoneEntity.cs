using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Entities
{
    [Table("WF_Zones")]
    public class ZoneEntity
    {
        public Guid Id { get; set; }
        public Guid SiteId { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        public bool IsArchived { get; set; }

        public static Zone MapToModel(ZoneEntity entity)
        {
            if (entity == null)
            {
                return null;
            }
            return new Zone
            {
                Id = entity.Id,
                SiteId = entity.SiteId,
                Name = entity.Name,
                IsArchived = entity.IsArchived
            };
        }

        public static List<Zone> MapToModels(IEnumerable<ZoneEntity> entities)
        {
            return entities?.Select(MapToModel).ToList();
        }

        public static ZoneEntity MapFromModel(Zone model)
        {
            return new ZoneEntity
            {
                Id = model.Id,
                Name = model.Name,
                SiteId = model.SiteId
            };
        }
    }
}
