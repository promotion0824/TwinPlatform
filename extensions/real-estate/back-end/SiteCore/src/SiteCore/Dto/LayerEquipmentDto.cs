using System;
using System.Collections.Generic;
using System.Linq;
using SiteCore.Domain;

namespace SiteCore.Dto
{
    public class LayerEquipmentDto
    {
        public Guid LayerGroupId { get; set; }

        public Guid EquipmentId { get; set; }

        public Guid? ZoneId { get; set; }

        public List<List<int>> Geometry { get; set; } = new List<List<int>>();

        public static LayerEquipmentDto MapFrom(LayerEquipment layerEquipment)
        {
            if (layerEquipment == null)
            {
                return null;
            }

            return new LayerEquipmentDto
            {
                LayerGroupId = layerEquipment.LayerGroupId,
                ZoneId = layerEquipment.ZoneId,
                Geometry = layerEquipment.Geometry.Select(list => list.Select(item => item).ToList()).ToList(),
                EquipmentId = layerEquipment.EquipmentId
            };
        }       
    }
}
