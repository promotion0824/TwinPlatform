using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RulesEngine.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Cache;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using Willow.Rules.Sources;
using Willow.ServiceBus;
using WillowRules.DTO;
using WillowRules.Logging;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace Willow.Rules.Web.Controllers;

/// <summary>
/// Controller for uploading or downlloading rules
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = nameof(CanViewRules))]
[ApiExplorerSettings(GroupName = "v1")]
public class RuleUploadController : ControllerBase
{
    private readonly ILogger<RuleUploadController> logger;
    private readonly IAuditLogger<RuleUploadController> auditLogger;
    private readonly WillowEnvironment willowEnvironment;
    private readonly IMessageSenderFrontEnd messageSender;
    private readonly IFileService fileService;
    private readonly IEpochTracker epochTracker;

    /// <summary>
    /// Creates a new <see cref="RuleUploadController"/>
    /// </summary>
    public RuleUploadController(
        ILogger<RuleUploadController> logger,
        IAuditLogger<RuleUploadController> auditLogger,
        WillowEnvironment willowEnvironment,
        IMessageSenderFrontEnd messageSender,
        IFileService fileService,
        IEpochTracker epochTracker)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
        this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
        this.messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        this.epochTracker = epochTracker ?? throw new ArgumentNullException(nameof(epochTracker));
    }

    private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
        Converters = new List<JsonConverter> { new TokenExpressionJsonConverter() },
        NullValueHandling = NullValueHandling.Ignore,
        TypeNameHandling = TypeNameHandling.Auto
    };

    /// <summary>
    /// Upload one or more rules in a JSON file
    /// </summary>
    [HttpPost("upload", Name = "Upload")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RuleUploadResultDto))]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanViewAdminPage))]
    [RequestSizeLimit(100_000_000)]//100MB
    public async Task<IActionResult> Upload(bool saveRules = true, bool saveGlobals = true, bool saveMLModels = false)
    {
        Dictionary<string, object> logScope = new()
        {
            ["SaveRules"] = saveRules,
            ["SaveGlobals"] = saveGlobals,
            ["SaveMLModels"] = saveMLModels
        };

        auditLogger.LogInformation(User, logScope, "Upload rules");

        //TODO: Review uploader (Uploady) to manage batch operations and batch level response data.
        //Currently the uploader sender sends one file at the time with no batch level response only on individual batch items

        var files = HttpContext.Request.Form.Files;

        long size = files.Sum(f => f.Length);

        logger.LogInformation("Upload bytes {Size}", size);

        var processResults = new List<FileService.ProcessResult>();

        foreach (var formFile in files.Where(v => v.Length > 0))
        {
            try
            {
                string tmpDirectory = Path.GetTempPath();
                string filePath = Path.Combine(tmpDirectory, formFile.FileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await formFile.CopyToAsync(stream);
                }

                logger.LogInformation("Uploaded file {filePath}", filePath);

                var sources = new List<FileServiceSourceType>();

                if (saveRules)
                {
                    sources.Add(FileServiceSourceType.Rule);
                }

                if (saveGlobals)
                {
                    sources.Add(FileServiceSourceType.Global);
                }

                if (saveMLModels)
                {
                    sources.Add(FileServiceSourceType.MLModel);
                }

                var result = await fileService.UploadRules(filePath, User.UserName(), save: true, sourceTypes: sources.ToArray());

                processResults.Add(result);

                // And remove the temporary file
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Could not remove temporary file {filePath}", filePath);
                }

                epochTracker.InvalidateCache();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }
        var uploadDto = new RuleUploadResultDto
        {
            Failures = processResults.SelectMany(pr => pr.Failures),
            Duplicates = processResults.SelectMany(pr => pr.Duplicates),
            ProcessedCount = processResults.Sum(pr => pr.ProcessedCount),
            UniqueCount = processResults.Sum(pr => pr.UniqueCount),
            DuplicateCount = processResults.Sum(pr => pr.DuplicateCount),
            FailureCount = processResults.Sum(pr => pr.FailureCount),
            Success = processResults.TrueForAll(pr => pr.Success)
        };

        //at least one sucessful change
        if (uploadDto.UniqueCount > 0)
        {
            // Do git sync
            logger.LogInformation("Requesting processor to perform Git sync");
            var gitRequest = RuleExecutionRequest.CreateGitSyncRequest(willowEnvironment.Id,
                requestedBy: User.UserName(), userEmail: User.Email(), uploadedRules: true);
            await messageSender.RequestRuleExecution(gitRequest, CancellationToken.None);
        }

        return Ok(uploadDto);
    }

    /// <summary>
    /// Get download rules token
    /// </summary>
    [HttpGet("downloadtoken", Name = "GetTokenForRulesDownload")]
    [ProducesResponseType(typeof(ShortLivedTokenDto), StatusCodes.Status200OK)]
    [Authorize(Policy = nameof(CanExportRules))]
    public async Task<IActionResult> GetTokenForRulesDownload()
    {
        string token = fileService.GetShortLivedToken();
        return Ok(new ShortLivedTokenDto { Token = token });
    }
}
