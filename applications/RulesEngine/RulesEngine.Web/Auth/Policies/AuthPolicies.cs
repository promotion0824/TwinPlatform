using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace RulesEngine.Web;

/*
	Policy Discussion

	1. Use a single policy per controller, don't stack them because the order and whether you mean And / Or / Not is unclear

	Policy: WillowEmployees can view all of rules engine
	Policy: Members of the RulesWriter group can edit rules
	Policy: Logged in users can view the introduction pages
	Policy: Members of the admin group can view the test pages
	Policy: Members of the admin group can view the progress and status pages

	So requirements on controllers are things like:

		CanViewRules
		CanEditRules
		CanViewModels
		CanViewMonitoringPage
		CanViewTestPage

	But these are the *result* of policy evaluations.

	Policies are CODE, they can evaluate multiple conditions in order and any condition can succeed or fail the whole evaluation.
*/

/// <summary>
/// Auth policy extensions
/// </summary>
public static class AuthPolicy
{
    /// <summary>
    /// CanViewRules
    /// </summary>
    public static readonly CanViewRules CanViewRules = new CanViewRules();
    /// <summary>
    /// CanViewStandardRules
    /// </summary>
    public static readonly CanViewStandardRules CanViewStandardRules = new CanViewStandardRules();
    /// <summary>
    /// CanEditRules
    /// </summary>
    public static readonly CanEditRules CanEditRules = new CanEditRules();
    /// <summary>
    /// CanEditStandardRules
    /// </summary>
    public static readonly CanEditStandardRules CanEditStandardRules = new CanEditStandardRules();
    /// <summary>
    /// CanExportRules
    /// </summary>
    public static readonly CanExportRules CanExportRules = new CanExportRules();
    /// <summary>
    /// CanExecuteRules
    /// </summary>
    public static readonly CanExecuteRules CanExecuteRules = new CanExecuteRules();
    /// <summary>
    /// CanViewModels
    /// </summary>
    public static readonly CanViewModels CanViewModels = new CanViewModels();
    /// <summary>
    /// CanViewTwins
    /// </summary>
    public static readonly CanViewTwins CanViewTwins = new CanViewTwins();
    /// <summary>
    /// CanViewInsights
    /// </summary>
    public static readonly CanViewInsights CanViewInsights = new CanViewInsights();
    /// <summary>
    /// CanViewCommands
    /// </summary>
    public static readonly CanViewCommands CanViewCommands = new CanViewCommands();
    /// <summary>
    /// CanViewAdminPage
    /// </summary>
    public static readonly CanViewAdminPage CanViewAdminPage = new CanViewAdminPage();
    /// <summary>
    /// CanManageJobs
    /// </summary>
    public static readonly CanManageJobs CanManageJobs = new CanManageJobs();
    /// <summary>
    /// CanViewSwitcher
    /// </summary>
    public static readonly CanViewSwitcher CanViewSwitcher = new CanViewSwitcher();

    /// <summary>
    /// Requirements to view a rule
    /// </summary>
    public static IWillowAuthorizationRequirement[] CanViewRuleRequirements =
    [
        CanViewRules,
        CanViewStandardRules
    ];

    /// <summary>
    /// Requirements to edit a rule
    /// </summary>
    public static IWillowAuthorizationRequirement[] CanEditRuleRequirements =
    [
        CanEditRules,
        CanEditStandardRules
    ];

