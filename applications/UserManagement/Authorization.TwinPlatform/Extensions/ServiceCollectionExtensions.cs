using Authorization.Common.Enums;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.HealthChecks;
using Authorization.TwinPlatform.Mappings;
using Authorization.TwinPlatform.Options;
using Authorization.TwinPlatform.Persistence.Contexts;
using Authorization.TwinPlatform.Persistence.Extensions;
using Authorization.TwinPlatform.Persistence.Strategy;
using Authorization.TwinPlatform.Services;
using Authorization.TwinPlatform.Services.Hosted;
using Authorization.TwinPlatform.Services.Hosted.Request;
using AutoMapper;
using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;
using Willow.DataAccess.SqlServer;
using SqlAuthenticationProvider = Microsoft.Data.SqlClient.SqlAuthenticationProvider;

namespace Authorization.TwinPlatform.Extensions;

/// <summary>
/// Class for ServiceCollection extension methods
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Method registers DB Context and add Permission Aggregator Services to the collection
    /// </summary>
    /// <param name="services">Service Collection Instance</param>
    /// <param name="configuration">Configuration Instance</param>
    /// <param name="environment">Web Host Environment.</param>
    public static void AddTwinPlatformPermissionService(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddDbContext(configuration, environment);
        AddAutoMapper(services);
        services.AddScoped<IPermissionAggregatorService, PermissionAggregatorService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IImportService, ImportService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IApplicationClientService, ApplicationClientService>();
        services.AddScoped<IClientAssignmentService, ClientAssignmentService>();
        services.AddTransient<IWillowExpressionService, WillowExpressionService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IRecordChangeNotifier, RecordChangeNotifierService>();
        services.AddSingleton<ICacheInvalidationService, CacheInvalidationService>();
    }

    /// <summary>
    /// Method registers DB Context and add all TP Auth Services to the Collection
    /// </summary>
    /// <param name="services">ServiceCollection Instance</param>
    /// <param name="configuration">Configuration Instance</param>
    /// <param name="environment">Web Host Environment.</param>
    public static void AddTwinPlatformAuthServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddDbContext(configuration, environment);

        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleAssignmentService, RoleAssignmentService>();
        services.AddScoped<IRolePermissionService, RolePermissionService>();
        services.AddScoped<IGroupTypeService, GroupTypeService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IUserGroupService, UserGroupService>();
        services.AddScoped<IGroupRoleAssignmentService, GroupRoleAssignmentService>();
        services.AddScoped<IPermissionAggregatorService, PermissionAggregatorService>();
        services.AddScoped<IImportService, ImportService>();
        services.AddTransient<IWillowExpressionService, WillowExpressionService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IApplicationClientService, ApplicationClientService>();
        services.AddScoped<IClientAssignmentService, ClientAssignmentService>();
        AddAutoMapper(services);
        services.AddScoped<IRecordChangeNotifier, RecordChangeNotifierService>();
    }

    /// <summary>
    /// Method to add Microsoft Graph API services to the Service Collection
    /// </summary>
    /// <param name="services">ServiceCollection Instance</param>
    /// <param name="graphAppConfigSection">Configuration Instance of graph API apps</param>
    public static void AddMicrosoftGraphAPIServices(this IServiceCollection services,
        IConfigurationSection graphAppConfigSection)
    {
        services.AddHttpClient(nameof(GraphApplicationClientService))
                .AddPolicyHandler(GraphApplicationClientService.GetRetryPolicy);

        foreach (var graphOption in graphAppConfigSection.Get<GraphApplicationOptions[]>()!)
        {
            services.AddSingleton<IGraphApplicationClientService>((sp) =>
            {
                HealthCheckAD healthCheck = graphOption.Type == ADType.AzureAD ?
                sp.GetRequiredService<HealthCheckAzureAD>() :
                sp.GetRequiredService<HealthCheckAzureB2C>();
                return new GraphApplicationClientService(graphOption, healthCheck, sp.GetRequiredService<IHttpClientFactory>());
            });
        }

        services.AddScoped<IGraphApplicationService, GraphApplicationService>();
        services.AddScoped<IAuthorizationGraphService, AuthorizationGraphService>();

        var backgroundQueueInstance = new BackgroundQueue<GroupMembershipCacheRefreshRequest>();
        services.AddSingleton<IBackgroundQueueReceiver<GroupMembershipCacheRefreshRequest>>(backgroundQueueInstance);
        services.AddSingleton<IBackgroundQueueSender<GroupMembershipCacheRefreshRequest>>(backgroundQueueInstance);
    }

    /// <summary>
    /// Method to add Microsoft Graph API services to the Service Collection
    /// </summary>
    /// <param name="services">ServiceCollection Instance</param>
    /// <param name="graphAppConfigSection">Configuration Instance of graph API apps</param>
    /// <param name="hostedServiceConfigSection">Hosted Service Config Section.</param>
    public static void AddMicrosoftGraphAPIWithHostedServices(this IServiceCollection services,
        IConfigurationSection graphAppConfigSection,
        IConfigurationSection hostedServiceConfigSection)
    {
        services.AddMicrosoftGraphAPIServices(graphAppConfigSection);

        // Add Hosted Service and its options
        services.Configure<GraphApplicationCacheRefreshOption>(hostedServiceConfigSection.GetSection(nameof(GraphApplicationCacheRefreshService)));
        services.AddHostedService<GraphApplicationCacheRefreshService>();
    }

    public static void AddEmailNotificationServices(this IServiceCollection services, IConfigurationSection hostedServiceConfigSection)
    {
        var backgroundChannel = new BackgroundChannel<EmailNotificationRequest>(Channel.CreateUnbounded<EmailNotificationRequest>());
        services.AddSingleton<IBackgroundChannelReceiver<EmailNotificationRequest>>(backgroundChannel);
        services.AddSingleton<IBackgroundChannelSender<EmailNotificationRequest>>(backgroundChannel);

        services.Configure<EmailNotificationServiceOption>(hostedServiceConfigSection.GetSection(nameof(EmailNotificationService)));
        services.AddHostedService<EmailNotificationService>();
    }

    /// <summary>
    /// Adds default authorization health checks to the service collection
    /// </summary>
    /// <param name="services">Service Collection Instance</param>
    /// <param name="additionalChecks">Action method to perform additional health checks</param>
    /// <returns>Instance of <see cref="IHealthChecksBuilder"/></returns>
    public static IHealthChecksBuilder AddAuthorizationHealthChecks(this IServiceCollection services, Action<IHealthChecksBuilder>? additionalChecks = null)
    {
        services.AddSingleton<HealthCheckSqlServer>();
        services.AddSingleton<HealthCheckAzureAD>();
        services.AddSingleton<HealthCheckAzureB2C>();

        var healthBuilder = services.AddHealthChecks()
            .AddCheck<HealthCheckSqlServer>("Sql Server")
            .AddCheck<HealthCheckAzureAD>("Azure AD")
            .AddCheck<HealthCheckAzureB2C>("Azure AD B2C");

        if (additionalChecks is not null)
            additionalChecks(healthBuilder);

        return healthBuilder;
    }

    private static void AddAutoMapper(this IServiceCollection services)
    {
        var mapperConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile<EntityMappings>();
        });
        mapperConfig.CompileMappings();

        var mapper = mapperConfig.CreateMapper();
        services.AddSingleton(mapper);
    }

    private static void AddDbContext(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddDbContextPool<TwinPlatformAuthContext>((sp,options) =>
            options.UseSqlServer(configuration.GetAuthorizationDbConnectionString(),
                opt =>
                {
                    opt.ExecutionStrategy(c=>new CustomAzureSqlExecutionStrategy(c,
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(1),
                        errorNumbersToAdd: null,
                        logger: sp.GetRequiredService<ILogger<CustomAzureSqlExecutionStrategy>>()))
                        .UseAzureSqlDefaults(environment.IsProduction())
                        .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
                }));

        var managedIdentityCredentialOption = new TokenCredentialOptions();
        // Fail fast with quick network timeout for /msi/token calls and do retry. Default retry is 5, with exponential backoff
        managedIdentityCredentialOption.Retry.NetworkTimeout = TimeSpan.FromSeconds(3);

        SqlAuthenticationProvider.SetProvider(SqlAuthenticationMethod.ActiveDirectoryManagedIdentity,
                                            new AzureSqlAuthProvider(environment.IsProduction() ?
                                            new ManagedIdentityCredential(options: managedIdentityCredentialOption) :
                                            null));

    }
}

