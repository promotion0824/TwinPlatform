using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Infrastructure;

namespace PlatformPortalXL.Auth.Services;

/// <summary>
/// A background service that performs a periodic cache refresh of twins with ancestors.
/// </summary>
/// <remarks>
/// On a schedule, loads all twins with a model type of 'dtmi:com:willowinc:Space;1', converts each to a
/// <see cref="ITwinWithAncestors"/> and caches the result, enabling fast lookup of a twin's ancestors.
/// </remarks>
public class AncestralTwinsCacheUpdateHostedService : TimedBackgroundService
{
    private readonly IAncestralTwinsSearchService _ancestralTwinsSearchService;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<AncestralTwinsCacheUpdateHostedService> _logger;
    private const int UpdateIntervalInMinutes = 15;

    public AncestralTwinsCacheUpdateHostedService(
        IAncestralTwinsSearchService ancestralTwinsSearchService,
        IMemoryCache memoryMemoryCache,
        ILogger<AncestralTwinsCacheUpdateHostedService> logger)
        : base(TimeSpan.FromMinutes(UpdateIntervalInMinutes))
    {
        _ancestralTwinsSearchService = ancestralTwinsSearchService;
        _memoryCache = memoryMemoryCache;
        _logger = logger;
    }

    protected override async Task ExecuteOnScheduleAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogTrace("Executing ancestral twins cache update");

            await UpdateAncestralCache("dtmi:com:willowinc:Space;1", stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AncestralTwinsCacheUpdateHostedService failed");
        }
    }

    private async Task UpdateAncestralCache(string model, CancellationToken stoppingToken)
    {
        var sw = Stopwatch.StartNew();
        var twinCount = 0;
        var cacheOptions = new MemoryCacheEntryOptions
        {
            Priority = TwinCaching.Priority,
            AbsoluteExpiration = TwinCaching.GetAbsoluteExpiration
        };

        try
        {
            await foreach (var twin in IterateTwins(model, stoppingToken))
            {
                var cacheKey = TwinCaching.CacheKeyForAncestorsLookup(twin.TwinId);
                _memoryCache.Set(cacheKey, twin, cacheOptions);
                twinCount++;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying ancestral '{Model}' twins", model);
        }

        _logger.LogInformation("Ancestral twins cache updated with {Count} twins took {Time:F0}ms", twinCount, sw.Elapsed.TotalMilliseconds);
    }

    private async IAsyncEnumerable<ITwinWithAncestors> IterateTwins(string model, [EnumeratorCancellation] CancellationToken stoppingToken)
    {
        var page = 1;
        bool queryNext;

        do
        {
            var twinsWithAncestors = (await _ancestralTwinsSearchService.GetTwinsByModel(model, page, stoppingToken)).ToArray();

            foreach (var twin in twinsWithAncestors)
            {
                yield return twin;
            }

            queryNext = twinsWithAncestors.Length > 0;

            page++;

        } while (queryNext && !stoppingToken.IsCancellationRequested);
    }
}
