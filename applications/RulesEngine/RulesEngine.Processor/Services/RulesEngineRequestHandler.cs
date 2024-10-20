using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Willow.Rules.Model;
using Willow.Rules.Processor;
using Willow.Rules.Repository;
using Willow.Rules.Sources;
using Willow.ServiceBus;

namespace Willow.Processor;

/// <summary>
/// Request handler
/// </summary>
public interface IRulesEngineRequestHandler : IMessageHandler
{
	/// <summary>
	/// Executes a Rules Engine Command
	/// </summary>
	Task<bool> Handle(RuleExecutionRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Connects service bus to the rules engine processor
/// </summary>
public class RulesEngineRequestHandler : BaseHandler<RuleExecutionRequest>, IRulesEngineRequestHandler
{
	private readonly WillowEnvironment willowEnvironment;
	private readonly RulesContext rulesContext;
	private readonly IMessageSenderBackEnd messageSender;
	private readonly IRuleOrchestrator ruleOrchestrator;
	private readonly IRepositoryRuleExecutionRequest repositoryRuleExecutionRequest;
	/// <summary>
	/// Creates a new <see cref="RulesEngineRequestHandler" />
	/// </summary>
	public RulesEngineRequestHandler(
		WillowEnvironment willowEnvironment,
		RulesContext rulesContext,      // one for the lifetime of the app, others for threads
		IMessageSenderBackEnd messageSender,
		IRuleOrchestrator ruleOrchestrator,
		IRepositoryRuleExecutionRequest repositoryRuleExecutionRequest,
		ILogger<RulesEngineRequestHandler> logger) : base(logger)
	{
		this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
		this.rulesContext = rulesContext ?? throw new ArgumentNullException(nameof(rulesContext));
		this.messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
		this.ruleOrchestrator = ruleOrchestrator ?? throw new ArgumentNullException(nameof(ruleOrchestrator));
		this.repositoryRuleExecutionRequest = repositoryRuleExecutionRequest ?? throw new ArgumentNullException(nameof(repositoryRuleExecutionRequest));
	}

	/// <summary>
	/// Initialize the hosted rules engine listener
	/// </summary>
	public override async Task Initialize()
	{
		logger.LogInformation("Starting Hosted Rules Engine");
		await messageSender.SendHeartBeat();

		logger.LogInformation("Current customer {willowEnvironment}", willowEnvironment.Id);

		// Update SQL
		logger.LogInformation("Checking database and migrating if necessary");
		DbInitializer.Initialize(rulesContext, logger);
		logger.LogInformation("Database completed migration");
	}

	/// <summary>
	/// Handle a rule execution request
	/// </summary>
	public override async Task<bool> Handle(RuleExecutionRequest request, CancellationToken cancellationToken)
	{
		//we still keep the Service Bus request handler which is useful for interrupt or adhoc work that doesn't need to be part of the processor queue.
		switch (request.Command)
		{
			case RuleExecutionCommandType.CheckHeartBeat:
				{
					logger.LogInformation("{customerId}: Message received, check heartbeat", request.CustomerEnvironmentId);
					await messageSender.SendHeartBeat();
					break;
				}
			case RuleExecutionCommandType.UpdateCache:
			case RuleExecutionCommandType.ProcessDateRange:
			case RuleExecutionCommandType.DeleteCommandInsights:
			case RuleExecutionCommandType.ReverseSyncInsights:
			case RuleExecutionCommandType.DeleteAllInsights:
			case RuleExecutionCommandType.DeleteAllMatchingInsights:
			case RuleExecutionCommandType.RebuildSearchIndex:
			case RuleExecutionCommandType.GitSync:
			case RuleExecutionCommandType.RunDiagnostics:
			case RuleExecutionCommandType.SyncCommandEnabled:
				{
					logger.LogInformation("{customerId}: Message received, {progressId}", request.CustomerEnvironmentId, request.ProgressId);
					await repositoryRuleExecutionRequest.UpsertOne(request);
					break;
				}
			case RuleExecutionCommandType.BuildRule:
			case RuleExecutionCommandType.DeleteRule:
			case RuleExecutionCommandType.ProcessCalculatedPoints:
				{
					logger.LogInformation("{customerId}: Message received, {progressId}", request.CustomerEnvironmentId, request.ProgressId);

					//Prevent the stacking of requests based on RuleId, even if string.empty
					if (!await repositoryRuleExecutionRequest.IsDuplicateRequest(request))
					{
						await repositoryRuleExecutionRequest.UpsertOne(request);
					}
					else
					{
						logger.LogInformation("{customerId}: Duplicate message received for {command} to process {ruleId}", request.CustomerEnvironmentId, request.Command.ToString(), request.RuleId);
					}

					break;
				}
			case RuleExecutionCommandType.Cancel:
				{
					logger.LogInformation("{customerId}: Cancel message received, {progressId}", request.CustomerEnvironmentId, request.ProgressId);
					await CancelRequest(request);
					break;
				}
			default:
				{
					logger.LogInformation("Message received with unknown command {command}", request.Command);
					return false;
				}
		}

		return true;
	}

	private async Task CancelRequest(RuleExecutionRequest request)
	{
		var queuedRequest = await repositoryRuleExecutionRequest.GetOne(request.Id);

		if (queuedRequest is not null)
		{
			await repositoryRuleExecutionRequest.DeleteOne(queuedRequest);
		}

		if (queuedRequest is null || queuedRequest.Requested)
		{
			ruleOrchestrator.Cancel(request.ProgressId);
		}
	}
}
