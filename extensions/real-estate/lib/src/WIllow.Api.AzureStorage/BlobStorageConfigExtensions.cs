using System;
using Microsoft.Extensions.Logging;

using Azure.Identity;

using Willow.Azure.Storage;
using Willow.Common;
using Willow.Logging;

namespace Willow.Api.AzureStorage
{
    public static class BlobStorageConfigExtensions
    {
        public static IBlobStore CreateBlobStore(this BlobStorageConfig config, string path, ILogger logger, bool createContainer = true)
        {
            var accountName = config?.AccountName ?? throw new ArgumentNullException(nameof(config.AccountName));
            var container   = config?.ContainerName ?? throw new ArgumentNullException(nameof(config.ContainerName));
            var accessKey   = config?.AccountKey;

            // If connection string provided then use that
            if(!string.IsNullOrWhiteSpace(config.ConnectionString) && !config.ConnectionString.StartsWith("[value", StringComparison.InvariantCultureIgnoreCase)) // "[value": Value in json file when no config elsewhere
            {
                logger?.LogInformation("Azure storage created with connection string", new { Container = container, ConnectionString = config.ConnectionString } );

                if(!string.IsNullOrWhiteSpace(path))
                {
                    container += "/" + path;
                }

                return new AzureBlobStorage(config.ConnectionString, container);
            }

            // If we have an access key build a connection string
            if(!string.IsNullOrWhiteSpace(accessKey) && !accessKey.StartsWith("[value", StringComparison.InvariantCultureIgnoreCase)) // "[value": Value in json file when no config elsewhere
            {
                var connectionString = $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accessKey};EndpointSuffix=core.windows.net";

                logger?.LogInformation("Azure storage created with access key", new { Container = container, ConnectionString = $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accessKey.Substring(0, 6)};EndpointSuffix=core.windows.net" } );

                if(!string.IsNullOrWhiteSpace(path))
                {
                    container += "/" + path;
                }

                return new AzureBlobStorage(connectionString, container);
            }

            // No access key, attempt managed identity access
            var storageUri  = $"https://{accountName}.blob.core.windows.net/{container}";
            var credentials = new DefaultAzureCredential();

            logger?.LogInformation("Azure storage created with managed identity", new { StorageUri = storageUri } );

            return new AzureBlobStorage(new Uri(storageUri), credentials, path ?? "", createContainer);
        }
    }
}
