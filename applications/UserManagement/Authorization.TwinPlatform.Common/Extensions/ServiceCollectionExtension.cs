using Authorization.TwinPlatform.Common.Abstracts;
using Authorization.TwinPlatform.Common.Authorization.Handlers;
using Authorization.TwinPlatform.Common.Authorization.Providers;
using Authorization.TwinPlatform.Common.Options;
using Authorization.TwinPlatform.Common.Services;
using Azure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Authorization.TwinPlatform.Common.Extensions;

/// <summary>
/// Class for DI Service Collection extension methods
/// </summary>
public static class ServiceCollectionExtension
{

    /// <summary>
    /// Adds MemoryCache, AuthorizationAPIOption and Registers IUserAuthorizationService,IAuthorizationApiTokenService with the Service Collection
    /// IUserAuthorizationService can be used within your controllers or AuthorizationHandlers to retrieve user's permissions
    /// <para><see href="https://willow.atlassian.net/wiki/spaces/WCP/pages/2351759595/How+to+integrate+with+User+Management+Services">Learn how to use this service</see></para>
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <param name="configurationSection">AuthorizationAPI Configuration section</param>
    /// <param name="IsDevelopment">True if development environment; false otherwise</param>
    /// <exception cref="KeyNotFoundException"></exception>
    public static void AddUserManagementCoreServices(this IServiceCollection services, IConfigurationSection configurationSection, bool IsDevelopment = false)
    {
        ArgumentNullException.ThrowIfNull(configurationSection);

        //Configure AuthorizationAPI Option
        var authAPIOption = configurationSection.Get<AuthorizationAPIOption>();
        if (authAPIOption == null)
            throw new KeyNotFoundException($"Unable to find the {AuthorizationAPIOption.APIName} configuration for User Management");
        services.Configure<AuthorizationAPIOption>(configurationSection);

        // Configure Http Client
        services.AddHttpClient(AuthorizationAPIOption.APIName, httpClient =>
        {
            httpClient.BaseAddress = new Uri(authAPIOption.BaseAddress);
            httpClient.Timeout = TimeSpan.FromMilliseconds(Math.Max(AuthorizationAPIOption.MinimumAPITimeoutMilliseconds, authAPIOption.APITimeoutMilliseconds));
        });

        // Configure Azure Token Service  -  Dependency for UserAuthorizationService
        services.AddSingleton<IAuthorizationApiTokenService>((sp) =>
        {
            ChainedTokenCredential chainedCredentials = IsDevelopment ?
            new(new AzureCliCredential(), new VisualStudioCredential(), new DefaultAzureCredential()) :
            new(new ManagedIdentityCredential(), new DefaultAzureCredential(false));

            return new AuthorizationApiTokenService(sp.GetRequiredService<IMemoryCache>(),
                sp.GetRequiredService<IOptions<AuthorizationAPIOption>>(),
                chainedCredentials);
        });
        services.AddSingleton<IImportService, ImportService>();

        services.AddScoped<IUserAuthorizationService, UserAuthorizationService>();
        services.AddScoped<IClientAuthorizationService, ClientAuthorizationService>();
        services.AddScoped<IAdminService, AdminService>();

        // Add Memory Cache for caching token
        services.AddMemoryCache();
    }

    /// <summary>
    /// <para>Extension method for registering Permission based Policy Evaluation</para>
    /// <para>This method internally calls <see cref="AddUserManagementCoreServices(IServiceCollection, IConfigurationSection)"/> for registering core permission api services</para>
    /// <para><see href="https://willow.atlassian.net/wiki/spaces/WCP/pages/2351759595/How+to+integrate+with+User+Management+Services">Learn how to use this approach for Authorization</see></para>
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <param name="configurationSection">AuthorizationAPI Configuration section</param>
    /// <param name="IsDevelopment">True if development environment; false otherwise</param>
    public static void AddPermissionBasedPolicyAuthorization(this IServiceCollection services, IConfigurationSection configurationSection, bool IsDevelopment = false)
    {
        //Register the Core Services required for Calling Permission API
        services.AddUserManagementCoreServices(configurationSection, IsDevelopment);

        // Configure Permission based Policy Provider
        services.AddSingleton<IAuthorizationPolicyProvider, AuthorizePermissionPolicyProvider>();

        // Configure Permission based Policy Handler
        services.AddScoped<IAuthorizationHandler, AuthorizePermissionPolicyHandler>();
    }

    const string importFileFormat = "usermanagement.import.{0}.json";

    /// <summary>
    /// Adds Environment Specific (based on AuthorizationAPI:Import:InstanceType) configuration json file to configuration builder
    /// </summary>
    /// <param name="builder">Instance of Configuration Builder</param>
    /// <param name="instanceType">Type of the environment.</param>
    /// <remarks>
    /// Methods assumes the configuration file names in format "usermanagement.import.{0}.json", 0 => AuthorizationAPI:Import:InstanceType
    /// </remarks>
    public static void AddEnvironmentSpecificConfigSource(this IConfigurationBuilder builder, string instanceType)
    {
        if (string.IsNullOrWhiteSpace(instanceType))
            return;

        string importFileName = string.Format(importFileFormat, instanceType);

        if (File.Exists(importFileName))
        {
            builder.AddJsonFile(importFileName, optional: false);
        }
    }
}
