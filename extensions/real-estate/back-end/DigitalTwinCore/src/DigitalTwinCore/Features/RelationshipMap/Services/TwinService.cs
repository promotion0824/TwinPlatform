using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DigitalTwinCore.Features.RelationshipMap.Models;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using BasicDigitalTwin = DigitalTwinCore.Features.RelationshipMap.Models.BasicDigitalTwin;

namespace DigitalTwinCore.Features.RelationshipMap.Services;

public interface ITwinService
{
    Task<List<BasicDigitalTwin>> GetTopLevelEntities(Guid siteId);
    Task<BasicDigitalTwin> GetTwin(Guid siteId, string id);
    Task<List<Edge>> GetRelatedTwins(Guid siteId, string twinId, Direction direction);
}

public enum Direction
{
    Forward,
    Backward
}

public class TwinService : ITwinService
{
    private readonly ITwinCachedService _cachedService;
    private readonly ILogger<TwinService> _logger;
    private readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<Azure.RequestFailedException>()
        .WaitAndRetryAsync(3, retryAttempt =>
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
        );

    private static readonly string[] TopLevelModels = {
        "dtmi:com:willowinc:Portfolio;1",
        "dtmi:com:willowinc:Building;1",
        "dtmi:com:willowinc:Land;1",
        "dtmi:com:willowinc:Floor;1",
        "dtmi:com:willowinc:mining:System;1",
        "dtmi:com:willowinc:Equipment;1"
    };

    public TwinService(ITwinCachedService cachedService, ILogger<TwinService> logger)
    {
        _cachedService = cachedService;
        _logger = logger;
    }

    public async Task<List<BasicDigitalTwin>> GetTopLevelEntities(Guid siteId)
    {
        List<BasicDigitalTwin> result = [];
        foreach (var modelId in TopLevelModels)
        {
            var twins = await _cachedService.QueryAsync<BasicDigitalTwin>(siteId, $"SELECT * FROM DIGITALTWINS DT WHERE IS_OF_MODEL(DT, '{SafeId(modelId)}')");
            result.AddRange(twins);
            if (result.Any()) break;
        }
        return result;
    }

    public async Task<BasicDigitalTwin> GetTwin(Guid siteId, string id)
    {
        return await _cachedService.GetDigitalTwinAsync<BasicDigitalTwin>(siteId, id);
    }

    public Task<List<Edge>> GetRelatedTwins(Guid siteId, string twinId, Direction direction)
    {
        return direction == Direction.Forward
            ? GetRelatedTwins(siteId, $"SELECT twin,rel from digitaltwins MATCH (equipment_twin)-[rel]->(twin) WHERE equipment_twin.$dtId='{SafeId(twinId)}'", "hasDocument", "installedBy", "manufacturedBy")
            : GetRelatedTwins(siteId, $"SELECT twin,rel from digitaltwins MATCH (equipment_twin)<-[rel]-(twin) WHERE equipment_twin.$dtId='{SafeId(twinId)}'");
    }

    private async Task<List<Edge>> GetRelatedTwins(Guid siteId, string query, params string[] skipRelationships)
    {
        var edges = new List<Edge>();

        var resultForward = await _retryPolicy.ExecuteAsync(async () => await _cachedService.QueryAsync<Dictionary<string, JsonDocument>>(siteId, query));

        foreach (var twinrel in resultForward)
        {
            var twin = twinrel["twin"];
            var rel = twinrel["rel"];

            var twinDto = twin.Deserialize<BasicDigitalTwin>()!;
            var relExtended = rel.Deserialize<ExtendedRelationship>()!;

            if (skipRelationships.Contains(relExtended.Name, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.IsNullOrEmpty(twinDto.Name))
            {
                twinDto.Name = twinDto.Id;
                _logger.LogWarning("Get related: {TwinId} has no name", twinDto.Id);
            }

            edges.Add(new Edge
            {
                Destination = twinDto,
                Id = relExtended.Id,
                RelationshipType = relExtended.Name,
                Substance = relExtended.Substance
            });
        }

        return edges;
    }

    private static string SafeId(string id)
    {
        return id.Replace("'", "\\'");
    }
}
