using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

using Willow.Platform.Users;

namespace PlatformPortalXL.Dto
{
    public class WorkgroupSimpleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public static WorkgroupSimpleDto MapFromModel(Workgroup model)
        {
            return new WorkgroupSimpleDto
            {
                Id = model.Id,
                Name = model.Name
            };
        }

        public static List<WorkgroupSimpleDto> MapFromModels(List<Workgroup> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }
}
