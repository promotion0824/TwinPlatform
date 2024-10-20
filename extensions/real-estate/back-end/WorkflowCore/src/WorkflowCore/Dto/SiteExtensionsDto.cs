using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Entities;

namespace WorkflowCore.Dto
{
    public class SiteExtensionsDto
    {
        public Guid? InspectionDailyReportWorkgroupId { get; set; }
        public DateTime? LastDailyReportDate { get; set; }

        public static SiteExtensionsDto Map(SiteExtensionEntity siteExtensionEntity)
        {
            return new SiteExtensionsDto
            {
                InspectionDailyReportWorkgroupId = siteExtensionEntity.InspectionDailyReportWorkgroupId,
                LastDailyReportDate = siteExtensionEntity.LastDailyReportDate
            };
        }

        public static List<SiteExtensionsDto> Map(IList<SiteExtensionEntity> siteExtensions)
        {
            return siteExtensions?.Select(Map).ToList();
        }
    }
}
