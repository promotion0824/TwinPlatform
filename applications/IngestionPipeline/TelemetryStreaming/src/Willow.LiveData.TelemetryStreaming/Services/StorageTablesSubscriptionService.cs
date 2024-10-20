namespace Willow.LiveData.TelemetryStreaming.Services;

using System.Dynamic;
using Azure.Data.Tables;
using LazyCache;
using Microsoft.Extensions.Options;
using Willow.LiveData.TelemetryStreaming.Models;

/// <summary>
/// Gets subscriptions from Azure Table Storage.
/// </summary>
/// <param name="tableConfig">The configuration for Azure Storage.</param>
/// <param name="appCache">A cache for storing subscriptions.</param>
/// <param name="tableServiceClient">The client for accessing table storage.</param>
/// <param name="logger">A logger instance.</param>
internal class StorageTablesSubscriptionService(IOptions<TableConfig> tableConfig, IAppCache appCache, TableServiceClient tableServiceClient, ILogger<StorageTablesSubscriptionService> logger) : ISubscriptionService
{
    private bool cacheEnabled = true;

    private static readonly string[] KnownKeys = ["odata.etag", "PartitionKey", "RowKey", nameof(Subscription.ConnectorId), nameof(Subscription.ExternalId)];

    private const string CacheKey = "subscriptions";

    /// <summary>
    /// Gets subscriptions for the given connector and external ID.
    /// </summary>
    /// <param name="connectorId">The connector ID.</param>
    /// <param name="externalId">The external ID.</param>
    /// <returns>An array of subscriptions.</returns>
    public async ValueTask<Subscription[]> GetSubscriptions(string connectorId, string externalId)
    {
        if (cacheEnabled && tableConfig.Value.CacheExpirationMinutes <= 0)
        {
            logger.LogWarning("Cache expiration minutes is {exp}. Caching will be disabled", tableConfig.Value.CacheExpirationMinutes);
            cacheEnabled = false;
        }

        Subscription[] subscriptions = await appCache.GetOrAddAsync(CacheKey,
            async () =>
            {
                logger.LogInformation("Subscriptions: Cache miss. Retrieving subscriptions from table storage");

                try
                {
                    subscriptions = await GetSubscriptionsFromStorage().ToArrayAsync();

                    logger.LogInformation("Subscriptions: Found {count} subscriptions", subscriptions.Length);

                    return subscriptions;

                    //appCache.Add(CacheKey, subscriptions, DateTimeOffset.Now.AddMinutes(tableConfig.Value.CacheExpirationMinutes));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error getting subscriptions");

                    return Array.Empty<Subscription>();
                }
            },
            DateTimeOffset.Now.AddMinutes(tableConfig.Value.CacheExpirationMinutes));

        return subscriptions.Where(s => s.ConnectorId == connectorId && s.ExternalId == externalId).ToArray();
    }

    /// <summary>
    /// Asynchronously gets all subscriptions from storage.
    /// </summary>
    private async IAsyncEnumerable<Subscription> GetSubscriptionsFromStorage()
    {
        var pages = tableServiceClient.GetTableClient(tableConfig.Value.StorageTableName).QueryAsync<TableEntity>();

        await foreach (var entity in pages)
        {
            if (entity == null)
            {
                continue;
            }

            if (KnownKeys.Any(k => !entity.ContainsKey(k)))
            {
                logger.LogWarning("Invalid subscription found in storage. Keys found: {key}", string.Join(',', [.. entity.Keys]));
                continue;
            }

            Subscription? subscription = null;

            try
            {
                dynamic? metadata = entity.Keys.Except(KnownKeys).Any() ? new ExpandoObject() : null;

                // Loop through unknown keys and add to metadata.
                foreach (var key in entity.Keys.Except(KnownKeys))
                {
                    ((IDictionary<string, object>)metadata!)[key] = entity[key];
                }

                subscription = new()
                {
                    ConnectorId = entity[nameof(Subscription.ConnectorId)].ToString()!,
                    ExternalId = entity[nameof(Subscription.ExternalId)].ToString()!,
                    SubscriberId = entity.PartitionKey!.ToLowerInvariant(),
                    Metadata = metadata,
                };
            }
            catch (Exception ex) when (ex is NullReferenceException || ex is ArgumentNullException || ex is FormatException)
            {
                logger.LogWarning("Subscription with invalid values found in storage: {key}", entity.RowKey);
            }

            if (subscription != null)
            {
                yield return subscription.Value;
            }
        }
    }
}
