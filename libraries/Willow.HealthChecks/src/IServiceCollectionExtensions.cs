namespace Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;

/// <summary>
/// Extensions for the <see cref="IServiceCollection"/> interface.
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds health checks.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> object that this method extends.</param>
    /// <param name="appName">The name of the application.</param>
    /// <returns>The service collection object for chaining.</returns>
    public static IHealthChecksBuilder AddWillowHealthChecks(this IServiceCollection services, string appName)
    {
        services.AddSingleton<IHealthCheckPublisher>(s => new HealthCheckPublisher(appName));

        return services.AddHealthChecks();
    }
}