    /// <summary>
    /// Extension method to add all the application registrations for policies that can be used on controllers
    /// </summary>
    public static void AddAuthPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(nameof(Web.CanViewRules), policy => policy.Requirements.Add(CanViewRules));
        options.AddPolicy(nameof(Web.CanViewStandardRules), policy => policy.Requirements.Add(CanViewStandardRules));
        options.AddPolicy(nameof(Web.CanEditRules), policy => policy.Requirements.Add(CanEditRules));
        options.AddPolicy(nameof(Web.CanEditStandardRules), policy => policy.Requirements.Add(CanEditStandardRules));
        options.AddPolicy(nameof(Web.CanExportRules), policy => policy.Requirements.Add(CanExportRules));
        options.AddPolicy(nameof(Web.CanExecuteRules), policy => policy.Requirements.Add(CanExecuteRules));
        options.AddPolicy(nameof(Web.CanViewModels), policy => policy.Requirements.Add(CanViewModels));
        options.AddPolicy(nameof(Web.CanViewTwins), policy => policy.Requirements.Add(CanViewTwins));
        options.AddPolicy(nameof(Web.CanViewInsights), policy => policy.Requirements.Add(CanViewInsights));
        options.AddPolicy(nameof(Web.CanViewCommands), policy => policy.Requirements.Add(CanViewCommands));
        options.AddPolicy(nameof(Web.CanViewAdminPage), policy => policy.Requirements.Add(CanViewAdminPage));
        options.AddPolicy(nameof(Web.CanViewSwitcher), policy => policy.Requirements.Add(CanViewSwitcher));
        options.AddPolicy(nameof(Web.CanManageJobs), policy => policy.Requirements.Add(CanManageJobs));
    }

    /// <summary>
    /// Add Auth policies to the service collection, must be same list as above
    /// </summary>
    public static void AddAuthPolicies(this IServiceCollection services)
    {
        services.AddSingleton<IWillowAuthorizationRequirement, CanViewRules>();
        services.AddSingleton<IWillowAuthorizationRequirement, CanViewStandardRules>();
        services.AddSingleton<IWillowAuthorizationRequirement, CanEditRules>();
        services.AddSingleton<IWillowAuthorizationRequirement, CanEditStandardRules>();
        services.AddSingleton<IWillowAuthorizationRequirement, CanExportRules>();
        services.AddSingleton<IWillowAuthorizationRequirement, CanExecuteRules>();
        services.AddSingleton<IWillowAuthorizationRequirement, CanViewModels>();
        services.AddSingleton<IWillowAuthorizationRequirement, CanViewTwins>();
        services.AddSingleton<IWillowAuthorizationRequirement, CanViewInsights>();
        services.AddSingleton<IWillowAuthorizationRequirement, CanViewCommands>();
        services.AddSingleton<IWillowAuthorizationRequirement, CanViewAdminPage>();
        services.AddSingleton<IWillowAuthorizationRequirement, CanManageJobs>();
        services.AddSingleton<IWillowAuthorizationRequirement, CanViewSwitcher>();

        // Add handlers for each policy
        services.AddSingleton<IAuthorizationHandler, CanViewRulesEvaluator>();
        services.AddSingleton<IAuthorizationHandler, CanViewStandardRulesEvaluator>();
        services.AddSingleton<IAuthorizationHandler, CanEditRulesEvaluator>();
        services.AddSingleton<IAuthorizationHandler, CanEditStandardRulesEvaluator>();
        services.AddSingleton<IAuthorizationHandler, CanExportRulesEvaluator>();
        services.AddSingleton<IAuthorizationHandler, CanExecuteRulesEvaluator>();
        services.AddSingleton<IAuthorizationHandler, CanViewModelsEvaluator>();
        services.AddSingleton<IAuthorizationHandler, CanViewTwinsEvaluator>();
        services.AddSingleton<IAuthorizationHandler, CanViewInsightsEvaluator>();
        services.AddSingleton<IAuthorizationHandler, CanViewCommandsEvaluator>();
        services.AddSingleton<IAuthorizationHandler, CanViewAdminPageEvaluator>();
        services.AddSingleton<IAuthorizationHandler, CanManageJobsEvaluator>();
        services.AddSingleton<IAuthorizationHandler, CanViewSwitcherEvaluator>();
    }

    /// <summary>
	/// Add roles to the service collection
	/// </summary>
    /// <remarks>
    ///
    /// THESE ROLES ARE USED IN NON-PROD TO GIVE WILLOWERS BROAD ACCESS
    /// Willow Non-Prod Admin
    /// Willow Non-Prod
    ///
    /// THESE ROLES ARE USED IN PROD FOR SUPPORT (Group membership is via PIM)
    /// Willow Support
    /// Willow Support Admin
    ///
    /// THESE ROLES ARE USED IN PROD FOR DEVELOPMENT/COMMISSIONING
    /// Willow Delivery
    /// Willow New Build
    /// Willow Performance Engineer
    ///
    /// THESE ARE THE ROLES THE CUSTOMER STARTS WITH IN PROD
    /// Property Leadership
    /// Occupancy and Space Management
    /// Operational Performance and Technology
    /// Facility and Infrastructure Management
    /// Technical and Maintenance Expertise
    ///
    /// </remarks>
	public static void AddRoles(this IServiceCollection services, string instanceType)
    {
        services.AddSingleton<IWillowRole, RulesAdmin>();
        services.AddSingleton<IWillowRole, RulesReader>();
        services.AddSingleton<IWillowRole, RulesWriter>();

        if (string.IsNullOrEmpty(instanceType) || instanceType == "nonprd")
        {
            // These two need to be non-prod only, so we need to look at the configuration == "prd"
            services.AddSingleton<IWillowRole, WillowNonProdAdminRole>();
            services.AddSingleton<IWillowRole, WillowNonProdRole>();
        }

        if (string.IsNullOrEmpty(instanceType) || instanceType == "prd")
        {
            // These two need to be prod-only and are PIM-able
            services.AddSingleton<IWillowRole, WillowSupportRole>();
            services.AddSingleton<IWillowRole, WillowSupportAdminRole>();
        }

        // These can exist in both prod and non-prod
        services.AddSingleton<IWillowRole, WillowDeliveryRole>();
        services.AddSingleton<IWillowRole, WillowNewBuildRole>();
        services.AddSingleton<IWillowRole, WillowPerformanceEngineerRole>();
        services.AddSingleton<IWillowRole, PropertyLeadershipRole>();
        services.AddSingleton<IWillowRole, OccupancyAndSpaceManagementRole>();
        services.AddSingleton<IWillowRole, OperationalPerformanceAndTechnologyRole>();
        services.AddSingleton<IWillowRole, FacilityAndInfrastructureManagementRole>();
        services.AddSingleton<IWillowRole, TechnicalAndMaintenanceExpertiseRole>();
        services.AddSingleton<IWillowRole, TechnicalAnalystRole>();
    }

    /// <summary>
    /// A permission set for readonly/viewonly permissions
    /// </summary>
    public static IWillowAuthorizationRequirement[] ReadOnlyPermissionSet
    {
        get
        {
            return [
               CanExportRules,
               CanViewInsights,
               CanViewCommands,
               CanViewRules,
               CanViewStandardRules,
               CanViewModels,
               CanViewTwins
            ];
        }
    }

    /// <summary>
    /// A permission set for read and write permissions
    /// </summary>
    public static IWillowAuthorizationRequirement[] ReadWritePermissionSet
    {
        get
        {
            return [
               CanEditRules,
               CanEditStandardRules,
               CanExportRules,
               CanViewInsights,
               CanViewCommands,
               CanViewModels,
               CanViewRules,
               CanViewStandardRules,
               CanViewTwins
            ];
        }
    }

    /// <summary>
    /// A permission set for admins
    /// </summary>
    public static IWillowAuthorizationRequirement[] AdminPermissionSet
    {
        get
        {
            return [
                CanViewRules,
                CanViewStandardRules,
                CanEditRules,
                CanEditStandardRules,
                CanExportRules,
                CanExecuteRules,
                CanViewModels,
                CanViewTwins,
                CanViewInsights,
                CanViewCommands,
                CanViewAdminPage,
                CanManageJobs,
                CanViewSwitcher
            ];
        }
    }
}
