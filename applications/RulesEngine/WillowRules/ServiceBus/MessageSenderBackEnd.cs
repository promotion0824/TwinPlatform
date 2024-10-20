using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Configuration.Customer;
using Willow.Rules.Sources;
using Willow.Rules.Repository;
using System.Threading;
using Azure.Identity;

namespace Willow.ServiceBus;

/// <summary>
/// Messaging methods for the Rules Processor (backend)
/// </summary>
public interface IMessageSenderBackEnd
{
	/// <summary>
	/// Send notification that all rule metadata have changed
	/// </summary>
	Task SendAllRuleMetadataUpdated(WillowEnvironment willowEnvironment);

	/// <summary>
	/// Send notification that rule metadata may have changed (post execution run)
	/// </summary>
	Task SendRuleMetadataUpdated();

	/// <summary>
	/// Send a heartbeat back to the UI
	/// </summary>
	Task SendHeartBeat();
}

public class MessageSenderBackEnd : MessageSenderBase, IMessageSenderBackEnd
{
	private readonly WillowEnvironment willowEnvironment;

	public MessageSenderBackEnd(
		IOptions<ServiceBusOptions> options,
		WillowEnvironment willowEnvironment,
		DefaultAzureCredential credentials,
		ILogger<MessageSenderBackEnd> logger) : base(options, credentials, logger)
	{
		this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
	}

	private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

	public async Task SendRuleMetadataUpdated()
	{
		//logger.LogDebug("Send rule metadata update");
		string correlationId = Guid.NewGuid().ToString();
		await this.Send(new RuleUpdatedMessage
		{
			EntityId = ""
		}, willowEnvironment.Id, correlationId);
	}

	public async Task SendHeartBeat()
	{
		logger.LogDebug($"Send heartbeat response");
		await this.Send(new HeartBeatMessage(), willowEnvironment.Id, Guid.NewGuid().ToString());
	}

	public async Task SendAllRuleMetadataUpdated(WillowEnvironment willowEnvironment)
	{
		string correlationId = Guid.NewGuid().ToString();
		await this.Send(new RuleUpdatedMessage
		{
			EntityId = string.Empty
		}, willowEnvironment.Id, correlationId);
	}
}
