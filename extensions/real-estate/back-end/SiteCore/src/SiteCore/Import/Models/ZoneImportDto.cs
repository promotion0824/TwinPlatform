using System;

namespace SiteCore.Import.Models
{
    public class ZoneImportDto
    {
        public Guid LayerGroupId { get; set; }

        public Guid? ZoneId { get; set; }

        public int ZIndex { get; set; }

        public string Geometry { get; set; }
    }
}
