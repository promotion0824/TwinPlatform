using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using Willow.Rules.Sources;
using Willow.ServiceBus;
using WillowRules.Extensions;
using WillowRules.Logging;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace Willow.Rules.Web.Controllers;

/// <summary>
/// Controller for rule instances
/// </summary>
/// <remarks>
/// After changing any method here you must run the APIClientGenerator project to update the typescript client
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = nameof(CanViewRules))]
[ApiExplorerSettings(GroupName = "v1")]
public class RuleInstanceController : ControllerBase
{
    private readonly ILogger<RuleInstanceController> logger;
    private readonly IAuditLogger<RuleInstanceController> auditLogger;
    private readonly WillowEnvironment willowEnvironment;
    private readonly IRepositoryRules repositoryRules;
    private readonly IRepositoryRuleInstances repositoryRuleInstances;
    private readonly IRepositoryRuleInstanceMetadata repositoryRuleInstanceMetadata;
    private readonly ITwinService twinService;
    private readonly IMessageSenderFrontEnd messageSender;
    private readonly IAuthorizationService authorizationService;
    private readonly IFileService fileService;

    /// <summary>
    /// Creates a new <see cref="RuleInstanceController"/>
    /// </summary>
    public RuleInstanceController(
        IRepositoryRules repositoryRules,
        IRepositoryRuleInstances repositoryRuleInstances,
        IRepositoryRuleInstanceMetadata repositoryRuleInstanceMetadata,
        ITwinService twinService,
        IMessageSenderFrontEnd messageSender,
        IAuthorizationService authorizationService,
        IFileService fileService,
        WillowEnvironment willowEnvironment,
        ILogger<RuleInstanceController> logger,
        IAuditLogger<RuleInstanceController> auditLogger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
        this.repositoryRules = repositoryRules ?? throw new ArgumentNullException(nameof(repositoryRules));
        this.repositoryRuleInstances = repositoryRuleInstances ?? throw new ArgumentNullException(nameof(repositoryRuleInstances));
        this.repositoryRuleInstanceMetadata = repositoryRuleInstanceMetadata ?? throw new ArgumentNullException(nameof(repositoryRuleInstanceMetadata));
        this.twinService = twinService ?? throw new ArgumentNullException(nameof(twinService));
        this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        this.messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
        this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
    }

