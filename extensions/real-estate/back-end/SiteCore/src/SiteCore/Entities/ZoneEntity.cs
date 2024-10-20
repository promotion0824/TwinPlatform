using SiteCore.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SiteCore.Entities
{
    [Table("Zones")]
    public class ZoneEntity
    {
        public Guid Id { get; set; }
        public Guid LayerGroupId { get; set; }
        public string Geometry { get; set; }
        public int Zindex { get; set; }

        public virtual LayerGroupEntity LayerGroup { get; set; }
        public virtual ICollection<LayerEquipmentEntity> LayerEquipments { get; set; } = new List<LayerEquipmentEntity>();

        public static ZoneEntity MapFrom(Zone zone)
        {
            if (zone == null)
            {
                return null;
            }

            return new ZoneEntity
            {
                Id = zone.Id,
                Geometry = zone.Geometry,
                LayerGroupId = zone.LayerGroupId,
                Zindex = zone.Zindex
            };
        }

        public static Zone MapToDomainObject(ZoneEntity zoneEntity)
        {
            if (zoneEntity == null)
            {
                return null;
            }

            return new Zone
            {
                Id = zoneEntity.Id,
                Geometry = zoneEntity.Geometry,
                LayerGroupId = zoneEntity.LayerGroupId,
                Zindex = zoneEntity.Zindex
            };
        }

        public static List<Zone> MapToDomainObjects(IEnumerable<ZoneEntity> zoneEntities)
        {
            return zoneEntities.Select(MapToDomainObject).ToList();
        }
    }
}
