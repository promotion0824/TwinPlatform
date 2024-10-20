using SiteCore.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SiteCore.Entities
{
    [Table("Floors")]
    public class FloorEntity
    {
        public FloorEntity()
        {
            LayerGroups = new HashSet<LayerGroupEntity>();
        }

        public Guid Id { get; set; }
        public Guid SiteId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public int SortOrder { get; set; }
        public string Geometry { get; set; }
        public bool IsDecomissioned { get; set; }
        public bool IsSiteWide { get; set; } = false;        
        public Guid? ModelReference { get; set; }
        public virtual SiteEntity Site { get; set; }
        public virtual ICollection<LayerGroupEntity> LayerGroups { get; set; }

        [InverseProperty(nameof(ModuleEntity.Floor))]
        public List<ModuleEntity> Modules { get; set; } = new List<ModuleEntity>();

        public static Floor MapToDomainObject(FloorEntity floorEntity)
        {
            return new Floor
            {
                Id = floorEntity.Id,
                SiteId = floorEntity.SiteId,
                Name = floorEntity.Name,
                Code = floorEntity.Code,
                SortOrder = floorEntity.SortOrder,
                Geometry = floorEntity.Geometry,
                IsDecomissioned = floorEntity.IsDecomissioned,
                IsSiteWide = floorEntity.IsSiteWide,
                ModelReference = floorEntity.ModelReference
            };
        }

        public static List<Floor> MapToDomainObjects(IEnumerable<FloorEntity> floorEntities)
        {
            return floorEntities.Select(MapToDomainObject).ToList();
        }
    }
}