    /// <summary>
    /// Gets a rule instance by id
    /// </summary>
    [HttpGet("ruleinstance", Name = "getRuleInstance")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RuleInstanceDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetRuleInstance(string id)
    {
        var instance = await this.repositoryRuleInstances.GetOne(id);

        if (instance is null) return NotFound(id);

        var rule = await repositoryRules.GetOne(instance.RuleId, updateCache: false);

        var auth = await authorizationService.CanViewRule(User, rule);

        if (!auth)
        {
            return Forbid();
        }

        var metadata = await this.repositoryRuleInstanceMetadata.GetOne(id);

        var result = new RuleInstanceDto(instance, metadata, true);

        return Ok(result);
    }

    /// <summary>
    /// Gets rule instances by modelId
    /// </summary>
    [HttpPost("RuleInstancesForModel", Name = "RuleInstancesForModel")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BatchDto<RuleInstanceDto>))]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetRuleInstancesForModel(string modelId, BatchRequestDto request)
    {
        var batch = await GetRuleInstancesForModelBatch(modelId, request);

        return Ok(batch);
    }

    /// <summary>
    /// Exports rule instances by modelId
    /// </summary>
    [HttpPost("ExportRuleInstancesForModel", Name = "ExportRuleInstancesForModel")]
    [FileResultContentType("text/csv")]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> ExportRuleInstancesForModel(string modelId, BatchRequestDto request)
    {
        var batch = GetRuleInstancesByModelIdAsync(modelId, request);

        return await CsvResult(batch, $"SkillInstancesForModel_{modelId}.csv");
    }

    private async Task<BatchDto<RuleInstanceDto>> GetRuleInstancesForModelBatch(string modelId, BatchRequestDto request)
    {
        // PROBLEM: THE ORDER MIGHT COME FROM THE RULE INSTANCE OR THE RULE INSTANCE METADATA
        // THIS IS NOT HANDLED CORRECTLY
        var batch = await this.repositoryRuleInstances.GetAllCombined(
            request.SortSpecifications,
            request.FilterSpecifications,
            x => x.PrimaryModelId.Equals(modelId),
            request.Page,
            request.PageSize);

        var result = new List<RuleInstanceDto>();

        foreach ((var ruleInstance, var metadata) in batch.Items)
        {
            var auth = await authorizationService.CanViewRule(User, ruleInstance);

            result.Add(new RuleInstanceDto(ruleInstance, metadata, auth));
        }

        var resultBatch = batch.Transform(result);

        return new BatchDto<RuleInstanceDto>(resultBatch);
    }

    /// <summary>
    /// Change the enabled or disabled state for a RuleInstance
    /// </summary>
    [HttpPost("rule-instance-enable", Name = "enableRuleInstance")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool[]))]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> RuleInstanceEnable(string ruleInstanceId, [FromBody] bool disabled)
    {
        auditLogger.LogInformation(User, new() { ["RuleInstanceId"] = ruleInstanceId, ["enabled"] = !disabled },
            "Rule instance {enabled} id {id}", (disabled ? "disabled" : "enabled"), ruleInstanceId);

        int count = await repositoryRuleInstances.SetDisabled(ruleInstanceId, disabled);
        logger.LogInformation("Updated {Count} records while setting disabled flag", count);

        //send a message to execution so the latest rule and instances will get loaded into memory.
        var request = RuleExecutionRequest.CreateRealtimeExecutionRequest(willowEnvironment.Id, User.UserName());

        await messageSender.RequestRuleExecution(request);

        return Ok(disabled);
    }

    /// <summary>
    /// Change the review status of the rule instance
    /// </summary>
    [HttpPost("rule-instance-review-status", Name = "UpdateRuleInstanceReviewStatus")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool[]))]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> UpdateRuleInstanceReviewStatus(string ruleInstanceId, [FromBody] ReviewStatus status)
    {
        var metadata = await repositoryRuleInstanceMetadata.GetOrAdd(ruleInstanceId);

        if (metadata is null)
        {
            return NotFound();
        }

        Dictionary<string, object> logScope = new()
        {
            ["RuleInstanceId"] = ruleInstanceId,
            ["Status"] = status,
            ["PreviousStatus"] = metadata.ReviewStatus
        };

        auditLogger.LogInformation(User, logScope, "Rule instance review status change from {s1} to {s2} id {id}", metadata.ReviewStatus, status, metadata.Id);

        metadata.ReviewStatus = status;

        await repositoryRuleInstanceMetadata.UpsertOne(metadata);

        return Ok(true);
    }

    /// <summary>
    /// Change the review status of the rule instance
    /// </summary>
    [HttpPost("rule-instance-tags", Name = "UpdateRuleInstanceTags")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool[]))]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> UpdateRuleInstanceTags(string ruleInstanceId, [FromBody] List<string> tags)
    {
        var metadata = await repositoryRuleInstanceMetadata.GetOrAdd(ruleInstanceId);

        if (metadata is null)
        {
            return NotFound();
        }

        auditLogger.LogInformation(User, new() { ["RuleInstanceId"] = ruleInstanceId, ["Tags"] = string.Join(",", tags) },
            "Rule instance tags updated to {s1} id {id}", JsonConvert.SerializeObject(tags), metadata.Id);

        metadata.Tags = tags;

        await repositoryRuleInstanceMetadata.UpsertOne(metadata);

        return Ok(true);
    }


    /// <summary>
    /// Gets the distinct tags
    /// </summary>
    [HttpGet("RuleInstanceTags", Name = "RuleInstanceTags")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<string>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetRuleTags()
    {
        var tags = new string[] { "Binding Issue", "Unknown" };

        return Ok(tags);
    }

    /// <summary>
    /// Add a review comment to the rule instance
    /// </summary>
    [HttpPost("rule-instance-review-comment", Name = "AddRuleInstanceReviewComment")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RuleCommentDto))]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> AddRuleInstanceReviewComment(string ruleInstanceId, [FromBody] string comment)
    {
        if (string.IsNullOrEmpty(comment))
        {
            return BadRequest("Comment required");
        }

        var metadata = await repositoryRuleInstanceMetadata.GetOrAdd(ruleInstanceId);

        if (metadata is null)
        {
            return NotFound();
        }

        auditLogger.LogInformation(User, new() { ["RuleInstanceId"] = ruleInstanceId, ["Comment"] = comment }, "AddRuleInstanceReviewComment");

        var ruleComment = new RuleComment()
        {
            Comment = comment,
            Created = DateTimeOffset.UtcNow,
            User = User.UserName()
        };

        metadata.AddComment(ruleComment);

        await repositoryRuleInstanceMetadata.UpsertOne(metadata);

        return Ok(new RuleCommentDto(ruleComment));
    }

    /// <summary>
    /// Get rule instances after a given starting point, filtered to a given rule id
    /// </summary>
    [Route("instanceafter/{id}", Name = "GetInstancesAfter")]
    [HttpPost]
    [Produces(typeof(BatchDto<RuleInstanceDto>))]
    public async Task<IActionResult> GetRuleInstancesAfter([FromRoute] string id, BatchRequestDto request)
    {
        var batch = await GetRuleInstancesAfterBatch(id, request);

        return Ok(batch);
    }

    /// <summary>
    /// Get a lite version of rule instances for a rule
    /// </summary>
    [Route("ruleinstancelist/{id}", Name = "GetRuleInstanceList")]
    [HttpPost]
    [Produces(typeof(RuleInstanceListItemDto[]))]
    public async Task<IActionResult> GetRuleInstanceList([FromRoute] string id)
    {
        var result = new List<RuleInstanceListItemDto>();

        try
        {
            foreach (var ri in await repositoryRuleInstances.GetRuleInstanceList(id))
            {
                result.Add(new RuleInstanceListItemDto(ri.id, ri.equipmentId, ri.equipmentName, ri.status));
            }

            //zero count probably means the rule has not been submitted yet, so give the user a chance to simulate before submitting
            if (result.Count == 0)
            {
                var rule = await repositoryRules.GetOne(id, updateCache: false);
                var twins = new List<BasicDigitalTwinPoco>();

                if (rule is not null)
                {
                    var modelId = !string.IsNullOrEmpty(rule.RelatedModelId) ? rule.RelatedModelId : rule.PrimaryModelId;
                    twins = await twinService.GetTwinsByModelWithInheritance(modelId);
                }
                else
                {
                    //id is sometimes primary model id
                    twins = await twinService.GetTwinsByModelWithInheritance(id);
                }

                foreach (var twin in twins)
                {
                    result.Add(new RuleInstanceListItemDto($"{twin.Id}_{id}", twin.Id, twin.name, RuleInstanceStatus.Valid));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get twins.");
        }

        //Sort the items by status, then by name so Valids can be selected firstr
        var sortedResult = result.OrderBy(item => item.status != RuleInstanceStatus.Valid).ThenBy(item => item.equipmentName);

        return Ok(sortedResult);
    }

    /// <summary>
    /// Exports rule instances after a given starting point, filtered to a given rule id
    /// </summary>
    [Route("exportinstanceafter/{id}", Name = "ExportRuleInstancesAfter")]
    [HttpPost]
    [FileResultContentType("text/csv")]
    public async Task<IActionResult> ExportRuleInstancesAfter([FromRoute] string id, BatchRequestDto request)
    {
        var batch = GetRuleInstancesAsync(id, request);

        return await CsvResult(batch, $"InstancesForSkill_{id}.csv");
    }

    /// <summary>
    /// Download rule instances in a ZIP file
    /// </summary>
    [Route("download-rule-instances/{id}", Name = "DownloadRuleInstances")]
    [HttpPost]
    [FileResultContentType("application/zip")]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanExportRules))]
    public async Task<IActionResult> DownloaRuleInstances([FromRoute] string id, BatchRequestDto request)
    {
        var batch = GetRuleInstancesAsync(id, request);

        var filePath = await this.fileService.ZipRuleInstances(batch.Select(v => v.ruleInstance));

        var zipstream = System.IO.File.Open(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);

        var now = DateTimeOffset.UtcNow;

        string fileName = $"{willowEnvironment.Id}-SkillInstances-{now.Year}-{now.Month:00}-{now.Day:00}-{now.Hour:00}-{now.Minute:00}.zip";

        logger.LogInformation("Returning zip file for rule instances");

        return File(zipstream, "application/zip", fileName, true);
    }

    /// <summary>
    /// Change the enabled, reviewStatus and comment for one or more rule instances
    /// </summary>
    [HttpPost("updateRuleInstanceProperties", Name = "UpdateRuleInstanceProperties")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool[]))]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> UpdateRuleInstanceProperties([FromBody] RuleInstancePropertiesDto ruleInstanceProperties)
    {
        var ruleInstanceIds = ruleInstanceProperties.Ids;
        var disabled = bool.TryParse(ruleInstanceProperties.Disabled, out var disabledValue);
        var reviewStatus = int.TryParse(ruleInstanceProperties.ReviewStatus, out var reviewStatusValue);
        var queueRealTime = false;

        var batch = await repositoryRuleInstances.GetAllCombined([], [], x => ruleInstanceIds.Contains(x.Id));

        foreach ((var ruleInstance, var originalMetadata) in batch.Items)
        {
            if (ruleInstance == null)
            {
                logger.LogWarning("Encountered null rule instance in batch processing");
                continue;
            }

            try
            {
                var auth = await authorizationService.CanViewRule(User, ruleInstance);
                if (!auth)
                {
                    logger.LogWarning("User {user} is not authorized to update rule instance {id}", User.UserName(), ruleInstance.Id);
                    continue;
                }

                //Check if metadata is null and initialize it if necessary
                var metadata = originalMetadata ?? await repositoryRuleInstanceMetadata.GetOrAdd(ruleInstance.Id);

                if (!string.IsNullOrWhiteSpace(ruleInstanceProperties.Disabled))
                {
                    auditLogger.LogInformation(User, "Rule instance {enabled} id {id}", (disabled ? "disabled" : "enabled"), ruleInstance.Id);
                    ruleInstance.Disabled = disabledValue;
                    queueRealTime = true;
                    await repositoryRuleInstances.QueueWrite(ruleInstance);
                }

                if (!string.IsNullOrWhiteSpace(ruleInstanceProperties.ReviewStatus))
                {
                    var status = (ReviewStatus)Enum.ToObject(typeof(ReviewStatus), reviewStatusValue);
                    auditLogger.LogInformation(User, "Rule instance review status change from {s1} to {s2} id {id}", metadata.ReviewStatus, status, metadata.Id);
                    metadata.ReviewStatus = status;
                    await repositoryRuleInstanceMetadata.QueueWrite(metadata);
                }

                if (!string.IsNullOrWhiteSpace(ruleInstanceProperties.Comment))
                {
                    auditLogger.LogInformation(User, "Added Rule instance comment for {id}", metadata.Id);
                    var ruleComment = new RuleComment()
                    {
                        Comment = ruleInstanceProperties.Comment,
                        Created = DateTimeOffset.UtcNow,
                        User = User.UserName()
                    };
                    metadata.AddComment(ruleComment);
                    await repositoryRuleInstanceMetadata.QueueWrite(metadata);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update rule instance {id}", ruleInstance.Id);
            }
        }

        await repositoryRuleInstances.FlushQueue();
        await repositoryRuleInstanceMetadata.FlushQueue();

        if (queueRealTime)
        {
            var request = RuleExecutionRequest.CreateRealtimeExecutionRequest(willowEnvironment.Id, User.UserName());
            await messageSender.RequestRuleExecution(request);
        }

        return Ok(true);
    }


    private async Task<BatchDto<RuleInstanceDto>> GetRuleInstancesAfterBatch(string id, BatchRequestDto request)
    {
        // Must alway have a ruleId for this call, doesn't allow getting all rule instance
        logger.LogInformation("Get rule instances for rule `{id}`", id);

        Expression<Func<RuleInstance, bool>> whereExpression = null;

        if (id != "all")
        {
            whereExpression = (r) => r.RuleId == id;
        }

        var batch = await this.repositoryRuleInstances.GetAllCombined(
            request.SortSpecifications,
            request.FilterSpecifications,
            whereExpression,
            request.Page,
            request.PageSize);

        logger.LogInformation("Get rules instances {id} - before={before} after={after}", id, batch.Before, batch.After);

        var result = new List<RuleInstanceDto>();

        foreach ((var ruleInstance, var metadata) in batch.Items)
        {
            var auth = await authorizationService.CanViewRule(User, ruleInstance);

            result.Add(new RuleInstanceDto(ruleInstance, metadata, auth));
        }

        var batch2 = batch.Transform(result);

        return new BatchDto<RuleInstanceDto>(batch2);
    }

    private IAsyncEnumerable<(RuleInstance ruleInstance, RuleInstanceMetadata metadata)> GetRuleInstancesAsync(string id, BatchRequestDto request)
    {
        // Must alway have a ruleId for this call, doesn't allow getting all rule instance
        logger.LogInformation("Get rule instances for rule `{id}`", id);

        Expression<Func<RuleInstance, bool>> whereExpression = null;

        if (id != "all")
        {
            whereExpression = (r) => r.RuleId == id;
        }

        return this.repositoryRuleInstances.GetAllCombinedAsync(
            request.SortSpecifications,
            request.FilterSpecifications,
            whereExpression,
            request.Page,
            request.PageSize);
    }

    private IAsyncEnumerable<(RuleInstance ruleInstance, RuleInstanceMetadata metadata)> GetRuleInstancesByModelIdAsync(string modelId, BatchRequestDto request)
    {
        logger.LogInformation("Get rule instances for model `{id}`", modelId);

        return this.repositoryRuleInstances.GetAllCombinedAsync(
            request.SortSpecifications,
            request.FilterSpecifications,
            whereExpression: x => x.PrimaryModelId == modelId,
            request.Page,
            request.PageSize);
    }

    private async Task<FileStreamResult> CsvResult(IAsyncEnumerable<(RuleInstance ruleInstance, RuleInstanceMetadata metadata)> batch, string fileName)
    {
        return await WebExtensions.CsvResultWithDynamicHeaders(GetDtos(batch).Select(v =>
        {
            dynamic expando = new ExpandoObject();
            expando.Id = v.Id;
            expando.RuleId = v.RuleId;
            expando.RuleName = v.RuleName;
            expando.EquipmentId = v.EquipmentId;
            expando.EquipmentName = v.EquipmentName;            
            expando.Valid = v.Valid;
            expando.Status = v.Status.ToString();
            expando.Enabled = !v.Disabled;
            expando.Received = v.TriggerCount;
            expando.Expression = v.RuleParametersBound.FirstOrDefault(x => x.FieldId == Fields.Result.Id)?.PointExpression;
            expando.RuleParametersBound = v.RuleParametersBound.FirstOrDefault(x => x.FieldId == Fields.Result.Id)?.Units;
            expando.RuleDependencyCount = v.ruleDependencyCount;
            expando.CapabilityCount = v.CapabilityCount;
            expando.ReviewStatus = v.ReviewStatus.ToString();
            expando.PointIds = string.Join(";", v.PointEntityIds.Select(v => v.Id));

            // expando["Tags"] = v.Tags != null ? string.Join(", ", v.Tags) : ""; // Disabled for now
            var expandoLookup = (IDictionary<string, object>)expando;
            foreach (var location in v.Locations.GroupLocationsByModel())
            {
                expandoLookup[location.Key] = location.Value;
            }
            return expando;
        }), fileName);
    }

    private async IAsyncEnumerable<RuleInstanceDto> GetDtos(IAsyncEnumerable<(RuleInstance ruleInstance, RuleInstanceMetadata metadata)> batch)
    {
        await foreach ((var ruleInstance, var metadata) in batch)
        {
            var auth = await authorizationService.CanViewRule(User, ruleInstance);

            yield return new RuleInstanceDto(ruleInstance, metadata, canViewRule: auth);
        }
    }
}
