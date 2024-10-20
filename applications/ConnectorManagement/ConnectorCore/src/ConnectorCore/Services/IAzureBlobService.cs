namespace ConnectorCore.Services;

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ConnectorCore.Models;

internal interface IAzureBlobService
{
    Task<BlobContainerClient> GetContainerClient(string containerName, ScannerBlobStorageOptions options);

    Task<IList<BlobItem>> GetBlobsAsync(BlobContainerClient containerClient, string prefix);
}
