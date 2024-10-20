using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SiteCore.Entities
{
    [Table("Metrics")]
    public class MetricEntity
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }

        [StringLength(64)]
        [Required]
        public string Key { get; set; }


        [StringLength(128)]
        [Required]
        public string Name { get; set; }

        [StringLength(64)]
        [Required]
        public string FormatString { get; set; }

        public decimal WarningLimit { get; set; }
        public decimal ErrorLimit { get; set; }

        [StringLength(1024)]
        [Required(AllowEmptyStrings = true)]
        public string Tooltip { get; set; }
    }
}
