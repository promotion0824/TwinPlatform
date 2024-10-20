using Azure.DigitalTwins.Core;
using DigitalTwinCore.DTO;
using DigitalTwinCore.Extensions;
using DigitalTwinCore.Features.RelationshipMap.Dtos;
using DigitalTwinCore.Features.RelationshipMap.Extensions;
using DigitalTwinCore.Features.TwinsSearch.Models;
using DigitalTwinCore.Services.AdtApi;
using DigitalTwinCore.Services.Adx;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DigitalTwinCore.Features.TwinsSearch.Services;

public interface IRelationshipMapService
{
    Task<TwinGraphDto> GetRelatedTwinsByHops(Guid siteId, string id);
}

public class RelationshipMapService : IRelationshipMapService
{
    private readonly IAdtApiService _adtApiService;
    private readonly IAdxHelper _adxHelper;
    private readonly ISiteAdtSettingsProvider _siteAdtSettingsProvider;
    private readonly IMemoryCache _memoryCache;

    public RelationshipMapService(IAdtApiService adtApiService, IAdxHelper adxHelper, ISiteAdtSettingsProvider siteAdtSettingsProvider, IMemoryCache memoryCache)
    {
        _adtApiService = adtApiService;
        _adxHelper = adxHelper;
        _siteAdtSettingsProvider = siteAdtSettingsProvider;
        _memoryCache = memoryCache;
    }

    public async Task<TwinGraphDto> GetRelatedTwinsByHops(Guid siteId, string id)
    {
        ConcurrentBag<TwinNodeDto> twins = new ConcurrentBag<TwinNodeDto>();
        ConcurrentBag<TwinRelationshipDto> relationships = new ConcurrentBag<TwinRelationshipDto>();

        var twinWithrelationships = await GetTwinRelationshipsCached(siteId, id);

        // Return the original twin info if there is no relationship 
        if (!twinWithrelationships.Any())
        {
            var twin = await GetTwin(siteId, id);
            return new TwinGraphDto { Nodes = new TwinNodeDto[] { 
                                                        new TwinNodeDto { 
                                                            Id = twin.Id,
                                                            Name = twin.Contents.TryGetValue("name", out var value) ? value.ToString() : default,
                                                            ModelId = twin.Metadata.ModelId, 
                                                            EdgeInCount = 0,
                                                            EdgeOutCount = 0,
                                                            EdgeCount = 0 } }, 
                                            Edges = Array.Empty<TwinRelationshipDto>() };
        }

        var baseTwin = twinWithrelationships.First();
        twins.Add(baseTwin.MapToTwinNodeSimpleDto(twinWithrelationships.Count, 
                                                  twinWithrelationships.Count(r => r.TargetId == baseTwin.Id),
                                                  twinWithrelationships.Count(r => r.SourceId == baseTwin.Id)));

        foreach (var relationship in twinWithrelationships)
        {
            if (!twins.Any(t => t.Id == relationship.OpponentId))
            {
                twins.Add(relationship.MapToTwinNodeSimpleDto());
            }
            relationships.Add(relationship.MapToTwinRelationshipSimpleDto());
        }

        return new TwinGraphDto { Nodes = twins.ToArray(), Edges = relationships.ToArray() };
    }

