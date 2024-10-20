using System.ComponentModel;

namespace PlatformPortalXL.Services.ContactUs;

public enum ContactUsProductArea
{
    Dashboards = 1,
    [Description("Search_Explore")]
    SearchAndExplore,
    Inspections,
    [Description("opsgenie")]
    Alarms,
    Integrations,
    [Description("live_data")]
    LiveDate,
    [Description("advanced_analytics")]
    AdvancedAnalytics,
    [Description("iad")]
    InfrastructureAndApplicationDevOps,
    Experience,
    [Description("iot_services")]
    IotServices,
    Connectors,
    [Description("rules_insights")]
    RulesAndInsights,
    Workflows,
    [Description("core_services")]
    CoreServices,
    [Description("new_build")]
    NewBuild


}
