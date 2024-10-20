using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RulesEngine.Web;
using RulesEngine.Web.DTO;
using Willow.Expressions;
using Willow.Rules.Model;
using Willow.Rules.Repository;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace Willow.Rules.Web.Controllers;

/// <summary>
/// Controller for calculated points
/// </summary>
/// <remarks>
/// After changing any method here you must run the APIClientGenerator project to update the tyepscript client
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = nameof(CanViewRules))]
[ApiExplorerSettings(GroupName = "v1")]
public class CalculatedPointController : ControllerBase
{
    private readonly ILogger<CalculatedPointController> logger;
    private readonly IRepositoryCalculatedPoint repositoryCalculatedPoints;

    /// <summary>
    /// Creates a new <see cref="CalculatedPointController"/>
    /// </summary>
    public CalculatedPointController(
        ILogger<CalculatedPointController> logger,
        IRepositoryCalculatedPoint repositoryCalculatedPoints)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.repositoryCalculatedPoints = repositoryCalculatedPoints ?? throw new ArgumentNullException(nameof(repositoryCalculatedPoints));
    }

    /// <summary>
    /// Gets a rule instance by id
    /// </summary>
    [HttpGet("CalculatedPoint", Name = "getCalculatedPoint")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CalculatedPointDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetCalculatedPoint(string id)
    {
        var combined = await this.repositoryCalculatedPoints.GetOneCombined(id);
        if (combined is null) return NotFound(id);
        var result = new CalculatedPointDto(combined.Value.calculatedPoint, combined.Value.instance, combined.Value.metadata, combined.Value.actor);
        return Ok(result);
    }

    /// <summary>
    /// Get calculated point after a given starting point
    /// </summary>
    [Route("calculatedPointsafter/{id}", Name = "GetCalculatedPointsAfter")]
    [HttpPost]
    [Produces(typeof(BatchDto<CalculatedPointDto>))]
    public async Task<IActionResult> GetCalculatedPointsAfter([FromRoute] string id, BatchRequestDto request)
    {
        var batch = await GetCalculatedPointsBatch(id, request);

        return Ok(batch);
    }

    /// <summary>
    /// Exports calculated point after a given starting point
    /// </summary>
    [Route("exportCalculatedPointsafter", Name = "ExportCalculatedPointsAfter")]
    [HttpPost]
    [FileResultContentType("text/csv")]
    public async Task<IActionResult> ExportCalculatedPointsAfter(string id, BatchRequestDto request)
    {
        var batch = await GetCalculatedPointsBatch(id, request);

        return WebExtensions.CsvResult(batch.Items.Select(v =>
        {
            dynamic expando = new ExpandoObject();

            expando.Valid = v.Valid;
            expando.TriggerCount = v.TriggerCount;
            expando.Id = v.Id;
            expando.Name = v.Name;
            expando.Expression = v.ValueExpression;
            expando.TrendId = v.TrendId;
            expando.TrendInterval = v.TrendInterval;
            expando.Unit = v.Unit;
            expando.ModelId = v.ModelId;
            expando.IsCapabilityOf = v.IsCapabilityOf;
            expando.ExternalId = v.ExternalId;
            expando.RuleId = v.RuleId;
            expando.TimeZone = v.TimeZone;
            expando.Type = Enum.GetName(typeof(UnitOutputType), v.Type);
            expando.Source = Enum.GetName(typeof(CalculatedPointSource), v.Source);
            expando.Action = Enum.GetName(typeof(ADTActionRequired), v.ActionRequired);
            expando.Status = Enum.GetName(typeof(ADTActionStatus), v.ActionStatus);

            var expandoLookup = (IDictionary<string, object>)expando;

            foreach (var location in v.TwinLocations.GroupLocationsByModel())
            {
                expandoLookup[location.Key] = location.Value;
            }

            return expando;
        }), "CalculatedPoints.csv");
    }

    private async Task<BatchDto<CalculatedPointDto>> GetCalculatedPointsBatch(string id, BatchRequestDto request)
    {
        var batch = await this.repositoryCalculatedPoints.GetAllCombined(
            request.SortSpecifications,
            request.FilterSpecifications,
            id == "all" ? null : f => f.RuleId == id,
            request.Page,
            request.PageSize);

        var batch2 = batch.Transform((v) => new CalculatedPointDto(v.calculatedPoint, v.instance, v.metadata, v.actor));

        return new BatchDto<CalculatedPointDto>(batch2);
    }
}
