using Abodit.Graph;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RulesEngine.Web;
using RulesEngine.Web.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.Expressions;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using Willow.Rules.Sources;
using Willow.ServiceBus;
using WillowRules.Extensions;
using WillowRules.Logging;
using WillowRules.Visitors;
using static Willow.Rules.Services.FileService;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace Willow.Rules.Web.Controllers;

/// <summary>
/// Controller for rules
/// </summary>
/// <remarks>
/// After changing any method here you must run the APIClientGenerator project to update the tyepscript client
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = nameof(CanViewRules))]
[ApiExplorerSettings(GroupName = "v1")]
public class RuleController : ControllerBase
{
    private readonly ILogger<RuleController> logger;
    private readonly IAuditLogger<RuleController> auditLogger;
    private readonly WillowEnvironment willowEnvironment;
    private readonly IRepositoryRuleMetadata repositoryRuleMetadata;
    private readonly IRepositoryRules repositoryRules;
    private readonly IRepositoryRuleExecutions repositoryRuleExecutions;
    private readonly IMessageSenderFrontEnd messageSender;
    private readonly IMetaGraphService metaGraphService;
    private readonly IModelService modelService;
    private readonly IAuthorizationService authorizationService;
    private readonly IPolicyDecisionService policyDecisionService;
    private readonly RuleTemplateRegistry ruleTemplateRegistry;
    private readonly IRepositoryCalculatedPoint repositoryCalculatedPoint;

