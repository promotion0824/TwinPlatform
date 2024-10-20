using MobileXL.Models;

namespace MobileXL.Dto
{
    public class SiteFeaturesDto
    {
        public bool IsTicketingDisabled { get; set; }
        public bool IsInsightsDisabled { get; set; }
        public bool Is2DViewerDisabled { get; set; }
        public bool IsReportsEnabled { get; set; }
        public bool Is3DAutoOffsetEnabled { get; set; }
        public bool IsInspectionEnabled { get; set; }
        public bool IsScheduledTicketsEnabled { get; set; }

        public static SiteFeaturesDto Map(SiteFeatures model)
        {
            return new SiteFeaturesDto
            {
                IsTicketingDisabled = model.IsTicketingDisabled,
                IsInsightsDisabled = model.IsInsightsDisabled,
                Is2DViewerDisabled = model.Is2DViewerDisabled,
                IsReportsEnabled = model.IsReportsEnabled,
                Is3DAutoOffsetEnabled = model.Is3DAutoOffsetEnabled,
                IsInspectionEnabled = model.IsInspectionEnabled,
                IsScheduledTicketsEnabled = model.IsScheduledTicketsEnabled,
            };
        }
    }
}
