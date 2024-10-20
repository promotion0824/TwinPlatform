using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RulesEngine.Web;
using RulesEngine.Web.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.Configuration;
using Willow.Rules.DTO;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using Willow.Rules.Sources;
using Willow.ServiceBus;
using WillowRules.DTO;
using WillowRules.Extensions;
using WillowRules.Logging;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace Willow.Rules.Web.Controllers;

/// <summary>
/// Controller for insights
/// </summary>
/// <remarks>
/// After changing any method here you must run the APIClientGenerator project to update the tyepscript client
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = nameof(CanViewInsights))]
[ApiExplorerSettings(GroupName = "v1")]
public class InsightsController : ControllerBase
{
    private readonly ILogger<InsightsController> logger;
    private readonly IAuditLogger<InsightsController> auditLogger;
    private readonly IRepositoryInsight repositoryInsight;
    private readonly IRepositoryADTSummary repositoryADTSummary;
    private readonly IRepositoryInsightChange repositoryInsightChange;
    private readonly IRepositoryRules repositoryRules;
    private readonly ICommandInsightService commandInsightService;
    private readonly IFileService fileService;
    private readonly WillowEnvironment willowEnvironment;
    private readonly IMessageSenderFrontEnd messageSender;
    private readonly string commandUrl;

    /// <summary>
    /// Creates a new <see cref="InsightsController"/>
    /// </summary>
    public InsightsController(
        ILogger<InsightsController> logger,
        IAuditLogger<InsightsController> auditLogger,
        IRepositoryInsight repositoryInsight,
        IRepositoryInsightChange repositoryInsightChange,
        IRepositoryRules repositoryRules,
        IRepositoryADTSummary repositoryADTSummary,
        ICommandInsightService commandInsightService,
        IFileService fileService,
        WillowEnvironment willowEnvironment,
        IMessageSenderFrontEnd messageSender,
        IOptions<CustomerOptions> options)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
        this.repositoryInsight = repositoryInsight ?? throw new ArgumentNullException(nameof(repositoryInsight));
        this.repositoryInsightChange = repositoryInsightChange ?? throw new ArgumentNullException(nameof(repositoryInsightChange));
        this.repositoryRules = repositoryRules ?? throw new ArgumentNullException(nameof(repositoryRules));
        this.repositoryADTSummary = repositoryADTSummary ?? throw new ArgumentNullException(nameof(repositoryADTSummary));
        this.commandInsightService = commandInsightService ?? throw new ArgumentNullException(nameof(commandInsightService));
        this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
        this.messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        this.commandUrl = options?.Value?.WillowCommandUrl;
    }

    /// <summary>
    /// Get the insights that have been created for a specific equipment item (or location)
    /// </summary>
    /// <returns>An array of insights (non-paged)</returns>
    [HttpPost("InsightsForEquipment", Name = "InsightsForEquipment")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InsightDtoBatchDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetInsightsForEquipment(string equipmentId, BatchRequestDto request)
    {
        var batch = await GetInsightsForEquipmentBatch(equipmentId, request);

        return Ok(batch);
    }

    /// <summary>
    /// Get insight status history
    /// </summary>
    /// <returns>An array of insight status</returns>
    [HttpPost("InsightStatusHistory", Name = "InsightStatusHistory")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InsightStatusDto[]))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetInsightStatusHistory(string id)
    {
        var changes = await repositoryInsightChange.Get(v => v.InsightId == id);

        return Ok(changes.OrderByDescending(v => v.Timestamp).Select(v => new InsightStatusDto(v)).ToArray());
    }

    /// <summary>
    /// Exports the insights that have been created for a specific equipment item (or location)
    /// </summary>
    [HttpPost("ExportInsightsForEquipment", Name = "ExportInsightsForEquipment")]
    [FileResultContentType("text/csv")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> ExportInsightsForEquipment(string equipmentId, BatchRequestDto request)
    {
        var batch = await GetInsightsForEquipmentBatch(equipmentId, request);

        return await CsvResult(batch.Items, $"InsightsForEquipment_{equipmentId}.csv");
    }

    private async Task<InsightDtoBatchDto> GetInsightsForEquipmentBatch(string equipmentId, BatchRequestDto request)
    {
        var filters = request.FilterSpecifications.ToList();

        //lambda filtering is not possbile because where clause is embedded into the pivot's where
        filters.Add(new FilterSpecificationDto()
        {
            field = nameof(Insight.EquipmentId),
            value = equipmentId,
            @operator = FilterSpecificationDto.EqualsLiteral
        });

        var batch = await this.repositoryInsight.GetAll(
            request.SortSpecifications,
            filters.ToArray(),
            page: request.Page,
            take: request.PageSize);

        var batchDto = batch.Transform((v) => new InsightDto(v, commandUrl));

        var impactScoreNames = await this.repositoryInsight.GetImpactScoreNames();

        return new InsightDtoBatchDto(batchDto, impactScoreNames.Select(v => new InsightImpactSummaryDto(v.Name, v.FieldId, v.Units, v.Count)).ToList());
    }

    /// <summary>
    /// Get one Insight but with additional fields from the Rule (description, recommendation, ...)
    /// </summary>
    [HttpGet("insight", Name = "getInsight")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InsightDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetInsight(string id)
    {
        logger.LogInformation("Get insight {id}", id);
        var insight = await this.repositoryInsight.GetOne(id);
        if (insight is null) return NotFound();

        var rule = await this.repositoryRules.GetOne(insight.RuleId);
        // rule is currently null for a Configuration error
        var insightDto = new InsightDto(insight, commandUrl)
        {
            Category = rule?.Category ?? ""
        };

        return Ok(insightDto);
    }

    /// <summary>
    /// Get all insights after a given point
    /// </summary>
    [Route("insightafter/{ruleId}", Name = "GetInsightsAfter")]
    [HttpPost]
    [Produces(typeof(InsightDtoBatchDto))]
    public async Task<IActionResult> GetInsightsAfter(string ruleId, BatchRequestDto request)
    {
        var batch = await GetInsightsAfterBatch(ruleId, request);

        return Ok(batch);
    }

    /// <summary>
    /// Get insights for which another insight is dependant on
    /// </summary>
    [Route("dependantinsights/{insightId}", Name = "GetDependantInsights")]
    [HttpGet]
    [Produces(typeof(InsightDependencyDto[]))]
    public async Task<IActionResult> GetDependantInsights(string insightId)
    {
        var insight = await repositoryInsight.GetOne(insightId);

        if (insight is null)
        {
            return NotFound();
        }

        if (insight.Dependencies.Any())
        {
            var insightIds = insight.Dependencies.Select(v => v.InsightId).ToArray();

            var insights = await this.repositoryInsight.Get(v => insightIds.Contains(v.Id));

            var result = insights.Select((v) => new InsightDependencyDto(insight.Dependencies.First(d => d.InsightId == v.Id), v)).ToArray();

            return Ok(result);
        }

        return NoContent();
    }

    /// <summary>
    /// Exports all insights after a given point
    /// </summary>
    [Route("exportinsightafter/{ruleId}", Name = "ExportInsightsAfter")]
    [HttpPost]
    [FileResultContentType("text/csv")]
    public async Task<IActionResult> ExportInsightsAfter(string ruleId, BatchRequestDto request)
    {
        var batch = await GetInsightsAfterBatch(ruleId, request);

        return await CsvResult(batch.Items, $"InsightsForSkill_{ruleId}.csv");
    }

    private async Task<InsightDtoBatchDto> GetInsightsAfterBatch(string ruleId, BatchRequestDto request)
    {
        if (ruleId == "none" || ruleId == "all") ruleId = "";
        logger.LogInformation("Get insights for `{ruleId}`", ruleId);

        var filters = request.FilterSpecifications.ToList();

        if (!string.IsNullOrEmpty(ruleId))
        {
            //lambda filtering is not possbile because where clause is embedded into the pivot's where
            filters.Add(new FilterSpecificationDto()
            {
                field = nameof(Insight.RuleId),
                value = ruleId,
                @operator = FilterSpecificationDto.EqualsLiteral
            });
        }

        var batch = await this.repositoryInsight.GetAll(
            request.SortSpecifications,
            [.. filters],
            page: request.Page,
            take: request.PageSize);

        var batchDto = batch.Transform((v) => new InsightDto(v, commandUrl));

        var impactScoreNames = string.IsNullOrWhiteSpace(ruleId)
        ? await repositoryInsight.GetImpactScoreNames()
        : await repositoryInsight.GetImpactScoreNames(ruleId);

        var impactSummaryDtos = impactScoreNames
            .Select(v => new InsightImpactSummaryDto(v.Name, v.FieldId, v.Units, v.Count)).ToList();

        return new InsightDtoBatchDto(batchDto, impactSummaryDtos);
    }

    /// <summary>
    /// Get the insights that have been created for a specific model
    /// </summary>
    [HttpPost("InsightsForModel", Name = "InsightsForModel")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InsightDtoBatchDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetInsightsForModel(string modelId, BatchRequestDto request)
    {
        var batch = await GetInsightsForModelBatch(modelId, request);

        return Ok(batch);
    }

    /// <summary>
    /// Exports the insights that have been created for a specific model
    /// </summary>
    [HttpPost("ExportInsightsForModel", Name = "ExportInsightsForModel")]
    [FileResultContentType("text/csv")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> ExportInsightsForModel(string modelId, BatchRequestDto request)
    {
        var batch = await GetInsightsForModelBatch(modelId, request);

        return await CsvResult(batch.Items, $"InsightsForModel_{modelId}.csv");
    }

    private async Task<InsightDtoBatchDto> GetInsightsForModelBatch(string modelId, BatchRequestDto request)
    {
        var filters = request.FilterSpecifications.ToList();

        if (modelId != "none")
        {
            //lambda filtering is not possbile because where clause is embedded into the pivot's where
            filters.Add(new FilterSpecificationDto()
            {
                field = nameof(Insight.PrimaryModelId),
                value = modelId,
                @operator = FilterSpecificationDto.EqualsLiteral
            });
        }

        // A small finite number
        var batch = await this.repositoryInsight.GetAll(
            request.SortSpecifications,
            filters.ToArray(),
            page: request.Page,
            take: request.PageSize);

        var batchDto = batch.Transform((v) => new InsightDto(v, commandUrl));

        var impactScoreNames = await this.repositoryInsight.GetImpactScoreNames();

        return new InsightDtoBatchDto(batchDto, impactScoreNames.Select(v => new InsightImpactSummaryDto(v.Name, v.FieldId, v.Units, v.Count)).ToList());
    }

    /// <summary>
    /// Delete an Insight
    /// </summary>
    [HttpPost("delete-insight", Name = "deleteInsight")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> InsightDelete(string insightId)
    {
        auditLogger.LogInformation(User, new() { ["InsightId"] = insightId }, "Delete insight {insight}", insightId);

        var insight = await repositoryInsight.GetOne(insightId);
        if (insight is null) return NotFound($"No such insight '{insightId}'");

        var statusCodeClosed = await commandInsightService.CloseInsightInCommand(insight);
        var statusCode = await commandInsightService.DeleteInsightFromCommand(insight);
        await repositoryInsight.DeleteOne(insight);
        logger.LogInformation("Deleted insight {insightId} and from command: {statusCodeClosed}, {statusCode}", insightId, statusCodeClosed, statusCode);

        return Ok("Deleted");
    }

    /// <summary>
    /// Delete all insights for a given rule
    /// </summary>
    [HttpPost("delete-insights-for-rule", Name = "deleteInsightsForRule")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> DeleteInsightsForRule(string ruleId)
    {
        auditLogger.LogInformation(User, new() { ["RuleId"] = ruleId }, "Delete insights for rule {ruleId}", ruleId);

        var request = RuleExecutionRequest.CreateDeleteAllMatchingInsightsRequest(willowEnvironment.Id, ruleId, User.UserName());

        await messageSender.RequestRuleExecution(request);

        return Ok("Deleted");
    }

    /// <summary>
    /// Disable all insights
    /// </summary>
    [HttpPost("disable-insights", Name = "disableInsights")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> DisableInsights()
    {
        auditLogger.LogInformation(User, "Disable all insights");

        await repositoryInsight.DisableAllInsights();

        var request = RuleExecutionRequest.CreateDeleteAllMatchingInsightsRequest(willowEnvironment.Id, string.Empty, User.UserName());

        await messageSender.RequestRuleExecution(request);

        return Ok("Disabled");
    }

    /// <summary>
    /// Delete all insights which are flagged not to sync to Command but which have already been syncd to Command
    /// </summary>
    [HttpPost("delete-not-syncd-insights-from-command", Name = "DeleteNotSyncdInsightsFromCommand")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanViewAdminPage))]
    public async Task<IActionResult> DeleteNotSyncdInsightsFromCommand(bool removeCommandId)
    {
        auditLogger.LogInformation(User, new() { ["RemoveCommandId"] = removeCommandId }, "Delete Command Insights");

        var request = RuleExecutionRequest.CreateDeleteCommandInsightsRequest(willowEnvironment.Id, removeCommandId, User.UserName());

        await messageSender.RequestRuleExecution(request);

        return Ok(request.CorrelationId);
    }

    /// <summary>
    /// Delete all insights from Rules Engine, optionally deleting from Command, Actors and/or TimeSeries
    /// </summary>
    [HttpPost("delete-all-insights", Name = "DeleteAllInsights")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanViewAdminPage))]
    public async Task<IActionResult> DeleteAllInsights(bool deleteFromCommand, bool deleteActors, bool deleteTimeseries)
    {
        Dictionary<string, object> logScope = new()
        {
            ["DeleteFromCommand"] = deleteFromCommand,
            ["DeleteActors"] = deleteActors,
            ["DeleteTimeseries"] = deleteTimeseries
        };

        auditLogger.LogInformation(User, logScope, "Delete All Insights");

        var request = RuleExecutionRequest.CreateDeleteAllInsightsRequest(willowEnvironment.Id, deleteFromCommand, deleteActors, deleteTimeseries, User.UserName());

        await messageSender.RequestRuleExecution(request);

        return Ok(request.CorrelationId);
    }

    /// <summary>
    /// Reverse sync command insight Id back to rules engine insights
    /// </summary>
    [HttpPost("reverse-sync-insights-from-command", Name = "ReverseSyncInsightsFromCommand")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanViewAdminPage))]
    public async Task<IActionResult> ReverseSyncInsightsFromCommand()
    {
        auditLogger.LogInformation(User, "Reverse Sync Insights");

        var request = RuleExecutionRequest.CreateReverseSyncInsightsRequest(willowEnvironment.Id, User.UserName());

        await messageSender.RequestRuleExecution(request);

        return Ok(request.CorrelationId);
    }

    /// <summary>
    /// Gets insight summary info
    /// </summary>
    [HttpGet("get-insights-summary", Name = "GetInsightsSummary")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InsightsSummaryDto))]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanViewInsights))]
    public async Task<IActionResult> GetInsightsSummary()
    {
        var summary = (await this.repositoryADTSummary.GetLatest())?.SystemSummary?.InsightSummary ?? new InsightSummary();

        return Ok(new InsightsSummaryDto()
        {
            TotalNotSynced = summary.TotalNotSynced,
            TotalLinked = summary.TotalLinked,
            TotalEnabled = summary.TotalEnabled,
            Total = summary.Total,
            TotalFaulted = summary.TotalFaulted,
            TotalInvalid = summary.TotalInvalid,
            TotalValidNotFaulted = summary.TotalValidNotFaulted,
            InsightsByModel = summary.InsightsByModel
        });
    }

    /// <summary>
    /// Post an insight to command and keep posting future updates
    /// </summary>
    [HttpPost("insight-to-command", Name = "postInsightToCommand")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> InsightEnable(string insightId, [FromBody] bool enabled)
    {
        auditLogger.LogInformation(User, new() { ["InsightId"] = insightId, ["enabled"] = enabled },
            "Insight to command {enabled} for id {id}", (enabled ? "enabled" : "disabled"), insightId);

        int count = await repositoryInsight.SyncWithCommand(insightId, enabled);
        logger.LogInformation("Updated {Count} records while setting insight enabled flag", count);

        if (enabled)
        {
            logger.LogInformation("Upsert insight to command {InsightId}", insightId);
            var insight = await repositoryInsight.GetOne(insightId, updateCache: false);
            var result = await commandInsightService.UpsertInsightToCommand(insight);
            await repositoryInsight.UpsertOne(insight);

            if (result != System.Net.HttpStatusCode.OK)
            {
                return StatusCode((int)result, "Insight sync did not return success");
            }
        }

        return Ok(enabled);
    }

    /// <summary>
    /// Get download token for insights
    /// </summary>
    [HttpGet("downloadtoken", Name = "GetTokenForInsightsDownload")]
    [ProducesResponseType(typeof(ShortLivedTokenDto), StatusCodes.Status200OK)]
    [Authorize(Policy = nameof(CanExportRules))]
    public async Task<IActionResult> GetTokenForInsightsDownload()
    {
        string token = fileService.GetShortLivedToken();
        return Ok(new ShortLivedTokenDto { Token = token });
    }

    private async Task<FileStreamResult> CsvResult(IEnumerable<InsightDto> data, string filename)
    {
        var impactScoreNames = await this.repositoryInsight.GetImpactScoreNames();

        // Convert the IEnumerable to IAsyncEnumerable
        async IAsyncEnumerable<dynamic> ConvertData()
        {
            foreach (var v in data)
            {
                dynamic expando = new ExpandoObject();

                expando.IsFaulty = v.IsFaulty;
                expando.IsValid = v.IsValid;
                expando.LastFaultedDate = v.LastFaultedDate;
                expando.TimeZone = v.TimeZone;
                expando.Description = v.Text;
                expando.Rule = v.RuleName;
                expando.RuleTags = string.Join(", ", v.RuleTags ?? []);
                expando.Sync = v.CommandEnabled;
                expando.EquipmentId = v.EquipmentId;
                expando.EquipmentName = v.EquipmentName;
                expando.CommandInsightId = v.CommandInsightId;
                expando.Status = v.Status;

                var expandoLookup = (IDictionary<string, object>)expando;

                foreach (var name in impactScoreNames)
                {
                    expandoLookup[name.Name] = v.ImpactScores.FirstOrDefault(score => score.FieldId == name.FieldId)?.Score;
                }

                foreach (var location in v.Locations.GroupLocationsByModel())
                {
                    expandoLookup[location.Key] = location.Value;
                }

                yield return expando;
            }
        }

        return await WebExtensions.CsvResultWithDynamicHeaders(ConvertData(), filename);
    }
}
