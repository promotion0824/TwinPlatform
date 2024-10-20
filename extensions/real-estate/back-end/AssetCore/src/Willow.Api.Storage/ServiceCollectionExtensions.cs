using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Azure.Identity;

using Willow.Common;
using Willow.Azure.Storage;
using Willow.Logging;

namespace Willow.Api.Storage
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureStorage<T>(this IServiceCollection services, IConfiguration configuration)
        { 
            services.AddSingleton<IBlobStore>( p=> 
            {
                var config      = p.GetRequiredService<IOptions<BlobStorageConfig>>();
                var logger      = p.GetService<ILogger<T>>();
                var accountName = config?.Value?.AccountName ?? "test";
                var container   = config?.Value?.ContainerName ?? config?.Value?.AssetFileContainer ?? "twinengine";
                var accessKey   = config?.Value?.AccountKey ?? config?.Value?.Key;

                // If we have an access key build a connection string
                if(!string.IsNullOrWhiteSpace(accessKey) && !accessKey.StartsWith("[value")) // It's returning "[value..." from the json file if not located elsewhere 
                { 
                    var connectionString = $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accessKey};EndpointSuffix=core.windows.net";

                    if(logger != null)
                        logger.LogInformation("Asset storage created", new { Container = container, ConnectionString = $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accessKey.Substring(0, 6)};EndpointSuffix=core.windows.net" } );

                    return new AzureBlobStorage(connectionString, container);
                }

                // No access key, attempt managed identity access
                var storageUri  = $"https://{accountName}.blob.core.windows.net/{container}";
                var credentials = new DefaultAzureCredential();

                if(logger != null)
                    logger.LogInformation("Asset storage created", new { StorageUri = storageUri, AccessKey = accessKey?.Substring(0, 6) ?? ""} );

                return new AzureBlobStorage(new Uri(storageUri), credentials, "");
            });

            return services;
        }    
    }
}
