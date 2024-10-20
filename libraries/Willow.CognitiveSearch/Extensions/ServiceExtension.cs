namespace Willow.CognitiveSearch.Extensions;

using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Willow.CognitiveSearch.Index;

/// <summary>
/// Service Extensions class for registering services.
/// </summary>
public static class ServiceExtension
{
    /// <summary>
    /// Adds AI Search Services from Willow Cognitive Search Library.
    /// </summary>
    /// <param name="services">Service Collection.</param>
    /// <param name="searchConfigSectionName"><see cref="AISearchSettings"/> configuration section name.</param>
    /// <returns>Service Collection for chaining.</returns>
    /// <remarks>
    /// Dependency : <see cref="DefaultAzureCredential"/> is required dependency for the service.
    /// </remarks>
    public static IServiceCollection AddAISearchServices(this IServiceCollection services, string searchConfigSectionName)
    {
        services.AddSingleton<HealthCheckSearch>();

        // Configure ACS Search Settings
        services.AddOptions<AISearchSettings>().BindConfiguration(searchConfigSectionName);

        // Add Unified Index Search Service.
        services.AddScoped<ISearchService<UnifiedItemDto>, SearchService<UnifiedItemDto>>();

        // Add Document Index Search Service.
        services.AddScoped<ISearchService<DocumentChunkDto>, SearchService<DocumentChunkDto>>();

        return services;
    }

    /// <summary>
    /// Adds AI Search and Index Builder service from Willow Cognitive Search Library.
    /// </summary>
    /// <param name="services">Service Collection.</param>
    /// <param name="searchConfigSectionName"><see cref="AISearchSettings"/> configuration section name.</param>
    /// <returns>Service Collection for chaining.</returns>
    /// <remarks>
    /// Dependency : <see cref="DefaultAzureCredential"/> is required dependency for the service.
    /// </remarks>
    public static IServiceCollection AddAIIndexBuildServices(this IServiceCollection services, string searchConfigSectionName)
    {
        services.AddAISearchServices(searchConfigSectionName);
        services.AddScoped<IIndexBuildService, IndexBuildService>();
        services.AddSingleton<PendingDocsQueue>((sp) =>
        {
            return new PendingDocsQueue(sp.GetRequiredService<HealthCheckSearch>(), sp.GetRequiredService<ILogger<PendingDocsQueue>>());
        });
        return services;
    }

    /// <summary>
    /// Adds AI Search, Index Builder and Indexer Builder service from Willow Cognitive Search Library.
    /// </summary>
    /// <param name="services">Service Collection.</param>
    /// <param name="searchConfigSectionName"><see cref="AISearchSettings"/> configuration section name.</param>
    /// <returns>Service Collection for chaining.</returns>
    /// <remarks>
    /// Dependency : <see cref="DefaultAzureCredential"/> is required dependency for the service.
    /// </remarks>
    public static IServiceCollection AddAIIndexerBuildServices(this IServiceCollection services, string searchConfigSectionName)
    {
        services.AddAIIndexBuildServices(searchConfigSectionName);
        services.AddTransient<IIndexerBuildService, IndexerBuildService>();
        return services;
    }
}
