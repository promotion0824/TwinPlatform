using Azure.DigitalTwins.Core;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Diagnostic;
using Willow.AzureDigitalTwins.Api.Messaging.Configuration;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;
using Willow.ServiceBus;
using Willow.AzureDigitalTwins.Api.Telemetry;

namespace Willow.AzureDigitalTwins.Api.Messaging.Handlers
{
	public class SyncCacheMessageHandler : TwinsChangeEventMessageHandlerBase
	{
		private readonly IAzureDigitalTwinCacheProvider _azureDigitalTwinCacheProvider;
		private readonly ILogger<SyncCacheMessageHandler> _logger;
        private readonly ITelemetryCollector _telemetryCollector;
        protected IAzureDigitalTwinCache _azureDigitalTwinCache => _azureDigitalTwinCacheProvider.GetOrCreateCache();

        public SyncCacheMessageHandler(IOptions<CacheSyncTopic> topicOptions,
            IAzureDigitalTwinCacheProvider azureDigitalTwinCacheProvider,
            IAzureDigitalTwinReader azureDigitalTwinReader,
            ILogger<SyncCacheMessageHandler> logger,
            ILogger<TwinsChangeEventMessageHandlerBase> loggerBase,
            HealthCheckServiceBus healthCheck,
            ITelemetryCollector telemetryCollector) : base (topicOptions, azureDigitalTwinReader, healthCheck, loggerBase, telemetryCollector)
		{
			_azureDigitalTwinCacheProvider = azureDigitalTwinCacheProvider;
			_logger = logger;
            _telemetryCollector = telemetryCollector;
        }

		public override async Task<MessageProcessingResult> ProcessReceivedMessage(
			ServiceBusReceivedMessage receivedMessage,
			CancellationToken cancellationToken = default)
		{
			_logger.LogDebug($"Processing cache sync message {receivedMessage.MessageId}");
            _telemetryCollector.TrackCacheSyncMessageCount(1);

			MessageProcessingResult result;
			try
			{
				if (!_azureDigitalTwinCacheProvider.IsCacheReady())
				{
					return MessageProcessingResult.Success("No need to update cache, currently getting the latest version loaded");
				}

				result = await base.ProcessReceivedMessage(receivedMessage, cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error processing cache sync message {receivedMessage.MessageId}");
				return MessageProcessingResult.Failed($"Error processing cache sync message {receivedMessage.MessageId}");
			}

            _logger.LogInformation($"Done processing cache sync message {receivedMessage.MessageId}");
            return result;
        }

        protected override async Task ProcessTwinCreateOrUpdate(BasicDigitalTwin twin)
        {
            _azureDigitalTwinCache.TwinCache.TryCreateOrReplaceTwin(twin);
        }

        protected override async Task ProcessTwinDelete(BasicDigitalTwin twin)
        {
            _azureDigitalTwinCache.TwinCache.TryRemoveTwin(twin.Id);
        }

        protected override async Task ProcessRelationshipCreateOrUpdate(BasicRelationship relationship)
        {
            _azureDigitalTwinCache.TwinCache.TryCreateOrReplaceRelationship(relationship);
        }

        protected override async Task ProcessRelationshipDelete(BasicRelationship relationship)
        {
            _azureDigitalTwinCache.TwinCache.TryRemoveRelationship(relationship.Id);
        }

        protected override async Task ProcessModelsCreate(IEnumerable<DigitalTwinsModelBasicData> models)
        {
            foreach (var model in models)
                _azureDigitalTwinCache.ModelCache.TryCreateOrReplaceModel(models);
        }

        protected override async Task ProcessModelDelete(DigitalTwinsModelBasicData model)
        {
            _azureDigitalTwinCache.ModelCache.TryRemoveModel(model.Id);
        }
    }
}
