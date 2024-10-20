using System;
using System.Collections.Generic;
using System.Linq;
using SiteCore.Domain;

namespace SiteCore.Dto
{
    public class EquipmentDto
    {
        public Guid Id { get; set; }

        public List<List<int>> Geometry { get; set; } = new List<List<int>>();

        public Guid? ZoneId { get; set; }

        public static EquipmentDto MapFrom(LayerEquipment layerEquipment)
        {
            if (layerEquipment == null)
            {
                return null;
            }

            return new EquipmentDto
            {
                Id = layerEquipment.EquipmentId,
                Geometry = layerEquipment.Geometry.Select(list => list.Select(item => item).ToList()).ToList(),
                ZoneId = layerEquipment.ZoneId
            };
        }
    }
}
