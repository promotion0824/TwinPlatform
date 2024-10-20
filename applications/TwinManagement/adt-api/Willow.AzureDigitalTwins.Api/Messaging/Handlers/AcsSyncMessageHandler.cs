using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Diagnostic;
using Willow.AzureDigitalTwins.Api.Messaging.Configuration;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;

namespace Willow.AzureDigitalTwins.Api.Messaging.Handlers;

/// <summary>
/// Service Bus Message handler for ACS sync topic.
/// </summary>
public class AcsSyncMessageHandler : TwinsChangeEventMessageHandlerBase
{
    private readonly ILogger<AcsSyncMessageHandler> _logger;
    private readonly IAcsService _acsService;
    private readonly ITelemetryCollector _telemetryCollector;

    public AcsSyncMessageHandler(IOptions<AcsSyncTopic> topicOptions,
        IAzureDigitalTwinReader azureDigitalTwinReader,
        HealthCheckServiceBus healthCheckServiceBus,
        ILogger<AcsSyncMessageHandler> logger,
        IAcsService acsService,
        ITelemetryCollector telemetryCollector) : base(topicOptions, azureDigitalTwinReader, healthCheckServiceBus, logger, telemetryCollector)
    {
        _logger = logger;
        _acsService = acsService;
        _telemetryCollector = telemetryCollector;
    }

    protected override Task ProcessModelDelete(DigitalTwinsModelBasicData model)
    {
        return Task.CompletedTask;
    }

    protected override Task ProcessModelsCreate(IEnumerable<DigitalTwinsModelBasicData> models)
    {
        return Task.CompletedTask;
    }

    protected override Task ProcessRelationshipCreateOrUpdate(BasicRelationship relationship)
    {
        return Task.CompletedTask;
    }

    protected override Task ProcessRelationshipDelete(BasicRelationship relationship)
    {
        return Task.CompletedTask;
    }

    protected async override Task ProcessTwinCreateOrUpdate(BasicDigitalTwin twin)
    {
        _logger.LogInformation("AcsSync  ProcessTwinCreateOrUpdate: {twin}", twin.Id);
        await _acsService.QueueForUpsertTwinAsync(twin);

        _telemetryCollector.TrackACSTwinUpsertCount(1);

        // Flush will be called periodically by the AcsFlushJobProcessor
    }

    protected async override Task ProcessTwinDelete(BasicDigitalTwin twin)
    {
        _logger.LogInformation("AcsSync  ProcessTwinDelete: {twin}", twin.Id);
        await _acsService.QueueForDeleteTwinAsync(twin.Id);

        _telemetryCollector.TrackACSTwinDeletionCount(1);

        // Flush will be called periodically by the AcsFlushJobProcessor
    }
}
