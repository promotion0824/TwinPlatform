using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetCoreTwinCreator.MappingId.Models
{
    [Table("AssetGeometry")]
    public class AssetGeometry
    {
        [Key]
        public int AssetRegisterId { get;set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(128)]
        public string Geometry { get; set; }
    }
}
