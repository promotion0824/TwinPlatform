using DirectoryCore.Domain;

namespace DirectoryCore.Dto
{
    public class SiteFeaturesDto
    {
        public bool IsTicketingDisabled { get; set; }
        public bool IsInsightsDisabled { get; set; }
        public bool Is2DViewerDisabled { get; set; }
        public bool IsReportsEnabled { get; set; }
        public bool Is3DAutoOffsetEnabled { get; set; }
        public bool IsInspectionEnabled { get; set; }
        public bool IsOccupancyEnabled { get; set; }
        public bool IsPreventativeMaintenanceEnabled { get; set; }
        public bool IsCommandsEnabled { get; set; }
        public bool IsScheduledTicketsEnabled { get; set; }
        public bool IsNonTenancyFloorsEnabled { get; set; }
        public bool IsHideOccurrencesEnabled { get; set; }
        public bool IsArcGisEnabled { get; set; }
        public bool IsTicketMappedIntegrationEnabled { get; set; }

        public static SiteFeaturesDto MapFrom(SiteFeatures model)
        {
            if (model == null)
            {
                model = new SiteFeatures();
            }

            return new SiteFeaturesDto
            {
                IsTicketingDisabled = model.IsTicketingDisabled,
                IsInsightsDisabled = model.IsInsightsDisabled,
                Is2DViewerDisabled = model.Is2DViewerDisabled,
                IsReportsEnabled = model.IsReportsEnabled,
                Is3DAutoOffsetEnabled = model.Is3DAutoOffsetEnabled,
                IsInspectionEnabled = model.IsInspectionEnabled,
                IsOccupancyEnabled = model.IsOccupancyEnabled,
                IsPreventativeMaintenanceEnabled = model.IsPreventativeMaintenanceEnabled,
                IsCommandsEnabled = model.IsCommandsEnabled,
                IsScheduledTicketsEnabled = model.IsScheduledTicketsEnabled,
                IsNonTenancyFloorsEnabled = model.IsNonTenancyFloorsEnabled,
                IsHideOccurrencesEnabled = model.IsHideOccurrencesEnabled,
                IsArcGisEnabled = model.IsArcGisEnabled,
                IsTicketMappedIntegrationEnabled = model.IsTicketMappedIntegrationEnabled
            };
        }
    }
}
