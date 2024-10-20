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
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using Willow.Rules.Sources;
using Willow.ServiceBus;
using WillowRules.Logging;
using static Willow.Rules.Services.FileService;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace Willow.Rules.Web.Controllers;

/// <summary>
/// Controller for global variables
/// </summary>
/// <remarks>
/// After changing any method here you must run the APIClientGenerator project to update the tyepscript client
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = nameof(CanViewRules))]
[ApiExplorerSettings(GroupName = "v1")]
public class GlobalVariableController : ControllerBase
{
    private readonly ILogger<GlobalVariableController> logger;
    private readonly IAuditLogger<GlobalVariableController> auditLogger;
    private readonly IRepositoryGlobalVariable repositoryGlobalVariable;
    private readonly IRepositoryRules repositoryRules;
    private readonly WillowEnvironment willowEnvironment;
    private readonly IMessageSenderFrontEnd messageSender;
    private readonly IRulesService rulesService;
    private readonly IAuthorizationService authorizationService;

    /// <summary>
    /// Creates a new <see cref="GlobalVariableController"/>
    /// </summary>
    public GlobalVariableController(
        IRepositoryGlobalVariable repositoryGlobalVariable,
        IRepositoryRules repositoryRules,
        WillowEnvironment willowEnvironment,
        IRulesService rulesService,
        IMessageSenderFrontEnd messageSender,
        IAuthorizationService authorizationService,
        ILogger<GlobalVariableController> logger,
        IAuditLogger<GlobalVariableController> auditLogger)
    {
        this.repositoryGlobalVariable = repositoryGlobalVariable ?? throw new ArgumentNullException(nameof(repositoryGlobalVariable));
        this.repositoryRules = repositoryRules ?? throw new ArgumentNullException(nameof(repositoryRules));
        this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
        this.rulesService = rulesService ?? throw new ArgumentNullException(nameof(rulesService));
        this.messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        this.authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
    }

    /// <summary>
    /// Get all globals
    /// </summary>
    [HttpPost("Globals", Name = "Globals")]
    [Produces(typeof(BatchDto<GlobalVariableDto>))]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetGlobalVariables(BatchRequestDto request)
    {
        var batch = await GetBatch(request);

        return Ok(batch);
    }

    /// <summary>
    /// Get rule or other global references for a global
    /// </summary>
    [HttpPost("GetGlobalReferences", Name = "GetGlobalReferences")]
    [Produces(typeof(RuleReferenceDto[]))]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetGlobalReferences(string id)
    {
        var global = await this.repositoryGlobalVariable.GetOne(id);

        if (global is null) return NotFound();

        var result = new List<RuleReferenceDto>();

        foreach (var item in await repositoryGlobalVariable.MatchGlobalVariableReferences(global.Name))
        {
            result.Add(new RuleReferenceDto(item));
        }

        foreach (var item in await repositoryRules.MatchRuleReferences(global.Name))
        {
            result.Add(new RuleReferenceDto(item));
        }

        return Ok(result);
    }

    /// <summary>
    /// Exports all rules
    /// </summary>
    [HttpPost("ExportGlobals", Name = "ExportGlobals")]
    [FileResultContentType("text/csv")]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> ExportRules(BatchRequestDto request)
    {
        var batch = await GetBatch(request);

        return CsvResult(batch, "Globals.csv");
    }

