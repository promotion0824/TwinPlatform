using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Willow.Storage.Blobs.Options;

namespace Willow.Storage.Blobs;

public interface IBlobService
{
    Task<string> UploadFile(string container, string filePath, Stream content, bool overwrite = false, CancellationToken cancellationToken = default);
    Task<string> UploadFile(string container, string filePath, Stream content, BlobUploadOptions blobUploadOptions, CancellationToken cancellationToken = default);
    Task<Stream?> DownloadFileWithSasUri(Uri blobUri, CancellationToken cancellationToken = default);
    Task<Stream?> DownloadFile(string container, string filePath, CancellationToken cancellationToken = default);
    Task<Stream?> DownloadFile(Uri blobUri, CancellationToken cancellationToken = default);
    Task<bool> DeleteBlob(string container, string blobPath, CancellationToken cancellationToken = default);
    Task DeleteAllFilesInBlobFolder(string container, string folder, CancellationToken cancellationToken = default);
    Task<IEnumerable<BlobItem>> GetBlobItems(string container, string? prefix = null);
    Task<IEnumerable<string>> GetBlobNameByTags(string container, IEnumerable<(string, string, string)> tagFilters);
    Task<AppendBlobClient> GetOrCreateAppendBlobClient(string container, string blob);
    Task MergeTags(string container, string blob, IDictionary<string, string> tags, CancellationToken cancellationToken = default, bool merge = true);
    Task<GetBlobTagResult> GetBlobTags(string container, string blob);
    Task<IDictionary<string, GetBlobTagResult>> GetAllBlobTags(string container);
    Task<int> GetBlobsCount(string container);
    Task SetDocumentTwinMetadata(string container, string blobName, string docMeta, string docRelatedTwinMeta);
    Task<IDictionary<string, string>> GetBlobMetadata(string container, string blobName);
    Task MergeBlobMetadata(string container, string blobName, IDictionary<string, string> metadata, bool merge = true);
    Task<string?> GetBlobMd5(string container, string blobName);
}

public class BlobService : IBlobService
{
    public const string DocumentTwinMetadataKey = "documentTwinMetadata";
    public const string DocumentTwinRelatedMetadataKey = "documentTwinRelatedMetadata";
    private const int MaxMetadataLength = 8000;

    private readonly BlobStorageOptions _options;
    private readonly TokenCredential? _tokenCredential;
    private readonly ILogger<BlobService> _logger;

    public BlobService(IOptions<BlobStorageOptions> blobStorageOptions)
    {
        _options = blobStorageOptions.Value;
    }

    public BlobService(IOptions<BlobStorageOptions> blobStorageOptions, TokenCredential tokenCredential, ILogger<BlobService> logger) : this(blobStorageOptions)
    {
        _tokenCredential = tokenCredential;
        _logger = logger;
    }

    public async Task<int> GetBlobsCount(string container)
    {
        int count = 0;
        var containerClient = GetContainerClient(container);
        var allBlobs = containerClient.GetBlobsAsync();
        await foreach (var blob in allBlobs)
        {
            count++;
        }

        return count;
    }

    public async Task<string> UploadFile(string container, string filePath, Stream content, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        var containerClient = GetContainerClient(container);
        var blobClient = containerClient.GetBlobClient(filePath);
        await blobClient.UploadAsync(content, overwrite, cancellationToken);
        return blobClient.Uri.AbsoluteUri;
    }

    public async Task<string> UploadFile(string container, string filePath, Stream content, BlobUploadOptions blobUploadOptions, CancellationToken cancellationToken = default)
    {
        var containerClient = GetContainerClient(container);
        var blobClient = containerClient.GetBlobClient(filePath);
        await blobClient.UploadAsync(content, blobUploadOptions, cancellationToken);
        return blobClient.Uri.AbsoluteUri;
    }

