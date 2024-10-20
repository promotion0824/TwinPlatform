using Azure.Messaging.ServiceBus;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Willow.ServiceBus.Constants;
using Willow.Rules.Configuration.Customer;
using Newtonsoft.Json;
using Willow.Rules.Services;

namespace Willow.ServiceBus;

/// <summary>
/// Sends messages to ServiceBus
/// </summary>
public abstract class MessageSenderBase
{
	private readonly string queueOrTopicName;
	protected readonly ILogger logger;
	private readonly ServiceBusOptions options;
	private readonly DefaultAzureCredential credentials;

	/// <summary>
	/// Creates a new <see cref="MessageSender" />
	/// </summary>
	public MessageSenderBase(
		IOptions<ServiceBusOptions> options,
		DefaultAzureCredential credentials,
		ILogger logger)
	{
		this.options = options.Value;
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));

		var sendOptions = options.Value.Send;

		string topicName = MessageConsumer.SubstituteEnvironment(sendOptions.TopicName);

		if (topicName is null) throw new ArgumentException("Topic name cannot be null");
		if (string.IsNullOrWhiteSpace(topicName)) throw new ArgumentException("Topic name cannot be an empty string");
		this.queueOrTopicName = topicName;
	}

	private static JsonSerializerSettings settings = new JsonSerializerSettings
	{
		TypeNameHandling = TypeNameHandling.Auto,
		Formatting = Formatting.None
	};


	private Lazy<ServiceBusSender> lazySender = null!;

	private Lazy<ServiceBusSender> CreateSender()
	{
		return new Lazy<ServiceBusSender>(() =>
		{
			var serviceBusClient = new ServiceBusClient(options.Namespace, credentials);
			var sender = serviceBusClient.CreateSender(queueOrTopicName ?? this.queueOrTopicName);
			return sender;
		});
	}

	int failCount = 50;

	protected async Task Send(object messageObject,
		string willowEnvironmentId,
		string correlationId,
		 CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrEmpty(willowEnvironmentId)) throw new ArgumentNullException("willowEnvironment cannot be null or empty");
		if (string.IsNullOrEmpty(correlationId)) throw new ArgumentNullException("correlationId cannot be null or empty");

		string topicName = MessageConsumer.SubstituteEnvironment(options.Send.TopicName);

		try
		{
			lazySender = lazySender ?? CreateSender();

			string body = JsonConvert.SerializeObject(messageObject, settings);

			var message = new ServiceBusMessage(body)
			{
				CorrelationId = correlationId,
				ContentType = MediaTypeNames.Application.Json,
				MessageId = correlationId,
				Subject = "Request to do work"
			};

			string typeName = messageObject.GetType().Name;

			message.ApplicationProperties.Add(ServiceBusMessageConstants.MessageType, typeName);

			// This is used in the subscription filter which ensures that messages for the same
			// environment are processes sequentially and can be split per-host
			message.ApplicationProperties.Add(ServiceBusMessageConstants.WillowEnvironmentId, willowEnvironmentId);


			/*
			TODO: Handle this exception that occurs after a long period of time. Recreate connection?

			Exception thrown: 'Azure.Identity.CredentialUnavailableException' in System.Private.CoreLib.dll: 'VisualStudioCodeCredential authentication unavailable. Token acquisition failed. Ensure that you have authenticated in VSCode Azure Account. See the troubleshooting guide for more information. https://aka.ms/azsdk/net/identity/vscodecredential/troubleshoot'
			 Inner exceptions found, see $exception in variables window for more details.
			 Innermost exception 	 Microsoft.Identity.Client.MsalUiRequiredException : AADSTS70043: The refresh token has expired or is invalid due to sign-in frequency checks by conditional access. The token was issued on 2022-01-06T15:20:50.9779047Z and the maximum allowed lifetime for this request is 2592000.
			Trace ID: 7c44409b-1897-4373-812e-cdbf48f3c600
			Correlation ID: d4aee281-4026-426b-af9a-ddd5072fad41
			Timestamp: 2022-02-06 19:33:59Z
			*/

			var sender = lazySender.Value;
			await sender.SendMessageAsync(message, cancellationToken);
			//logger.LogInformation("Sent message: {type} - {willowEnvironment} - {body}", typeName, willowEnvironmentId, body);
		}
		catch (AuthenticationFailedException afe)
		{
			logger.LogError("Failed to send message: " + afe.Message);
			// Keep going, when testing locally token times out but maybe we can recover?
			// In production no real chance this will recover without reconfiguration but maybe an Azure outage?
			if (failCount-- < 0)
			{
				lazySender = CreateSender();
				failCount = 50;
			}

		}
		catch (Exception ex)
		{
			logger.LogError(ex, ex.Message);
		}
	}

}