    /// <summary>
    /// Delete a global variable
    /// </summary>
    [HttpDelete("global-variable/{id}", Name = "deleteGlobalVariable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> DeleteGlobalVariable([FromRoute] string id)
    {
        auditLogger.LogInformation(User, new() { ["Id"] = id }, "Delete global variable {id}", id);

        var gv = await this.repositoryGlobalVariable.GetOne(id);

        if (gv is null) return NotFound();

        var succeeded = await authorizationService.CanEditRule(User, gv);

        if (!succeeded)
        {
            return Forbid();
        }

        await this.repositoryGlobalVariable.DeleteOne(gv);

        var gitRequest = RuleExecutionRequest.CreateGitSyncRequest(willowEnvironment.Id,
            requestedBy: User.UserName(), userEmail: User.Email(), ruleId: gv.Id, deleteRule: true, syncFolder: GlobalSource.Folder);

        await messageSender.RequestRuleExecution(gitRequest, CancellationToken.None);

        return Ok();
    }

    /// <summary>
    /// Get a global variable
    /// </summary>
    [HttpGet("GlobalVariable", Name = "getGlobalVariable")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GlobalVariableDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetGlobalVariable(string id)
    {
        var global = await this.repositoryGlobalVariable.GetOne(id);

        if (global is null) return NotFound();

        var succeeded = await authorizationService.CanViewRule(User, global);

        if (!succeeded)
        {
            return Forbid();
        }

        var policies = new List<AuthorizationDecisionDto>();

        var canEdit = await authorizationService.CanEditRule(User, global);

        policies.Add(new AuthorizationDecisionDto()
        {
            Name = AuthPolicy.CanEditRules.Name,
            Success = canEdit
        });

        var result = new GlobalVariableDto(global, canViewGlobal: succeeded, policies: new AuthenticatedUserAndPolicyDecisionsDto(policies));

        return Ok(result);
    }

    /// <summary>
    /// Validate a global variable
    /// </summary>
    [HttpPost]
    [Route("ValidateGlobalVariable", Name = "ValidateGlobalVariable")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ValidationReponseDto))]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanViewRules))]
    public async Task<IActionResult> ValidateGlobalVariable(GlobalVariableDto data)
    {
        logger.LogInformation("Validate Global {data}", data.Id);

        List<ValidationReponseElementDto> validations =
        [
            .. data.Expression.ValidateRuleParameters(required: true, field: nameof(GlobalVariableDto.Expression)),
        ];

        if (validations.Any())
        {
            return UnprocessableEntity(new ValidationReponseDto { results = validations.ToArray() });
        }

        var global = new Model.GlobalVariable();

        global.Update(data, out validations);

        if (validations.Any())
        {
            return UnprocessableEntity(new ValidationReponseDto { results = validations.ToArray() });
        }

        return Ok(true);
    }

    /// <summary>
    /// Upsert a global variable
    /// </summary>
    [HttpPost]
    [Route("upsertGlobalVariable", Name = "upsertGlobalVariable")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GlobalVariableDto))]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(GlobalVariableDto))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ValidationReponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> UpsertGlobalVariable(string id, [FromBody] GlobalVariableDto data)
    {
        auditLogger.LogInformation(User, new() { ["Id"] = id, ["Data"] = data }, "PostGlobalVariable {id} {data}", id, data);

        Model.GlobalVariable global;
        bool changed = false;
        List<ValidationReponseElementDto> validations = new();

        // New global was created
        if (id == null)
        {
            global = new Model.GlobalVariable()
            {
                Id = data.Name.ToIdStandard()
            };

            // Checks duplicates
            bool isDuplicate = await this.repositoryGlobalVariable.Count(r => r.Id == global.Id) > 0;
            if (isDuplicate)
            {
                return Conflict(new { message = $"A global variable with the id '{global.Id}' was already found." });
            }

            changed = true;
        }
        else // Getting existing
        {
            global = await this.repositoryGlobalVariable.GetOne(id, updateCache: false);

            if (global is null) return NotFound();

            var succeeded = await authorizationService.CanEditRule(User, global);

            if (!succeeded)
            {
                return Forbid();
            }
        }

        changed |= global.Update(data, out validations);

        validations.AddRange(data.Expression.ValidateRuleParameters(required: true, field: nameof(GlobalVariableDto.Expression)));

        if (!validations.Any())
        {
            (bool ok, string error, var expression) = await rulesService.ParseGlobal(global);

            if (!ok)
            {
                validations.Add(new ValidationReponseElementDto(nameof(GlobalVariable.Expression), error));
            }
        }

        if (validations.Any())
        {
            return UnprocessableEntity(new ValidationReponseDto { results = validations.ToArray() });
        }

        if (changed)
        {
            logger.LogInformation("Updating global \"" + global.Name + "\" in database");

            await this.repositoryGlobalVariable.UpsertOne(global);

            logger.LogInformation("Requesting processor to perform Git sync");

            var gitRequest = RuleExecutionRequest.CreateGitSyncRequest(willowEnvironment.Id,
                requestedBy: User.UserName(), userEmail: User.Email(), ruleId: global.Id);

            await messageSender.RequestRuleExecution(gitRequest, CancellationToken.None);

            var referencedRules = await repositoryRules.MatchRuleReferences(global.Name);

            if (referencedRules.Count > 0)
            {
                foreach (var rule in referencedRules)
                {
                    var messageObject = RuleExecutionRequest.CreateRuleExpansionRequest(willowEnvironment.Id,
                        force: true, requestedBy: User.UserName(), ruleId: rule.Id);

                    await messageSender.RequestRuleExecution(messageObject, CancellationToken.None);
                }

                logger.LogInformation("Expanding {count} rules after global updated {id}", referencedRules.Count, global.Id);
            }
        }
        else
        {
            logger.LogInformation("No changes to global variable");
        }

        var result = new GlobalVariableDto(global, canViewGlobal: true);

        return Ok(result);
    }

    /// <summary>
    /// Gets the distinct tags
    /// </summary>
    [HttpGet("GlobalVariableTags", Name = "GlobalVariableTags")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<string>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetGlobalVariableTags()
    {
        var tags = await repositoryGlobalVariable.GetTags();
        return Ok(tags);
    }


    private async Task<Batch<GlobalVariableDto>> GetBatch(BatchRequestDto request)
    {
        var batch = await this.repositoryGlobalVariable.GetAll(
            request.SortSpecifications,
            request.FilterSpecifications,
            page: request.Page,
            take: request.PageSize);

        var result = new List<GlobalVariableDto>();

        foreach (var global in batch.Items)
        {
            var auth = await authorizationService.CanViewRule(User, global);

            result.Add(new GlobalVariableDto(global, canViewGlobal: auth));
        }

        return batch.Transform(result);
    }

    private static IActionResult CsvResult(Batch<GlobalVariableDto> batch, string fileName)
    {
        return WebExtensions.CsvResult(batch.Items.Select(v =>
        new
        {
            v.Id,
            VariableType = v.VariableType.ToString(),
            v.Name,
            v.Description,
            v.Expression,
            Tags = v.Tags != null ? string.Join(", ", v.Tags) : "",
        }), fileName);
    }
}

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
