using SiteCore.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SiteCore.Entities
{
    [Table("ModuleTypes")]
    public class ModuleTypeEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Prefix { get; set; }

        public int SortOrder { get; set; }

        public bool CanBeDeleted { get; set; }

        public bool Is3D { get; set; }
        public Guid SiteId { get; set; }
        public bool IsDefault { get; set; }

        public Guid? ModuleGroupId { get; set; }

        [ForeignKey(nameof(ModuleGroupId))]
        public ModuleGroupEntity ModuleGroup { get; set; }

        [InverseProperty(nameof(ModuleEntity.ModuleType))]
        public List<ModuleEntity> Modules { get; set; }

        public static ModuleType MapToDomainObject(ModuleTypeEntity moduleTypeEntity)
        {
            if (moduleTypeEntity == null)
            {
                return null;
            }

            return new ModuleType
            {
                Id = moduleTypeEntity.Id,
                Name = moduleTypeEntity.Name,
                CanBeDeleted = moduleTypeEntity.CanBeDeleted,
                ModuleGroup = moduleTypeEntity.ModuleGroup?.Name,
                Prefix = moduleTypeEntity.Prefix,
                SortOrder = moduleTypeEntity.SortOrder,
                Is3D = moduleTypeEntity.Is3D,
                SiteId = moduleTypeEntity.SiteId,
                IsDefault = moduleTypeEntity.IsDefault,
                HasModuleAssignments = moduleTypeEntity.Modules != null && moduleTypeEntity.Modules.Any(),
                Group = moduleTypeEntity.ModuleGroup != null ? new ModuleGroup { 
                    Id = moduleTypeEntity.ModuleGroup.Id,
                    Name = moduleTypeEntity.ModuleGroup.Name,
                    SortOrder = moduleTypeEntity.ModuleGroup.SortOrder,
                    SiteId = moduleTypeEntity.ModuleGroup.SiteId
                } : null
            };
        }

        public static List<ModuleType> MapToDomainObjects(IEnumerable<ModuleTypeEntity> imageTypeEntities)
        {
            return imageTypeEntities?.Select(MapToDomainObject).ToList();
        }
    }
}
