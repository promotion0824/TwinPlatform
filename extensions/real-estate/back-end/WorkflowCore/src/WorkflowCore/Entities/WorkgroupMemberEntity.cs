using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Entities
{
    [Table("WF_WorkgroupMembers")]
    public class WorkgroupMemberEntity
    {
        public Guid WorkgroupId { get; set; }

        public Guid MemberId { get; set; }

        public static WorkgroupMember MapToModel(WorkgroupMemberEntity entity)
        {
            return new WorkgroupMember
            {
                WorkgroupId = entity.WorkgroupId,
                MemberId = entity.MemberId
            };
        }

        public static List<WorkgroupMember> MapToModels(IEnumerable<WorkgroupMemberEntity> entities)
        {
            return entities?.Select(MapToModel).ToList();
        }

        public static WorkgroupMemberEntity MapFromModel(WorkgroupMember model)
        {
            return new WorkgroupMemberEntity
            {
                WorkgroupId = model.WorkgroupId,
                MemberId = model.MemberId
            };
        }
        public static List<WorkgroupMemberEntity> MapFromModels(IEnumerable<WorkgroupMember> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }
}
