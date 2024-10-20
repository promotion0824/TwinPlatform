using Azure;
using Azure.Core;
using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using Polly;
using System.Net;
using System.Text.Json;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Extensions;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;

namespace Willow.AzureDigitalTwins.Services.Domain.Instance.Writers;


public class AzureDigitalTwinWriter : IAzureDigitalTwinWriter
{
    private readonly DigitalTwinsClient _digitalTwinsClient;
    private readonly IAzureDigitalTwinModelParser _azureDigitalTwinModelParser;
    private readonly ILogger<AzureDigitalTwinWriter> _logger;
    private readonly IEnumerable<IAzureDigitalTwinValidator> _twinValidators;

    public AzureDigitalTwinWriter(InstanceSettings settings, TokenCredential tokenCredential,
        IAzureDigitalTwinModelParser azureDigitalTwinModelParser,
        ILogger<AzureDigitalTwinWriter> logger,
        IEnumerable<IAzureDigitalTwinValidator> twinValidators)
    {
        _digitalTwinsClient = new DigitalTwinsClient(settings.InstanceUri, tokenCredential);
        _azureDigitalTwinModelParser = azureDigitalTwinModelParser;
        _logger = logger;
        _twinValidators = twinValidators;
    }

    public async Task<BasicDigitalTwin> CreateOrReplaceDigitalTwinAsync(BasicDigitalTwin twin, CancellationToken cancellationToken = default)
    {
        _azureDigitalTwinModelParser.EnsureRequiredFields(twin);
        await _twinValidators.ValidateAsync(twin, throwIfError: true);

        try
        {
            var response = await ProcessWithRetry(async () => await _digitalTwinsClient.CreateOrReplaceDigitalTwinAsync(twin.Id, twin, cancellationToken: cancellationToken));
            _logger.LogInformation("CreateOrReplaceDigitalTwinAsync Twin Id: {TwinId}", twin.Id);
            return response.Value;
        }
        catch (RequestFailedException)
        {
            _logger.LogError("Failed to create Twin with payload : {Twin}", JsonSerializer.Serialize(twin));
            throw;
        }

    }

    public async Task UpdateDigitalTwinAsync(BasicDigitalTwin twin, JsonPatchDocument jsonPatchDocument)
    {
        try
        {
            await ProcessWithRetry(async () => await _digitalTwinsClient.UpdateDigitalTwinAsync(twin.Id, jsonPatchDocument));
            _logger.LogInformation("UpdateDigitalTwinAsync Twin Id: {TwinId}", twin.Id);
        }
        catch (RequestFailedException)
        {
            _logger.LogError("Failed to update Twin with payload : {Twin}", JsonSerializer.Serialize(twin));
            throw;
        }
    }

    public async Task DeleteDigitalTwinAsync(string twinId)
    {
        var response = await ProcessWithRetry(() => _digitalTwinsClient.DeleteDigitalTwinAsync(twinId));
        _logger.LogInformation("DeleteDigitalTwinAsync Twin Id: {TwinId} {Status}", twinId, response.Status);
    }

    public async Task<BasicRelationship> CreateOrReplaceRelationshipAsync(BasicRelationship relationship, CancellationToken cancellationToken = default)
    {
        relationship.SetId();

        var response = await ProcessWithRetry(() => _digitalTwinsClient.CreateOrReplaceRelationshipAsync(relationship.SourceId, relationship.Id, relationship, relationship.ETag, cancellationToken));
        return response.Value;
    }

    public async Task DeleteRelationshipAsync(string twinId, string relationshipId)
    {
        var response = await ProcessWithRetry(() => _digitalTwinsClient.DeleteRelationshipAsync(twinId, relationshipId));
        _logger.LogInformation("DeleteRelationshipAsync Twin Id: {TwinId} {Status}", twinId, response.Status);
    }

    public async Task<IEnumerable<DigitalTwinsModelBasicData>> CreateModelsAsync(IEnumerable<DigitalTwinsModelBasicData> dtdlModels, CancellationToken cancellationToken = default)
    {
        await ProcessWithRetry(() => _digitalTwinsClient.CreateModelsAsync(dtdlModels.Select(x => x.DtdlModel), cancellationToken));
        return dtdlModels;
    }

    public async Task DeleteModelAsync(string modelId)
    {
        var response = await ProcessWithRetry(() => _digitalTwinsClient.DeleteModelAsync(modelId));
        _logger.LogInformation("DeleteModelAsync Model Id: {ModelId} {Status}", modelId, response.Status);
    }

    private static async Task<T> ProcessWithRetry<T>(Func<Task<T>> action)
    {
        Task<T> response = null;

        var retryPolicy = Policy.Handle<RequestFailedException>(x => x.Status == (int)HttpStatusCode.TooManyRequests)
            .WaitAndRetry(5, i => TimeSpan.FromSeconds(1));

        retryPolicy.Execute(() =>
        {
            response = action();
        });

        return await response;
    }

    public bool IsServiceReady()
    {
        return true;
    }
}
