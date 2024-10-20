using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PlatformPortalXL.Services.CognitiveSearch.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTwinSearchServices(this IServiceCollection services, IConfiguration configuration)
    {
        // If fine-grained auth is enabled AzureSearchScopedTwinSearch should be used, otherwise use SearchService.
        services.AddScoped<ISearchService, ScopedTwinSearchService>();
        services.AddScoped<ISearchService, SearchService>();

        services.Configure<AzureCognitiveSearchOptions>(configuration.GetSection(nameof(AzureCognitiveSearchOptions)));
        services.AddScoped<TwinSearchClient>();

        return services;
    }
}
