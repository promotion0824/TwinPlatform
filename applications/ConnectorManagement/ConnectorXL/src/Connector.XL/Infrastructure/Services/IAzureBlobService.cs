namespace Connector.XL.Infrastructure.Services;

using global::Azure;
using global::Azure.Storage.Blobs;
using global::Azure.Storage.Blobs.Models;

internal interface IAzureBlobService
{
    BlobClient GetBlobClient(string containerName, string blobName);

    AsyncPageable<BlobItem> GetAllBlobsAsync(string containerName, string path);
}
