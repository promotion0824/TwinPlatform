using System;

namespace SiteCore.Domain
{
    public class Zone
    {
        public Guid Id { get; set; }
        public Guid LayerGroupId { get; set; }
        public string Geometry { get; set; }
        public int Zindex { get; set; }
    }
}
