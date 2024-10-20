using Azure.DigitalTwins.Core;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDataExplorer.Options;
using Willow.AzureDigitalTwins.Api.Diagnostic;
using Willow.AzureDigitalTwins.Api.Messaging.Configuration;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;
using Willow.ServiceBus;
using Willow.AzureDigitalTwins.Api.Telemetry;

namespace Willow.AzureDigitalTwins.Api.Messaging.Handlers
{
	public class AdxSyncMessageHandler : TwinsChangeEventMessageHandlerBase
	{
		private IExportService _exportService;
		private readonly IAdxSetupService _adxSetupService;
		private readonly ILogger<AdxSyncMessageHandler> _logger;
		private readonly IOptions<AzureDataExplorerOptions> _adxOptions;
		private readonly bool _adxSyncEnabled;
        private readonly ITelemetryCollector _telemetryCollector;

        public AdxSyncMessageHandler(IOptions<AdxSyncTopic> topicOptions,
			IAzureDigitalTwinReader azureDigitalTwinReader,
			IExportService exportService,
			IAdxSetupService adxSetupService,
			IOptions<AzureDataExplorerOptions> adxOptions,
			ILogger<AdxSyncMessageHandler> logger,
            ILogger<TwinsChangeEventMessageHandlerBase> loggerBase,
            IConfiguration configuration,
            HealthCheckServiceBus healthCheck,
            ITelemetryCollector telemetryCollector
			) : base(topicOptions, azureDigitalTwinReader, healthCheck, loggerBase, telemetryCollector)
		{
			_exportService = exportService;
			_adxSetupService = adxSetupService;
			_adxOptions = adxOptions;
			_logger = logger;
			var syncEnabled = configuration.GetValue<string>("ServiceBus:Topics:AdxSyncTopic:Enabled");
			_adxSyncEnabled = bool.TryParse(syncEnabled, out bool enabled) && enabled;
            _telemetryCollector = telemetryCollector;

            _logger.LogInformation("ADX sync enabled: {enabled}", _adxSyncEnabled);
		}

        public override async Task<MessageProcessingResult> ProcessReceivedMessage(
            ServiceBusReceivedMessage receivedMessage,
            CancellationToken cancellationToken = default)
        {
            if (!_adxSyncEnabled)
            {
                _logger.LogTrace("ADX sync disabled, dropping message {MessageId}", receivedMessage.MessageId);
                return MessageProcessingResult.Success();
            }

			_logger.LogDebug("Processing Adx sync message {MsgId}", receivedMessage.MessageId);
            _telemetryCollector.TrackAdxSyncMessageCount(1);

            if (!IsConfigurationValid())
            {
                _logger.LogError("Missing Adx configuration, message {MessageId}", receivedMessage.MessageId);
                return MessageProcessingResult.Failed("Invalid ADX configuration");
            }

            MessageProcessingResult result;
            try
            {
                var isAdxInitialized = await _adxSetupService.IsAdxInitializedAsync();
                if (!isAdxInitialized)
                {
                    _logger.LogError("ADX schema not initialized, message {MessageId}", receivedMessage.MessageId);
                    return MessageProcessingResult.Failed("ADX schema not initialized");
                }

				result = await base.ProcessReceivedMessage(receivedMessage, cancellationToken);

            }
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error processing Adx sync message {MessageId}", receivedMessage.MessageId);
				return MessageProcessingResult.Failed($"Error processing Adx sync message {receivedMessage.MessageId}");
			}

            _logger.LogInformation("Done processing Adx sync message {MessageId}", receivedMessage.MessageId);

            return result;
        }

        private bool IsConfigurationValid()
        {
            if (_adxOptions is null || _adxOptions.Value is null)
                return false;

            var hasClusterUri = _adxOptions.Value.ClusterUri is not null;
            var hasClusterNameAndRegion = _adxOptions.Value.ClusterRegion is not null && _adxOptions.Value.ClusterName is not null;

            if (!hasClusterNameAndRegion && !hasClusterUri)
                return false;

            if (_adxOptions.Value.DatabaseName is null)
                return false;

            return true;
        }

        protected override async Task ProcessTwinCreateOrUpdate(BasicDigitalTwin twin)
        {
            _logger.LogInformation("AdxSync  ProcessTwinCreateOrUpdate: {twin}", twin.Id);
            _telemetryCollector.TrackADXTwinCreationCount(1);

            await _exportService.AppendTwinToAdx(twin);
        }

        protected override async Task ProcessTwinDelete(BasicDigitalTwin twin)
        {
            _logger.LogInformation("AdxSync  ProcessTwinDelete: {twin}", twin.Id);
            _telemetryCollector.TrackADXTwinDeletionCount(1);

            await _exportService.AppendTwinToAdx(twin, true);
        }

        protected override async Task ProcessRelationshipCreateOrUpdate(BasicRelationship relationship)
        {
            _logger.LogInformation("AdxSync  ProcessRelationshipCreateOrUpdate: {rel}", relationship.Id);
            _telemetryCollector.TrackADXRelationshipCreationCount(1);

            await _exportService.AppendRelationshipToAdx(relationship);
        }

        protected override async Task ProcessRelationshipDelete(BasicRelationship relationship)
        {
            _logger.LogInformation("AdxSync  ProcessRelationshipDelete: {rel}", relationship.Id);
            _telemetryCollector.TrackADXRelationshipDeletionCount(1);

            await _exportService.AppendRelationshipToAdx(relationship, true);
        }

        protected override async Task ProcessModelsCreate(IEnumerable<DigitalTwinsModelBasicData> models)
        {
            // Batch the list into batches of 250 to avoid 429s on ~2000 models import at once
            // Might need to config this in app setting for different environment base on capacity of ADX.
            // TODO: Determine the location to do batching. Could be in IngestInline so it's not the caller's responsibility to do this.
            var batches = models.Chunk(250);
            foreach (var batch in batches)
            {
                _logger.LogInformation("AdxSync  ProcessModelsCreate: {batch}",
                batch?.Any() == false ? "n/a" : string.Join(", ", batch.Select(m => m.Id)));
                if (batch is not null) _telemetryCollector.TrackADXModelCreationCount(batch.Length);

                await _exportService.AppendModelsToAdx(batch);
            }
        }

        protected override async Task ProcessModelDelete(DigitalTwinsModelBasicData model)
        {
            _logger.LogInformation("AdxSync  ProcessModelDelete: {model}", model.Id);
            _telemetryCollector.TrackADXModelDeletionCount(1);

            await _exportService.AppendModelsToAdx(new List<DigitalTwinsModelBasicData> { model }, true);
        }
    }
}
