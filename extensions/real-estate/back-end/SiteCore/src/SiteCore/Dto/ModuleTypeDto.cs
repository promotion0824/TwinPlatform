using SiteCore.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiteCore.Dto
{
    public class ModuleTypeDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Prefix { get; set; }

        public string ModuleGroup { get; set; }

        public bool Is3D { get; set; }
        public Guid SiteId { get; set; }
        public bool IsDefault { get; set; }
        public ModuleGroupDto Group { get; set; }
        public bool CanBeDeleted { get; set; }
        public int SortOrder { get; set; }

        public static ModuleTypeDto MapFrom(ModuleType moduleType)
        {
            if (moduleType == null)
            {
                return null;
            }

            var group = moduleType.Group != null ? new ModuleGroupDto
            {
                Id = moduleType.Group.Id,
                Name = moduleType.Group.Name,
                SortOrder = moduleType.Group.SortOrder,
                SiteId = moduleType.Group.SiteId
            } : null;

            return new ModuleTypeDto
            {
                Id = moduleType.Id,
                Name = moduleType.Name,
                Is3D = moduleType.Is3D,
                Prefix = moduleType.Prefix,
                ModuleGroup = group?.Name,
                SiteId = moduleType.SiteId,
                CanBeDeleted = moduleType.CanBeDeleted,
                Group = group,
                IsDefault = moduleType.IsDefault,
                SortOrder = moduleType.SortOrder
            };
        }

        public static List<ModuleTypeDto> MapFrom(IEnumerable<ModuleType> moduleTypes)
        {
            return moduleTypes?.Select(MapFrom).ToList();
        }
    }
}
