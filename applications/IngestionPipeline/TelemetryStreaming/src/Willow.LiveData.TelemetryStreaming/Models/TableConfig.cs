namespace Willow.LiveData.TelemetryStreaming.Models;

/// <summary>
/// Configuration for table storage.
/// </summary>
internal record TableConfig
{
    /// <summary>
    /// Gets the URI of the storage account.
    /// </summary>
    public required Uri StorageAccountUri { get; init; }

    /// <summary>
    /// Gets the name of the storage table.
    /// </summary>
    public required string StorageTableName { get; init; }

    /// <summary>
    /// Gets the length of time, in minutes, to cache subscriptions.
    /// </summary>
    public required int CacheExpirationMinutes { get; init; } = TimeSpan.FromHours(23).Minutes;
}
