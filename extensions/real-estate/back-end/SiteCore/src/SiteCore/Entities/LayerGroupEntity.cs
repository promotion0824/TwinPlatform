using SiteCore.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SiteCore.Entities
{
    [Table("LayerGroups")]
    public class LayerGroupEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
        public Guid FloorId { get; set; }
        public int Zindex { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool Is3D { get; set; }
        public virtual FloorEntity Floor { get; set; }
        public virtual List<LayerEntity> Layers { get; set; } = new List<LayerEntity>();
        public virtual List<ZoneEntity> Zones { get; set; } = new List<ZoneEntity>();
        public virtual List<LayerEquipmentEntity> LayerEquipments { get; set; } = new List<LayerEquipmentEntity>();

        public static LayerGroup MapToDomainObject(LayerGroupEntity layerGroupEntity)
        {
            if (layerGroupEntity == null)
            {
                return null;
            }

            return new LayerGroup
            {
                Id = layerGroupEntity.Id,
                FloorId = layerGroupEntity.FloorId,
                Name = layerGroupEntity.Name,
                Zindex = layerGroupEntity.Zindex,
                SortOrder = layerGroupEntity.SortOrder,
                CreatedOn = layerGroupEntity.CreatedOn,
                Is3D = layerGroupEntity.Is3D,
                Equipments = LayerEquipmentEntity.MapToDomainObjects(layerGroupEntity.LayerEquipments),
                Zones = ZoneEntity.MapToDomainObjects(layerGroupEntity.Zones),
                Layers = LayerEntity.MapToDomainObjects(layerGroupEntity.Layers),
                Floor = FloorEntity.MapToDomainObject(layerGroupEntity.Floor)
            };
        }

        public static List<LayerGroup> MapToDomainObjects(IEnumerable<LayerGroupEntity> zoneGroupEntities)
        {
            return zoneGroupEntities.Select(MapToDomainObject).ToList();
        }
    }
}

