using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace WorkflowCore.Services.Background;

public class AddTwinIdToInspectionHostedService : BackgroundService
{
    private readonly ILogger<AddTwinIdToInspectionHostedService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public AddTwinIdToInspectionHostedService(ILogger<AddTwinIdToInspectionHostedService> logger,
        IServiceProvider serviceProvider, IConfiguration configuration = null)
        => (_serviceProvider, _logger, _configuration) = (serviceProvider, logger, configuration);
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        var jobIsEnabled = _configuration?.GetValue<bool>("BackgroundJobOptions:Inspection:EnableProcess") ?? false;
        if (jobIsEnabled)
        {
            _logger.LogInformation(
                "the 'adding TwinId to the Inspection' process is enabled and running");

            try
            {
                _logger.LogInformation(
                    "'adding TwinId to the Inspection' process started and is in progress");
                using var scope = _serviceProvider.CreateScope();
                var inspectionService =
                    scope.ServiceProvider.GetRequiredService<IInspectionService>();
                var batchSize = _configuration?.GetValue<int>("BackgroundJobOptions:Inspection:BatchSize") ?? 100;
                await inspectionService.AddTwinIdToInspectionsAsync(batchSize, stoppingToken);
                _logger.LogInformation(
                    "'adding TwinId to the Inspection' process is done");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(
                    $"'adding TwinId to the Inspection' process failed with exception, message: {ex.Message} {Environment.NewLine} stack trace: {ex.StackTrace}");
            }
        }

    }
}
