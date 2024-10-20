using Azure.DigitalTwins.Core;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Diagnostic;
using Willow.AzureDigitalTwins.Api.Helpers;
using Willow.AzureDigitalTwins.Api.Messaging.Configuration;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;
using Willow.ServiceBus;

namespace Willow.AzureDigitalTwins.Api.Messaging.Handlers
{
    public abstract class TwinsChangeEventMessageHandlerBase : ITopicMessageHandler
    {
        private readonly Topic _topicOptions;
        private readonly IAzureDigitalTwinReader _azureDigitalTwinReader;
        protected readonly HealthCheckServiceBus _healthCheckServiceBus;
        private readonly ILogger<TwinsChangeEventMessageHandlerBase> _logger;
        private readonly ITelemetryCollector _telemetryCollector;

        private const int _maxConcurrentCalls = 20;

        public TwinsChangeEventMessageHandlerBase(IOptions<Topic> topicOptions,
            IAzureDigitalTwinReader azureDigitalTwinReader,
            HealthCheckServiceBus healthCheckServiceBus,
            ILogger<TwinsChangeEventMessageHandlerBase> logger,
            ITelemetryCollector telemetryCollector)
        {
            _topicOptions = topicOptions.Value;
            _azureDigitalTwinReader = azureDigitalTwinReader;
            _healthCheckServiceBus = healthCheckServiceBus;
            _logger = logger;
            _telemetryCollector = telemetryCollector;
        }

        public ServiceBusProcessorOptions ServiceBusProcessorOptions => new()
        {
            MaxConcurrentCalls = _maxConcurrentCalls,
            AutoCompleteMessages = false,
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(10)
        };

        public string TopicName => _topicOptions.TopicName;

        public string SubscriptionName => _topicOptions.SubscriptionName;

        public string ServiceBusInstance => _topicOptions.ServiceBusName;

        public virtual async Task<MessageProcessingResult> ProcessReceivedMessage(
            ServiceBusReceivedMessage receivedMessage,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return MessageProcessingResult.Failed("Message processing cancelled");
            }


            var messageTypeName = receivedMessage.ApplicationProperties[ChangeEvent.MessageCloudEventsType].ToString();

            var processingAction = GetProcessingType(receivedMessage, messageTypeName);

            if (processingAction == null || !processingAction.ContainsKey(messageTypeName))
            {
                return MessageProcessingResult.Failed($"Processing action not found for type {messageTypeName}");
            }

            await processingAction[messageTypeName](receivedMessage);

            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckServiceBus, HealthCheckServiceBus.Healthy, _logger);

