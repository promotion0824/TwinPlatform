using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using SiteCore.Domain;

namespace SiteCore.Entities
{
    [Table("Modules")]
    public class ModuleEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid FloorId { get; set; }

        [ForeignKey(nameof(FloorId))]
        public FloorEntity Floor { get; set; }

        public Guid ModuleTypeId { get; set; }

        public Guid VisualId { get; set; }

        public int? ImageWidth { get; set; }

        public int? ImageHeight { get; set; }

        public string Url { get; set; }

        [ForeignKey(nameof(ModuleTypeId))]
        public ModuleTypeEntity ModuleType { get; set; }

        public static Module MapToDomainObject(ModuleEntity moduleEntity)
        {
            if (moduleEntity == null)
            {
                return null;
            }

            return new Module
            {
                Id = moduleEntity.Id,
                Name = moduleEntity.Name,
                FloorId = moduleEntity.FloorId,
                ModuleType = ModuleTypeEntity.MapToDomainObject(moduleEntity.ModuleType),
                ModuleTypeId = moduleEntity.ModuleTypeId,
                VisualId = moduleEntity.VisualId
            };
        }

        public static List<Module> MapToDomainObjects(IEnumerable<ModuleEntity> layerEntities)
        {
            return layerEntities.Select(MapToDomainObject).ToList();
        }
    }
}
