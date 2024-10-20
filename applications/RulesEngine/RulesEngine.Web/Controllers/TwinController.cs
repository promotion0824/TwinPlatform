using Abodit.Graph;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RulesEngine.Web;
using RulesEngine.Web.DTO;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Willow.Rules.Configuration;
using Willow.Rules.DTO;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using WillowRules.DTO;
using WillowRules.Extensions;

namespace Willow.Rules.Web.Controllers;

/// <summary>
/// Controller for twins
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = nameof(CanViewTwins))]
[ApiExplorerSettings(GroupName = "v1")]
public class TwinController : ControllerBase
{
    private readonly ILogger<TwinController> logger;
    private readonly IRepositoryRuleInstances repositoryRuleInstances;
    private readonly IRepositoryInsight repositoryInsight;
    private readonly ITwinService twinService;
    private readonly ITwinSystemService twinSystemService;
    private readonly IRepositoryTimeSeriesBuffer repositoryTimeSeriesBuffer;
    private readonly IAuthorizationService authorizationService;
    private readonly IModelService modelService;

    /// <summary>
    /// Creates a new <see cref="TwinController"/>
    /// </summary>
    public TwinController(IRepositoryRuleInstances repositoryRuleInstances,
        IRepositoryInsight repositoryInsight,
        ITwinService twinService,
        ITwinSystemService twinSystemService,
        IRepositoryTimeSeriesBuffer repositoryTimeSeriesBuffer,
        IAuthorizationService authorizationService,
        IModelService modelService,
        ILogger<TwinController> logger
        )
    {
        this.repositoryRuleInstances = repositoryRuleInstances ?? throw new ArgumentNullException(nameof(repositoryRuleInstances));
        this.repositoryInsight = repositoryInsight ?? throw new ArgumentNullException(nameof(repositoryInsight));
        this.twinService = twinService ?? throw new ArgumentNullException(nameof(twinService));
        this.modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
        this.twinSystemService = twinSystemService ?? throw new ArgumentNullException(nameof(twinSystemService));
        this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        this.repositoryTimeSeriesBuffer = repositoryTimeSeriesBuffer ?? throw new ArgumentNullException(nameof(repositoryTimeSeriesBuffer));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get twins by model
    /// </summary>
    /// <remarks>
    /// This returns twins found using inheritance so will not be the
    /// same as the count show in the UI for the list of models and counts
    /// </remarks>
    /// <param name="modelId">The id of the model beginning dtmi:</param>
    /// <param name="request">The filter/sorting detaiuls for the result</param>
    [HttpPost("TwinsByModel", Name = "TwinsByModel")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TwinDtoBatchDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetTwinsByModel(string modelId, BatchRequestDto request)
    {
        var result = await this.twinService.GetTwinsByModelWithInheritance(modelId);

        var batchResult = GetBatch(result, request);

        return Ok(batchResult);
    }

    /// <summary>
    /// Exports the twins by model
    /// </summary>
    [HttpPost("ExportTwinsByModel", Name = "ExportTwinsByModel")]
    [FileResultContentType("text/csv")]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> ExportTwinsByModel(string modelId, BatchRequestDto request)
    {
        var result = await this.twinService.GetTwinsByModelWithInheritance(modelId);

        var batchResult = GetBatch(result, request);

        return WebExtensions.CsvResult(batchResult.Items.Select(v =>
        {
            dynamic expando = new ExpandoObject();

            expando.Name = v.Name;
            expando.Id = v.Id;
            expando.ModelId = v.ModelId;
            expando.Position = v.Position;
            expando.Description = v.Description;
            expando.Unit = v.Unit;
            expando.ExternalID = v.ExternalID;
            expando.ConnectorID = v.ConnectorID;
            expando.TrendID = v.TrendID;
            expando.TrendInterval = v.TrendInterval;
            expando.ValueExpression = v.ValueExpression;

            var expandoLookup = (IDictionary<string, object>)expando;

            foreach (var name in batchResult.ContentTypes)
            {
                if (v.Contents.TryGetValue(name.Name, out var value))
                {
                    expandoLookup[name.Name] = value;
                }
                else
                {
                    expandoLookup[name.Name] = null;
                }
            }

            expando.Contents = JsonConvert.SerializeObject(v.Contents);

            foreach (var location in v.Locations.GroupLocationsByModel())
            {
                expandoLookup[location.Key] = location.Value;
            }

            return expando;
        }), $"TwinsForModel_{modelId}.csv");
    }

    /// <summary>
    /// Gets subgraphs around each specified node in the list
    /// </summary>
    /// <param name="twinIds">An array of ids to find, usually current, previous, previous to that</param>
    /// <returns>A graph of nodes and edges, possibly disconnected</returns>
    [HttpPost("TwinsGraph", Name = "TwinsGraph")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TwinGraphDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetTwinGraph([FromBody] string[] twinIds)
    {
        logger.LogInformation($"GetTwinGraph {string.Join(", ", twinIds)}");
        var graph = await twinSystemService.GetTwinSystemGraph(twinIds);

        var result = TwinGraphDto.From(graph, twinIds);
        return Ok(result);
    }

    /// <summary>
    /// Gets a twin and related information (using the old cached twin approach)
    /// </summary>
    /// <param name="equipmentId"></param>
    /// <returns>An augmented twin with relationships</returns>
    [HttpGet("EquipmentWithRelationships", Name = "EquipmentWithRelationships")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EquipmentDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetEquipmentWithRelationships(string equipmentId)
    {
        logger.LogInformation("Get equipment {equipmentId}", equipmentId);

        try
        {
            if (equipmentId == "all")
            {
                List<BasicDigitalTwinPoco> topLevelEntites = await twinService.GetTopLevelEntities();
                if (!topLevelEntites.Any())
                {
                    var count = twinService.CountCached();
                    logger.LogWarning("Did not find any top-level entities (total count {count})", count);
                    return NotFound($"Did not find any top-level entities (count {count})");
                }
                equipmentId = topLevelEntites[0].Id;
            }

            var (twin, forward, reverse) = await twinService.GetDigitalTwinWithRelationshipsAsync(equipmentId);

            if (twin is null)
            {
                logger.LogWarning("Did not find {equipmentId}", equipmentId);
                return NotFound($"Did not find {equipmentId}");
            }

            var graph = await twinSystemService.GetTwinSystemGraph([twin.Id]);
            var twinContext = TwinDataContext.Create(twin, graph);
            var timeZone = string.IsNullOrEmpty(twinContext.TimeZone) ? TimeZoneInfo.Utc.Id : twinContext.TimeZone;

            var ontology = await modelService.GetModelGraphCachedAsync();
            var modelNode = ontology.First(v => v.Id == twin.ModelId());
            var successors = ontology.Successors<ModelData>(modelNode, Relation.RDFSType);
            var propertyUsage = twin.GetPropertyUsage(ontology, ontology.ToDictionary(v => v.DtdlModel.id)).ToList();

            var propertyDtos = propertyUsage.Select(v => new ModelPropertyDto(v.model.Id, v.property)
            {
                PropertyName = $"{v.propertyPath}.{v.property.name}".Trim('.')
            }).ToArray();

            var twinpoco = new TwinDto(twin);

            TimeSeriesStatus? capabilityStatus = null;

            //if it is a capability, get the status
            var timeseries = await repositoryTimeSeriesBuffer.GetByTwinId(equipmentId);

            if (timeseries is not null)
            {
                capabilityStatus = timeseries.GetStatus();
            }

            var result = new EquipmentDto
            {
                EquipmentId = equipmentId,
                Name = twinpoco.Name,
                Description = twinpoco.Description,
                RelatedEntities =
                    forward.Select(x => new RelatedEntityDto(x.Destination.name, x.Destination.Id, x.Destination.Metadata.ModelId, x.RelationshipType, x.Substance, x.Destination.unit)).ToArray(),
                InverseRelatedEntities =
                    reverse.Select(x => new RelatedEntityDto(x.Destination.name, x.Destination.Id, x.Destination.Metadata.ModelId, x.RelationshipType, x.Substance, x.Destination.unit)).ToArray(),

                Capabilities = reverse.Where(r => r.RelationshipType == "isCapabilityOf")
                    .Select(x => new CapabilityDto(x.Destination.name, x.Destination.Id,
                         x.Destination.Metadata.ModelId,
                         "isCapabilityOf",
                         x.Destination.unit,
                        x.Destination.tags is null ? "" : string.Join(",", x.Destination.tags.Keys)))
                    .Append(new CapabilityDto(twinpoco.Name, twinpoco.Id, twinpoco.ModelId, "self", twinpoco.Unit,
                        twinpoco.tags is null ? "" : string.Join(" ", twinpoco.tags.Keys)))
                    .DistinctBy(dto => dto.id)
                    .ToArray(),

                ModelId = twinpoco.ModelId,
                Tags = twinpoco.tags is null ? "" : string.Join(", ", twinpoco.tags.Keys),
                Unit = twinpoco.Unit,
                TrendInterval = twinpoco.TrendInterval,
                ValueExpression = twinpoco.ValueExpression,
                SiteId = twinpoco.SiteId,
                EquipmentUniqueId = twinpoco.EquipmentUniqueId,
                ExternalId = twinpoco.ExternalID,
                ConnectorId = twinpoco.ConnectorID,
                TrendId = twinpoco.TrendID,
                Timezone = timeZone,
                Contents = twinpoco.Contents,
                CapabilityStatus = capabilityStatus,
                LastUpdatedOn = twinpoco.LastUpdatedOn,
                Properties = propertyDtos,
                Locations = twinpoco.Locations,
                //For now we can use the ConnectorID as all calc points use the same
                IsCalculatedPointTwin = !string.IsNullOrWhiteSpace(twinpoco.ConnectorID) && twinpoco.ConnectorID == EventHubSettings.RulesEngineConnectorId
            };
            return Ok(result);
        }
        catch (NullReferenceException)
        {
            return NotFound($"Did not find digital twin {equipmentId}");
        }
        catch (Azure.Identity.AuthenticationFailedException)
        {
            logger.LogError("Azure.Identity.AuthenticationFailedException - non-recoverable, if running locally try AZ LOGIN again");
            return StatusCode(StatusCodes.Status500InternalServerError, "Azure.Identity.AuthenticationFailedException");
        }
    }

    /// <summary>
    /// Gets rule instances for a twin
    /// </summary>
    /// <param name="twinId"></param>
    /// <param name="request">A filter for the result</param>
    /// <returns>Related instances</returns>
    [HttpPost("EquipmentRuleInstances", Name = "EquipmentRuleInstances")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BatchDto<RuleInstanceDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetEquipmentRuleInstances(string twinId, BatchRequestDto request)
    {
        var batch = await GetEquipmentRuleInstancesBatch(twinId, request);

        return Ok(batch);
    }

    /// <summary>
    /// Get the insights count per twin for a given graph
    /// </summary>
    /// <returns>An array of insights count per twin id</returns>
    [HttpPost("GetInsightsCountForTwinGraph", Name = "GetInsightsCountForTwinGraph")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TwinInsightsCountDto[]))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetInsightsCountForTwinGraph(string[] twinIds)
    {
        var graph = await twinSystemService.GetTwinSystemGraph(twinIds);

        var result = TwinGraphDto.From(graph, twinIds);

        var results = new List<TwinInsightsCountDto>();

        var nodeTwinIds = result.Nodes
            .Select(v => v.TwinId)
            .ToHashSet();

        foreach ((string twinId, int count) in await repositoryInsight.GetEquipmentIds())
        {
            if (nodeTwinIds.Contains(twinId))
            {
                results.Add(new TwinInsightsCountDto(twinId, count));
            }
        }

        return Ok(results);
    }

    /// <summary>
    /// Exports rule instances for a twin
    /// </summary>
    [HttpPost("ExportEquipmentRuleInstances", Name = "ExportEquipmentRuleInstances")]
    [FileResultContentType("text/csv")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> ExportEquipmentRuleInstances(string twinId, BatchRequestDto request)
    {
        var batch = await GetEquipmentRuleInstancesBatch(twinId, request);

        // Convert the IEnumerable to IAsyncEnumerable
        async IAsyncEnumerable<dynamic> ConvertData()
        {
            foreach (var v in batch.Items)
            {
                dynamic expando = new ExpandoObject();

                expando.Id = v.Id;
                expando.EquipmentId = v.EquipmentId;
                expando.EquipmentName = v.EquipmentName;
                expando.Valid = v.Valid;
                expando.Status = v.Status;
                expando.Enabled = !v.Disabled;
                expando.Received = v.TriggerCount;

                var expandoLookup = (IDictionary<string, object>)expando;

                foreach (var location in v.Locations.GroupLocationsByModel())
                {
                    expandoLookup[location.Key] = location.Value;
                }

                yield return expando;
            }
        }

        return await WebExtensions.CsvResultWithDynamicHeaders(ConvertData(), $"SkillInstancesForEquipment_{twinId}.csv");
    }

    private async Task<BatchDto<RuleInstanceDto>> GetEquipmentRuleInstancesBatch(string twinId, BatchRequestDto request)
    {
        logger.LogInformation("Get rule instances for twin {twinId}", twinId);

        //add quotes around the value, we are searching json
        var likeValue = $"%\"{twinId}\"%";

        Expression<Func<RuleInstance, bool>> whereExpression = (ri) => true;

        if (twinId != "all")
        {
            //gets direct instances for equipment, but also indirect instances for sensors which is a useful shortcut when looking at a capability
            whereExpression = (ri) => ri.Id == twinId || ri.EquipmentId == twinId || EF.Functions.Like((string)(object)ri.TwinLocations, likeValue) || EF.Functions.Like((string)(object)ri.PointEntityIds, likeValue);
        }

        var batch = await this.repositoryRuleInstances.GetAllCombined(
            request.SortSpecifications,
            request.FilterSpecifications,
            whereExpression: whereExpression,
            page: request.Page,
            take: request.PageSize);

        var result = new List<RuleInstanceDto>();

        foreach ((var ruleInstance, var metadata) in batch.Items)
        {
            var auth = await authorizationService.CanViewRule(User, ruleInstance);

            result.Add(new RuleInstanceDto(ruleInstance, metadata, auth));
        }

        var resultBatch = batch.Transform(result);

        return new BatchDto<RuleInstanceDto>(resultBatch);
    }

    private TwinDtoBatchDto GetBatch(IEnumerable<BasicDigitalTwinPoco> result, BatchRequestDto request)
    {
        var contentTypes = new Dictionary<string, TwinDtoContentType>();

        foreach ((var key, var content) in result.Where(v => v.Contents is not null).SelectMany(v => v.Contents))
        {
            if (key != "TagString" && content is not null)
            {
                if (contentTypes.ContainsKey(key))
                {
                    continue;
                }

                var valueType = content.GetType();

                var isBool = valueType == typeof(bool);
                var isNumber = valueType == typeof(double);
                var isString = valueType == typeof(string);

                if (isBool || isNumber || isString)
                {
                    var twinDtoContentType = new TwinDtoContentType()
                    {
                        Name = key,
                        IsBool = isBool,
                        IsNumber = isNumber,
                        IsString = isString
                    };

                    contentTypes.Add(key, twinDtoContentType);
                }
            }
        }

        var queryable = result.AsQueryable();

        Expression<Func<BasicDigitalTwinPoco, bool>> whereExpression = null;

        foreach (var filterSpecification in request.FilterSpecifications)
        {
            var getExpression = (FilterSpecificationDto filter) =>
            {
                switch (filter.field)
                {
                    case nameof(BasicDigitalTwinPoco.name):
                        {
                            return filter.CreateExpression((BasicDigitalTwinPoco v) => v.name, filter.ToString(null), isSql: false);
                        }
                    case nameof(BasicDigitalTwinPoco.Id):
                        {
                            return filter.CreateExpression((BasicDigitalTwinPoco v) => v.Id, filter.ToString(null), isSql: false);
                        }
                    case nameof(BasicDigitalTwinPoco.ModelId):
                        {
                            return filter.CreateExpression((BasicDigitalTwinPoco v) => v.ModelId(), filter.ToString(null), isSql: false);
                        }
                    case nameof(BasicDigitalTwinPoco.position):
                        {
                            return filter.CreateExpression((BasicDigitalTwinPoco v) => v.position, filter.ToString(null), isSql: false);
                        }
                    case nameof(BasicDigitalTwinPoco.description):
                        {
                            return filter.CreateExpression((BasicDigitalTwinPoco v) => v.description, filter.ToString(null), isSql: false);
                        }
                    case nameof(BasicDigitalTwinPoco.unit):
                        {
                            return filter.CreateExpression((BasicDigitalTwinPoco v) => v.unit, filter.ToString(null), isSql: false);
                        }
                    case nameof(BasicDigitalTwinPoco.TimeZone):
                        {
                            return filter.CreateExpression((BasicDigitalTwinPoco v) => v.TimeZoneName(), filter.ToString(null), isSql: false);
                        }
                    default:
                        {
                            if (contentTypes.TryGetValue(filter.field, out var contentType))
                            {
                                if (contentType.IsBool)
                                {
                                    return filter.CreateExpression((BasicDigitalTwinPoco v) => v.Contents.GetValueOrDefault(filter.field) as bool?, filter.ToBoolean(null), isSql: false);
                                }

                                if (contentType.IsNumber)
                                {
                                    return filter.CreateExpression((BasicDigitalTwinPoco v) => v.Contents.GetValueOrDefault(filter.field) as double?, filter.ToDouble(null), isSql: false);
                                }

                                if (contentType.IsString)
                                {
                                    return filter.CreateExpression((BasicDigitalTwinPoco v) => v.Contents.GetValueOrDefault(filter.field) as string, filter.ToString(null), isSql: false);
                                }
                            }


                            throw new InvalidOperationException($"Filter for {filter.field} is invalid for type {nameof(BasicDigitalTwinPoco)}");
                        }
                }
            };

            var expression = getExpression(filterSpecification);

            whereExpression = whereExpression is null ? expression : whereExpression.And(expression);
        }

        if (whereExpression is not null)
        {
            queryable = queryable.Where(whereExpression);
        }

        bool first = true;

        IOrderedQueryable<BasicDigitalTwinPoco> sortResult = null;

        foreach (var sortSpecification in request.SortSpecifications)
        {
            switch (sortSpecification.field)
            {
                case nameof(BasicDigitalTwinPoco.name):
                    {
                        sortResult = queryable.AddSort(sortResult!, first, x => x.name, sortSpecification.sort);
                        break;
                    }
                case nameof(BasicDigitalTwinPoco.Id):
                    {
                        sortResult = queryable.AddSort(sortResult!, first, x => x.Id, sortSpecification.sort);
                        break;
                    }
                case nameof(BasicDigitalTwinPoco.ModelId):
                    {
                        sortResult = queryable.AddSort(sortResult!, first, x => x.ModelId(), sortSpecification.sort);
                        break;
                    }
                case nameof(BasicDigitalTwinPoco.position):
                    {
                        sortResult = queryable.AddSort(sortResult!, first, x => x.position, sortSpecification.sort);
                        break;
                    }
                case nameof(BasicDigitalTwinPoco.description):
                    {
                        sortResult = queryable.AddSort(sortResult!, first, x => x.description, sortSpecification.sort);
                        break;
                    }
                case nameof(BasicDigitalTwinPoco.unit):
                    {
                        sortResult = queryable.AddSort(sortResult!, first, x => x.unit, sortSpecification.sort);
                        break;
                    }
                case nameof(BasicDigitalTwinPoco.TimeZone):
                    {
                        sortResult = queryable.AddSort(sortResult!, first, x => x.TimeZoneName(), sortSpecification.sort);
                        break;
                    }
                default:
                    {
                        sortResult = queryable.AddSort(sortResult!, first, x => x.Contents.GetValueOrDefault(sortSpecification.field), sortSpecification.sort);
                        break;
                    }
            }

            first = false;
        }

        var total = queryable.Count();

        queryable = sortResult ?? queryable;

        queryable = queryable.Page(request.Page, request.PageSize, out int skipped);

        int countBefore = skipped;
        int countAfter = total - skipped;

        var batch = new Batch<BasicDigitalTwinPoco>("", countBefore, countAfter, total, queryable, "");

        var batchResult = batch.Transform(v => new TwinDto(v));

        return new TwinDtoBatchDto(batchResult, contentTypes.Values.ToList());
    }
}
