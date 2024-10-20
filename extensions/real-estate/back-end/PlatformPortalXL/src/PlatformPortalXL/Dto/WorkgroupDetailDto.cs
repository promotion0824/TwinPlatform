using Authorization.TwinPlatform.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;

using Willow.Platform.Users;

namespace PlatformPortalXL.Dto
{
    public class WorkgroupDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid SiteId { get; set; }
        public List<Guid> MemberIds { get; set; }

        public static WorkgroupDetailDto MapFromModel(Workgroup model)
        {
            return new WorkgroupDetailDto
            {
                Id = model.Id,
                Name = model.Name,
                SiteId = model.SiteId,
                MemberIds = model.MemberIds
            };
        }

        public static WorkgroupDetailDto MapFromModel(GroupModel model)
        {
            if (model is null)
            {
                return null;
            }

            return new WorkgroupDetailDto
            {
                Id = model.Id,
                Name = model.Name,
                MemberIds = model.Users?.Select(u => u.Id)?.ToList()
            };
        }

        public static List<WorkgroupDetailDto> MapFromModels(IEnumerable<GroupModel> models)
        {
            return models?.Select(MapFromModel).ToList();
        }

        public static List<WorkgroupDetailDto> MapFromModels(IEnumerable<Workgroup> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }

    public record WorkgroupDto(Guid Id, string Name);
}