    private async Task<List<RelationshipMapRelationship>> GetTwinRelationshipsCached(Guid siteId, string id)
    {
        var cacheKey = $"{nameof(GetTwinRelationshipsCached)}/{siteId}/{id}".ToLowerInvariant(); 
        return await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(30);
            return await GetRelatedTwinsAdx(siteId, id);
        });
    }

    private async Task<BasicDigitalTwin> GetTwin(Guid siteId, string twinId)
    {
        var siteAdtSettings = await _siteAdtSettingsProvider.GetForSiteAsync(siteId);
        return await _adtApiService.GetTwin(siteAdtSettings.InstanceSettings, twinId);
    }

    #region Using ADX
    private async Task<List<RelationshipMapRelationship>> GetRelatedTwinsAdx(Guid siteId, string twinId)
    {
        var siteAdtSettings = await _siteAdtSettingsProvider.GetForSiteAsync(siteId);

        var relationshipQuery = GetAllRelationshipsQuery(twinId);

        using var reader = await _adxHelper.Query(siteAdtSettings.AdxDatabase, relationshipQuery);
        return reader.Parse<RelationshipMapRelationship>().ToList();
    }

    private static string GetInRelationshipsQuery(string twinId)
    {
        return GetRelationshipsQuery(twinId, "TargetId", "SourceId");
    }

    private static string GetOutRelationshipsQuery(string twinId)
    {
        return GetRelationshipsQuery(twinId, "SourceId", "TargetId");
    }

    private static string GetFilteredRelationshipQuery()
    {
        var queryBuilder = new StringBuilder();
        queryBuilder.AppendLine("let FilteredRelationships = materialize(");
        queryBuilder.AppendLine($"{AdxConstants.ActiveRelationshipsFunction}");
        queryBuilder.AppendLine($"| project SourceId, TargetId, RelId = Id, RelName = Name, RelRaw = Raw");
        queryBuilder.AppendLine($"| where RelName!in ({string.Join(',', RelationshipMapConstants.SkipRelationshipNames.Select(x => $"\"{x}\""))}));");
        return queryBuilder.ToString();
    }

    private static string GetRelationshipCountQuery()
    {
        var queryBuilder = new StringBuilder();
        queryBuilder.AppendLine("let TwinRelationshipCount = materialize(");
        queryBuilder.AppendLine($"({AdxConstants.ActiveTwinsFunction}");
        queryBuilder.AppendLine($"| join kind = leftouter FilteredRelationships on $left.Id == $right.SourceId");
        queryBuilder.AppendLine($"| summarize Out = countif(isnotempty(TargetId)) by Id, Name, ModelId)");
        queryBuilder.AppendLine($"| join");
        queryBuilder.AppendLine($"({AdxConstants.ActiveTwinsFunction}");
        queryBuilder.AppendLine($"| join kind = leftouter FilteredRelationships on $left.Id == $right.TargetId");
        queryBuilder.AppendLine($"| summarize In = countif(isnotempty(SourceId)) by Id, Name, ModelId)");
        queryBuilder.AppendLine($"on $left.Id == $right.Id");
        queryBuilder.AppendLine($"| project OpponentId = Id, OpponentName = Name, OpponentModelId = ModelId, In, Out");
        queryBuilder.AppendLine($"| extend OpponentRelationshipCount = In + Out);");
        return queryBuilder.ToString();
    }

    private static string GetRelationshipsQuery(string twinId, string sourceOrTarget, string targetOrSource)
    {
        var queryBuilder = new StringBuilder();
        queryBuilder.AppendLine($"{AdxConstants.ActiveTwinsFunction}");
        queryBuilder.AppendLine($"| where Id == \"{twinId}\"");
        queryBuilder.AppendLine($"| project Id, Name, ModelId");
        queryBuilder.AppendLine($"| join FilteredRelationships");
        queryBuilder.AppendLine($"on $left.Id == $right.{sourceOrTarget}");
        queryBuilder.AppendLine($"| join TwinRelationshipCount");
        queryBuilder.AppendLine($"on $left.{targetOrSource} == $right.OpponentId");
        return queryBuilder.ToString();
    }

    private static string GetAllRelationshipsQuery(string twinId)
    {
        var queryBuilder = new StringBuilder();
        queryBuilder.AppendLine(GetFilteredRelationshipQuery());
        queryBuilder.AppendLine(GetRelationshipCountQuery());
        queryBuilder.AppendLine(GetInRelationshipsQuery(twinId));
        queryBuilder.AppendLine("| union");
        queryBuilder.AppendLine($"({GetOutRelationshipsQuery(twinId)})");
        return queryBuilder.ToString();
    }
    #endregion

    #region Using ADT
    private async Task<List<RelationshipMapRelationshipDto>> GetRelatedTwinsAdt(Guid siteId, string twinId)
    {
        return await GetRelatedTwinsAdt(siteId, $"SELECT target,rel from digitaltwins MATCH (source)-[rel]-(target) WHERE source.$dtId='{twinId}'", Direction.Out);
    }


    private async Task<List<RelationshipMapRelationshipDto>> GetRelatedTwinsAdt(Guid siteId, string query, Direction direction)
    {
        var siteAdtSettings = await _siteAdtSettingsProvider.GetForSiteAsync(siteId);

        var targetRelationships = new List<RelationshipMapRelationshipDto>();

        var result = await _adtApiService.QueryTwins<Dictionary<string, JsonDocument>>(siteAdtSettings.InstanceSettings, query).ToListAsync();

        foreach (var twinrel in result)
        {
            var target = twinrel["target"];
            var rel = twinrel["rel"];

            var twin = target.Deserialize<BasicDigitalTwin>()!;
            var relationship = rel.Deserialize<BasicRelationship>()!;

            if (RelationshipMapConstants.SkipRelationshipNames.Contains(relationship.Name, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            targetRelationships.Add(new RelationshipMapRelationshipDto
            {
                Id = relationship.Id,
                Name= relationship.Name,
                Direction = direction,
                TargetTwin = new RelationshipMapTwinDto { Id = twin.Id}
            });
        }

        return targetRelationships;
    }

    private async Task<List<RelationshipMapRelationshipDto>> GetTwinRelationshipsAsync(Guid siteId, string id)
    {
        var siteAdtSettings = await _siteAdtSettingsProvider.GetForSiteAsync(siteId);

        var relsTask = _adtApiService.GetRelationships(siteAdtSettings.InstanceSettings, id);
        var incomingRelsTask = _adtApiService.GetIncomingRelationships(siteAdtSettings.InstanceSettings, id);
        await Task.WhenAll(relsTask, incomingRelsTask);
        var result = RelationshipMapRelationshipDto.MapFrom(relsTask.Result).Concat(RelationshipMapRelationshipDto.MapFrom(incomingRelsTask.Result));

        return result.Where(r => !RelationshipMapConstants.SkipRelationshipNames.Contains(r.Name)).ToList();
    }
    #endregion
}
