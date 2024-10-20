using System.ComponentModel;

namespace PlatformPortalXL.Services.ContactUs;

public enum ContactUsCategory
{
    Dashboards=1,
    [Description("Search_&_Explore")]
    SearchAndExplore,
    Reports,
    Tickets,
    Inspections,
    Marketplace,
    TimeSeries,
    Admin,
    Insights,
    Copilot
}
