using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RulesEngine.Processor.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Configuration.Customer;
using Willow.Rules.Logging;
using Willow.Rules.Services;
using Willow.ServiceBus.Constants;

namespace Willow.ServiceBus;

/// <summary>
/// Consumes service bus messages and dispatches them to IMessageHandler instances
/// </summary>
public class MessageConsumer : BackgroundService
{
	private readonly ServiceBusClient serviceBusClient;
	private readonly ServiceBusOptions serviceBusOptions;
	private readonly ILogger<MessageConsumer> logger;
	private readonly ILogger throttledLogger;
	private readonly IServiceProvider serviceProvider;
	private readonly ITelemetryCollector telemetryCollector;
	private readonly HealthCheckServiceBus healthCheckServiceBus;

	private ServiceBusProcessor? serviceBusProcessor;

	/// <summary>
	/// Signals when the app is told to stop
	/// </summary>
	private CancellationTokenSource overallCancellationTokenSource = new CancellationTokenSource();

	/// <summary>
	/// Creates a new MessageConsumer
	/// </summary>
	public MessageConsumer(
		IOptions<ServiceBusOptions> serviceBusOptions,
		ILogger<MessageConsumer> logger,
		IServiceProvider serviceProvider,
		DefaultAzureCredential credential,
		ITelemetryCollector telemetryCollector,
		HealthCheckServiceBus healthCheckServiceBus)
	{
		this.serviceBusOptions = serviceBusOptions.Value;
		this.serviceBusClient = new ServiceBusClient(this.serviceBusOptions.Namespace, credential);
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		this.healthCheckServiceBus = healthCheckServiceBus ?? throw new ArgumentNullException(nameof(healthCheckServiceBus));
		this.telemetryCollector = telemetryCollector ?? throw new ArgumentNullException(nameof(telemetryCollector));
		healthCheckServiceBus.Current = HealthCheckServiceBus.Starting;
		throttledLogger = logger.Throttle(TimeSpan.FromSeconds(15));
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		logger.LogWarning("Stop async has been called on Message Consumer background service");
		this.overallCancellationTokenSource.Cancel();
		//await CloseServiceBus(cancellationToken);
		await base.StopAsync(cancellationToken);
	}

	private static ServiceBusProcessorOptions serviceBusProcessorOptions = new ServiceBusProcessorOptions
	{
		PrefetchCount = 0,  // default anyway
		AutoCompleteMessages = true,
		MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(120)
	};

	/// <summary>
	/// Substitutes the user name and machine name into a string
	/// using templated substitution. This is used during development
	/// to generate topic names.
	/// </summary>
	internal static string SubstituteEnvironment(string template)
	{
		var machineName = Environment.MachineName;
		var userName = Environment.UserName;
		return template
			.Replace("%USERNAME%", userName)
			.Replace("%COMPUTERNAME%", machineName);
	}

	private async Task InitializeServiceBus(CancellationToken cancellationToken)
	{
		//start as healthy, the error delegate will set to failed
		healthCheckServiceBus.Current = HealthCheckServiceBus.Healthy;

		string topicName = SubstituteEnvironment(serviceBusOptions.Receive.TopicName);
		string subscriptionName = SubstituteEnvironment(serviceBusOptions.Receive.SubscriptionName);
		logger.LogInformation($"MessageConsumer starting listening to subscription = {subscriptionName} topic = {topicName}");
		serviceBusProcessor = serviceBusClient.CreateProcessor(topicName, subscriptionName);
		serviceBusProcessor.ProcessMessageAsync += MessageHandler;
		serviceBusProcessor.ProcessErrorAsync += ErrorHandler;
		await serviceBusProcessor.StartProcessingAsync(cancellationToken);
	}

