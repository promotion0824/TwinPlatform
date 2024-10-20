namespace Microsoft.Extensions.DependencyInjection;

using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Willow.Hosting;

/// <summary>
/// Extensions for the <see cref="IServiceCollection"/> interface.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a <see cref="DefaultAzureCredential"/> singleton, configured based on the environment.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> object that this method extends.</param>
    /// <param name="environment">The current host environment.</param>
    /// <returns>The service collection so that calls can be chained.</returns>
    [Obsolete("Use AddTokenCredential for better performance")]
    public static IServiceCollection AddDefaultAzureCredential(this IServiceCollection services, IWebHostEnvironment environment)
    {
        var isProd = environment.IsProduction();

        var defaultAzureCredential = new DefaultAzureCredential(
                   new DefaultAzureCredentialOptions
                   {
                       ExcludeAzureCliCredential = isProd,
                       ExcludeAzureDeveloperCliCredential = isProd,
                       ExcludeAzurePowerShellCredential = isProd,
                       ExcludeVisualStudioCodeCredential = isProd,
                       ExcludeVisualStudioCredential = isProd,
                       ExcludeSharedTokenCacheCredential = false,
                       ExcludeEnvironmentCredential = isProd,
                       ExcludeInteractiveBrowserCredential = isProd,
                       ExcludeManagedIdentityCredential = !isProd,
                       ExcludeWorkloadIdentityCredential = false,
                   });

        return services.AddSingleton(defaultAzureCredential);
    }

    /// <summary>
    /// Adds a <see cref="TokenCredential"/> singleton, configured based on the environment.
    /// </summary>
    /// <remarks>
    /// There is no direct way to set the timeout on the <see cref="ManagedIdentityCredential"/>. This method
    /// constructs a <see cref="HttpClient"/> in the same manner as the underlying code.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> object that this method extends.</param>
    /// <param name="environment">The current host environment.</param>
    /// <param name="configure">Action to configure options for Managed Identity credential.</param>
    /// <returns>The service collection so that calls can be chained.</returns>
    public static IServiceCollection AddTokenCredential(this IServiceCollection services, IWebHostEnvironment environment, Action<ManagedIdentityCredentialOptions> configure)
    {
        services.Configure(configure);

        return services.AddSingleton(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<ManagedIdentityCredentialOptions>>();
            var optionsValue = options?.Value ?? new ManagedIdentityCredentialOptions();

            TokenCredential credential =
                environment.IsProduction()
                    ? CreateManagedIdentityCredential(optionsValue)
                    : CreateDefaultAzureCredential();
            return credential;
        });
    }

    private static DefaultAzureCredential CreateDefaultAzureCredential()
    {
        return new DefaultAzureCredential(
            new DefaultAzureCredentialOptions
            {
                ExcludeVisualStudioCodeCredential = false,
                ExcludeManagedIdentityCredential = true,
            });
    }

    private static ManagedIdentityCredential CreateManagedIdentityCredential(ManagedIdentityCredentialOptions options)
    {
        var tokenOptions = new TokenCredentialOptions
        {
            Retry = { NetworkTimeout = TimeSpan.FromSeconds(options.Timeout) },
        };

        return new ManagedIdentityCredential(options: tokenOptions);
    }
}
