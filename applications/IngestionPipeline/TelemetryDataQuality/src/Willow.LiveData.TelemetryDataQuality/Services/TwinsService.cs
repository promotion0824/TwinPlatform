// -----------------------------------------------------------------------
// <copyright file="TwinsCacheService.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.LiveData.TelemetryDataQuality.Services;

using Microsoft.Extensions.Options;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.LiveData.TelemetryDataQuality.Models;
using Willow.LiveData.TelemetryDataQuality.Options;
using Willow.LiveData.TelemetryDataQuality.Services.Abstractions;
using Willow.Model.Adt;
using Willow.Model.Requests;
using Willow.Telemetry;

/// <summary>
/// Represents a service for working with twins.
/// </summary>
internal class TwinsService(
    IOptions<TwinsCacheOption> twinsCacheOption,
    ITwinsClient twinsClient,
    ICacheService<TwinDetails> twinsCacheService,
    IMetricsCollector metricsCollector,
    HealthCheckTwinsApi healthCheckTwinsApi,
    ILogger<TwinsService> logger) : ITwinsService
{
    private readonly List<TwinDetails> modelledTwins = [];

    public async Task LoadTwins(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Initializing cache");
        var twinResults = await this.GetAllTwinsAsync(cancellationToken);

        await PopulateTwinCache(twinResults, cancellationToken);
        logger.LogInformation("Cache loaded with {Count} twins", twinResults.Count);
    }

    public async Task<TwinDetails?> GetTwin(string? externalId)
    {
        if (string.IsNullOrEmpty(externalId))
        {
            return null;
        }

        var cacheKey = $"{Constants.TwinsCachePrefix}-{externalId}";
        try
        {
            return await twinsCacheService.GetAsync(cacheKey);
        }
        catch (Exception e)
        {
            logger.LogWarning("Error getting twin from cache {ExternalId}: {Error}", cacheKey, e.Message);
            RecordCacheError(1, cacheKey);
            return null;
        }
    }

    public IEnumerable<TwinDetails> GetAllTwins()
    {
        return this.modelledTwins;
    }

    /// <summary>
    /// Retrieves all capability twins from the twins-api service.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A list of twin details for capabilities with non-empty ExternalId.</returns>
    private async Task<List<TwinDetails>> GetAllTwinsAsync(CancellationToken cancellationToken)
    {
        var twinResults = new List<TwinDetails>();
        var request = CreateGetTwinsRequest();
        var continuationToken = string.Empty;

        try
        {
            while (continuationToken is not null)
            {
                var page = await twinsClient.GetTwinsAsync(
                    request,
                    twinsCacheOption.Value.PageSize,
                    continuationToken: continuationToken,
                    cancellationToken: cancellationToken);

                // Only interested in capability twins with external ID
                var filteredTwins = page.Content.Where(t => t.Twin.Contents.TryGetValue("externalID", out var externalId) && !string.IsNullOrEmpty(externalId.ToString())).ToList();

                twinResults.AddRange(filteredTwins.Select(CreateDetailsFromTwin));
                continuationToken = page.ContinuationToken;
            }

            healthCheckTwinsApi.Current = HealthCheckTwinsApi.Healthy;
        }
        catch (ApiException apiEx) when (apiEx.StatusCode == 401)
        {
            logger.LogError("Failed to retrieve twins: {Error}", apiEx.Message);
            healthCheckTwinsApi.Current = HealthCheckTwinsApi.AuthorizationFailure;
        }
        catch (ApiException e)
        {
            logger.LogError("Error retrieving twins: {Error}", e.Message);
            healthCheckTwinsApi.Current = HealthCheckTwinsApi.FailingCalls;
        }
        catch (Exception e)
        {
            logger.LogError("Error getting twins: {Error}", e.Message);
        }

        metricsCollector.TrackMetric(
            "TotalTwinsOnCacheInitialization",
            twinResults.Count,
            MetricType.Counter,
            "Total count of twins on cache initialization (on startup, and on expiration)");

        return twinResults;
    }

    /// <summary>
    /// Creates a GetTwinsInfoRequest object to retrieve twin information.
    /// </summary>
    /// <returns>A GetTwinsInfoRequest object with the specified parameters.</returns>
    /// <remarks>For the purposes of the Telemetry Data Quality app, we are only interested
    /// in those twins that are derived from Capability model.
    /// </remarks>
    private static GetTwinsInfoRequest CreateGetTwinsRequest()
    {
        return new GetTwinsInfoRequest
        {
            IncludeRelationships = false,
            ExactModelMatch = false,
            ModelId = ["dtmi:com:willowinc:Capability;1"],
        };
    }

    /// <summary>
    /// Creates a TwinDetails object from a TwinWithRelationships object.
    /// </summary>
    /// <param name="item">The TwinWithRelationships object to create TwinDetails from.</param>
    /// <returns>A TwinDetails object created from the provided TwinWithRelationships.</returns>
    private static TwinDetails CreateDetailsFromTwin(TwinWithRelationships item)
    {
        return new TwinDetails
        {
            Id = item.Twin.Id,
            ModelId = item.Twin.Metadata.ModelId,
            ConnectorId = item.Twin.Contents.TryGetValue("mappedConnectorId", out var mappedConnectorId)
                ? mappedConnectorId.ToString()
                : item.Twin.Contents.TryGetValue("connectorID", out var connectorId)
                    ? connectorId.ToString()
                    : null,
            ExternalId = item.Twin.Contents.TryGetValue("externalID", out var externalId)
                ? externalId.ToString()!
                : string.Empty,
            Unit = item.Twin.Contents.TryGetValue("unit", out var unit)
                ? unit?.ToString()
                : null,
            TrendInterval = item.Twin.Contents.TryGetValue("trendInterval", out var trendInterval)
                            && int.TryParse(trendInterval.ToString(), out var parsedTrendInterval)
                ? parsedTrendInterval
                : 300,
        };
    }

    /// <summary>
    /// Populates the cache with the provided list of twin details.
    /// </summary>
    /// <param name="twinDetails">The list of twin details to update the cache with.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    private async Task PopulateTwinCache(List<TwinDetails> twinDetails, CancellationToken cancellationToken = default)
    {
        var deletedTwins = GetDeletedTwins(twinDetails).ToList();
        if (deletedTwins.Count != 0)
        {
            logger.LogInformation("Removing {Count} twins from cache", deletedTwins.Count);
            await RemoveTwins(deletedTwins);
        }

        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = 100,
        };

        await Parallel.ForEachAsync(twinDetails.Where(t => !string.IsNullOrEmpty(t.ExternalId)),
            parallelOptions,
            async (twin, _) =>
            {
                var key = $"{Constants.TwinsCachePrefix}-{twin.ExternalId}";
                try
                {
                    await twinsCacheService.SetAsync(key, twin, TimeSpan.FromHours(twinsCacheOption.Value.RefreshCacheHours));
                    this.modelledTwins.Add(twin);
                }
                catch (Exception e)
                {
                    logger.LogWarning("Error caching twin {ExternalId}: {Error}", twin.ExternalId, e.Message);
                    RecordCacheError(1, key);
                }
            });
    }

    /// <summary>
    /// Retrieves the list of deleted twins that were modelled, comparing with the provided new twin list.
    /// </summary>
    /// <param name="newTwinDetails">The list of new twin details.</param>
    /// <returns>The list of deleted twin details.</returns>
    private IEnumerable<TwinDetails> GetDeletedTwins(List<TwinDetails> newTwinDetails)
    {
        return this.modelledTwins.Where(twin => newTwinDetails.All(t => t.ExternalId != twin.ExternalId));
    }

    /// <summary>
    /// Removes the specified twins from the cache.
    /// </summary>
    /// <param name="twins">The list of twin details to be removed.</param>
    private async Task RemoveTwins(IEnumerable<TwinDetails> twins)
    {
        foreach (var twin in twins)
        {
            await twinsCacheService.RemoveAsync($"{Constants.TwinsCachePrefix}-{twin.ExternalId}");
            this.modelledTwins.Remove(twin);
        }
    }

    /// <summary>
    /// Records the error that occurred during caching of twin.
    /// </summary>
    /// <param name="count">The number of cache errors occurred.</param>
    /// <param name="cacheKey">The cache key of the twin.</param>
    private void RecordCacheError(int count, string cacheKey)
    {
        metricsCollector.TrackMetric("TwinCacheError",
            count,
            MetricType.Counter,
            null,
            new Dictionary<string, string>
            {
                { "CacheKey", cacheKey },
            });
    }
}
