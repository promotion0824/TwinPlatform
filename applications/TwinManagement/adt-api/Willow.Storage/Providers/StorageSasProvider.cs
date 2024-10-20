using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;

namespace Willow.Storage.Providers;

public interface IStorageSasProvider
{
    Task<string> GenerateContainerSasTokenAsync(string storageAccountName,
                                                string containerName,
                                                TimeSpan expiration,
                                                CancellationToken cancellationToken = default);

    Task<string> GenerateBlobSasTokenAsync(string storageAccountName,
                                            string containerName,
                                            string blobName,
                                            TimeSpan? expiration = null,
                                            BlobSasPermissions? permissions = null,
                                            CancellationToken cancellationToken = default);
}

public class StorageSasProvider (TokenCredential tokenCredential) : IStorageSasProvider
{
    private const int MaxUserDelegationExpiryDay = 7;

    public async Task<string> GenerateContainerSasTokenAsync(
        string storageAccountName,
        string containerName,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        var startOn = DateTimeOffset.UtcNow.AddMinutes(-5); // To account for clock skew
        var expiresAt = DateTimeOffset.UtcNow.Add(expiration);

        var blobContainerClient = GetBlobContainerClient(storageAccountName, containerName);
        var blobServiceClient = blobContainerClient.GetParentBlobServiceClient();
        var userDelegatedKey =
            await blobServiceClient.GetUserDelegationKeyAsync(
                startOn,
                expiresAt,
                cancellationToken);

        var sasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = blobContainerClient.Name,
            Resource = "c",
            StartsOn = startOn,
            ExpiresOn = expiresAt
        };

        sasBuilder.SetPermissions(
            BlobContainerSasPermissions.Read |
            BlobContainerSasPermissions.Add |
            BlobContainerSasPermissions.Create |
            BlobContainerSasPermissions.Write |
            BlobContainerSasPermissions.Delete |
            BlobContainerSasPermissions.List
        );

        var sasToken = sasBuilder.ToSasQueryParameters(userDelegatedKey, blobServiceClient.AccountName);
        return sasToken.ToString();
    }

    public async Task<string> GenerateBlobSasTokenAsync(
        string storageAccountName,
        string containerName,
        string blobName,
        TimeSpan? expiration = null,
        BlobSasPermissions? permissions = null,
        CancellationToken cancellationToken = default)
    {
        var blobContainerClient = GetBlobContainerClient(storageAccountName, containerName);
        var blobClient = blobContainerClient.GetBlobClient(blobName);
        var blobServiceClient = blobContainerClient.GetParentBlobServiceClient();

        var userDelegatedKeyExpiry = expiration.HasValue
            ? DateTimeOffset.UtcNow.Add(expiration.Value)
            : DateTimeOffset.UtcNow.AddDays(MaxUserDelegationExpiryDay);

        var userDelegatedKey = await blobServiceClient.GetUserDelegationKeyAsync(null, userDelegatedKeyExpiry, cancellationToken);

        var sasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = blobContainerClient.Name,
            BlobName = blobClient.Name,
            Resource = "b",
            Protocol = SasProtocol.Https,
            ExpiresOn = userDelegatedKey.Value.SignedExpiresOn
        };

        sasBuilder.SetPermissions(permissions ?? BlobSasPermissions.Read);

        var sasToken = sasBuilder.ToSasQueryParameters(userDelegatedKey, blobServiceClient.AccountName);

        return sasToken.ToString();
    }

    private BlobContainerClient GetBlobContainerClient(string storageAccountName, string containerName)
    {
        var blobServiceClient = new BlobServiceClient(new Uri($"https://{storageAccountName}.blob.core.windows.net"), tokenCredential);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
        return blobContainerClient;
    }
}
