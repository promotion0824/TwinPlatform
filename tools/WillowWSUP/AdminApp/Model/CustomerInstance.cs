namespace Willow.AdminApp;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Willow.Infrastructure.Entities;

/// <summary>
/// Customer Instance
/// </summary>
public class CustomerInstance
{
    /*
    * Subscription for app insights not the customer instance
    */
    private const string appInsightsSubscription = "fd259995-1de7-4ae8-8431-0d150dcca6f4";
    private const string appInsightsSubscriptionDev = "48a16780-c719-4528-a0f2-4e7640a9c850";
    private const string appInsightsSubscriptionTPD = "7dab7b5a-d968-43a5-a50f-9509244c297b"; // K8s-Internal

    /// <summary>
    /// Creates a single-tenant instance
    /// </summary>
    public CustomerInstance(bool isDevelopment,
        string region, string customerInstanceCode, string fullCustomerInstanceName, string Name,
        string domain,
        DeploymentPhase deploymentPhase,
        string resourceGroup, string subscription,
        LifeCycleState lifeCycleState,
        ApplicationSpecification[] applications
    )
    {
        this.Name = Name;
        this.Domain = domain;
        this.DeploymentPhase = deploymentPhase;
        this.ResourceGroup = resourceGroup;
        this.Subscription = subscription;
        this.IsDevelopment = isDevelopment;
        this.Region = region;
        this.CustomerInstanceCode = customerInstanceCode;
        this.FullCustomerInstanceName = fullCustomerInstanceName;
        this.LifeCycleState = lifeCycleState;

        string sub = isDevelopment ? appInsightsSubscriptionDev : appInsightsSubscription;
        string regionHack = isDevelopment ? "eus" : region;   // CI and PPE are in EUS not EUS2 where the ACAs are

        this.applicationInsightsUrl = $"https://portal.azure.com/#view/Microsoft_OperationsManagementSuite_Workspace/Logs.ReactView/resourceId/%2Fsubscriptions%2F{sub}%2FresourceGroups%2Frg-{devOrPrd}-{regionHack}%2Fproviders%2Fmicrosoft.insights%2Fcomponents%2Fain-{devOrPrd}-{regionHack}/source/LogsBlade.AnalyticsShareLinkToQuery/";
        this.Applications = applications.ToList();
    }

    /// <summary>
    /// Creates a TPD instance (not-single-tenant) with various missing properties
    /// </summary>
    public CustomerInstance(bool isDevelopment,
        string region, string customerInstanceCode, string Name,
        string domain,
        string resourceGroup,
        string ain,
        LifeCycleState lifeCycleState,
        ApplicationSpecification[] applications)
    {
        this.Name = Name;
        this.Domain = domain;
        this.DeploymentPhase = DeploymentPhase.Public;
        this.ResourceGroup = "";
        this.Subscription = "";
        this.IsDevelopment = isDevelopment;
        this.Region = region;
        this.CustomerInstanceCode = customerInstanceCode;
        this.LifeCycleState = lifeCycleState;
        this.applicationInsightsUrl = $"https://portal.azure.com/#@willowinc.com/resource/subscriptions/{appInsightsSubscriptionTPD}/resourceGroups/{resourceGroup}/providers/Microsoft.Insights/components/{ain}/logs";
        this.Applications = applications.ToList();
    }

    /// <summary>
    /// The friendly name for the customer instance
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The host name for the customer instance
    /// </summary>
    public string Domain { get; }

    public DeploymentPhase DeploymentPhase { get; }

    /// <summary>
    /// The resource group for the customer instance
    /// </summary>
    public string ResourceGroup { get; }

    /// <summary>
    /// Link to the resoure group in Azure portal
    /// </summary>
    public string ResourceGroupLink => $"https://portal.azure.com/#@willowinc.com/resource/subscriptions/{this.Subscription}/resourceGroups/{this.ResourceGroup}/overview";

    /// <summary>
    /// The subscription for the customer instance
    /// </summary>
    public string Subscription { get; }

    /// <summary>
    /// Is this non-production?
    /// </summary>
    public bool IsDevelopment { get; }

    private string devOrPrd => IsDevelopment ? "dev" : "prd";

    /// <summary>
    /// The region it's hosted in
    /// </summary>
    public string Region { get; }

    /// <summary>
    /// The short customer code
    /// </summary>
    public string CustomerInstanceCode { get; }

    /// <summary>
    /// The Full customer instance name
    /// </summary>
    public string FullCustomerInstanceName { get; } = string.Empty;

    /// <summary>
    /// Is the customer using the suite or is it still being commissioned
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public LifeCycleState LifeCycleState { get; }

    /// <summary>
    /// Applications installed for this customer instance
    /// </summary>
    public List<ApplicationSpecification> Applications { get; }

    // NewBuild
    /// <summary>
    /// Flag for whether or not it is a NewBuild Customer
    /// </summary>
    public bool IsNewBuild { get; set; }

    /// <summary>
    /// What applications are installed in TPD
    /// </summary>
    public static ApplicationSpecification[] TpdAUSet { get; set; } =
    [
        ApplicationSpecification.CommandMultiTenantAU,
        ApplicationSpecification.PublicApiMultitenant,
    ];

    /// <summary>
    /// What applications are installed in TPD
    /// </summary>
    public static ApplicationSpecification[] TpdEUSet { get; set; } =
    [
        ApplicationSpecification.CommandMultiTenantEU,
        ApplicationSpecification.PublicApiMultitenant,
    ];
    /// <summary>
    /// What applications are installed in TPD
    /// </summary>
    public static ApplicationSpecification[] TpdUSSet { get; set; } =
    [
        ApplicationSpecification.CommandMultiTenantUS,
        ApplicationSpecification.PublicApiMultitenant,
    ];

    private readonly string applicationInsightsUrl;

    /// <summary>
    /// Url to open logs
    /// </summary>
    public string LogUrl
    {
        get
        {
            string query = $$"""
            traces
            | where customDimensions.FullCustomerInstanceName  == "{{this.FullCustomerInstanceName}}"
            | order by timestamp desc
            """;

            string queryEncoded = Uri.EscapeDataString(query);

            return this.applicationInsightsUrl.TrimEnd('/') + $"/query/{queryEncoded}/timespan/PT1H";
        }
    }

    public string LogUrlForApp(string appQuery)
    {
        string query = $$"""
            traces
            | where customDimensions.FullCustomerInstanceName  == "{{this.FullCustomerInstanceName}}"
            | order by timestamp desc
            """;

        if (!string.IsNullOrWhiteSpace(appQuery))
        {
            query += $"| where {appQuery}";
        }

        string queryEncoded = Uri.EscapeDataString(query);

        return this.applicationInsightsUrl.TrimEnd('/') + $"/query/{queryEncoded}/timespan/PT1H";
    }

    public string ExceptionUrlForApp(string appQuery)
    {
        string query = $$"""
            exceptions
            | where customDimensions.FullCustomerInstanceName  == "{{this.FullCustomerInstanceName}}"
            | order by timestamp desc
            """;

        if (!string.IsNullOrWhiteSpace(appQuery))
        {
            query += $"| where {appQuery}";
        }

        string queryEncoded = Uri.EscapeDataString(query);

        return this.applicationInsightsUrl.TrimEnd('/') + $"/query/{queryEncoded}/timespan/PT1H";
    }
}
