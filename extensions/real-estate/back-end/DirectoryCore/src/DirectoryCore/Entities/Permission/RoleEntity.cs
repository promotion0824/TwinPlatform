using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using DirectoryCore.Domain;

namespace DirectoryCore.Entities.Permission
{
    [Table("Roles")]
    public class RoleEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(100)]
        public string Name { get; set; }

        public static Role MapTo(RoleEntity role)
        {
            if (role == null)
            {
                return null;
            }

            return new Role { Id = role.Id, Name = role.Name };
        }

        public static List<Role> MapTo(List<RoleEntity> roles)
        {
            return roles?.Select(MapTo).ToList() ?? new List<Role>();
        }
    }
}
