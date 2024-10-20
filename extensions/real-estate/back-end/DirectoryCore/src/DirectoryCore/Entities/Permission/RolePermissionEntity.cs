using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DirectoryCore.Entities.Permission
{
    [Table("RolePermission")]
    public class RolePermissionEntity
    {
        public Guid RoleId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(50)]
        public string PermissionId { get; set; }
    }
}
