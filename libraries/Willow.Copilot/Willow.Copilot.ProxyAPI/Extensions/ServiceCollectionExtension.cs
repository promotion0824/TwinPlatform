namespace Willow.Copilot.ProxyAPI.Extensions;

using Willow.Copilot.ProxyAPI;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension method for registering copilot services.
/// </summary>
public static class ServiceCollectionExtension
{
    private static Action<IServiceProvider, HttpClient> ConfigureHttpClient(CopilotSettings settings)
        => (serviceProvider, httpClient) =>
        {
            httpClient.BaseAddress = new Uri(settings.BaseAddress);
        };

    /// <summary>
    /// Configures copilot client services for dependency injection.
    /// </summary>
    /// <param name="services">IServiceCollection instance.</param>
    /// <param name="settings">Instance of <see cref="CopilotSettings"/>.</param>
    /// <returns>IServiceCollection instance for chaining.</returns>
    public static IServiceCollection ConfigureCopilotClients(this IServiceCollection services, CopilotSettings settings)
    {
        services.AddHttpClient<ICopilotClient, CopilotClient>(ConfigureHttpClient(settings));

        return services;
    }
}
