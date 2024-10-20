// -----------------------------------------------------------------------
// <copyright file="RedisCacheService.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.LiveData.TelemetryDataQuality.Services;

using System.Text.Json;
using Polly;
using Polly.Retry;
using StackExchange.Redis;
using Willow.LiveData.TelemetryDataQuality.Services.Abstractions;
using Willow.Telemetry;

/// <inheritdoc />
internal class RedisCacheService<T>(
    IConnectionMultiplexer redisConnection,
    ILogger<RedisCacheService<T>> logger,
    IMetricsCollector metricsCollector) : ICacheService<T>
{
    private readonly IDatabase redisDatabase = redisConnection.GetDatabase();

    private readonly AsyncRetryPolicy retryPolicy = Policy.Handle<RedisException>()
        .WaitAndRetryAsync(3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (exception, _, retryCount, _) =>
            {
                logger.LogWarning("Retry {RetryCount} failed due to: {Exception}", retryCount, exception.Message);
                metricsCollector.TrackMetric("RedisConnectionRetry",
                    retryCount,
                    MetricType.Counter,
                    "The number of retries for Redis connection attempts");
            });

    public async Task<T?> GetAsync(string key)
    {
        return await retryPolicy.ExecuteAsync(async () =>
        {
            var redisValue = await redisDatabase.StringGetAsync(key);
            return redisValue.IsNull ? default : JsonSerializer.Deserialize<T>(redisValue!);
        });
    }

    public async Task<bool> SetAsync(string key, T item, TimeSpan? expiration = null)
    {
        return await retryPolicy.ExecuteAsync(async () =>
        {
            // Default expiry of 7 days so that keys don't remain in Redis forever
            var expirationTime = expiration ?? TimeSpan.FromDays(7);
            var redisValue = JsonSerializer.Serialize(item);
            return await redisDatabase.StringSetAsync(key, redisValue, expirationTime);
        });
    }

    public async Task RemoveAsync(string key)
    {
        await retryPolicy.ExecuteAsync(async () =>
        {
            await redisDatabase.KeyDeleteAsync(key);
        });
    }
}
