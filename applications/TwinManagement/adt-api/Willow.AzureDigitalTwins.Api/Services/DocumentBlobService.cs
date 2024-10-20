using Azure.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Willow.AzureDigitalTwins.Api.Messaging.Configuration;
using Willow.Storage.Blobs;

namespace Willow.AzureDigitalTwins.Api.Services;

/// <summary>
/// Interface for Document Blob Service
/// </summary>
public interface IDocumentBlobService : IBlobService
{

}

/// <summary>
/// Document Blob Service Class
/// </summary>
public class DocumentBlobService : BlobService, IDocumentBlobService
{
    public DocumentBlobService(IOptions<DocumentStorageOptions> blobStorageOptions, TokenCredential tokenCredential, ILogger<BlobService> logger) : base(blobStorageOptions, tokenCredential, logger)
    {
        if (string.IsNullOrWhiteSpace(blobStorageOptions.Value.ConnectionString))
            blobStorageOptions.Value.ConnectionString = null;

    }
}
