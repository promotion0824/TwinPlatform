using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using WorkflowCore.Services.MappedIntegration.Services;

namespace WorkflowCore.Services.Background;

/// <summary>
/// Auto sync Mapped-Nuvolo Ticket metadata
/// </summary>
public class SyncTicketMetadataHostedService : BackgroundService
{
    private readonly ILogger<SyncTicketMetadataHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _period = TimeSpan.FromMinutes(30);

    public SyncTicketMetadataHostedService(ILogger<SyncTicketMetadataHostedService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        await Task.Yield();
        _logger.LogInformation("Ticket Metadata Sync : SyncTicketMetadataHostedService is running.");
        try
        {
            using var timer = new PeriodicTimer(_period);

            while (!stoppingToken.IsCancellationRequested &&
                   await timer.WaitForNextTickAsync(stoppingToken))
            {
                await SynchronizeTicketMetaData();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ticket Metadata Sync : Error occurred while syncing ticket metadata");
        }

        _logger.LogInformation("Ticket Metadata Sync : SyncTicketMetadataHostedService is done.");

    }

    private async Task SynchronizeTicketMetaData()
    {
        _logger.LogInformation("Ticket Metadata Sync : SyncTicketMetadataHostedService is syncing ticket metadata.");
        using (var scope = _serviceProvider.CreateScope())
        {
            var mappedSyncMetadataService = scope.ServiceProvider.GetRequiredService<IMappedSyncMetadataService>();
            await mappedSyncMetadataService.SyncTicketMetadata();
        }

        _logger.LogInformation("Ticket Metadata Sync : SyncTicketMetadataHostedService is done syncing ticket metadata.");
    }
}

