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
/// Controller for model variables
/// </summary>
/// <remarks>
/// After changing any method here you must run the APIClientGenerator project to update the tyepscript client
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = nameof(CanViewRules))]
[ApiExplorerSettings(GroupName = "v1")]
public class MLModelController : ControllerBase
{
    private readonly ILogger<MLModelController> logger;
    private readonly IAuditLogger<MLModelController> auditLogger;
    private readonly IRepositoryMLModel repositoryMLModel;
    private readonly IRepositoryRules repositoryRules;
    private readonly IRepositoryGlobalVariable repositoryGlobalVariable;
    private readonly WillowEnvironment willowEnvironment;
    private readonly IMessageSenderFrontEnd messageSender;
    private readonly IMLService mlService;

    /// <summary>
    /// Creates a new <see cref="MLModelController"/>
    /// </summary>
    public MLModelController(
        IRepositoryMLModel repositoryMLModel,
        IRepositoryRules repositoryRules,
        IRepositoryGlobalVariable repositoryGlobalVariable,
        WillowEnvironment willowEnvironment,
        IMessageSenderFrontEnd messageSender,
        IMLService mlService,
        ILogger<MLModelController> logger,
        IAuditLogger<MLModelController> auditLogger)
    {
        this.repositoryMLModel = repositoryMLModel ?? throw new ArgumentNullException(nameof(repositoryMLModel));
        this.repositoryRules = repositoryRules ?? throw new ArgumentNullException(nameof(repositoryRules));
        this.repositoryGlobalVariable = repositoryGlobalVariable ?? throw new ArgumentNullException(nameof(repositoryGlobalVariable));
        this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
        this.messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        this.mlService = mlService ?? throw new ArgumentNullException(nameof(mlService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
    }

    /// <summary>
    /// Get all models
    /// </summary>
    [HttpPost("MLModels", Name = "MLModels")]
    [Produces(typeof(BatchDto<MLModelDto>))]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetMLModels(BatchRequestDto request)
    {
        var batch = await GetBatch(request);

        return Ok(batch);
    }

    /// <summary>
    /// Get rule references for a model
    /// </summary>
    [HttpPost("GetMLModelReferences", Name = "GetMLModelReferences")]
    [Produces(typeof(RuleReferenceDto[]))]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetMLModelReferences(string id)
    {
        var model = await this.repositoryMLModel.GetOne(id);

        if (model is null) return NotFound();

        var result = new List<RuleReferenceDto>();

        foreach (var item in await repositoryGlobalVariable.MatchGlobalVariableReferences(model.FullName))
        {
            result.Add(new RuleReferenceDto(item));
        }

        foreach (var item in await repositoryRules.MatchRuleReferences(model.FullName))
        {
            result.Add(new RuleReferenceDto(item));
        }

        return Ok(result);
    }

    /// <summary>
    /// Exports all rules
    /// </summary>
    [HttpPost("ExportMLModels", Name = "ExportMLModels")]
    [FileResultContentType("text/csv")]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> ExportRules(BatchRequestDto request)
    {
        var batch = await GetBatch(request);

        return CsvResult(batch, "MLModels.csv");
    }

    /// <summary>
    /// Delete a model variable
    /// </summary>
    [HttpDelete("model-variable/{id}", Name = "deleteMLModel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> DeleteMLModel([FromRoute] string id)
    {
        auditLogger.LogInformation(User, new() { ["ModelId"] = id }, "Delete ml model {id}", id);

        var gv = this.repositoryMLModel.GetModelWithoutBinary(id);

        if (gv is null) return NotFound();

        await this.repositoryMLModel.DeleteOne(gv);

        var gitRequest = RuleExecutionRequest.CreateGitSyncRequest(willowEnvironment.Id,
            requestedBy: User.UserName(), userEmail: User.Email(), ruleId: gv.Id, deleteRule: true, syncFolder: MLModelSource.Folder);

        await messageSender.RequestRuleExecution(gitRequest, CancellationToken.None);

        return Ok();
    }

    /// <summary>
    /// Get a model variable
    /// </summary>
    [HttpGet("MLModel", Name = "getMLModel")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MLModelDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public IActionResult GetMLModel(string id)
    {
        var model = this.repositoryMLModel.GetModel(id);

        if (model is null) return NotFound();

        (_, var validation) = mlService.ValidateModel(model);

        var result = new MLModelDto(model, validation);

        return Ok(result);
    }

    private async Task<Batch<MLModelDto>> GetBatch(BatchRequestDto request)
    {
        var batch = await this.repositoryMLModel.GetAllModels(
            request.SortSpecifications,
            request.FilterSpecifications,
            page: request.Page,
            take: request.PageSize);

        return batch.Transform((v) => new MLModelDto(v, ""));
    }

    private static IActionResult CsvResult(Batch<MLModelDto> batch, string fileName)
    {
        return WebExtensions.CsvResult(batch.Items.Select(v =>
        new
        {
            Id = v.Id,
            FullName = v.FullName,
            Description = v.Description,
            ModelName = v.ModelName,
            ModelVersion = v.ModelVersion
        }), fileName);
    }
}

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