	private async Task CloseServiceBus(CancellationToken cancellationToken)
	{
		try
		{
			logger.LogInformation($"MessageConsumer STOP listening to subscription = {serviceBusOptions.Receive.SubscriptionName} topic = {serviceBusOptions.Receive.TopicName}");
			if (serviceBusProcessor is ServiceBusProcessor)
			{
				logger.LogInformation("ServiceBus processor StopProcesingAsync()");
				await serviceBusProcessor.StopProcessingAsync(cancellationToken);
				serviceBusProcessor.ProcessMessageAsync -= MessageHandler;
				serviceBusProcessor.ProcessErrorAsync -= ErrorHandler;
				logger.LogInformation("ServiceBus processor DisposeAsync()");
				await serviceBusProcessor.DisposeAsync();
				logger.LogInformation("ServiceBus processor disposed");
			}
		}
		catch (OperationCanceledException)
		{
			// ignore, normal cancellation
		}
	}

	// See https://blog.stephencleary.com/2020/05/backgroundservice-gotcha-startup.html

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// See https://github.com/dotnet/runtime/issues/36063#issuecomment-518913079

		return Task.Run(async () =>
		{
			await Task.Yield();

			try
			{
				logger.LogInformation("MessageConsumer executing");
				await Task.Delay(1000);
				await InitializeServiceBus(stoppingToken);

				// No code here as the callbacks from service bus handle that, just wait for shutdown
				var t = new TaskCompletionSource();
				stoppingToken.Register(() => t.SetResult());
				await t.Task;

				if (serviceBusProcessor is ServiceBusProcessor)
				{
					await serviceBusProcessor.StopProcessingAsync();
				}
				await serviceBusClient.DisposeAsync();
				logger.LogInformation("MessageConsumer terminated");
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Message Consumer failed to start");
			}
		});
	}

	private async Task MessageHandler(ProcessMessageEventArgs args)
	{
		using (var scope = serviceProvider.CreateScope())
		{
			try
			{
				healthCheckServiceBus.Current = HealthCheckServiceBus.Healthy;

				var assembly = System.Reflection.Assembly.GetEntryAssembly();
				var version = assembly?.GetName()?.Version;
				string name = assembly?.GetName()?.Name ?? "<missing>";

				throttledLogger.LogInformation("Message received by {app} running {version}", name, version?.ToString());

				if (args.CancellationToken.IsCancellationRequested)
				{
					logger.LogWarning("Message is already cancelled");
					return;
				}

				if (!args.Message.ApplicationProperties.ContainsKey(ServiceBusMessageConstants.WillowEnvironmentId))
				{
					logger.LogDebug($"Message is missing '{ServiceBusMessageConstants.WillowEnvironmentId}' header, dropping it");
					await args.DeadLetterMessageAsync(args.Message, "Missing environment id header");
					return;
				}

				if (!args.Message.ApplicationProperties.ContainsKey(ServiceBusMessageConstants.MessageType))
				{
					logger.LogDebug($"Message is missing '{ServiceBusMessageConstants.MessageType}' header, dropping it");
					await args.DeadLetterMessageAsync(args.Message, "Missing dotnet type");
					return;
				}

				var willowEnvironmentId = args.Message.ApplicationProperties[ServiceBusMessageConstants.WillowEnvironmentId]?.ToString();
				var typeName = args.Message.ApplicationProperties[ServiceBusMessageConstants.MessageType]?.ToString();

				if (string.IsNullOrEmpty(willowEnvironmentId))
				{
					await args.DeadLetterMessageAsync(args.Message, $"Empty '{ServiceBusMessageConstants.WillowEnvironmentId}' in header");
					return;
				}

				if (string.IsNullOrEmpty(typeName))
				{
					await args.DeadLetterMessageAsync(args.Message, $"Empty '{ServiceBusMessageConstants.MessageType}' in header");
					return;
				}

				// TODO: Should pass this through and into logging and any subsequent calls
				// args.Message.CorrelationId

				// If we wait for processing to complete it could be a long time
				// and then the lock fails on the message, so we have to complete
				// it here, which isn't ideal.
				// no need to do ... await args.CompleteMessageAsync(args.Message);
				// when autocomplete is set

				var messageHandlers = scope.ServiceProvider.GetServices<IMessageHandler>();

				var suitableMessageHandlers = messageHandlers.Where(mh => mh.CanHandle(typeName)).ToList();

				if (!suitableMessageHandlers.Any())
				{
					logger.LogWarning("No handler accepted message type '{typeName}'", typeName);
					// cannot dead letter it because we already completed it
					//await args.DeadLetterMessageAsync(args.Message, $"No handler accepted {typeName}");
					return;
				}

				// Now using Autocomplete on messages
				// logger.LogTrace("Marking message completed for {typeName}, start processing it", typeName);
				// await args.CompleteMessageAsync(args.Message);

				bool atLeastOne = false;
				foreach (var messageHandler in suitableMessageHandlers)
				{
					bool ok = await messageHandler.Handle(args.Message.Body, overallCancellationTokenSource.Token);
					atLeastOne = atLeastOne || ok;
				}

				if (!atLeastOne)
				{
					logger.LogWarning("None of the messages handlers was successful with the message {correlationId}", args.Message.CorrelationId);
				}
			}
			catch (AggregateException aex)
			{
				foreach (var ex in aex.InnerExceptions)
				{
					if (ex is Azure.Identity.CredentialUnavailableException cuex)
					{
						logger.LogError($"Credential unavailable exception while processing message {args.Message.MessageId}\n" + cuex.Message);
					}
					else
					{
						logger.LogError(ex, $"Other exception while processing message {args.Message.MessageId}");
					}
				}
			}
			catch (Azure.Identity.CredentialUnavailableException cux)
			{
				logger.LogError($"Overall Credential unavailable exception while processing message {args.Message.MessageId}\n" + cux.Message);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, $"Exception while processing message {args.Message.MessageId}");
				// No need to abandon explicitly, that's handled
				//await args.AbandonMessageAsync(args.Message, cancellationToken: args.CancellationToken);
			}
		}
	}

	private static RateLimiter rateLimiter = new RateLimiter(10 /* different exceptions */, 1 /* minute*/);

	private async Task ErrorHandler(ProcessErrorEventArgs args)
	{
		// Rate limit logging on entity path and exception type
		string rateLimitKey = args.EntityPath + args.Exception.GetType().Name;

		healthCheckServiceBus.Current = HealthCheckServiceBus.ConnectionFailed;

		rateLimiter.Limit(rateLimitKey, () =>
		{
			string message = $"Error handler exception on `{args.EntityPath}` {args.Exception.GetType()} `{args.Exception.Message}`";
			if (args.Exception is Azure.Identity.AuthenticationFailedException)
				// these happen in testing and they are long and not useful to see whole exception
				throttledLogger.LogError("Auth failed, will wait to see if it improves");
			else if (args.Exception is Azure.Messaging.ServiceBus.ServiceBusException sbe)
				// Still don't have the lock figured out yet
				throttledLogger.LogError($"Error handler exception `{sbe.Message}`");
			else
				throttledLogger.LogError(args.Exception, message);
		}, logger);

		if (args.Exception is Azure.Identity.AuthenticationFailedException)
		{
			return;
		}

		// System.Net.Sockets.SocketException (0xFFFDFFFF)
		// Name or service not known ErrorCode: HostNotFound(ServiceCommunicationProblem)
		if (args.Exception is System.Net.Sockets.SocketException)
		{
			logger.LogCritical("Shutting down listener connection failed (SocketException)");
			await CloseServiceBus(CancellationToken.None);
			await Task.Delay(20000);
			logger.LogCritical("Restarting listener");
			await InitializeServiceBus(CancellationToken.None);
			return;
			// Non-recoverable, handle it and stop
			//channel.Writer.Complete(args.Exception);
		}
	}
}
