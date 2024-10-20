using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkflowCore.Entities
{
    [Table("WF_SiteExtensions")]
    public class SiteExtensionEntity
    {
        [Key]
        public Guid SiteId { get; set; }
        public Guid? InspectionDailyReportWorkgroupId { get; set; }
        public DateTime? LastDailyReportDate { get; set; }
    }
}
