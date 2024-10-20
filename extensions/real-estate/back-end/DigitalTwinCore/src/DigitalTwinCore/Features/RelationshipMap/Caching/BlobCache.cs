using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Willow.Api.AzureStorage;

namespace DigitalTwinCore.Features.RelationshipMap.Caching
{
    public interface IBlobCache
    {
        Task<TItem> GetOrCreateAsync<TItem>(string key, Func<IBlobCacheEntry, Task<TItem>> factory);
    }

    /// <summary>
    /// Very simple caching implementation using blob storage
    /// The cache key is used as the file name
    /// Expiration is controlled via blob properties
    /// The Channel offloads whe upload to a background queue and not block the main thread
    /// </summary>
    public sealed class BlobCache : IBlobCache, IDisposable
    {
        private readonly IOptions<BlobStorageConfig> _blobStorageOptions;
        private readonly Channel<(TimeSpan timeSpan, string key, object value)> _channel;
        private readonly Task _uploader;
        private const string ExpiresOn = nameof(ExpiresOn);

        public BlobCache(IOptions<BlobStorageConfig> blobStorageOptions)
        {
            _blobStorageOptions = blobStorageOptions;
            BlobContainerClient = new Lazy<BlobContainerClient>(() => GetBlobClient().GetAwaiter().GetResult(), LazyThreadSafetyMode.ExecutionAndPublication);
            _channel = Channel.CreateUnbounded<(TimeSpan timeSpan, string key, object value)>(new UnboundedChannelOptions
            {
                SingleReader = true
            });
            _uploader = Task.Run(async () =>
            {
                while (await _channel.Reader.WaitToReadAsync())
                {
                    var (timeSpan, key, value) = await _channel.Reader.ReadAsync();
                    await Upload(timeSpan, key, value);
                }
            });
        }

        public async Task<TItem> GetOrCreateAsync<TItem>(string key, Func<IBlobCacheEntry, Task<TItem>> factory)
        {
            ValidateCacheKey(key);
            var (exists, value) = await TryGetValueAsync<TItem>(key);
            if (exists)
            {
                return value;
            }

            var cacheEntry = new BlobCacheEntry();
            value = await factory(cacheEntry);
            await _channel.Writer.WaitToWriteAsync();
            await _channel.Writer.WriteAsync((cacheEntry.Expiration, key, value));
            return value;
        }

        private async Task Upload(TimeSpan timeSpan, string key, object value)
        {
            var client = BlobContainerClient.Value;
            var blobClient = client.GetBlobClient(key);
            var metadata = new Dictionary<string, string>
            {
                {ExpiresOn, DateTimeOffset.UtcNow.Add(timeSpan).ToUnixTimeMilliseconds().ToString()}
            };

            var contentBytes = JsonSerializer.SerializeToUtf8Bytes(value, new JsonSerializerOptions
            {
                WriteIndented = false
            });
            await using var content = new MemoryStream(contentBytes);
            await blobClient.UploadAsync(content, metadata: metadata);
        }

        private async Task<(bool, TItem)> TryGetValueAsync<TItem>(string key)
        {
            var client = BlobContainerClient.Value;
            var blobClient = client.GetBlobClient(key);
            if (!await blobClient.ExistsAsync())
            {
                return (false, default);
            }

            var blobProperties = await blobClient.GetPropertiesAsync();
            var blobExpiresOn = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(blobProperties.Value.Metadata[ExpiresOn]));
            if (IsExpired(blobExpiresOn))
            {
                await blobClient.DeleteIfExistsAsync();

                return (false, default);
            }

            var value = (await blobClient.DownloadContentAsync()).Value.Content.ToObjectFromJson<TItem>();
            return (true, value);
        }

        private static bool IsExpired(DateTimeOffset blobExpiresOn)
        {
            return blobExpiresOn.CompareTo(DateTimeOffset.UtcNow) <= 0;
        }

        private Lazy<BlobContainerClient> BlobContainerClient
        {
            get;
        }

        private async Task<BlobContainerClient> GetBlobClient()
        {
            var connectionString = $"DefaultEndpointsProtocol=https;AccountName={_blobStorageOptions.Value.AccountName};AccountKey={_blobStorageOptions.Value.AccountKey};EndpointSuffix=core.windows.net";
            var serviceClient = new BlobServiceClient(connectionString);
            var containerClient = serviceClient.GetBlobContainerClient("twins-cache");

            await containerClient.CreateIfNotExistsAsync().ConfigureAwait(false);

            return containerClient;
        }

        private static void ValidateCacheKey(object key)
        {
            NameValidator.ValidateBlobName(key.ToString());
        }

        public void Dispose()
        {
            _channel?.Writer.Complete();
            _uploader.GetAwaiter().OnCompleted(() => _uploader?.Dispose());
        }
    }
}
