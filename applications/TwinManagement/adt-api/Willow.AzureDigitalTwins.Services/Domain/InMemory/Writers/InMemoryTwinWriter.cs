using Azure;
using Azure.DigitalTwins.Core;
using Willow.AzureDigitalTwins.Services.Extensions;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;

namespace Willow.AzureDigitalTwins.Services.Domain.InMemory.Writers;

public class InMemoryTwinWriter : IAzureDigitalTwinWriter
{
    protected IAzureDigitalTwinModelParser AzureDigitalTwinModelParser { get; }

    protected IAzureDigitalTwinCacheProvider AzureDigitalTwinCacheProvider { get; }

    protected IAzureDigitalTwinCache AzureDigitalTwinCache => AzureDigitalTwinCacheProvider.GetOrCreateCache();

    public InMemoryTwinWriter(IAzureDigitalTwinModelParser azureDigitalTwinModelParser, IAzureDigitalTwinCacheProvider azureDigitalTwinCacheProvider)
    {
        AzureDigitalTwinModelParser = azureDigitalTwinModelParser;
        AzureDigitalTwinCacheProvider = azureDigitalTwinCacheProvider;
    }

    public virtual Task UpdateDigitalTwinAsync(BasicDigitalTwin twin, JsonPatchDocument jsonPatchDocument)
    {
        AzureDigitalTwinCache.TwinCache.TryCreateOrReplaceTwin(twin);
        return Task.CompletedTask;
    }

    public virtual async Task<IEnumerable<DigitalTwinsModelBasicData>> CreateModelsAsync(IEnumerable<DigitalTwinsModelBasicData> dtdlModels, CancellationToken cancellationToken = default)
    {
        AzureDigitalTwinCache.ModelCache.TryCreateOrReplaceModel(dtdlModels);
        return dtdlModels;
    }

    public virtual Task<BasicDigitalTwin> CreateOrReplaceDigitalTwinAsync(BasicDigitalTwin twin, CancellationToken cancellationToken = default)
    {
        AzureDigitalTwinCache.TwinCache.TryCreateOrReplaceTwin(twin);

        return Task.FromResult(twin);
    }

    public virtual Task<BasicRelationship> CreateOrReplaceRelationshipAsync(BasicRelationship relationship, CancellationToken cancellationToken = default)
    {
        relationship.SetId();

        AzureDigitalTwinCache.TwinCache.TryCreateOrReplaceRelationship(relationship);

        return Task.FromResult(relationship);
    }

    public virtual Task DeleteDigitalTwinAsync(string twinId)
    {
        AzureDigitalTwinCache.TwinCache.TryRemoveTwin(twinId);
        return Task.CompletedTask;
    }

    public virtual Task DeleteModelAsync(string modelId)
    {
        AzureDigitalTwinCache.ModelCache.TryRemoveModel(modelId);
        return Task.CompletedTask;
    }

    public virtual Task DeleteRelationshipAsync(string twinId, string relationshipId)
    {
        AzureDigitalTwinCache.TwinCache.TryRemoveRelationship(relationshipId);
        return Task.CompletedTask;
    }

    public bool IsServiceReady()
    {
        return AzureDigitalTwinCacheProvider.IsCacheReady();
    }
}
