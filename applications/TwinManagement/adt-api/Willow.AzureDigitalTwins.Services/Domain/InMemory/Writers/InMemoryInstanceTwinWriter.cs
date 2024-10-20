using Azure;
using Azure.DigitalTwins.Core;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;

namespace Willow.AzureDigitalTwins.Services.Domain.InMemory.Writers;

public class InMemoryInstanceTwinWriter : InMemoryTwinWriter
{
    private readonly IAzureDigitalTwinWriter _azureDigitalTwinWriter;

    public InMemoryInstanceTwinWriter(IAzureDigitalTwinModelParser azureDigitalTwinModelParser,
        IAzureDigitalTwinCacheProvider azureDigitalTwinCacheProvider,
        IAzureDigitalTwinWriter azureDigitalTwinWriter)
        : base(azureDigitalTwinModelParser, azureDigitalTwinCacheProvider)
    {
        _azureDigitalTwinWriter = azureDigitalTwinWriter;
    }

    public override async Task DeleteModelAsync(string modelId)
    {
        await _azureDigitalTwinWriter.DeleteModelAsync(modelId);
        await base.DeleteModelAsync(modelId);
    }

    public override async Task<IEnumerable<DigitalTwinsModelBasicData>> CreateModelsAsync(IEnumerable<DigitalTwinsModelBasicData> dtdlModels, CancellationToken cancellationToken = default)
    {
        await _azureDigitalTwinWriter.CreateModelsAsync(dtdlModels, cancellationToken);
        return await base.CreateModelsAsync(dtdlModels, cancellationToken);
    }

    public override async Task<BasicDigitalTwin> CreateOrReplaceDigitalTwinAsync(BasicDigitalTwin twin, CancellationToken cancellationToken = default)
    {
        await _azureDigitalTwinWriter.CreateOrReplaceDigitalTwinAsync(twin, cancellationToken);
        return await base.CreateOrReplaceDigitalTwinAsync(twin, cancellationToken);
    }

    public override async Task<BasicRelationship> CreateOrReplaceRelationshipAsync(BasicRelationship relationship, CancellationToken cancellationToken = default)
    {
        await _azureDigitalTwinWriter.CreateOrReplaceRelationshipAsync(relationship, cancellationToken);
        return await base.CreateOrReplaceRelationshipAsync(relationship, cancellationToken);
    }

    public async override Task UpdateDigitalTwinAsync(BasicDigitalTwin twin, JsonPatchDocument jsonPatchDocument)
    {
        await _azureDigitalTwinWriter.UpdateDigitalTwinAsync(twin, jsonPatchDocument);
        await base.UpdateDigitalTwinAsync(twin, jsonPatchDocument);
    }

    public override async Task DeleteDigitalTwinAsync(string twinId)
    {
        await _azureDigitalTwinWriter.DeleteDigitalTwinAsync(twinId);
        await base.DeleteDigitalTwinAsync(twinId);
    }

    public override async Task DeleteRelationshipAsync(string twinId, string relationshipId)
    {
        await _azureDigitalTwinWriter.DeleteRelationshipAsync(twinId, relationshipId);
        await base.DeleteRelationshipAsync(twinId, relationshipId);
    }
}
