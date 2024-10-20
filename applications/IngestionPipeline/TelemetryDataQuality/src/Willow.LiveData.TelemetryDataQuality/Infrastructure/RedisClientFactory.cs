// -----------------------------------------------------------------------
// <copyright file="RedisClientFactory.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.LiveData.TelemetryDataQuality.Infrastructure;

using Azure.Core;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Willow.LiveData.TelemetryDataQuality.Options;
using Willow.Security.KeyVault;
using Willow.Telemetry;

internal class RedisClientFactory(
    IOptions<RedisOption> redisConfig,
    ILogger<RedisClientFactory> logger,
    IMetricsCollector metricsCollector,
    ISecretManager secretManager,
    TokenCredential tokenCredential)
{
    private readonly RedisOption redisOption = redisConfig.Value;
    private const string ConnectionStringKey = "Redis--ConnectionString";

    /// <summary>
    /// Creates a Redis connection multiplexer.
    /// </summary>
    /// <returns>A Task of type IConnectionMultiplexer.</returns>
    public async Task<IConnectionMultiplexer?> CreateConnectionMultiplexer()
    {
        try
        {
            var connectionString = await secretManager.GetSecretAsync(ConnectionStringKey);
            if (connectionString is null)
            {
                throw new SecretNotFoundException(ConnectionStringKey);
            }

            var configuration = redisOption.UseManagedIdentity
                ? await ConfigureRedisManagedIdentityAuth(connectionString.Value)
                : ConfigureRedisAccessKeyAuth(connectionString.Value);

            return await ConnectionMultiplexer.ConnectAsync(configuration);
        }
        catch (SecretReloadException ex)
        {
            logger.LogError(ex, "Maximum retries exceeded while loading secrets");
            metricsCollector.TrackMetric("SecretReloadFailed", 1, MetricType.Counter);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating Redis connection multiplexer");
            metricsCollector.TrackMetric("RedisConnectionFailed", 1, MetricType.Counter);
        }

        return null;
    }

    /// <summary>
    /// Configures Redis connection options for managed identity authentication.
    /// </summary>
    /// <param name="connectionString">The Redis connection string.</param>
    /// <returns>A Task of type ConfigurationOptions.</returns>
    private async Task<ConfigurationOptions> ConfigureRedisManagedIdentityAuth(string connectionString)
    {
        var hostName = connectionString.Split(",")[0];
        var configuration = ConfigurationOptions.Parse(hostName, true);
        await configuration.ConfigureForAzureWithTokenCredentialAsync(tokenCredential);

        return configuration;
    }

    /// <summary>
    /// Configures Redis connection options for access key authentication.
    /// </summary>
    /// <param name="connectionString">The Redis connection string.</param>
    /// <returns>The parsed Redis connection options.</returns>
    private static ConfigurationOptions ConfigureRedisAccessKeyAuth(string connectionString)
    {
       return ConfigurationOptions.Parse(connectionString, true);
    }
}
