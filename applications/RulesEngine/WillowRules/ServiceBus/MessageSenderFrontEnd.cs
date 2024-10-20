using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Willow.Rules.Configuration.Customer;
using Willow.Rules.Model;
using Willow.Rules.Sources;

namespace Willow.ServiceBus;

/// <summary>
/// Messaging methods for Rules Web (frontend)
/// </summary>
public interface IMessageSenderFrontEnd
{
	/// <summary>
	/// Check that the rule execution processor is running
	/// </summary>
	Task RequestHeartBeat(WillowEnvironment willowEnvironment, CancellationToken cancellationToken = default);

	/// <summary>
	/// Send a request to the rules processor
	/// </summary>
	Task RequestRuleExecution(RuleExecutionRequest messageObject, CancellationToken cancellationToken = default);
}

public class MessageSenderFrontEnd : MessageSenderBase, IMessageSenderFrontEnd
{
	public MessageSenderFrontEnd(
		IOptions<ServiceBusOptions> options,
		DefaultAzureCredential credentials,
		ILogger<MessageSenderFrontEnd> logger) : base(options, credentials, logger)
	{
	}
	public async Task RequestRuleExecution(RuleExecutionRequest messageObject, CancellationToken cancellationToken = default)
	{
		logger.LogDebug("Send rule request");
		await base.Send(messageObject, messageObject.CustomerEnvironmentId, messageObject.CorrelationId, cancellationToken);
	}

	public async Task RequestHeartBeat(WillowEnvironment willowEnvironment, CancellationToken cancellationToken = default)
	{
		logger.LogDebug("Send heart beat request");

		var messageObject = RuleExecutionRequest.CreateHeartbeatRequest(willowEnvironment.Id);

		await this.Send(messageObject, willowEnvironment.Id, messageObject.CorrelationId, cancellationToken);
	}
}
