using Azure;
using Azure.DigitalTwins.Core;
using Willow.Model.Adt;
using Willow.Model.Requests;

namespace Willow.AzureDigitalTwins.Services.Interfaces;

public interface IAzureDigitalTwinReader
{
    Task<IEnumerable<DigitalTwinsModelBasicData>> GetModelsAsync(string rootModelId = null);
    Task<DigitalTwinsModelBasicData> GetModelAsync(string modelId);
    Task<int> GetTwinsCountAsync();
    Task<int> GetRelationshipsCountAsync();
    Task<IEnumerable<BasicRelationship>> GetIncomingRelationshipsAsync(string twinId);
    Task<BasicDigitalTwin> GetDigitalTwinAsync(string twinId);
    Task<IEnumerable<BasicRelationship>> GetTwinRelationshipsAsync(string twinId, string relationshipName = null);
    Task<IEnumerable<BasicRelationship>> GetRelationshipsAsync(IEnumerable<string> ids = null);
    Task<BasicRelationship> GetRelationshipAsync(string relationshipId, string twinId);
    Task<Model.Adt.Page<BasicDigitalTwin>> GetTwinsAsync(
        GetTwinsInfoRequest request = null,
        IEnumerable<string> twinIds = null,
    int pageSize = 100,
    bool includeCountQuery = false,
    string continuationToken = null);

    Task<Model.Adt.Page<BasicDigitalTwin>> GetTwinsByIdsAsync(IEnumerable<string> twinIds = null);

    Task<int> GetTwinsCountAsyncWithSearch(GetTwinsInfoRequest request = null);
    Task<Model.Adt.Page<BasicDigitalTwin>> QueryTwinsAsync(string query, int pageSize = 100, string continuationToken = null);
    Task<Model.Adt.Page<BasicDigitalTwin>> QueryTwinsAsync(string query, int pageSize = 100, string continuationToken = null, string countQuery = null);
    Task<IEnumerable<(BasicDigitalTwin, IEnumerable<BasicRelationship>, IEnumerable<BasicRelationship>)>> AppendRelationships(IEnumerable<BasicDigitalTwin> twins, bool includeRelationships, bool includeIncomingRelationships);
    AsyncPageable<T> QueryAsync<T>(string query);
    bool IsServiceReady();
    Task<Model.Adt.Page<BasicRelationship>> GetRelationshipsAsync(string continuationToken = null);
}
