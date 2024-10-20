using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

using Willow.Platform.Users;

namespace PlatformPortalXL.Dto
{
    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public static RoleDto Map(Role model)
        {
            if (model == null)
            {
                return null;
            }

            return new RoleDto
            {
                Id = model.Id,
                Name = model.Name
            };
        }

        public static List<RoleDto> Map(IEnumerable<Role> models) => models?.Select(Map).ToList();
    }
}
