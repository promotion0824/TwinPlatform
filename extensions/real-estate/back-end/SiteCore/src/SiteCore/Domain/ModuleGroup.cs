using System;

namespace SiteCore.Domain
{
    public class ModuleGroup
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
        public Guid SiteId { get; set; }
    }
}
