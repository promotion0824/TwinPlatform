using System;

namespace SiteCore.Domain
{
    public class Floor
    {
        public Guid Id { get; set; }
        public Guid SiteId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int SortOrder { get; set; }
        public string Geometry { get; set; }
        public bool IsDecomissioned { get; set; }
        public bool IsSiteWide { get; set; } = false;        
        public Guid? ModelReference { get; set; }
    }
}
