using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RulesEngine.Web;
using RulesEngine.Web.DTO;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using WillowRules.Logging;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace Willow.Rules.Web.Controllers;

/// <summary>
/// Controller for commands
/// </summary>
/// <remarks>
/// After changing any method here you must run the APIClientGenerator project to update the tyepscript client
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = nameof(CanViewCommands))]
[ApiExplorerSettings(GroupName = "v1")]
public class CommandController : ControllerBase
{
    private readonly ILogger<CommandController> logger;
    private readonly IAuditLogger<UserController> auditLogger;
    private readonly IRepositoryCommand repositoryCommand;
    private readonly ICommandService commandService;

    /// <summary>
    /// Creates a new <see cref="CommandController"/>
    /// </summary>
    public CommandController(
        IRepositoryCommand repositoryCommand,
        ICommandService commandService,
        ILogger<CommandController> logger,
        IAuditLogger<UserController> auditLogger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
        this.repositoryCommand = repositoryCommand ?? throw new ArgumentNullException(nameof(repositoryCommand));
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
    }

    /// <summary>
    /// Get all commands after a given point
    /// </summary>
    [Route("commandafter/{ruleId}", Name = "GetCommandsAfter")]
    [HttpPost]
    [Produces(typeof(BatchDto<CommandDto>))]
    public async Task<IActionResult> GetCommandsAfter(string ruleId, BatchRequestDto request)
    {
        var batch = await GetCommandsAfterBatch(ruleId, request);

        return Ok(batch);
    }

