using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Willow.Storage.Blobs;
using Willow.Storage.Blobs.Options;
using Willow.Storage.Providers;
using Willow.Storage.Repositories;

namespace Willow.Storage;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStorageService(this IServiceCollection services, IConfigurationSection storageConfigSection)
    {
        services.AddOptions<BlobStorageOptions>().Configure<IConfiguration>((options, configuration) =>
            {
                configuration.GetSection(storageConfigSection.Key).Bind(options);
            });
        services.AddTransient<IStorageSasProvider, StorageSasProvider>();
        services.AddTransient<IBlobService, BlobService>();

        return services;
    }

    public static void AddGitHubRepositoryService(this IServiceCollection services)
    {
        services.AddHttpClient(nameof(GitHubRepositoryService), client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            client.DefaultRequestHeaders.Add("User-Agent", "Other");
        });
        services.AddTransient<IRepositoryService, GitHubRepositoryService>();
    }
}