    /// <summary>
    /// Creates a new <see cref="RuleController"/>
    /// </summary>
    public RuleController(
        ILogger<RuleController> logger,
        IAuditLogger<RuleController> auditLogger,
        WillowEnvironment willowEnvironment,
        IRepositoryRuleMetadata repositoryRuleMetadata,
        IRepositoryRules repositoryRules,
        IRepositoryRuleExecutions repositoryRuleExecutions,
        IMessageSenderFrontEnd messageSender,
        IMetaGraphService metaGraphService,
        IModelService modelService,
        IAuthorizationService authorizationService,
        IPolicyDecisionService policyDecisionService,
        RuleTemplateRegistry ruleTemplateRegistry,
        IRepositoryCalculatedPoint repositoryCalculatedPoint)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
        this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
        this.repositoryRuleMetadata = repositoryRuleMetadata ?? throw new ArgumentNullException(nameof(repositoryRuleMetadata));
        this.repositoryRules = repositoryRules ?? throw new ArgumentNullException(nameof(repositoryRules));
        this.repositoryRuleExecutions = repositoryRuleExecutions ?? throw new ArgumentNullException(nameof(repositoryRuleExecutions));
        this.messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        this.ruleTemplateRegistry = ruleTemplateRegistry ?? throw new System.ArgumentNullException(nameof(ruleTemplateRegistry));
        this.metaGraphService = metaGraphService ?? throw new ArgumentNullException(nameof(metaGraphService));
        this.modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
        this.policyDecisionService = policyDecisionService ?? throw new ArgumentNullException(nameof(policyDecisionService));
        this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        this.repositoryCalculatedPoint = repositoryCalculatedPoint ?? throw new ArgumentNullException(nameof(repositoryCalculatedPoint));
    }

    /// <summary>
    /// Get all rules
    /// </summary>
    [HttpPost("Rules", Name = "Rules")]
    [Produces(typeof(BatchDto<RuleDto>))]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetRules(BatchRequestDto request)
    {
        var batch = await GetRulesBatch(request);

        return Ok(batch);
    }

    /// <summary>
    /// Exports all rules
    /// </summary>
    [HttpPost("ExportRules", Name = "ExportRules")]
    [FileResultContentType("text/csv")]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> ExportRules(BatchRequestDto request)
    {
        var batch = await GetRulesBatch(request);

        return CsvResult(batch, "Skills.csv");
    }

    private async Task<Batch<RuleDto>> GetRulesBatch(BatchRequestDto request)
    {
        var batch = await this.repositoryRules.GetAllCombined(
           request.SortSpecifications,
           request.FilterSpecifications,
           page: request.Page,
           take: request.PageSize);

        var result = new List<RuleDto>();

        foreach ((var rule, var metadata) in batch.Items)
        {
            var auth = await authorizationService.CanViewRule(User, rule);

            result.Add(new RuleDto(rule, metadata, canViewRule: auth));
        }

        return batch.Transform(result);
    }

    /// <summary>
    /// Get the rules that apply to a model
    /// </summary>
    [HttpPost("RulesForModel", Name = "RulesForModel")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BatchDto<RuleDto>))]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetRulesForModel(string modelId, BatchRequestDto request)
    {
        var batch = await GetRulesForModelBatch(modelId, request);

        return Ok(batch);
    }

    /// <summary>
    /// Exports the rules that apply to a model
    /// </summary>
    [HttpPost("ExportRulesForModel", Name = "ExportRulesForModel")]
    [FileResultContentType("text/csv")]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> ExportRulesForModel(string modelId, BatchRequestDto request)
    {
        var batch = await GetRulesForModelBatch(modelId, request);

        return CsvResult(batch, $"SkillsForModel_{modelId}.csv");
    }

    private async Task<Batch<RuleDto>> GetRulesForModelBatch(string modelId, BatchRequestDto request)
    {
        var batch = await this.repositoryRules.GetAllCombined(
            request.SortSpecifications,
            request.FilterSpecifications,
            whereExpression: x => x.PrimaryModelId == modelId,
            page: request.Page,
            take: request.PageSize);

        var result = new List<RuleDto>();

        foreach ((var rule, var metadata) in batch.Items)
        {
            var auth = await authorizationService.CanViewRule(User, rule);

            result.Add(new RuleDto(rule, metadata, canViewRule: auth));
        }

        return batch.Transform(result);
    }

    /// <summary>
    /// Gets the distinct catgory names currently in use for rules
    /// </summary>
    [HttpGet("RuleCategories", Name = "RuleCategories")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<string>))]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetRuleCategories()
    {
        var categories = await this.repositoryRules.GetCategories();
        return Ok(categories);
    }

    /// <summary>
    /// Get a rule with associated metadata
    /// </summary>
    [HttpGet("Rule", Name = "getRule")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RuleDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetRule(string id)
    {
        auditLogger.LogInformation(User, new() { ["RuleId"] = id }, "GetRule");

        var rule = await this.repositoryRules.GetOne(id, updateCache: false);

        if (rule is null)
        {
            return NotFound();
        }

        var succeeded = await authorizationService.CanViewRule(User, rule);

        if (!succeeded)
        {
            return Forbid();
        }

        var metadata = await this.repositoryRuleMetadata.GetOrAdd(id);

        var policies = new List<AuthorizationDecisionDto>();

        var canEdit = await authorizationService.CanEditRule(User, rule);

        policies.Add(new AuthorizationDecisionDto()
        {
            Name = AuthPolicy.CanEditRules.Name,
            Success = canEdit
        });

        var result = new RuleDto(rule, metadata, canViewRule: true, policies: new AuthenticatedUserAndPolicyDecisionsDto(policies));

        return Ok(result);
    }

    /// <summary>
    /// Delete a rule
    /// </summary>
    [HttpDelete("Rule", Name = "deleteRule")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RuleDto))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ValidationReponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> DeleteRule(string id)
    {
        auditLogger.LogInformation(User, new() { ["RuleId"] = id }, "DeleteRule");

        // Checking if rule exists. Also, needs to obtain rule for git sync to know which
        // file to delete
        Model.Rule rule = await repositoryRules.GetOne(id);

        if (rule is null)
        {
            return NotFound();
        }

        var succeeded = await authorizationService.CanEditRule(User, rule);

        if (!succeeded)
        {
            return Forbid();
        }

        // Even though the RuleInstance processed will delete the rule, delete rule first to prevent any
        // further access while data is being cleared
        await repositoryRules.DeleteRuleById(id);
        var request = RuleExecutionRequest.CreateDeleteRuleRequest(willowEnvironment.Id, id, User.UserName());

        await messageSender.RequestRuleExecution(request);

        logger.LogInformation("Requesting processor to perform Git sync");

        var gitRequest = RuleExecutionRequest.CreateGitSyncRequest(willowEnvironment.Id,
            requestedBy: User.UserName(), userEmail: User.Email(), ruleId: rule.Id,
            syncFolder: RuleSource.GetFolder(rule), deleteRule: true);
        await messageSender.RequestRuleExecution(gitRequest, CancellationToken.None);

        return Ok();
    }

    /// <summary>
    /// Gets a list of rules the provided rule can possibly depend on
    /// </summary>
    [HttpPost]
    [Route("GetRuleDependencies", Name = "GetRuleDependencies")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RuleDependencyListItemDto[]))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(RuleDto))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> RuleDependencies(string id, [FromBody] RuleDto data)
    {
        logger.LogInformation("RuleDependencies {id}", id);

        var rule = (await this.repositoryRules.GetOne(id, updateCache: false));

        if (rule is null)
        {
            return NotFound();
        }

        var succeeded = await authorizationService.CanViewRule(User, rule);

        if (!succeeded)
        {
            return Forbid();
        }

        rule.Update(data, out var validations);

        if (validations.Any())
        {
            return UnprocessableEntity(new ValidationReponseDto { results = validations.ToArray() });
        }

        var result = new List<RuleDependencyListItemDto>();

        var ontology = await modelService.GetModelGraphCachedAsync();
        var modelNode = ontology.First(v => v.Id == rule.PrimaryModelId);

        //firstChildOnly: only visit the last property if graph based (assuming last is a capability)
        var referencedCapabilityIds = rule.GetModelIdsForRule(firstPropertyOnly: true)
                                          .Where(v => modelService.IsCapability(v))
                                          .ToList();

        var serializedMetaGraph = await metaGraphService.GetSerializedMetaGraphCached(new ProgressTrackerDummy());

        var metaGraph = serializedMetaGraph.GetGraph(x => x.ModelId);

        var inheritedNodes = new ModelData[] { modelNode }
                                        .Union(ontology
                                            .Predecessors<ModelData>(modelNode, Relation.RDFSType));

        var startNode = serializedMetaGraph.Nodes.FirstOrDefault(v => v.ModelId == rule.PrimaryModelId);

        var metaGraphs = serializedMetaGraph.Nodes
            .Where(v => modelService.InheritsFromOrEqualTo(v.ModelId, rule.PrimaryModelId))
            .SelectMany(v =>
            {
                return metaGraph.DistanceToEverywhere(v);
            });

        var nodesByDistance = metaGraphs
            .GroupBy(x => x.distance)
            .OrderBy(g => g.Key)
            .ToList();

        foreach (var item in (await repositoryRules.GetAllCombined(new Rules.DTO.SortSpecificationDto[0], new Rules.DTO.FilterSpecificationDto[0])).Items)
        {
            var dependencyRule = item.rule;
            var existingMetdata = item.metadata;

            if (dependencyRule.Id != id)
            {
                var availableRelationships = new List<string>();

                bool isSibling = ontology.IsAncestorOrDescendantOrEqual(rule.PrimaryModelId, dependencyRule.PrimaryModelId);

                if (isSibling)
                {
                    availableRelationships.Add(RuleDependencyRelationships.Sibling);
                }

                bool isReferencedCapability = referencedCapabilityIds.Any(id => ontology.IsAncestorOrDescendantOrEqual(id, dependencyRule.PrimaryModelId));

                if (isReferencedCapability)
                {
                    availableRelationships.Add(RuleDependencyRelationships.ReferencedCapability);
                }

                bool isRelated = false;
                int distance = 0;

                foreach (var group in nodesByDistance)
                {
                    foreach (var node in group.Select(x => x.node))
                    {
                        if (modelService.InheritsFromOrEqualTo(node.ModelId, dependencyRule.PrimaryModelId))
                        {
                            isRelated = true;
                            break;
                        }
                    }

                    if (isRelated)
                    {
                        distance = group.Key;
                        break;
                    }
                }


                availableRelationships.Add(RuleDependencyRelationships.RelatedTo);

                bool circularReference = dependencyRule.Dependencies.Any(v => v.RuleId == id);

                var existingDependency = rule.Dependencies.FirstOrDefault(v => v.RuleId == dependencyRule.Id);

                string relationship = existingDependency?.Relationship ?? availableRelationships[0];

                if (relationship == "isFedBy")
                {
                    //backward compatibility fix
                    relationship = RuleDependencyRelationships.RelatedTo;
                }

                bool isEnabled = existingDependency is not null;

                result.Add(new RuleDependencyListItemDto(dependencyRule, existingMetdata, isEnabled, relationship, availableRelationships.ToArray(), circularReference, distance));
            }
        }

        return Ok(result);
    }

    /// <summary>
    /// Upsert a rule to database and trigger instance re-generation
    /// </summary>
    [HttpPost]
    [Route("upsertrule", Name = "upsertRule")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RuleDto))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ValidationReponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> PostRule(string id, [FromBody] RuleDto data)
    {
        auditLogger.LogInformation(User, new() { ["RuleId"] = id, ["RuleData"] = data }, "UpdateRule");

        Model.Rule rule;
        bool changed = false;
        List<ValidationReponseElementDto> validations = new();
        string oldSyncFolder = null;

        // New rule was created
        if (id == null)
        {
            rule = new Rule(data.Name, data.TemplateId, data.PrimaryModelId)
            {
                RelatedModelId = data.RelatedModelId,
                Elements = SetRuleElements(data.Elements),
                IsDraft = true
            };

            // Checks if the rule is a duplicate (already exists in the rules database)
            bool isDuplicate = await this.repositoryRules.Count(r => r.Id == rule.Id) > 0;
            if (isDuplicate)
            {
                return Conflict(new { message = $"A rule with the id '{rule.Id}' was already found." });
            }

            changed = true;
        }
        else // Getting preexisiting rule
        {
            rule = await this.repositoryRules.GetOne(id, updateCache: false);

            if (rule is null)
            {
                return NotFound();
            }

            var succeeded = await authorizationService.CanEditRule(User, rule);

            if (!succeeded)
            {
                return Forbid();
            }

            //Keep track of the rule's current folder incase the primaryModel changed
            oldSyncFolder = RuleSource.GetFolder(rule);

            changed |= ObjectMappingExtensions.SetValue(false, rule, (x) => rule.IsDraft);
        }

        validations.AddRange(data.Parameters.ValidateRuleParameters(required: true, field: nameof(RuleDto.Parameters), requiresResultField: true));
        validations.AddRange(data.RuleTriggers.ValidateRuleTriggers(required: false, field: nameof(RuleDto.RuleTriggers)));

        if (validations.Any())
        {
            return UnprocessableEntity(new ValidationReponseDto { results = validations.ToArray() });
        }

        changed |= rule.Update(data, out validations);

        if (validations.Any())
        {
            return UnprocessableEntity(new ValidationReponseDto { results = validations.ToArray() });
        }

        if (changed)
        {
            logger.LogInformation("Updating rule \"" + rule.Name + "\" in database");
            await this.repositoryRules.UpsertOne(rule);
            await this.repositoryRuleMetadata.RuleUpdated(rule.Id, User.UserName());

            if (!rule.IsDraft)
            {
                // Send a message to rebuild it
                logger.LogInformation("Requesting processor to rebuild rule \"" + rule.Name + "\"");
                var messageObject = RuleExecutionRequest.CreateRuleExpansionRequest(willowEnvironment.Id,
                    force: true, requestedBy: User.UserName(), ruleId: rule.Id);

                await messageSender.RequestRuleExecution(messageObject, CancellationToken.None);

                // Do git sync

                logger.LogInformation("Requesting processor to perform Git sync");
                var gitRequest = RuleExecutionRequest.CreateGitSyncRequest(willowEnvironment.Id,
                    requestedBy: User.UserName(), userEmail: User.Email(), ruleId: rule.Id,
                    syncFolder: rule.PrimaryModelId,
                    oldSyncFolder: !string.IsNullOrWhiteSpace(oldSyncFolder) &&
                    !string.Equals(rule.PrimaryModelId.TrimModelId(), oldSyncFolder, StringComparison.OrdinalIgnoreCase) ? oldSyncFolder : null,
                    rebuildUploadedRules: false);
                await messageSender.RequestRuleExecution(gitRequest, CancellationToken.None);
            }
        }
        else
        {
            logger.LogInformation("No changes to rule");
        }

        var metadata = await this.repositoryRuleMetadata.GetOrAdd(rule.Id, User.UserName());

        var result = new RuleDto(rule, metadata, canViewRule: true);

        return Ok(result);
    }

    /// <summary>
    /// Gets the names of all the available RuleTemplates
    /// </summary>
    /// <returns></returns>
    [HttpGet("RuleTemplates", Name = "getRuleTemplates")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RuleTemplateDto[]))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public IActionResult GetRuleTemplates()
    {
        return Ok(RuleExtensions.GetPopulatedRuleTemplateDtos());
    }

    /// <summary>
    /// Gets a list of predefined units
    /// </summary>
    /// <returns></returns>
    [HttpGet("GetUnits", Name = "getUnits")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UnitDto[]))]
    [ProducesDefaultResponseType]
    public IActionResult GetUnits()
    {
        return Ok(Unit.PredefinedUnits.Select(v => new UnitDto(v)).ToArray());
    }

    /// <summary>
    /// Creates a new rule from a template
    /// </summary>
    [HttpPost("createRule", Name = "createRule")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RuleDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> CreateRuleFromTemplate(string templateId)
    {
        auditLogger.LogInformation(User, new() { ["RuleTemplateId"] = templateId }, "CreateRule");

        var template = RuleExtensions.GetPopulatedRuleTemplateDtos().FirstOrDefault(t => t.Id == templateId);
        var rule = new Model.Rule() { TemplateId = templateId, IsDraft = true };

        var setDefaultParameters = (RuleUIElementDto[] source, IList<RuleParameter> target) =>
        {
            if (source is null || source.Length == 0) return target;

            foreach (var parameter in source)
            {
                target.Add(new RuleParameter(parameter.Name, parameter.Id, parameter.ValueString, parameter.Units));
            }

            return target;
        };

        rule.Recommendations = "Try turning it off and back on";
        rule.Elements = SetRuleElements(template.Elements);
        rule.Parameters = setDefaultParameters(template.Parameters, rule.Parameters);
        rule.ImpactScores = setDefaultParameters(template.ImpactScores, rule.ImpactScores);

        //Include Template name here for Create Rule Page title, e.g. 'Create a TemplateName rule' iso just 'Create rule'
        var ruleDto = new RuleDto(rule, new RuleMetadata(rule.Id, ""), canViewRule: true) { TemplateName = template.Name };

        return Ok(ruleDto);
    }

    private static List<RuleUIElement> SetRuleElements(RuleUIElementDto[] source)
    {
        var target = new List<RuleUIElement>();

        if (source is null || source.Length == 0) return target;

        foreach (var element in source)
        {
            target.Add(element.CreateUIElement());
        }

        return target;
    }

    /// <summary>
    /// Syncs working directory of rules with remote Git repository
    /// </summary>
    [HttpPost("SyncWithRemote", Name = "SyncWithRemote")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> SyncWithRemote()
    {
        logger.LogInformation("SyncWithRemote");

        logger.LogInformation("Requesting processor to perform Git sync");
        var gitRequest = RuleExecutionRequest.CreateGitSyncRequest(willowEnvironment.Id,
            requestedBy: User.UserName(), userEmail: User.Email());
        await messageSender.RequestRuleExecution(gitRequest, CancellationToken.None);

        return Ok(gitRequest != null);
    }

    /// <summary>
    /// Expands the rule if necessary and returns a count of rule instances
    /// </summary>
    /// <param name="ruleId"></param>
    /// <returns></returns>
    /// <remarks>
    /// Issue: This might be too slow for an HTTP request, may need a progress state instead
    /// and an API to collect finished state
    /// </remarks>
    [HttpGet("RuleMetadata", Name = "getRuleMetadata")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RuleMetadataDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetRuleMetadata(string ruleId)
    {
        logger.LogInformation("GetRuleMetadata");
        var rule = await this.repositoryRules.GetOne(ruleId);
        if (rule is null)
        {
            return NotFound();
        }

        var succeeded = await authorizationService.CanViewRule(User, rule);

        if (!succeeded)
        {
            return Forbid();
        }

        var metadata = await this.repositoryRuleMetadata.GetOne(ruleId);
        if (metadata is null) return NotFound();
        return Ok(new RuleMetadataDto(metadata));
    }

    /// <summary>
    /// Gets the current execution state of a processing rule execution
    /// </summary>
    /// <param name="ruleId"></param>
    /// <returns>Rule execution state</returns>
    [HttpGet("RuleExecution", Name = "getRuleExecution")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RuleExecutionDto))]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetRuleExecution(string ruleId)
    {
        var ruleExecution = await this.repositoryRuleExecutions.GetOneByRuleId(ruleId);
        if (ruleExecution is null) return NotFound();
        return Ok(new RuleExecutionDto(ruleExecution));
    }

    /// <summary>
    /// Enable posting insights and commands to command for the rule
    /// </summary>
    [HttpPost("enable-insight-to-command", Name = "EnabledInsightToCommand")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> InsightEnable(string ruleId, [FromBody] bool enabled)
    {
        auditLogger.LogInformation(User, new() { ["RuleId"] = ruleId, ["enabled"] = enabled },
            "Enable rule to sync: {enabled} id {id}", (enabled ? "enabled" : "disabled"), ruleId);

        var rule = await this.repositoryRules.GetOne(ruleId);
        if (rule is null)
        {
            return NotFound();
        }

        var succeeded = await authorizationService.CanEditRule(User, rule);

        if (!succeeded)
        {
            return Forbid();
        }

        int count = await repositoryRules.EnableSyncForRule(ruleId, enabled);

        // to make sure sync is properly enabled for insights re-execute it on the processor. If the user
        // enabled/disabled, but processor is busy creating new insights they may be out of sync with the flag
        var request = RuleExecutionRequest.CreateSyncCommandEnabledRequest(willowEnvironment.Id, User.UserName(), ruleId);

        await messageSender.RequestRuleExecution(request);

        logger.LogInformation("Updated {Count} records while setting rule enabled flag", count);
        return Ok(enabled);
    }

    private static IActionResult CsvResult(Batch<RuleDto> batch, string fileName)
    {
        return WebExtensions.CsvResult(batch.Items.Select(v =>
        new
        {
            v.Name,
            v.RuleMetadata?.LastModified,
            v.RuleMetadata?.ModifiedBy,
            v.Category,
            Tags = v.Tags != null ? string.Join(", ", v.Tags) : "",
            Equipment = v.PrimaryModelId,
            Instances = v.RuleMetadata?.RuleInstanceCount,
            v.IsDraft,
            Valid = v.RuleMetadata?.ValidInstanceCount,
            Sync = v.CommandEnabled,
            Insights = v.RuleMetadata?.InsightsGenerated,
            Commands = v.RuleMetadata?.CommandsGenerated,
            v.RuleMetadata?.ScanError,
            v.Id,
            v.TemplateId
        }), fileName);
    }

    /// <summary>
    /// Validate rule
    /// </summary>
    [HttpPost]
    [Route("validateRule", Name = "validateRule")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ValidationReponseDto))]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> ValidateRule(RuleDto data)
    {
        logger.LogInformation("Validate Rule {data}", data.Id);

        List<ValidationReponseElementDto> validations =
        [
            .. data.Parameters.ValidateRuleParameters(required: true, field: nameof(RuleDto.Parameters), requiresResultField: true),
            .. data.ImpactScores.ValidateRuleParameters(required: false, field: nameof(RuleDto.ImpactScores)),
            .. data.Filters.ValidateRuleParameters(required: false, field: nameof(RuleDto.Filters)),
            .. data.RuleTriggers.ValidateRuleTriggers(required: false, field: nameof(RuleDto.RuleTriggers)),
            .. ValidateRuleElements(data.Elements),
        ];

        if (validations.Any())
        {
            return UnprocessableEntity(new ValidationReponseDto { results = validations.ToArray() });
        }

        return Ok(true);
    }

    private static IList<ValidationReponseElementDto> ValidateRuleElements(IEnumerable<RuleUIElementDto> source)
    {
        List<ValidationReponseElementDto> validations = new();
        foreach (var ui in source)
        {
            if (ui.ElementType == RuleUIElementType.PercentageField)
            {
                if (ui.ValueDouble < 0 || ui.ValueDouble > 1)
                {
                    //UI shows it 0 - 100 and not 0 - 1
                    validations.Add(new ValidationReponseElementDto(ui.Id, "Must be a number between 0 and 100"));
                }
            }
            if (ui.Id == Fields.PercentageOfTimeOff.Id && source.FirstOrDefault(v => v.Id == Fields.PercentageOfTime.Id) is RuleUIElementDto percentageField)
            {
                if (ui.ValueDouble > percentageField.ValueDouble)
                {
                    validations.Add(new ValidationReponseElementDto(ui.Id, $"{Fields.PercentageOfTimeOff.Name} cannot be greater than {percentageField.Name}"));
                }
            }
        }

        return validations;
    }

    /// <summary>
    /// Enable ADT twins for enabled instances for the rule
    /// </summary>
    [HttpPost("enableADTSync", Name = "EnableADTSync")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> EnableADTSync(string ruleId, [FromBody] bool enabled)
    {
        auditLogger.LogInformation(User, new() { ["RuleId"] = ruleId, ["enabled"] = enabled },
            "ADT sync for rule {ruleId} {enabled}", ruleId, (enabled ? "enabled" : "disabled"));

        var rule = await this.repositoryRules.GetOne(ruleId);
        if (rule is null)
        {
            return NotFound();
        }

        var succeeded = await authorizationService.CanEditRule(User, rule);

        if (!succeeded)
        {
            return Forbid();
        }

        _ = await repositoryRules.SyncRuleWithADT(ruleId, enabled);

        if (enabled)
        {
            await ScheduleProcessCalculatedPoints();
        }
        else
        {
            var cpCount = await repositoryCalculatedPoint.ScheduleDeleteCalculatedPointsByRuleId(ruleId, CancellationToken.None);

            logger.LogInformation("{CpCount} calculated points scheduled for deletion for {RuleId}", cpCount, ruleId);

            await ScheduleProcessCalculatedPoints();

            //Expand the rule to refresh the calculated points to the correct state
            var processCPRule = RuleExecutionRequest.CreateRuleExpansionRequest(willowEnvironment.Id, force: true, requestedBy: User.UserName(), ruleId: ruleId);
            await messageSender.RequestRuleExecution(processCPRule, CancellationToken.None);
        }

        async Task ScheduleProcessCalculatedPoints()
        {
            var processCPsRequest = RuleExecutionRequest.CreateProcessCalculatedPointsRequest(willowEnvironment.Id, requestedBy: User.UserName(), ruleId: ruleId);
            await messageSender.RequestRuleExecution(processCPsRequest, CancellationToken.None);
        }

        return Ok(enabled);
    }

    /// <summary>
    /// Gets the distinct tags
    /// </summary>
    [HttpGet("RuleTags", Name = "RuleTags")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<string>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetRuleTags()
    {
        var tags = await this.repositoryRules.GetTags();
        return Ok(tags);
    }
}
