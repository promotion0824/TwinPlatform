namespace Willow.FeatureFlagsProvider.FeatureFlags;

using ConfigCat.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// The service collection extensions.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add the feature flags provider to the service collection.
    /// </summary>
    /// <param name="services">The existing service collection.</param>
    /// <param name="configuration">The configuration for the feature flags provider.</param>
    public static void AddFeatureFlagsProvider(this IServiceCollection services, IConfiguration configuration)
    {
        var configCatSettings = configuration.GetSection("ConfigCat").Get<ConfigCatSettings>();

        if (!string.IsNullOrWhiteSpace(configCatSettings?.SdkKey))
        {
            var configCatClient = ConfigCatClient.Get(configCatSettings.SdkKey,
                                    options =>
                                    {
                                        options.PollingMode = PollingModes.AutoPoll(pollInterval: TimeSpan.FromSeconds(configCatSettings.PollingIntervalInSeconds));
                                    });

            services.AddSingleton(configCatClient);
            services.AddScoped<IFeatureFlagsProvider, ConfigCatFeatureFlagsProvider>();
        }
    }
}
