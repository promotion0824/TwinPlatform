using System;
using System.Collections.Generic;
using System.Linq;
using DirectoryCore.Domain;

namespace DirectoryCore.Dto
{
    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public static RoleDto MapFrom(Role role)
        {
            if (role == null)
            {
                return null;
            }

            return new RoleDto { Id = role.Id, Name = role.Name };
        }

        public static IList<RoleDto> MapFrom(IEnumerable<Role> roles) =>
            roles.Select(MapFrom).ToList();
    }
}
