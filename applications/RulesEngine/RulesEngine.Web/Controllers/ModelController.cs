using Abodit.Graph;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RulesEngine.Web;
using RulesEngine.Web.DTO;
using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Willow.Rules.DTO;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using Willow.Rules.Sources;
using WillowRules.DTO;

namespace Willow.Rules.Web.Controllers;

/// <summary>
/// Controller for models
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = nameof(CanViewModels))]
[ApiExplorerSettings(GroupName = "v1")]
public class ModelController : ControllerBase
{
    private readonly ILogger<ModelController> logger;
    private readonly WillowEnvironment willowEnvironment;
    private readonly IMemoryCache memoryCache;
    private readonly IModelService modelService;
    private readonly ITwinService twinService;
    private readonly ITwinGraphService twinGraphService;
    private readonly IMetaGraphService metaGraphService;

    /// <summary>
    /// Creates a new <see cref="ModelController"/>
    /// </summary>
    public ModelController(ILogger<ModelController> logger,
        WillowEnvironment willowEnvironment,
        IMemoryCache memoryCache,
        IModelService modelService,
        ITwinService twinService,
        ITwinGraphService twinGraphService,
        IMetaGraphService metaGraphService
        )
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
        this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        this.modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
        this.twinService = twinService ?? throw new ArgumentNullException(nameof(twinService));
        this.twinGraphService = twinGraphService ?? throw new ArgumentNullException(nameof(twinGraphService));
        this.metaGraphService = metaGraphService ?? throw new ArgumentNullException(nameof(metaGraphService));
    }

    /// <summary>
    /// Gets a single model by model id
    /// </summary>
    [HttpGet("Model", Name = "Model")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ModelDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetModel(string modelId)
    {
        logger.LogInformation($"GetModel {modelId}", modelId);
        var model = await this.modelService.GetSingleModelAsync(modelId);
        if (model is null) return NotFound();

        var ontology = await modelService.GetModelGraphCachedAsync();
        var modelNode = ontology.First(v => v.Id == model.Id);

        var successors = ontology.Successors<ModelData>(modelNode, Relation.RDFSType).Where(v => v != modelNode);

        return Ok(new ModelDto(model, successors, loadInheritedProperties: true));
    }

    /// <summary>
    /// Gets all models used in the twin
    /// </summary>
    /// <returns></returns>
    [HttpGet("Models", Name = "Models")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ModelDto[]))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetModels()
    {
        logger.LogInformation($"GetModels");
        var models = await this.modelService.GetModelsCachedAsync();

        var ontology = await modelService.GetModelGraphCachedAsync();
        var ontologyLookup = ontology.ToDictionary(v => v.Id);

        var result = models.AsParallel().Select(v =>
        {
            if (ontologyLookup.TryGetValue(v.Id, out var modelNode))
            {
                var successors = ontology.Successors<ModelData>(modelNode, Relation.RDFSType).Where(v => v != modelNode);

                return new ModelDto(v, successors);
            }

            return new ModelDto(v);
        }).ToArray();

        return Ok(result);
    }

    /// <summary>
    /// Gets a graph of the ontology with inheritance relationships between models
    /// </summary>
    /// <returns>A graph of the ontology</returns>
    [HttpPost("Ontology", Name = "Ontology")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BatchDtoModelSimpleGraphDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetOntology(string modelId, BatchRequestDto request)
    {
        logger.LogInformation("GetOntology");

        var result = await this.metaGraphService.GetOntologyWithCountsCached(modelId, new ProgressTrackerDummy());

        if (result is null) return NotFound("Run caching before this");

        var batch = GetBatch(result, request);

        return Ok(new BatchDtoModelSimpleGraphDto(batch, result.Relationships));
    }

    /// <summary>
    /// Exports the models
    /// </summary>
    [HttpPost("ExportOntology", Name = "ExportOntology")]
    [FileResultContentType("text/csv")]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> ExportOntology(string modelId, BatchRequestDto request)
    {
        var result = await this.metaGraphService.GetOntologyWithCountsCached(modelId, new ProgressTrackerDummy());

        if (result is null) return NotFound("Run caching before this");

        var batch = GetBatch(result, request);

        return WebExtensions.CsvResult(batch.Items.Select(v =>
        new
        {
            Model = v.ModelId,
            Name = v.Label,
            Units = string.Join(",", v.Units),
            Exact = v.Count,
            v.Total
        }), $"Models_{modelId}.csv");
    }

    /// <summary>
    /// Gets the metagraph which can be used for autcomplete and other purposes
    /// </summary>
    [HttpGet("ModelsAutocomplete", Name = "ModelsAutocomplete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ModelSimpleGraphDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetModelsAutocomplete()
    {
        logger.LogInformation("GetModelsAutocomplete");
        var result = await this.metaGraphService.GetOntologyWithCountsCached(null, new ProgressTrackerDummy());

        if (result is null)
        {
            logger.LogInformation($"No result from {nameof(MetaGraphService.GetMetaGraphDtoCached)}");
            return UnprocessableEntity("Something went wrong");
        }

        logger.LogInformation($"GetModelsAutoComplete found {result.Nodes.Length}, {result.Relationships.Length}");

        return Ok(result);
    }

    /// <summary>
    /// Gets the system graph for a single model from the meta graph
    /// </summary>
    [HttpGet("ModelSystemGraph", Name = "ModelSystemGraph")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ModelSimpleGraphDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetModelSystemGraph(string modelId)
    {
        logger.LogInformation("API: GetModelSystemGraph {modelId}", modelId);
        var result = await metaGraphService.GetModelSystemGraphCachedAsync(willowEnvironment, modelId, new ProgressTrackerDummy());
        if (result is null)
        {
            logger.LogInformation($"No result from {nameof(MetaGraphService.GetModelSystemGraphCachedAsync)}");
            return UnprocessableEntity("Something went wrong");
        }
        logger.LogInformation("API: GetModelSystemGraph {modelId} {nodes} {edges}", modelId, result.Nodes.Count(), result.Relationships.Count());
        return Ok(result);
    }

    private Batch<ModelSimpleDto> GetBatch(ModelSimpleGraphDto result, BatchRequestDto request)
    {
        var queryable = result.Nodes.AsQueryable().Where(v => !v.ModelId.Contains("rec_3_3"));

        Expression<Func<ModelSimpleDto, bool>> whereExpression = null;

        foreach (var filterSpecification in request.FilterSpecifications)
        {
            var getExpression = (FilterSpecificationDto filter) =>
            {
                switch (filter.field)
                {
                    case nameof(ModelSimpleDto.ModelId):
                        {
                            return filter.CreateExpression((ModelSimpleDto v) => v.ModelId, filter.ToString(null), isSql: false);
                        }
                    case nameof(ModelSimpleDto.Label):
                        {
                            return filter.CreateExpression((ModelSimpleDto v) => v.Label, filter.ToString(null), isSql: false);
                        }
                    case nameof(ModelSimpleDto.Units):
                        {
                            return filter.CreateExpression((ModelSimpleDto v) => string.Join("", v.Units), filter.ToString(null), isSql: false);
                        }
                    case nameof(ModelSimpleDto.Count):
                        {
                            return filter.CreateExpression((ModelSimpleDto v) => v.Count, filter.ToInt32(null));
                        }
                    case nameof(ModelSimpleDto.CountInherited):
                        {
                            return filter.CreateExpression((ModelSimpleDto v) => v.CountInherited, filter.ToInt32(null));
                        }
                    case nameof(ModelSimpleDto.Total):
                        {
                            return filter.CreateExpression((ModelSimpleDto v) => v.Total, filter.ToInt32(null));
                        }
                    case nameof(ModelSimpleDto.IsCapability):
                        {
                            return filter.CreateExpression((ModelSimpleDto v) => v.IsCapability, filter.ToBoolean(null));
                        }
                    default:
                        {
                            throw new InvalidOperationException($"Filter for {filter.field} is invalid for type {nameof(ModelSimpleDto)}");
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

        IOrderedQueryable<ModelSimpleDto> sortResult = null;

        foreach (var sortSpecification in request.SortSpecifications)
        {
            switch (sortSpecification.field)
            {
                case nameof(ModelSimpleDto.ModelId):
                    {
                        sortResult = queryable.AddSort(sortResult!, first, x => x.ModelId, sortSpecification.sort);
                        break;
                    }
                case nameof(ModelSimpleDto.Label):
                    {
                        sortResult = queryable.AddSort(sortResult!, first, x => x.Label, sortSpecification.sort);
                        break;
                    }
                case nameof(ModelSimpleDto.Count):
                    {
                        sortResult = queryable.AddSort(sortResult!, first, x => x.Count, sortSpecification.sort);
                        break;
                    }
                case nameof(ModelSimpleDto.CountInherited):
                    {
                        sortResult = queryable.AddSort(sortResult!, first, x => x.CountInherited, sortSpecification.sort);
                        break;
                    }
                case nameof(ModelSimpleDto.Total):
                    {
                        sortResult = queryable.AddSort(sortResult!, first, x => x.Total, sortSpecification.sort);
                        break;
                    }
                case nameof(ModelSimpleDto.Units):
                    {
                        sortResult = queryable.AddSort(sortResult!, first, x => string.Join("", x.Units), sortSpecification.sort);
                        break;
                    }
                case nameof(ModelSimpleDto.IsCapability): // Valid
                    {
                        sortResult = queryable.AddSort(sortResult!, first, x => x.IsCapability, sortSpecification.sort);
                        break;
                    }
                default:
                    {
                        throw new InvalidOperationException($"Sort for {sortSpecification.field} is invalid for type {nameof(ModelSimpleDto)}");
                    }
            }

            first = false;
        }

        var total = queryable.Count();

        queryable = sortResult ?? queryable;

        queryable = queryable.Page(request.Page, request.PageSize, out int skipped);

        int countBefore = skipped;
        int countAfter = total - skipped;
        return new Batch<ModelSimpleDto>("", countBefore, countAfter, total, queryable, "");
    }
}
