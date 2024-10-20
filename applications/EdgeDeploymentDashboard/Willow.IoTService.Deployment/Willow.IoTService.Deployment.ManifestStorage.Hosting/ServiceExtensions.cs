using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Willow.IoTService.Deployment.ManifestStorage.Hosting;

public static class ServiceExtensions
{
    private static void UseManifestStorage(this IServiceCollection services, string templateStorageName)
    {
        services.AddTransient<IManifestStorageService>(sp => new ManifestStorageService(templateStorageName,
            sp.GetRequiredService<TokenCredential>()));
        services.AddMemoryCache();
    }

    public static void UseManifestStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var config = configuration.GetSection(nameof(ManifestStorageOptions))
                                  .Get<ManifestStorageOptions>();
        if (config != null) services.UseManifestStorage(config.TemplateStorageName);
        else throw new ArgumentException($"Missing configuration section: {nameof(ManifestStorageOptions)}");
    }
}
