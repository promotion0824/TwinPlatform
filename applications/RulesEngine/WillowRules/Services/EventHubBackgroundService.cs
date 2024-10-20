using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RulesEngine.Processor.Services;
using Willow.Rules;
using Willow.Rules.Configuration;
using Willow.Rules.Logging;

namespace WillowRules.Services;

/// <summary>
/// Background service that listens for messages on EventHubService to batch and process to ADX via Event Hub
/// </summary>
public class EventHubBackgroundService : BackgroundService
{
	private readonly IOptions<CustomerOptions> customerOptions;
	private readonly IEventHubService eventHubService;
	private readonly HealthCheckCalculatedPoints healthCheckEventHub;
	private readonly ILogger<EventHubBackgroundService> logger;
	private readonly DefaultAzureCredential credential;

	/// <summary>
	/// Creates a new <see cref="EventHubBackgroundService" />
	/// </summary>
	public EventHubBackgroundService(IOptions<CustomerOptions> customerOptions,
		IEventHubService eventHubService,
		HealthCheckCalculatedPoints healthCheckEventHub,
		DefaultAzureCredential credential,
		ILogger<EventHubBackgroundService> logger)
	{
		this.customerOptions = customerOptions ?? throw new ArgumentNullException(nameof(customerOptions));
		this.eventHubService = eventHubService ?? throw new ArgumentNullException(nameof(eventHubService));
		this.healthCheckEventHub = healthCheckEventHub ?? throw new ArgumentNullException(nameof(healthCheckEventHub));
		this.credential = credential ?? throw new ArgumentNullException(nameof(credential));
		this.logger = logger;
	}

	/// <summary>
	/// Main loop for background service
	/// </summary>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("EventHub sender background service starting");

		logger.LogInformation("Eventhub settings: NamespaceName: {namespaceName}, QueueName: {queueName}",
			customerOptions.Value.EventHub?.NamespaceName, customerOptions.Value.EventHub?.QueueName);

		var throttleLogger = logger.Throttle(TimeSpan.FromSeconds(30));

		EventHubProducerClient? producerClient;

		bool isConfigured = !string.IsNullOrWhiteSpace(customerOptions.Value.EventHub?.NamespaceName)
							&& !string.IsNullOrWhiteSpace(customerOptions.Value.EventHub?.QueueName);

		if (!isConfigured)
		{
			producerClient = null;
			logger.LogWarning("Will not be able to write to ADX, EventHub settings for {customerName} is not configured", customerOptions.Value.Name);
			this.healthCheckEventHub.Current = HealthCheckCalculatedPoints.NotConfigured;
		}
		else
		{
			producerClient = GetProducerClient(credential);
			if (producerClient is null)
			{
				this.healthCheckEventHub.Current = HealthCheckCalculatedPoints.AuthorizationFailure;
			}
			else
			{
				this.healthCheckEventHub.Current = HealthCheckCalculatedPoints.NoCalculatedPoints;
			}
		}

		DateTimeOffset nextRetry = DateTimeOffset.Now;

		//Continue reading messages else writer will fill queue
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				var batch = await eventHubService.Reader.ReadMultipleAsync(maxBatchSize: 200, cancellationToken: stoppingToken);

				if (isConfigured && producerClient is null && DateTimeOffset.Now > nextRetry)
				{
					producerClient = GetProducerClient(credential);  // may still be null
					nextRetry = DateTimeOffset.Now.AddMinutes(10);
				}

				if (producerClient is not null)
				{
					await SendBatch(producerClient, batch, throttleLogger);

					throttleLogger.LogInformation("EventHub queued points: {readerCount} / {maxCapacity}", eventHubService.Reader.Count, EventHubService.MaxQueueCapacity);
				}
				else
				{
					throttleLogger.LogInformation("EventHub dropped points: {batchSize}", batch.Count);
				}
			}
			catch (UnauthorizedAccessException ex)
			{
				logger.LogError(ex, "EventHub failed to send data, giving up for several minutes");
				this.healthCheckEventHub.Current = HealthCheckCalculatedPoints.AuthorizationFailure;
				producerClient = null;
			}
			catch (OperationCanceledException)
			{
				logger.LogInformation("EventHub sender service told to shut down");
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "EventHub failed to send batch event data");
			}
		}
		logger.LogInformation("EventHub sender background service closing");
	}

	private EventHubProducerClient? GetProducerClient(DefaultAzureCredential credential)
	{
		try
		{
			var eventHubNameSpace = customerOptions.Value.EventHub.NamespaceName;
			var eventHubName = customerOptions.Value.EventHub.QueueName;

			var producer = new EventHubProducerClient($"{eventHubNameSpace}.servicebus.windows.net", eventHubName, credential);

			logger.LogInformation($"EventHubProducerClient created");

			return producer;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "EventHub failed to create EventHubProducerClient for {customerName}", customerOptions.Value.Name);
		}

		return null;
	}

	/// <summary>
	/// Send batch data to Event hub
	/// </summary>
	/// <remarks>
	/// Max allowed EventDataBatch size: 1048576 Bytes
	/// </remarks>
	public async Task SendBatch(EventHubProducerClient producerClient, IEnumerable<EventHubServiceDto> batchData, ILogger logger)
	{
		if (producerClient is null) return;

		try
		{
			var settings = new JsonSerializerSettings()
			{
				NullValueHandling = NullValueHandling.Ignore,
			};

			using (EventDataBatch eventBatch = await producerClient.CreateBatchAsync())
			{
				foreach (var item in batchData)
				{
					if (string.IsNullOrWhiteSpace(item.ExternalId) && string.IsNullOrWhiteSpace(item.TrendId))
					{
						logger.LogWarning("EventHub Item has no trend id or external id");
						continue;
					}

					var eventData = new EventData(JsonConvert.SerializeObject(item, settings: settings));

					eventBatch.TryAdd(eventData);
				}
				await producerClient.SendAsync(eventBatch);
			}
			this.healthCheckEventHub.Current = HealthCheckCalculatedPoints.Healthy;
		}
		catch (UnauthorizedAccessException ex)
		{
			logger.LogError(ex, "EventHub Failed to send data, giving up");
			this.healthCheckEventHub.Current = HealthCheckCalculatedPoints.AuthorizationFailure;
			throw;
		}
		catch (Microsoft.Azure.Amqp.AmqpException aex)
		{
			logger.LogError(aex, "EventHub Failed to send data, AMQP error, will keep trying");
			if (aex.Message.Contains("Unauthorized"))
			{
				this.healthCheckEventHub.Current = HealthCheckCalculatedPoints.AuthorizationFailure;
			}
			else
			{
				this.healthCheckEventHub.Current = HealthCheckCalculatedPoints.FailingCalls;
			}
		}
		catch (Exception ex)
		{
			this.healthCheckEventHub.Current = HealthCheckCalculatedPoints.FailingCalls;

			logger.LogError(ex, "EventHub Failed to send data will keep trying");
		}
	}
}
