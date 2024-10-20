namespace Connector.XL.Infrastructure.Services;

using Connector.XL.Common.Models;
using global::Azure;
using global::Azure.Identity;
using global::Azure.Storage;
using global::Azure.Storage.Blobs;
using global::Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

internal class AzureBlobService : IAzureBlobService
{
    private readonly ConnectorExportImportOptions options;

    public AzureBlobService(IOptions<ConnectorExportImportOptions> options)
    {
        this.options = options.Value;
    }

    public AsyncPageable<BlobItem> GetAllBlobsAsync(string containerName, string path)
    {
        var blobContainerClient = GetBlobServiceClient().GetBlobContainerClient(containerName);
        return blobContainerClient.GetBlobsAsync(prefix: path);
    }

    public BlobClient GetBlobClient(string containerName, string blobName)
    {
        var blobContainerClient = GetBlobServiceClient().GetBlobContainerClient(containerName);
        return blobContainerClient.GetBlobClient(blobName);
    }

    private BlobServiceClient GetBlobServiceClient()
    {
        var accountUri = new Uri($"https://{options.StorageAccountName}.blob.core.windows.net");

        if (!string.IsNullOrEmpty(options.StorageKey))
        {
            return new BlobServiceClient(accountUri, new StorageSharedKeyCredential(options.StorageAccountName, options.StorageKey));
        }

        return new BlobServiceClient(accountUri, new DefaultAzureCredential());
    }
}
