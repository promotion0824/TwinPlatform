namespace Willow.IoTService.Deployment.ManifestStorage;

public interface IManifestStorageService
{
    string BaseDeploymentTemplateName { get; }

    Task<(string containerName, string blobName)> UploadManifestAsync(Guid deploymentId,
                                                                      Stream content,
                                                                      CancellationToken cancellationToken = default);

    Task<(string containerName, string blobName, Stream content)> DownloadManifestAsync(Guid deploymentId,
                                                                                        CancellationToken cancellationToken = default);


    Task<(string containerName, string blobName)> UploadTemplateAsync(string templateType,
                                                                      string version,
                                                                      Stream templateFileStream,
                                                                      bool shouldOverwrite = false,
                                                                      CancellationToken cancellationToken = default);


    Task<(string containerName, string blobName, Stream content)> DownloadTemplateAsync(string templateType,
                                                                                        string version,
                                                                                        CancellationToken cancellationToken = default);
}
