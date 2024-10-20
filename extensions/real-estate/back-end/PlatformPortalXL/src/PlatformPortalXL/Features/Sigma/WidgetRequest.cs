using System;

namespace PlatformPortalXL.Features.Sigma
{
    public class WidgetRequest
    {
        public Guid CustomerId { get; set; }
        public string ScopeId { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public Guid? ReportId { get; set; }
        public string ReportName { get; set; }
		public string[] TenantIds { get; set; }
		public string[] SelectedDayRange { get; set; }
		public string[] SelectedBusinessHourRange { get; set; }
    }
}
