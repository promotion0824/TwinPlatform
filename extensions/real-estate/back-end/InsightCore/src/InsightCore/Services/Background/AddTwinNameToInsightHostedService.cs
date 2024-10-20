using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InsightCore.Services.Background;

public class AddTwinNameToInsightHostedService:BackgroundService
{
    private readonly ILogger<AddTwinNameToInsightHostedService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public AddTwinNameToInsightHostedService(ILogger<AddTwinNameToInsightHostedService> logger,
        IServiceProvider serviceProvider, IConfiguration configuration = null)
        => (_serviceProvider, _logger, _configuration) = (serviceProvider, logger, configuration);
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        var jobIsEnabled = _configuration?.GetValue<bool>("BackgroundJobOptions:EnableProcess") ?? false;
        if (jobIsEnabled)
        {
            _logger.LogInformation(
                "The process for adding TwinName to the Insight enabled and running");

            try
            {
                _logger.LogInformation(
                    "The process for adding TwinName to the Insight started and is in progress");
                using var scope = _serviceProvider.CreateScope();
                var insightService =
                    scope.ServiceProvider.GetRequiredService<IInsightService>();
                var batchSize = _configuration?.GetValue<int>("BackgroundJobOptions:BatchSize") ?? 100;
                await insightService.AddMissingTwinDetailsToInsightsAsync(batchSize, stoppingToken);
                _logger.LogInformation(
                    "The process for adding TwinName to the Insight is done");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(
                    $"The process for adding TwinId to the Insight failed with exception, message: {ex.Message} {Environment.NewLine} stack trace: {ex.StackTrace}");
            }
        }

    }
}
