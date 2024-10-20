using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetCoreTwinCreator.MappingId.Models
{
    [Table("AssetCategoryExtension")]
    public class AssetCategoryExtensionEntity
    {
        [Key]
        public Guid SiteId { get; set; }

        [Key]
        public Guid CategoryId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(100)]
        public string ModuleTypeNamePath { get; set; }
    }
}
