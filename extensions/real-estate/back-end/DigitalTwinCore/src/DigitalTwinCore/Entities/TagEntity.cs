using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalTwinCore.Entities
{
    [Table("DT_Tags")]
    public class TagEntity
    {
        [Key]
        [Required(AllowEmptyStrings = false)]
        [StringLength(128)]
        public string Name { get; set; }

        public int TagType { get; set; }
    }
}
