namespace Willow.Model.Responses;

/// <summary>
/// Blob upload information.
/// </summary>
/// <param name="SasToken">Shared Access Signature Token for storage account access.</param>
/// <param name="ContainerName">Name of the container.</param>
/// <param name="BlobPaths">Path of each file name in the storage container.</param>
public record BlobUploadInfo(string SasToken, string ContainerName, Dictionary<string, string> BlobPaths);
