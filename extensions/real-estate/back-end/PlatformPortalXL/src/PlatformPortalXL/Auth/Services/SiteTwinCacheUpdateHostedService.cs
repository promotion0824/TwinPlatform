using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Infrastructure;
using PlatformPortalXL.Services;

namespace PlatformPortalXL.Auth.Services;

public interface ISiteIdToTwinIdMatchingService
{
    Task<string> FindMatchToMostSignificantSpatialTwin(Guid siteId);
    Task UpdateSiteIdsToTwinMappings();
}

/// <summary>
/// A background service that performs a periodic cache refresh of site to twin mapping.
/// </summary>
/// <remarks>
/// Cache the most significant spatial twins to improve the performance of <see cref="AccessControlService.AuthorizeSiteAsync"/>.
/// </remarks>
public class SiteTwinCacheUpdateHostedService : TimedBackgroundService
{
    private readonly ISiteIdToTwinIdMatchingService _matchingService;
    private readonly ILogger<SiteTwinCacheUpdateHostedService> _logger;

    public SiteTwinCacheUpdateHostedService(
        ISiteIdToTwinIdMatchingService matchingService,
        ILogger<SiteTwinCacheUpdateHostedService> logger)
        : base(TimeSpan.FromMinutes(15))
    {
        _matchingService = matchingService;
        _logger = logger;
    }

    protected override async Task ExecuteOnScheduleAsync(CancellationToken _)
    {
        try
        {
            _logger.LogDebug("SiteToTwinCacheUpdateService: UpdateTwinCache starting ...");

            try
            {
                await _matchingService.UpdateSiteIdsToTwinMappings();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update twin cache");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SiteToTwinCacheUpdateService failed");
        }
    }
}
