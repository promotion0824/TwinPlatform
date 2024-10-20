using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace PlatformPortalXL.Infrastructure;

/// <summary>
/// Represents a base class for implementing a periodic background service that runs at a specified interval.
/// </summary>
public abstract class TimedBackgroundService : BackgroundService
{
    private readonly TimeSpan _updateInterval;

    protected TimedBackgroundService(TimeSpan updateInterval)
    {
        _updateInterval = updateInterval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ExecuteOnScheduleAsync(stoppingToken);

        using var timer = new PeriodicTimer(_updateInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ExecuteOnScheduleAsync(stoppingToken);
        }
    }

    /// <summary>
    /// Perform scheduled work each time the timer elapses.
    /// </summary>
    protected abstract Task ExecuteOnScheduleAsync(CancellationToken stoppingToken);
}
