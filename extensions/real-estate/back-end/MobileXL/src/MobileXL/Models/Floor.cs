using System;

namespace MobileXL.Models
{
    public class Floor
    {
        public Guid Id { get; set; }
        public Guid SiteId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public double? Area { get; set; }
        public string Geometry { get; set; }
        public double? NetLettableArea { get; set; }
        public int SortOrder { get; set; }
    }
}
