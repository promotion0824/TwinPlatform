namespace Willow.Api.Client.Sdk;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Willow.Api.Client.Sdk.Directory.Options;
using Willow.Api.Client.Sdk.Directory.Services;
using Willow.Api.Client.Sdk.Marketplace.Options;
using Willow.Api.Client.Sdk.Marketplace.Services;
using Willow.Api.Common.Extensions;

/// <summary>
/// A set of extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    private static Action<IServiceProvider, HttpClient> AddHttpClient(IApiOptions apiOptions)
        => (_, httpClient) =>
        {
            httpClient.BaseAddress = new Uri($"{apiOptions.BaseAddress.TrimEnd('/')}/");
        };

    /// <summary>
    /// Adds the directory API to the service collection.
    /// </summary>
    /// <param name="services">The existing services collection.</param>
    /// <param name="directoryApiConfigSection">The config section to use for the directory api.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddDirectoryApi(
        this IServiceCollection services,
        IConfigurationSection directoryApiConfigSection)
    {
        services.AddCurrentHttpContext();

        var options = new DirectoryApiOptions();
        directoryApiConfigSection.Bind(options);

        services.AddHttpClient<IUserLookupService, UserLookupService>(AddHttpClient(options));
        return services;
    }

    /// <summary>
    /// Adds the MarketPlace API to the service collection.
    /// </summary>
    /// <param name="services">The existing services collection.</param>
    /// <param name="marketplaceApiConfigSection">The config section to use for the marketplace api.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddMarketplaceApi(
        this IServiceCollection services, IConfigurationSection marketplaceApiConfigSection)
    {
        var options = new MarketplaceApiOptions();
        marketplaceApiConfigSection.Bind(options);
        services.AddHttpClient<IMarketplaceApiService, MarketplaceApiService>(AddHttpClient(options));

        return services;
    }
}
