using System;
using System.Collections.Generic;

namespace SiteCore.Domain
{
    public class LayerEquipment
    {
        public Guid LayerGroupId { get; set; }
        public Guid EquipmentId { get; set; }
        public Guid? ZoneId { get; set; }
        public List<List<int>> Geometry { get; set; } = new List<List<int>>();
    }
}
