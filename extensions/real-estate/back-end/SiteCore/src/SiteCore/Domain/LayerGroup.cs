using System;
using System.Collections.Generic;

namespace SiteCore.Domain
{
    public class LayerGroup
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid FloorId { get; set; }
        public int Zindex { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool Is3D { get; set; }
        public Floor Floor { get; set; }
        public List<Layer> Layers { get; set; } = new List<Layer>();
        public List<Zone> Zones { get; set; } = new List<Zone>();
        public List<LayerEquipment> Equipments { get; set; } = new List<LayerEquipment>();
    }
}