    public async Task MergeTags(string container, string blob, IDictionary<string, string> tags, CancellationToken cancellationToken = default, bool merge = true)
    {
        var containerClient = GetContainerClient(container);
        var blobClient = containerClient.GetBlobClient(blob);
        var response = await blobClient.GetTagsAsync();
        var mergeTags = response.Value.Tags;
        if (merge)
        {
            foreach (var (key, value) in tags)
            {
                mergeTags[key] = value;
            }
        }

        await blobClient.SetTagsAsync(merge ? mergeTags : tags, cancellationToken: cancellationToken);
    }

    public async Task<GetBlobTagResult> GetBlobTags(string container, string blob)
    {
        var containerClient = GetContainerClient(container);
        var blobClient = containerClient.GetBlobClient(blob);

        return await blobClient.GetTagsAsync();
    }

    public async Task<IDictionary<string, GetBlobTagResult>> GetAllBlobTags(string container)
    {
        var containerClient = GetContainerClient(container);
        var blobItems = containerClient.GetBlobsAsync();

        var blobTags = new Dictionary<string, GetBlobTagResult>();
        await foreach (var blobItem in blobItems)
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Name);
            blobTags.Add(blobItem.Name, await blobClient.GetTagsAsync());
        }

        return blobTags;
    }

    public async Task SetDocumentTwinMetadata(string container, string blobName, string docMeta, string docRelatedTwinMeta)
    {
        var containerClient = GetContainerClient(container);
        var blobClient = containerClient.GetBlobClient(blobName);
        var blobPropResponse = await blobClient.GetPropertiesAsync();
        var blobMetatdata = blobPropResponse.Value.Metadata;
        blobMetatdata[DocumentTwinMetadataKey] = docMeta;
        if (!string.IsNullOrEmpty(docRelatedTwinMeta))
        {
            if (docMeta.Length + docRelatedTwinMeta.Length < MaxMetadataLength)
                blobMetatdata[DocumentTwinRelatedMetadataKey] = docRelatedTwinMeta;
            else
                _logger.LogWarning("Document Twin Related Metadata is too long to be stored in blob metadata.");
        }

        await blobClient.SetMetadataAsync(blobMetatdata);
    }

    public async Task MergeBlobMetadata(string container, string blobName, IDictionary<string, string> metadata, bool merge = true)
    {
        var containerClient = GetContainerClient(container);
        var blobClient = containerClient.GetBlobClient(blobName);
        var blobPropResponse = await containerClient.GetBlobClient(blobName).GetPropertiesAsync();
        var mergeMetadata = blobPropResponse.Value.Metadata;
        if (merge)
        {
            foreach (var (key, value) in metadata)
            {
                mergeMetadata[key] = value;
            }
        }

        await blobClient.SetMetadataAsync(merge ? mergeMetadata : metadata);
    }

    public async Task<IDictionary<string, string>> GetBlobMetadata(string container, string blobName)
    {
        var containerClient = GetContainerClient(container);
        var blobPropResponse = await containerClient.GetBlobClient(blobName).GetPropertiesAsync();
        return blobPropResponse.Value.Metadata;
    }

    public async Task<string?> GetBlobMd5(string container, string blobName)
    {
        try
        {
            var containerClient = GetContainerClient(container);
            var blobPropResponse = await containerClient.GetBlobClient(blobName).GetPropertiesAsync();
            return Convert.ToBase64String(blobPropResponse.Value.ContentHash);
        }
        catch (RequestFailedException ex)
        {
            if (ex.Status == 404)
            {
                _logger.LogWarning($"Blob {blobName} not found in container {container}");
                return null;
            }

            throw;
        }
    }

    public async Task<IEnumerable<string>> GetBlobNameByTags(string container, IEnumerable<(string, string, string)> tagFilters)
    {
        var blobServiceClient = GetServiceClient();

        var query = @$"@container = '{container}'";
        if (tagFilters != null && tagFilters.Any())
        {
            var filterTags = string.Join(" AND ", tagFilters.Select(x => @$"""{x.Item1}"" {x.Item2} '{x.Item3}'").ToArray());
            query += $" AND {filterTags}";
        }

        var matchingBlobNames = new List<string>();
        await foreach (var filterBlobItem in blobServiceClient.FindBlobsByTagsAsync(query))
        {
            matchingBlobNames.Add(filterBlobItem.BlobName);
        }
        return matchingBlobNames;
    }

    public async Task<bool> DeleteBlob(string container, string blobPath, CancellationToken cancellationToken = default)
    {
        var containerClient = GetContainerClient(container);
        var blobClient = containerClient.GetBlobClient(blobPath);
        return await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task DeleteAllFilesInBlobFolder(string container, string folder, CancellationToken cancellationToken = default)
    {
        var containerClient = GetContainerClient(container);
        var blobItems = containerClient.GetBlobsAsync(prefix: folder, cancellationToken: cancellationToken);

        await foreach (BlobItem blobItem in blobItems)
        {
            BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);
            await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }
    }

    public async Task<Stream?> DownloadFileWithSasUri(Uri blobUri, CancellationToken cancellationToken = default)
    {
        var blobClient = new BlobClient(blobUri);
        return await DownloadFile(blobClient, cancellationToken);
    }

    public async Task<Stream?> DownloadFile(Uri blobUri, CancellationToken cancellationToken = default)
    {
        var blobClient = new BlobClient(blobUri, _tokenCredential);
        return await DownloadFile(blobClient, cancellationToken);
    }

    public async Task<Stream?> DownloadFile(string container, string filePath, CancellationToken cancellationToken = default)
    {
        var containerClient = GetContainerClient(container);
        var blobClient = containerClient.GetBlobClient(filePath);
        return await DownloadFile(blobClient, cancellationToken);
    }

    private static async Task<Stream?> DownloadFile(BlobClient blobClient, CancellationToken cancellationToken = default)
    {
        if (!await blobClient.ExistsAsync(cancellationToken))
            return null;

        var download = await blobClient.DownloadAsync(cancellationToken);
        return download.Value.Content;
    }

    public async Task<IEnumerable<BlobItem>> GetBlobItems(string container, string? prefix = null)
    {
        var containerClient = GetContainerClient(container);
        // AsyncPageable does not support linq,
        // use async foreach instead.
        var blobItems = new List<BlobItem>();
        var blobs = containerClient.GetBlobsAsync(prefix: prefix);
        await foreach (var blob in blobs)
        {
            blobItems.Add(blob);
        }

        return blobItems;
    }

    /// <summary>
    /// Get or create append blob client
    /// </summary>
    /// <returns>Append blob client</returns>
    public async Task<AppendBlobClient> GetOrCreateAppendBlobClient(string container, string blob)
    {
        var containerClient = GetContainerClient(container);
        var appendBlobClient = containerClient.GetAppendBlobClient(blob);
        if (!await appendBlobClient.ExistsAsync())
        {
            await appendBlobClient.CreateAsync();
        }

        return appendBlobClient;
    }

    private BlobServiceClient GetServiceClient()
    {
        var blobClientOptions = new BlobClientOptions();
        blobClientOptions.Retry.NetworkTimeout = _options.Timeout;

        if (!string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            return new BlobServiceClient(_options.ConnectionString, blobClientOptions);
        }

        var accountUri = new Uri($"https://{_options.AccountName}.blob.core.windows.net");

        if (_tokenCredential is not null)
        {
            return new BlobServiceClient(accountUri, _tokenCredential, blobClientOptions);
        }

        if (_options.AccountName is not null)
        {
            return new BlobServiceClient(accountUri, new DefaultAzureCredential(), blobClientOptions);
        }

        throw new NotSupportedException("Blob Service authentication type not supported.");
    }

    private BlobContainerClient GetContainerClient(string container)
    {
        var serviceClient = GetServiceClient();
        return serviceClient.GetBlobContainerClient(container);
    }
}
