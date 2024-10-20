using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Willow.CommandAndControlAPI.SDK.Dtos;
using Willow.Rules.Logging;

namespace Willow.Rules.Services;

/// <summary>
/// Background service that listens for messages on CommandService to batch and process to the Willow Command and Control Api
/// </summary>
public class CommandBackgroundService : BackgroundService
{
	private readonly ICommandService commandService;
	private readonly ILogger<CommandBackgroundService> logger;

	/// <summary>
	/// Creates a new <see cref="CommandBackgroundService" />
	/// </summary>
	public CommandBackgroundService(
		ICommandService commandService,
		ILogger<CommandBackgroundService> logger)
	{
		this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
		this.logger = logger;
	}

	/// <summary>
	/// Main loop for background service
	/// </summary>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Command sender background service starting");

		var throttleLogger = logger.Throttle(TimeSpan.FromSeconds(30));

		//Continue reading messages else writer will fill queue
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				var batch = await commandService.Reader.ReadMultipleAsync(maxBatchSize: 200, cancellationToken: stoppingToken);

				using (throttleLogger.TimeOperation(TimeSpan.FromSeconds(30), "Sending {count}/{total} to Command And Control", batch.Count, commandService.Reader.Count))
				{
					using (var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(120)))
					{
						var combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(tokenSource.Token, stoppingToken);
						var request = new PostRequestedCommandsDto()
						{
							Commands = batch
						};
						await commandService.SendCommandUpdate(request, combinedCancellationTokenSource.Token);
					}
				}
			}
			catch (OperationCanceledException)
			{
				// ignore, shutting down
			}
			catch (Exception ex)
			{
				throttleLogger.LogError(ex, "Command Service failed to send batch to ADT");
			}
		}

		logger.LogInformation("Command sender background service closing");
	}
}