    /// <summary>
    /// Get the commands that have been created for a specific equipment item
    /// </summary>
    /// <returns>An array of commands (non-paged)</returns>
    [HttpPost("CommandsForEquipment", Name = "CommandsForEquipment")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BatchDto<CommandDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetCommandsForEquipment(string equipmentId, BatchRequestDto request)
    {
        var batch = await GetCommandsForEquipmentBatch(equipmentId, request);

        return Ok(batch);
    }

    /// <summary>
    /// Exports the commands that have been created for a specific equipment item
    /// </summary>
    [HttpPost("ExportCommandsForEquipment", Name = "ExportCommandsForEquipment")]
    [FileResultContentType("text/csv")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> ExportCommandsForEquipment(string equipmentId, BatchRequestDto request)
    {
        var batch = await GetCommandsForEquipmentBatch(equipmentId, request);

        return await CsvResult(batch.Items, $"CommandsForEquipment_{equipmentId}.csv");
    }

    /// <summary>
    /// Get the commands that have been created for a specific erule instance
    /// </summary>
    /// <returns>An array of commands (non-paged)</returns>
    [HttpPost("CommandsForRuleInstance", Name = "CommandsForRuleInstance")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BatchDto<CommandDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetCommandsForRuleInstance(string ruleInstanceId, BatchRequestDto request)
    {
        var batch = await GetCommandsForRuleInstanceBatch(ruleInstanceId, request);

        return Ok(batch);
    }

    /// <summary>
    /// Exports the commands that have been created for a specific rule instance
    /// </summary>
    [HttpPost("ExportCommandsForRuleInstance", Name = "ExportCommandsForRuleInstance")]
    [FileResultContentType("text/csv")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> ExportCommandsForRuleInstance(string ruleInstanceId, BatchRequestDto request)
    {
        var batch = await GetCommandsForRuleInstanceBatch(ruleInstanceId, request);

        return await CsvResult(batch.Items, $"CommandsForRuleInstance_{ruleInstanceId}.csv");
    }

    /// <summary>
    /// Get a single Command
    /// </summary>
    [HttpGet("command", Name = "getCommand")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CommandDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetCommand(string id)
    {
        logger.LogInformation("Get command {id}", id);

        var command = await this.repositoryCommand.GetOne(id);

        if (command is null) return NotFound();

        return base.Ok(new CommandDto(command));
    }

    /// <summary>
    /// Exports all commands after a given point
    /// </summary>
    [Route("exportcommandafter/{ruleId}", Name = "ExportCommandsAfter")]
    [HttpPost]
    [FileResultContentType("text/csv")]
    public async Task<IActionResult> ExportCommandsAfter(string ruleId, BatchRequestDto request)
    {
        var batch = await GetCommandsAfterBatch(ruleId, request);

        return await CsvResult(batch.Items, $"CommandsForSkill_{ruleId}.csv");
    }

    /// <summary>
    /// Post and enable/disable a command to sync Command and Control
    /// </summary>
    [HttpPost("post-to-command", Name = "postToCommand")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> CommandEnable(string commandId, [FromBody] bool enabled)
    {
        auditLogger.LogInformation(User, new() { ["CommandId"] = commandId, ["enabled"] = enabled },
            "Command to Command and Control {enabled} id {id}", (enabled ? "enabled" : "disabled"), commandId);

        int count = await repositoryCommand.EnableSync(commandId, enabled);
        logger.LogInformation("Updated {Count} records while setting command enabled flag", count);

        if (enabled)
        {
            logger.LogInformation("Upsert command to sync {CommandId}", commandId);
            var command = await repositoryCommand.GetOne(commandId);
            if (command is not null)
            {
                if (command.CanSync())
                {
                    var result = await commandService.SendCommandUpdate(command.CreateCommandPostRequest());

                    if (result != System.Net.HttpStatusCode.OK)
                    {
                        return StatusCode((int)result, "Command update did not return success");
                    }
                }
                else
                {
                    return StatusCode((int)System.Net.HttpStatusCode.BadRequest, "Command not allowed to sync");
                }
            }
        }

        return Ok(enabled);
    }

    private async Task<BatchDto<CommandDto>> GetCommandsAfterBatch(string ruleId, BatchRequestDto request)
    {
        Expression<Func<Command, bool>> whereExpression = (v) => true;

        if (ruleId != "all" && ruleId != "none")
        {
            whereExpression = (v) => v.RuleId == ruleId;
        }

        var batch = await this.repositoryCommand.GetAll(
            request.SortSpecifications,
            request.FilterSpecifications,
            whereExpression: whereExpression,
            page: request.Page,
            take: request.PageSize);

        var batchDto = batch.Transform(v => new CommandDto(v));

        return new BatchDto<CommandDto>(batchDto);
    }

    private async Task<FileStreamResult> CsvResult(IEnumerable<CommandDto> data, string fileName)
    {
        return WebExtensions.CsvResult(data.Select(v =>
        {
            dynamic expando = new ExpandoObject();

            expando.Id = v.Id;
            expando.IsTriggered = v.IsTriggered;
            expando.CommandId = v.CommandId;
            expando.CommandName = v.CommandName;
            expando.CommandType = v.CommandType.ToString();
            expando.RuleId = v.RuleId;
            expando.RuleName = v.RuleName;
            expando.RuleInstanceId = v.RuleInstanceId;
            expando.TwinId = v.TwinId;
            expando.TwinName = v.TwinName;
            expando.EquipmentId = v.EquipmentId;
            expando.EquipmentName = v.EquipmentName;
            expando.Value = v.Value;
            expando.StartTime = v.StartTime;
            expando.EndTime = v.EndTime;
            expando.Unit = v.Unit;
            expando.ExternalId = v.ExternalId;
            expando.LastSyncDate = v.LastSyncDate;

            return expando;
        }), fileName);
    }

    private async Task<BatchDto<CommandDto>> GetCommandsForEquipmentBatch(string equipmentId, BatchRequestDto request)
    {
        Expression<Func<Command, bool>> whereExpression = (v) => true;

        if (equipmentId != "all")
        {
            whereExpression = (v) => v.EquipmentId == equipmentId || v.TwinId == equipmentId;
        }

        var batch = await this.repositoryCommand.GetAll(
            request.SortSpecifications,
            request.FilterSpecifications,
            whereExpression: whereExpression,
            page: request.Page,
            take: request.PageSize);

        return new BatchDto<CommandDto>(batch.Transform((v) => new CommandDto(v)));
    }

    private async Task<BatchDto<CommandDto>> GetCommandsForRuleInstanceBatch(string ruleInstanceId, BatchRequestDto request)
    {
        var batch = await this.repositoryCommand.GetAll(
            request.SortSpecifications,
            request.FilterSpecifications,
            whereExpression: (v) => v.RuleInstanceId == ruleInstanceId,
            page: request.Page,
            take: request.PageSize);

        return new BatchDto<CommandDto>(batch.Transform((v) => new CommandDto(v)));
    }
}
