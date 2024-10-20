using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.Dto
{
    public class ModuleTypeDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Prefix { get; set; }

        public string ModuleGroup { get; set; }

        public bool Is3D { get; set; }

        public bool IsDefault { get; set; }
        public bool CanBeDeleted { get; set; }
        public int SortOrder { get; set; }
        public ModuleGroupDto Group { get; set; }

        public static ModuleTypeDto MapFrom(ModuleType moduleType)
        {
            if (moduleType == null)
            {
                return null;
            }

            return new ModuleTypeDto
            {
                Id = moduleType.Id,
                Name = moduleType.Name,
                Is3D = moduleType.Is3D,
                Prefix = moduleType.Prefix,
                ModuleGroup = moduleType.ModuleGroup,
                IsDefault = moduleType.IsDefault,
                CanBeDeleted = moduleType.CanBeDeleted,
                SortOrder = moduleType.SortOrder,
                Group = moduleType.Group != null ? new ModuleGroupDto { 
                    Id = moduleType.Group.Id,
                    SortOrder = moduleType.Group.SortOrder,
                    Name = moduleType.Group.Name
                } : null
            };
        }

        public static List<ModuleTypeDto> MapFrom(IEnumerable<ModuleType> moduleTypes)
        {
            return moduleTypes?.Select(MapFrom).ToList();
        }
    }
}
