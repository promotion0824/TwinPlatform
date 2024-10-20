using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SiteCore.Entities
{
    [Table("ModuleGroups")]
    public class ModuleGroupEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid SiteId { get; set; }

        public int SortOrder { get; set; }
    }
}