            return MessageProcessingResult.Success();
        }

        private Dictionary<string, Func<ServiceBusReceivedMessage, Task>> GetProcessingType(ServiceBusReceivedMessage message, string messageType)
        {
            if (messageType.Contains(ChangeEvent.MessageTwinEventNamespace))
                return GetTwinProcessingAction();

            if (messageType.Contains(ChangeEvent.MessageRelationshipEventNamespace))
                return GetRelationshipProcessingAction();

            if (messageType.Contains(ChangeEvent.MessageModelEventNamespace))
                return GetModelProcessingAction();

            return null;
        }

        protected abstract Task ProcessTwinCreateOrUpdate(BasicDigitalTwin twin);
        protected abstract Task ProcessTwinDelete(BasicDigitalTwin twin);

        protected abstract Task ProcessRelationshipCreateOrUpdate(BasicRelationship relationship);
        protected abstract Task ProcessRelationshipDelete(BasicRelationship relationship);

        protected abstract Task ProcessModelsCreate(IEnumerable<DigitalTwinsModelBasicData> models);
        protected abstract Task ProcessModelDelete(DigitalTwinsModelBasicData model);

        private Dictionary<string, Func<ServiceBusReceivedMessage, Task>> GetTwinProcessingAction()
        {
            return new Dictionary<string, Func<ServiceBusReceivedMessage, Task>>
            {
                {  $"{ChangeEvent.MessageTwinEventNamespace}.{ChangeEvent.MessageCloudEventsUpdateAction}", async x =>
                    {
                        var twinId = x.ApplicationProperties[ChangeEvent.MessageCloudEventsSubject].ToString();

                        var twin = await _azureDigitalTwinReader.GetDigitalTwinAsync(twinId);

                        _logger.LogDebug($"Twin update event for Twin: {twinId}");
                        _telemetryCollector.TrackServiceBusTwinUpdateEventCount(1);

                        await ProcessTwinCreateOrUpdate(twin);
                    }
                },
                {
                    $"{ChangeEvent.MessageTwinEventNamespace}.{ChangeEvent.MessageCloudEventsDeleteAction}", async x =>
                    {
                        var twin = JsonSerializer.Deserialize<BasicDigitalTwin>(x.Body.ToString());

                        _logger.LogDebug($"Twin delete event for Twin: {twin.Id}");
                        _telemetryCollector.TrackServiceBusTwinDeletionEventCount(1);

                        await ProcessTwinDelete(twin);
                    }
                },
                {
                    $"{ChangeEvent.MessageTwinEventNamespace}.{ChangeEvent.MessageCloudEventsCreateAction}", async x =>
                    {
                        var twin = JsonSerializer.Deserialize<BasicDigitalTwin>(x.Body.ToString());

                        _logger.LogDebug($"Twin creation event for Twin: {twin.Id}");
                        _telemetryCollector.TrackServiceBusTwinCreationEventCount(1);

                        await ProcessTwinCreateOrUpdate(twin);
                    }
                }
            };
        }

        private Dictionary<string, Func<ServiceBusReceivedMessage, Task>> GetModelProcessingAction()
        {
            return new Dictionary<string, Func<ServiceBusReceivedMessage, Task>>
            {
                {
                    $"{ChangeEvent.MessageModelEventNamespace}.{ChangeEvent.MessageCloudEventsDeleteAction}", async x =>
                    {
                        var model = JsonSerializer.Deserialize<DigitalTwinsModelBasicData>(x.Body.ToString());

                        _logger.LogInformation($"Model deletion event for Model: {model.Id}");
                        _telemetryCollector.TrackServiceBusModelDeletionEventCount(1);

                        await ProcessModelDelete(model);
                    }
                },
                {
                    $"{ChangeEvent.MessageModelEventNamespace}.{ChangeEvent.MessageCloudEventsCreateAction}", async x =>
                    {
                        var getModelsTask = JsonSerializer.Deserialize<IEnumerable<string>>(x.Body.ToString()).Select(
                            // BulkModelProcessor refresh the cache right after importing the models in to ADT
                            // AdxSyncMessageHandler resolve IAzureDigitalTwinReader of this class with an instance of AzureDigitalTwinCacheReader
                            // so it is safe to retrieve the model from the cache since it is refreshed already.
                            async x => await _azureDigitalTwinReader.GetModelAsync(x));

                        var models = await Task.WhenAll(getModelsTask);

                        _logger.LogInformation($"Model creation event for {models.Count()} Models");
                       _telemetryCollector.TrackServiceBusModelCreationEventCount(models.Count());

                        await ProcessModelsCreate(models.Where(m => m is not null));
                    }
                }
            };
        }

        private Dictionary<string, Func<ServiceBusReceivedMessage, Task>> GetRelationshipProcessingAction()
        {
            return new Dictionary<string, Func<ServiceBusReceivedMessage, Task>>
            {
                {  $"{ChangeEvent.MessageRelationshipEventNamespace}.{ChangeEvent.MessageCloudEventsUpdateAction}", async x =>
                    {
                        var relationshipPath = x.ApplicationProperties[ChangeEvent.MessageCloudEventsSubject].ToString().Split('/');
                        var relationshipId = relationshipPath[2];
                        var twinId = relationshipPath[0];

                        var relationship = await _azureDigitalTwinReader.GetRelationshipAsync(relationshipId, twinId);

                        _logger.LogDebug($"Relationship update event for relationship: {relationship.Id}");
                        _telemetryCollector.TrackServiceBusRelationshipCreationEventCount(1);

                        await ProcessRelationshipCreateOrUpdate(relationship);
                    }
                },
                {
                    $"{ChangeEvent.MessageRelationshipEventNamespace}.{ChangeEvent.MessageCloudEventsDeleteAction}", async x =>
                    {
                        var relationship = JsonSerializer.Deserialize<BasicRelationship>(x.Body.ToString());

                        _logger.LogDebug($"Relationship deletion event for relationship: {relationship.Id}");
                        _telemetryCollector.TrackServiceBusRelationshipDeletionEventCount(1);

                        await ProcessRelationshipDelete(relationship);
                    }
                },
                {
                    $"{ChangeEvent.MessageRelationshipEventNamespace}.{ChangeEvent.MessageCloudEventsCreateAction}", async x =>
                    {
                        var relationship = JsonSerializer.Deserialize<BasicRelationship>(x.Body.ToString());
                        _logger.LogDebug($"Relationship creation event for relationship: {relationship.Id}");
                        _telemetryCollector.TrackServiceBusRelationshipUpdateEventCount(1);

                        await ProcessRelationshipCreateOrUpdate(relationship);
                    }
                }
            };
        }

        public void OnError(Exception ex)
        {
            // TODO: May want to classify the errors and set the health check accordingly.
            HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckServiceBus, HealthCheckServiceBus.FailedToConnect, _logger);
        }
    }
}
