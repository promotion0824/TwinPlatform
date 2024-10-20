namespace ConnectorCore.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Azure.Identity;
    using Azure.Storage;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using ConnectorCore.Models;
    using MassTransit.Internals;

    internal class AzureBlobService : IAzureBlobService
    {
        public async Task<IList<BlobItem>> GetBlobsAsync(BlobContainerClient container, string prefix)
        {
            return await container.GetBlobsAsync(prefix: prefix).ToListAsync();
        }

        public async Task<BlobContainerClient> GetContainerClient(string containerName, ScannerBlobStorageOptions options)
        {
            var container = GetBlobServiceClient(options).GetBlobContainerClient(containerName);

            await container.CreateIfNotExistsAsync();

            return container;
        }

        private BlobServiceClient GetBlobServiceClient(ScannerBlobStorageOptions options)
        {
            var accountUri = new Uri($"https://{options.StorageAccountName}.blob.core.windows.net");

            if (!string.IsNullOrEmpty(options.StorageKey))
            {
                return new BlobServiceClient(accountUri,
                    new StorageSharedKeyCredential(options.StorageAccountName, options.StorageKey));
            }

            return new BlobServiceClient(accountUri, new DefaultAzureCredential());
        }
    }
}
