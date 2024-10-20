using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using SiteCore.Domain;

namespace SiteCore.Entities
{
    [Table("LayerEquipment")]
    public class LayerEquipmentEntity
    {
        public Guid LayerGroupId { get; set; }

        public Guid EquipmentId { get; set; }

        public Guid? ZoneId { get; set; }

        public string Geometry { get;set; }

        public LayerGroupEntity LayerGroup { get; set; }

        public ZoneEntity Zone { get; set; }

        public static LayerEquipment MapToDomainObject(LayerEquipmentEntity layerEquipment)
        {
            return new LayerEquipment
            {
                LayerGroupId = layerEquipment.LayerGroupId,
                EquipmentId = layerEquipment.EquipmentId,
                Geometry = JsonConvert.DeserializeObject<List<List<int>>>(layerEquipment.Geometry),
                ZoneId = layerEquipment.ZoneId
            };
        }

        public static List<LayerEquipment> MapToDomainObjects(IEnumerable<LayerEquipmentEntity> floorEntities)
        {
            return floorEntities.Select(MapToDomainObject).ToList();
        }
    }
}
