using EFCore.BulkExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.DTO;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Services;

namespace Willow.Rules.Processor;

/// <summary>
/// Manages commands during execution
/// </summary>
public interface ICommandsManager
{
	/// <summary>
	/// Flush commands to db
	/// </summary>
	Task FlushToDatabase(DateTimeOffset now, ConcurrentDictionary<string, ActorState> actors, Dictionary<string, List<RuleInstance>> ruleInstances, string ruleId, ProgressTrackerForRuleExecution progressTracker, SystemSummary summary);
}

/// <summary>
/// Manages commands during execution
/// </summary>
public class CommandsManager : ICommandsManager
{
	private readonly ILogger<CommandsManager> logger;
	private readonly IRepositoryCommand repositoryCommand;
	private readonly ICommandService commandService;
	private readonly ITelemetryCollector telemetryCollector;

	/// <summary>
	/// Constructor
	/// </summary>
	public CommandsManager(
		IRepositoryCommand repositoryCommand,
		ICommandService commandService,
		ITelemetryCollector telemetryCollector,
		ILogger<CommandsManager> logger)
	{
		this.repositoryCommand = repositoryCommand ?? throw new ArgumentNullException(nameof(repositoryCommand));
		this.telemetryCollector = telemetryCollector ?? throw new ArgumentNullException(nameof(telemetryCollector));
		this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Flush commands to db
	/// </summary>
	public async Task FlushToDatabase(
		DateTimeOffset now,
		ConcurrentDictionary<string, ActorState> actors,
		Dictionary<string, List<RuleInstance>> ruleInstances,
        string ruleId,
        ProgressTrackerForRuleExecution progressTracker,
		SystemSummary summary)
	{
		var throttledLogger = logger.Throttle(TimeSpan.FromSeconds(15));
		var removedLogger = logger.Throttle(TimeSpan.FromSeconds(1));

		try
		{
			//exclude user updatable fields, but only for updates, inserts must still write because it gets its initial value from the rule instance
			var config = new BulkConfig()
			{
				PropertiesToExcludeOnUpdate = new List<string>()
				{
					nameof(Command.Enabled)
				}
			};

			//get latest command flag from db in case of user updates
			var commandValues = (await repositoryCommand.GetCommandValues()).ToDictionary(v => v.id);
			int commandCount = 0;
			int syncCount = 0;
			int total = ruleInstances.SelectMany(v => v.Value).Distinct().Sum(v => v.RuleTriggersBound.Count);
			int deleteCount = 0;
			var lastSyncTime = DateTimeOffset.UtcNow;

			await progressTracker.ReportFlushingCommands(0, total);

			using (var timedLogger = logger.TimeOperation("Write commands to database"))
			{
				foreach (var ruleInstance in ruleInstances.SelectMany(v => v.Value).Distinct())
				{
					if (actors.TryGetValue(ruleInstance.Id, out var actor))
					{
						var commands = actor.CreateCommands(ruleInstance);

						foreach (var command in commands)
						{
							commandCount++;

							bool currentlyTriggered = false;

							if (commandValues.TryGetValue(command.Id, out var commandValue))
							{
								command.Enabled = commandValue.enabled;
								currentlyTriggered = commandValue.isTriggered;
							}
							bool shouldSync = command.CanSync();

							if (!command.IsTriggered && !currentlyTriggered)
							{
								//if the command isn't currently triggerred and the new value is also not triggered, dont continously clear the command
								shouldSync = false;
							}

							if (shouldSync)
							{
								syncCount++;
								command.LastSyncDate = lastSyncTime;
								await commandService.QueueCommand(command.CreateRequestedCommandDto());
							}

							await this.repositoryCommand.QueueWrite(command, queueSize: 400, batchSize: 400, updateCache: false, config: config);

							await progressTracker.ReportFlushingCommands(commandCount, total);

							throttledLogger.LogInformation("Flushing commands {count}/{total}", commandCount, total);

							commandValues.Remove(command.Id);

							summary.AddToSummary(command);
						}
					}
				}

				//any values left over are orphaned commands. This could be due to a command being removed/renamed in a rule
				foreach (var id in commandValues.Keys)
				{
					var command = await repositoryCommand.GetOne(id);

					if (command is not null)
					{
						if(!string.IsNullOrEmpty(ruleId) && command.RuleId != ruleId)
						{
							continue;
						}

						deleteCount++;
						removedLogger.LogInformation("Removing deprecated command #{c} {id}", deleteCount, id);

						//only clear if currently triggered
						if (command.IsTriggered && command.CanSync())
						{
							//force an end date before sending
							command.EndTime = now;
							await commandService.QueueCommand(command.CreateRequestedCommandDto());
						}

						await repositoryCommand.DeleteOne(command);
					}
				}

				// Report the final total
				await progressTracker.ReportFlushingCommands(commandCount, total);

				telemetryCollector.TrackCommands(commandCount, syncCount);

				logger.LogInformation("Flushed commands {count}/{total}. Total synced {syncTotal}", commandCount, total, syncCount);
			}

			await repositoryCommand.FlushQueue(updateCache: false, config: config);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to write all commands");
		}
	}
}
