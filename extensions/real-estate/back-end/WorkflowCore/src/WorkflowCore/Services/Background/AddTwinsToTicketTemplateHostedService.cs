using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace WorkflowCore.Services.Background;

public class AddTwinsToTicketTemplateHostedService : BackgroundService
{
	private readonly ILogger<AddTwinsToTicketTemplateHostedService> _logger;
	private readonly IConfiguration _configuration;
	private readonly IServiceProvider _serviceProvider;

	public AddTwinsToTicketTemplateHostedService(ILogger<AddTwinsToTicketTemplateHostedService> logger,
		IServiceProvider serviceProvider, IConfiguration configuration = null)
		=> (_serviceProvider, _logger, _configuration) = (serviceProvider, logger, configuration);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        var jobIsEnabled = _configuration?.GetValue<bool>("BackgroundJobOptions:TicketTemplate:EnableProcess") ?? false;
        if (jobIsEnabled)
        {
            _logger.LogInformation(
                "the 'adding Twins to the TicketTemplate' process is enabled and running");

            try
            {
                _logger.LogInformation(
                    "'adding Twins to the TicketTemplate' process started and is in progress");
                using var scope = _serviceProvider.CreateScope();
                var workflowService =
                    scope.ServiceProvider.GetRequiredService<IWorkflowService>();
                var batchSize = _configuration?.GetValue<int>("BackgroundJobOptions:TicketTemplate:BatchSize") ?? 100;
                await workflowService.AddTwinsToTicketTemplateAsync(batchSize, stoppingToken);
                _logger.LogInformation(
                    "'adding Twins to the TicketTemplate' process is done");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(
                    $"'adding Twins to the TicketTemplate' process failed with exception, message: {ex.Message} {Environment.NewLine} stack trace: {ex.StackTrace}");
            }
        }
    }
}
