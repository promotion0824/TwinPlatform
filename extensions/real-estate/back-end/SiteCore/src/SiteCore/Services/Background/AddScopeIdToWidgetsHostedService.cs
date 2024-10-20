using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SiteCore.Services.Background;

public class AddScopeIdToWidgetsHostedService : BackgroundService
{
    private readonly ILogger<AddScopeIdToWidgetsHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly WidgetBackgroundJobOptions _options;

    public AddScopeIdToWidgetsHostedService(ILogger<AddScopeIdToWidgetsHostedService> logger,
        IServiceProvider serviceProvider,
        IOptions<WidgetBackgroundJobOptions> options = null)
        => (_serviceProvider, _logger, _options) = (serviceProvider, logger, options?.Value);

    public static string ProcessName = "adding ScopeId to the Widget";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        var jobIsEnabled = _options?.EnableProcess ?? true;
        if (jobIsEnabled)
        {
            _logger.LogInformation($"the {ProcessName} process is enabled and running");

            try
            {
                _logger.LogInformation($"{ProcessName} process started and is in progress");

                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IWidgetService>();
                var batchSize = _options?.BatchSize ?? 100;

                await service.AddScopedFromSiteWidgetsAsync(batchSize, stoppingToken);
                await service.AddScopedFromPortfolioWidgetsAsync(batchSize, stoppingToken);

                _logger.LogInformation($"{ProcessName} process is done");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{ProcessName} process failed with exception, message: {ex.Message} {Environment.NewLine} stack trace: {ex.StackTrace}");
            }
        }
    }
}
