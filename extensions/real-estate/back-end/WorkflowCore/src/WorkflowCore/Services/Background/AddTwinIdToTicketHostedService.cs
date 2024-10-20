using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace WorkflowCore.Services.Background;

public class AddTwinIdToTicketHostedService : BackgroundService
{
	private readonly ILogger<AddTwinIdToTicketHostedService> _logger;
	private readonly IConfiguration _configuration;
	private readonly IServiceProvider _serviceProvider;

	public AddTwinIdToTicketHostedService(ILogger<AddTwinIdToTicketHostedService> logger,
		IServiceProvider serviceProvider, IConfiguration configuration = null)
		=> (_serviceProvider, _logger, _configuration) = (serviceProvider, logger, configuration);
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await Task.Yield();
		var jobIsEnabled = _configuration?.GetValue<bool>("BackgroundJobOptions:Ticket:EnableProcess") ?? false;
		if (jobIsEnabled)
		{
			_logger.LogInformation(
                "the 'adding TwinId to the Ticket' process is enabled and running");

			try
			{
				_logger.LogInformation(
                    "'adding TwinId to the Ticket' process started and is in progress");
				using var scope = _serviceProvider.CreateScope();
				var workflowService =
					scope.ServiceProvider.GetRequiredService<IWorkflowService>();
				var batchSize = _configuration?.GetValue<int>("BackgroundJobOptions:Ticket:BatchSize") ?? 100;
				await workflowService.AddTwinIdToTicketAsync(batchSize, stoppingToken);
				_logger.LogInformation(
                    "'adding TwinId to the Ticket' process is done");
			}
			catch (Exception ex)
			{
				_logger.LogInformation(
					$"'adding TwinId to the Ticket' process failed with exception, message: {ex.Message} {Environment.NewLine} stack trace: {ex.StackTrace}");
			}
		}

	}
}
