using Azure;
using Azure.Core;
using Azure.Storage.Blobs;

namespace Willow.IoTService.Deployment.ManifestStorage;

public class ManifestStorageService(string templateStorageName, TokenCredential tokenCredential) : IManifestStorageService
{
    private const string _templateContainerPrefix = "manifest-templates";
    private const string _manifestContainerName = "deployment-manifests";
    private const string _baseDeploymentTemplateName = "base-deployment";
    private readonly Uri _templateBlobUri = new($"https://{templateStorageName}.blob.core.windows.net/");

    public async Task<(string containerName, string blobName)> UploadManifestAsync(Guid deploymentId,
                                                                                   Stream content,
                                                                                   CancellationToken cancellationToken = default)
    {
        var serviceClient = new BlobServiceClient(_templateBlobUri, tokenCredential);
        var containerClient = serviceClient.GetBlobContainerClient(_manifestContainerName);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobName = $"{deploymentId:D}.json";
        await containerClient.UploadBlobAsync(blobName,
                                              content,
                                              cancellationToken);

        return (_manifestContainerName, blobName);
    }

    public async Task<(string containerName, string blobName, Stream content)> DownloadManifestAsync(Guid deploymentId,
                                                                                                     CancellationToken cancellationToken = default)
    {
        var serviceClient = new BlobServiceClient(_templateBlobUri, tokenCredential);
        var containerClient = serviceClient.GetBlobContainerClient(_manifestContainerName);
        var blobName = $"{deploymentId:D}.json";
        var blobClient = containerClient.GetBlobClient(blobName);
        if (!await containerClient.ExistsAsync(cancellationToken) || !await blobClient.ExistsAsync(cancellationToken)) throw new TemplateNotFoundException($"Manifest not found for deployment {deploymentId}");

        var response = await blobClient.DownloadAsync(cancellationToken);
        return (_manifestContainerName, blobName, response.Value.Content);
    }

    public async Task<(string containerName, string blobName)> UploadTemplateAsync(string templateType,
                                                                                   string version,
                                                                                   Stream templateFileStream,
                                                                                   bool shouldOverwrite = false,
                                                                                   CancellationToken cancellationToken = default)
    {
        var serviceClient = new BlobServiceClient(_templateBlobUri, tokenCredential);

        var containerName = GetContainerName(templateType);
        var containerClient = serviceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobName = GetBlobName(version);
        var blobClient = containerClient.GetBlobClient(blobName);
        try
        {
            await blobClient.UploadAsync(templateFileStream,
                                         shouldOverwrite,
                                         cancellationToken);
        }
        catch (RequestFailedException e)
        {
            throw new TemplateExistsException($"Template already exists for {templateType} version {version}", e);
        }

        return (containerName, blobName);
    }

    public async Task<(string containerName, string blobName, Stream content)> DownloadTemplateAsync(string templateType,
                                                                                                     string version,
                                                                                                     CancellationToken cancellationToken = default)
    {
        var serviceClient = new BlobServiceClient(_templateBlobUri, tokenCredential);

        var containerName = GetContainerName(templateType);
        var containerClient = serviceClient.GetBlobContainerClient(containerName);
        var blobName = GetBlobName(version);
        var blobClient = containerClient.GetBlobClient(blobName);
        if (!await containerClient.ExistsAsync(cancellationToken) || !await blobClient.ExistsAsync(cancellationToken)) throw new TemplateNotFoundException($"Template not found for {templateType}");

        var response = await blobClient.DownloadAsync(cancellationToken);
        return (containerName, blobName, response.Value.Content);
    }

    public string BaseDeploymentTemplateName => _baseDeploymentTemplateName;

    private static string GetBlobName(string version)
    {
        return $"{version.Replace('.', '-')}.json";
    }

    private static string GetContainerName(string moduleType)
    {
        return $"{_templateContainerPrefix}-{moduleType}".ToLowerInvariant();
    }
}
