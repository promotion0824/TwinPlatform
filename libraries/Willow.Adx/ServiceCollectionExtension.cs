namespace Microsoft.Extensions.DependencyInjection;

using Willow.Adx;

/// <summary>
/// Add Willow ADX Service.
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    /// Add Willow ADX Service. Requires memory cache to be registered.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> object that this method extends.</param>
    /// <param name="configure">Configures the instance.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance so that calls can be chained.</returns>
    public static IServiceCollection AddWillowAdxService(this IServiceCollection services, Action<AdxConfig> configure)
    {
        services.Configure(configure);
        services.AddTransient<IAdxService, AdxService>();
        return services;
    }
}
