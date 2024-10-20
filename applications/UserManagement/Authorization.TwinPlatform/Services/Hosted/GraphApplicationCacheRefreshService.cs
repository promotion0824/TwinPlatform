using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Services.Hosted.Request;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Authorization.TwinPlatform.Services.Hosted;

/// <summary>
/// Graph Application Cache Refresh Hosted Service
/// </summary>
public class GraphApplicationCacheRefreshService : BackgroundService
{
    private readonly ILogger<GraphApplicationCacheRefreshService> _logger;
    private readonly IServiceProvider _provider;
    private readonly IBackgroundQueueReceiver<GroupMembershipCacheRefreshRequest> _backgroundQueue;
    private readonly GraphApplicationCacheRefreshOption _option;

    public GraphApplicationCacheRefreshService(IServiceProvider provider,
        ILogger<GraphApplicationCacheRefreshService> logger,
        IBackgroundQueueReceiver<GroupMembershipCacheRefreshRequest> backgroundQueue,
        IOptions<GraphApplicationCacheRefreshOption> options)
    {
        _provider = provider;
        _logger = logger;
        _backgroundQueue = backgroundQueue;

        _option = options.Value;
    }

    /// <summary>
    /// Execute method that gets called by the system when the background service initiates.
    /// </summary>
    /// <param name="stoppingToken">Instance of cancellation token.</param>
    /// <returns>Awaitable task.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_option.Enabled)
        {
            _logger.LogWarning("Hosted Graph Application Service disabled. Returning without execution.");
            return;
        }

        // activate background queue, so that the queue start to accept the messages
        _backgroundQueue.SetStatus(status: true);

        _logger.LogInformation("Started Graph Application Cache Refresh Service with Refresh Interval: {Interval} Minutes.",_option.RefreshInterval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RefreshCache();
            // Wait for the specified interval before refreshing cache again
            await Task.Delay(_option.RefreshInterval, stoppingToken);
        }
        _logger.LogInformation("Stopping Graph Application Cache Refresh Service.");
    }

    /// <summary>
    /// Refresh cache logic.
    /// </summary>
    /// <returns>Awaitable task.</returns>
    private async Task RefreshCache()
    {
        try
        {
            var requestToProcess = _backgroundQueue.GetAll();
            // If nothing to process return.
            if (requestToProcess.Count == 0)
                return;

            _logger.LogDebug("Initiating Refresh cache.");
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            using (var currentScope = _provider.CreateScope())
            {
                // Resolve Graph Application Service
                var graphApplicationService = currentScope.ServiceProvider.GetRequiredService<IGraphApplicationService>();

                var requestTasks = requestToProcess.Select(async (r) => {

                    // GetGroupMemberships method automatically cache the result
                    await graphApplicationService.GetGroupMemberships(r.ClientService, r.groupId, r.useTransitiveMembership, useCache: false);

                    // Remove the element from the queue
                    _backgroundQueue.Dequeue(r.GetIdentifier());
                });

                await Task.WhenAll(requestTasks);
            }
            stopWatch.Stop();
            _logger.LogDebug("Refresh cache completed in {totalMinutes} minutes.",stopWatch.Elapsed.TotalMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Error occurred while refreshing group membership cache.");
        }
    }
}
