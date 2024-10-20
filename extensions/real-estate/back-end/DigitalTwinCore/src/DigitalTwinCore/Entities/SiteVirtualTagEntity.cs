using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalTwinCore.Entities
{
    [Table("DT_SiteVirtualTags")]
    public class SiteVirtualTagEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid SiteId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(128)]
        public string Tag { get; set; }

        [StringLength(256)]
        public string MatchModelId { get; set; }

        [StringLength(1024)]
        public string MatchTagList { get; set; }
    }
}
