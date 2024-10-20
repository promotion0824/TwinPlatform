using Willow.Storage.Blobs.Options;

namespace Willow.AzureDigitalTwins.Api.Messaging.Configuration;

/// <summary>
/// Option class for holding document storage
/// </summary>
public class DocumentStorageOptions : BlobStorageOptions
{
    /// <summary>
    /// Used in AI Search Indexer Datasource. ResourceId will be used as datasource connection string if connection string is empty.
    /// </summary>
    public string ResourceId { get; set; }  
}
