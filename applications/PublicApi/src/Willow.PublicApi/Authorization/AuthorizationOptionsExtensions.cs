namespace Willow.PublicApi.Authorization;

using static Willow.PublicApi.Authorization.Permissions;

internal static class AuthorizationOptionsExtensions
{
    public static void AddAuthorizationServicePolicy(this AuthorizationOptions options, string policyName, string permission)
    {
        options.AddPolicy(policyName, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.AddRequirements(new AuthorizationServiceRequirement(permission));
        });
    }

    public static void AddTimeSeriesPolicies(this AuthorizationOptions options)
    {
        options.AddAuthorizationServicePolicy("TimeseriesRead", TimeSeries.Read);
        options.AddAuthorizationServicePolicy("TimeseriesWrite", TimeSeries.Write);
    }

    public static void AddTwinsPolicies(this AuthorizationOptions options)
    {
        options.AddAuthorizationServicePolicy("TwinsRead", Twins.Read);
        options.AddAuthorizationServicePolicy("TwinsWrite", Twins.Write);
    }

    public static void AddModelsPolicies(this AuthorizationOptions options)
    {
        options.AddAuthorizationServicePolicy("ModelsRead", Models.Read);
        options.AddAuthorizationServicePolicy("ModelsWrite", Models.Write);
    }

    public static void AddInsightsPolicies(this AuthorizationOptions options)
    {
        options.AddAuthorizationServicePolicy("InsightsRead", Insights.Read);
        options.AddAuthorizationServicePolicy("InsightsWrite", Insights.Write);
    }

    public static void AddTicketsPolicies(this AuthorizationOptions options)
    {
        options.AddAuthorizationServicePolicy("TicketsRead", Tickets.Read);
        options.AddAuthorizationServicePolicy("TicketsWrite", Tickets.Write);
    }

    public static void AddInspectionsPolicies(this AuthorizationOptions options)
    {
        options.AddAuthorizationServicePolicy("InspectionsRead", Inspections.Read);
        options.AddAuthorizationServicePolicy("InspectionsWrite", Inspections.Write);
    }

    public static void AddDocumentsPolicies(this AuthorizationOptions options)
    {
        options.AddAuthorizationServicePolicy("DocumentsRead", Documents.Read);
        options.AddAuthorizationServicePolicy("DocumentsWrite", Documents.Write);
    }
}
