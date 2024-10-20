using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DirectoryCore.Entities.Permission
{
    [Table("Permissions")]
    public class PermissionEntity
    {
        [Key]
        [Required(AllowEmptyStrings = false)]
        [StringLength(50)]
        public string Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(50)]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = true)]
        [StringLength(200)]
        public string Description { get; set; }
    }
}
