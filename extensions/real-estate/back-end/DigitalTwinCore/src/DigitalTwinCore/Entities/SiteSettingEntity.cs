using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalTwinCore.Entities
{
    [Table("DT_SiteSettings")]
    public class SiteSettingEntity
    {
        [Key]
        public Guid SiteId { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(256)]
        public string InstanceUri { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(32)]
        public string SiteCodeForModelId { get; set; }

        [StringLength(50)]
        public string AdxDatabase { get; set; }
    }
}
