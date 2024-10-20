namespace Willow.Api.Common.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Willow.Api.Common.Infrastructure;
using Willow.Api.Common.Runtime;

/// <summary>
/// A set of extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the ApplicationContextOptions to the service collection.
    /// </summary>
    /// <typeparam name="T">The Type of configuration section to add to the services.</typeparam>
    /// <param name="services">The existing services collection.</param>
    /// <param name="configurationSection">The configuration section to add.</param>
    /// <returns>An object of type T loaded from the configuration section.</returns>
    public static T AddApplicationContextOptions<T>(
        this IServiceCollection services,
        IConfigurationSection configurationSection)
        where T : class, new()
    {
        var options = new T();
        services.Configure<T>(configurationSection);
        configurationSection.Bind(options);
        return options;
    }

    /// <summary>
    /// Adds the HttpContextAccessor and CurrentHttpContext to the service collection.
    /// </summary>
    /// <param name="services">The current collection of services.</param>
    /// <returns>The updated services collection.</returns>
    public static IServiceCollection AddCurrentHttpContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentHttpContext, CurrentHttpContext>();
        return services;
    }

    /// <summary>
    /// Adds the FileDownloadService to the service collection.
    /// </summary>
    /// <param name="services">The current collection of services.</param>
    /// <returns>The updated services collection.</returns>
    public static IServiceCollection AddFileDownloadService(this IServiceCollection services)
    {
        services.AddHttpClient<IFileDownloadService, FileDownloadService>();

        return services;
    }
}
