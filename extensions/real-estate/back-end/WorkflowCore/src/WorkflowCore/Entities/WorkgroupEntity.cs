using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Entities
{
    [Table("WF_Workgroups")]
    public class WorkgroupEntity
    {
        public Guid Id { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(100)]
        public string Name { get; set; }

        public Guid SiteId { get; set; }

        public static Workgroup MapToModel(WorkgroupEntity entity)
        {
            return new Workgroup
            {
                Id = entity.Id,
                Name = entity.Name,
                SiteId = entity.SiteId
            };
        }

        public static List<Workgroup> MapToModels(IEnumerable<WorkgroupEntity> entities)
        {
            return entities?.Select(MapToModel).ToList();
        }

        public static WorkgroupEntity MapFromModel(Workgroup model)
        {
            return new WorkgroupEntity
            {
                Id = model.Id,
                Name = model.Name,
                SiteId = model.SiteId
            };
        }
    }
}
