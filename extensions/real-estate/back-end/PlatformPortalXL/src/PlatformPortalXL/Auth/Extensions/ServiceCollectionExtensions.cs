using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Auth.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register all authorization requirements.
    /// </summary>
    /// <remarks>
    /// Requirements are registered here so requirements can be resolved via IOC. See UserPolicyAuthService for usage.
    /// </remarks>
    public static IServiceCollection AddAuthorizationRequirements(this IServiceCollection services)
    {
        var typeT = typeof(WillowAuthorizationRequirement);
        var types = typeT.Assembly.GetTypes().Where(p => typeT.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);

        foreach (var implementationType in types)
        {
            services.Add(new ServiceDescriptor(typeT, implementationType, ServiceLifetime.Singleton));
        }

        return services;
    }

    public static IServiceCollection AddAuthorizationHandlers(this IServiceCollection services)
    {
        var typeT = typeof(IWillowAuthorizationHandler);
        var types = typeT.Assembly.GetTypes().Where(p => typeT.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);

        foreach (var implementationType in types)
        {
            services.Add(new ServiceDescriptor(typeof(IAuthorizationHandler), implementationType, ServiceLifetime.Singleton));
        }

        return services;
    }

    public static IServiceCollection AddAccessControlServices(this IServiceCollection services)
    {
        services.AddUserManagementServices();

        services.AddSingleton<IAuthService, AuthorizationService>();
        services.AddSingleton<IPolicyDecisionService, PolicyDecisionService>();

        services.AddTransient<IUserAuthorizedSitesService, UserAuthorizedSitesService>();

        return services;
    }

    private static void AddUserManagementServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();

        services.AddTransient<IUserManagementService, UserManagementService>();
        services.AddHostedService<UserManagementImportHostedService>();
    }

    public static IServiceCollection AddAncestralTwinLookupServices(this IServiceCollection services)
    {
        services.AddSingleton<IAncestralTwinsSearchService, AncestralTwinsSearchService>();
        services.AddHostedService<AncestralTwinsCacheUpdateHostedService>();

        return services;
    }

    public static void AddSupportForLegacyAuthPermissionsAgainstUserManagement(this IServiceCollection services)
    {
        services.AddSingleton<ISiteIdToTwinIdMatchingService, SiteIdToTwinIdMatchingService>();

        services.AddSingleton<WillowAuthorizationRequirement, ViewSites>();
        services.AddSingleton<IAuthorizationHandler, ViewSitesLegacyPermissionEvaluator>();

        services.AddHostedService<SiteTwinCacheUpdateHostedService>();
    }
}
