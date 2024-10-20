using Azure;
using Azure.DigitalTwins.Core;
using Willow.Model.Adt;

namespace Willow.AzureDigitalTwins.Services.Interfaces;

public interface IAzureDigitalTwinWriter
{
    Task UpdateDigitalTwinAsync(BasicDigitalTwin twin, JsonPatchDocument jsonPatchDocument);
    Task<BasicDigitalTwin> CreateOrReplaceDigitalTwinAsync(BasicDigitalTwin twin, CancellationToken cancellationToken = default);
    Task<IEnumerable<DigitalTwinsModelBasicData>> CreateModelsAsync(IEnumerable<DigitalTwinsModelBasicData> dtdlModels, CancellationToken cancellationToken = default);
    Task DeleteModelAsync(string modelId);
    Task<BasicRelationship> CreateOrReplaceRelationshipAsync(BasicRelationship relationship, CancellationToken cancellationToken = default);
    Task DeleteDigitalTwinAsync(string twinId);
    Task DeleteRelationshipAsync(string twinId, string relationshipId);
    bool IsServiceReady();
}
