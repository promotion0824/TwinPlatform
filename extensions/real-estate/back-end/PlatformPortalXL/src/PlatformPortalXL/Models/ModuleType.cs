using System;

namespace PlatformPortalXL.Models
{
    public class ModuleType
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Prefix { get; set; }

        public string ModuleGroup { get; set; }

        public bool Is3D { get; set; }
        public Guid SiteId { get; set; }
        public bool IsDefault { get; set; }
        public bool CanBeDeleted { get; set; }
        public int SortOrder { get; set; }
        public ModuleGroup Group { get; set; }
    }
}
