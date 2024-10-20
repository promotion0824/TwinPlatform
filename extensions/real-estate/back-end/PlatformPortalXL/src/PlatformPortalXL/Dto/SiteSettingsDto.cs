using System;

using Willow.Workflow;

namespace PlatformPortalXL.Dto
{
    public class SiteSettingsDto
    {
        public Guid? InspectionDailyReportWorkgroupId { get; set; }

        public static SiteSettingsDto Map(SiteSettings model)
        {
            if (model == null)
            {
                return null;
            }

            return new SiteSettingsDto
            {
                InspectionDailyReportWorkgroupId = model.InspectionDailyReportWorkgroupId
            };
        }
    }
}
