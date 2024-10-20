using System;

namespace SiteCore.Dto
{
    public class ModuleGroupDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int SortOrder { get; set; }

        public Guid SiteId { get; set; }
    }
}
