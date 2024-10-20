namespace Willow.Storage.Blobs.Options;

public class BlobStorageOptions
{
    public string? AccountName { get; set; }
    public string? ConnectionString { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);
}
