using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Dto
{
    public class WorkgroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid SiteId { get; set; }
        public List<Guid> MemberIds { get; set; }

        public static WorkgroupDto MapFromModel(Workgroup model)
        {
            return new WorkgroupDto
            {
                Id = model.Id,
                Name = model.Name,
                SiteId = model.SiteId,
                MemberIds = model.MemberIds
            };
        }

        public static List<WorkgroupDto> MapFromModels(List<Workgroup> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }
}
