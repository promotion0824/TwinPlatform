using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RulesEngine.Web;
using RulesEngine.Web.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Sources;
using Willow.ServiceBus;
using WillowRules.Logging;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace Willow.Rules.Web.Controllers;

/// <summary>
/// Controller for administrative apis
/// </summary>
/// <remarks>
/// After changing any method here you must run the APIClientGenerator project to update the typescript client
/// </remarks>
[Route("api/[controller]")]
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
public partial class AdminController : ControllerBase
{
    private readonly IUserService userService;
    private readonly ILogger<AdminController> logger;
    private readonly IAuditLogger<UserController> auditLogger;
    private readonly IMemoryCache memoryCache;
    private readonly WillowEnvironment willowEnvironment;
    private readonly IRepositoryRuleExecutions repositoryRuleExecutions;
    private readonly IRepositoryRuleExecutionRequest repositoryRuleExecutionRequest;
    private readonly IRepositoryProgress repositoryProgress;
    private readonly IRepositoryLogEntry repositoryLogEntry;
    private readonly IMessageSenderFrontEnd messageSender;

    /// <summary>
    /// Creates a new <see cref="AdminController"/>
    /// </summary>
    public AdminController(
        IUserService userService,
        ILogger<AdminController> logger,
        IAuditLogger<UserController> auditLogger,
        IMemoryCache memoryCache,
        WillowEnvironment willowEnvironment,
        IRepositoryRuleExecutions repositoryRuleExecutions,
        IRepositoryRuleExecutionRequest repositoryRuleExecutionRequest,
        IRepositoryProgress repositoryProgress,
        IRepositoryLogEntry repositoryLogEntry,
        IMessageSenderFrontEnd messageSender)
    {
        this.userService = userService;
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
        this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
        this.repositoryRuleExecutions = repositoryRuleExecutions ?? throw new ArgumentNullException(nameof(repositoryRuleExecutions));
        this.repositoryRuleExecutionRequest = repositoryRuleExecutionRequest ?? throw new ArgumentNullException(nameof(repositoryRuleExecutionRequest));
        this.repositoryProgress = repositoryProgress ?? throw new ArgumentNullException(nameof(repositoryProgress));
        this.repositoryLogEntry = repositoryLogEntry ?? throw new ArgumentNullException(nameof(repositoryLogEntry));
        this.messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
    }

    /// <summary>
    /// Get rule execution status
    /// </summary>
    [HttpPost("list-rule-executions", Name = "List rule executions")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RuleExecutionDto[]))]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> ListRuleExecutions()
    {
        logger.LogInformation("List rule executions");
        List<RuleExecutionDto> ruleExecutions = new();
        await foreach (var ruleExecution in this.repositoryRuleExecutions.GetAll())
        {
            ruleExecutions.Add(new RuleExecutionDto(ruleExecution));
        }
        return Ok(ruleExecutions);
    }

    /// <summary>
    /// Refresh the twins and models cache
    /// </summary>
    /// <returns>An object containing the correlation Id which can be used to track progress</returns>
    [Route("refresh-cache", Name = "Refresh Cache")]
    [HttpPost]
    [Produces(typeof(string))]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> RefreshADTCache(bool force = false)
    {
        auditLogger.LogInformation(User, new() { ["Forced"] = force }, "Refresh Cache");

        var request = RuleExecutionRequest.CreateCacheRefreshRequest(willowEnvironment.Id, force, User.UserName());

        await messageSender.RequestRuleExecution(request);
        return Ok(request.CorrelationId);
    }

    /// <summary>
    /// Rebuild the search index
    /// </summary>
    /// <returns>An object containing the correlation Id which can be used to track progress</returns>
    [Route("rebuild-search-index", Name = "Rebuild Search Index")]
    [HttpPost]
    [Produces(typeof(string))]
    [Authorize(Policy = nameof(CanViewAdminPage))]
    public async Task<IActionResult> RebuildSearchIndex(bool force = false, bool recreateIndex = false)
    {
        auditLogger.LogInformation(User, new() { ["Forced"] = force, ["RecreateIndex"] = recreateIndex }, "Rebuild search. Recreate index: {recreate}", recreateIndex);

        var request = RuleExecutionRequest.CreateSearchIndexRefreshRequest(willowEnvironment.Id, force, User.UserName(), recreateIndex: recreateIndex);

        await messageSender.RequestRuleExecution(request);
        return Ok(request.CorrelationId);
    }

    /// <summary>
    /// Run diagnostics
    /// </summary>
    /// <returns>An object containing the correlation Id which can be used to track progress</returns>
    [Route("run-diagnostics", Name = "Run Diagnostics")]
    [HttpPost]
    [Produces(typeof(string))]
    [Authorize(Policy = nameof(CanViewAdminPage))]
    public async Task<IActionResult> RunDiagnostics()
    {
        auditLogger.LogInformation(User, "Run Diagnostics");

        var request = RuleExecutionRequest.CreatDiagnosticsRequest(willowEnvironment.Id, User.UserName());

        await messageSender.RequestRuleExecution(request);
        return Ok(request.CorrelationId);
    }

    /// <summary>
    /// Rebuild rule instances for a willow environment, optionally rebuild a single rule
    /// </summary>
    /// <returns>An object containing the correlation Id which can be used to track progress</returns>
    [Route("rebuild-rules", Name = "Rebuild Rules")]
    [HttpPost]
    [Produces(typeof(string))]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> RebuildRules(string ruleId = "", bool force = false)
    {
        auditLogger.LogInformation(User, new() { ["RuleId"] = ruleId, ["Forced"] = force }, "Rebuild rules {ruleId}", ruleId);

        var request = RuleExecutionRequest.CreateRuleExpansionRequest(willowEnvironment.Id, force, User.UserName(), ruleId: ruleId);

        await messageSender.RequestRuleExecution(request);

        return Ok(request.CorrelationId);
    }

    /// <summary>
    /// Rebuild calculated point rule instances for a willow environment
    /// </summary>
    /// <returns>An object containing the correlation Id which can be used to track progress</returns>
    [Route("rebuild-calculated-points", Name = "Rebuild Calculated Points")]
    [HttpPost]
    [Produces(typeof(string))]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> RebuildCalculatedPoints()
    {
        auditLogger.LogInformation(User, "Rebuild calculated points");

        var request = RuleExecutionRequest.CreateRuleExpansionRequest(willowEnvironment.Id, false, User.UserName(), calculatedPointsOnly: true);

        await messageSender.RequestRuleExecution(request);

        return Ok(request.CorrelationId);
    }

    /// <summary>
    /// Execute a range of datetime against live data running rules to create insights
    /// </summary>
    /// <param name="daysAgo">How many days back to go</param>
    /// <returns>An object containing the correlation Id which can be used to track progress</returns>
    [Route("execute-rules", Name = "Execute rules")]
    [HttpPost]
    [Produces(typeof(string))]
    [Authorize(Policy = nameof(CanExecuteRules))]
    public async Task<IActionResult> ExecuteRules(int daysAgo = 28)
    {
        auditLogger.LogInformation(User, new() { ["DaysAgo"] = daysAgo }, "Execute rules");

        var startDate = DateTime.UtcNow.Date.AddDays(-Math.Abs(daysAgo));

        var request = RuleExecutionRequest.CreateBatchExecutionRequest(willowEnvironment.Id, startDate, User.UserName());

        await messageSender.RequestRuleExecution(request);

        return Ok(request.CorrelationId);
    }

    /// <summary>
    /// Execute a range of datetime against live data running a single rule
    /// </summary>
    /// <param name="ruleId">The rule id to execute</param>
    /// <param name="daysAgo">How many days back to go</param>
    /// <param name="deleteInsights">Delete insights first before execution</param>
    /// <returns>An object containing the correlation Id which can be used to track progress</returns>
    [Route("execute-single-rule", Name = "Execute single rule")]
    [HttpPost]
    [Produces(typeof(string))]
    [Authorize(Policy = nameof(CanExecuteRules))]
    public async Task<IActionResult> ExecuteSingleRule(string ruleId, int daysAgo = 7, bool deleteInsights = false)
    {
        auditLogger.LogInformation(User, new() { ["RuleId"] = ruleId, ["DaysAgo"] = daysAgo, ["deleteInsights"] = deleteInsights }, "Execute single rule {id}. Delete insights first: {deleteInsights}", ruleId, deleteInsights);

        var startDate = DateTime.UtcNow.Date.AddDays(-Math.Abs(daysAgo));

        if (deleteInsights)
        {
            var deleteRequest = RuleExecutionRequest.CreateDeleteAllMatchingInsightsRequest(willowEnvironment.Id, ruleId, User.UserName());

            await messageSender.RequestRuleExecution(deleteRequest);
        }

        var request = RuleExecutionRequest.CreateSingleRuleExecutionRequest(willowEnvironment.Id, startDate, ruleId, User.UserName());

        await messageSender.RequestRuleExecution(request);

        return Ok(request.CorrelationId);
    }

    /// <summary>
    /// Get recent progress
    /// </summary>
    /// <returns>The most recent progress records from the database</returns>
    [Route("get-progress", Name = "GetProgress")]
    [HttpGet]
    [Produces(typeof(AdminProgressDto))]
    [Authorize(Policy = nameof(CanViewAdminPage))]
    public async Task<IActionResult> GetProgress()
    {
        var progresses = await repositoryProgress.Get();
        var requests = await repositoryRuleExecutionRequest.Get();
        var result = new AdminProgressDto(progresses, requests);

        return Ok(result);
    }

    /// <summary>
    /// Gets processor logs ordered by timestamp descending
    /// </summary>
    /// <param name="progressId">ProgressId id filter</param>
    /// <param name="limit">How many records to return</param>
    /// <param name="ascending">Whether logs must start at earliest</param>
    /// <param name="level">Which level to filter on</param>
    /// <param name="hoursBack">How many hours back filter on</param>
    /// <returns>An array of log line objects</returns>
    /// <remarks>
    /// This uses an AppInsights workspace Id key which must be configured in app settings
    /// </remarks>
    [Route("get-logs", Name = "GetLogs")]
    [HttpGet]
    [Produces(typeof(BatchDto<LogEntryDto>))]
    [Authorize(Policy = nameof(CanViewAdminPage))]
    public async Task<IActionResult> GetLogs(string progressId = "", int limit = 0, bool ascending = false, string level = "", int hoursBack = 0)
    {
        Expression<Func<LogEntry, bool>> queryExpression = (v) => true;

        if (!string.IsNullOrEmpty(progressId))
        {
            queryExpression = queryExpression.And(v => v.ProgressId == progressId);
        }

        if (!string.IsNullOrEmpty(level))
        {
            queryExpression = queryExpression.And(v => v.Level == level);
        }

        if (hoursBack > 0)
        {
            var startTime = DateTime.Now.AddHours(-hoursBack);
            queryExpression = queryExpression.And(v => v.TimeStamp > startTime);
        }

        int count = await this.repositoryLogEntry.Count(queryExpression);

        var batch = !ascending ?
                     await this.repositoryLogEntry.GetDescending(queryExpression, v => v.TimeStamp, limit: limit > 0 ? limit : null) :
                     await this.repositoryLogEntry.GetAscending(queryExpression, v => v.TimeStamp, limit: limit > 0 ? limit : null);

        var result = batch.Select(v => new LogEntryDto(v)).ToArray();

        return Ok(new BatchDto<LogEntryDto>(result, count));
    }

    /// <summary>
    /// Cancels a the specified job
    /// </summary>
    [Route("cancel-job", Name = "CancelJob")]
    [HttpPost]
    [Produces(typeof(ProgressDto))]
    [Authorize(Policy = nameof(CanManageJobs))]
    public async Task<IActionResult> CancelJob([FromBody] ProgressDto model)
    {
        auditLogger.LogInformation(User, new() { ["Progress"] = model }, "Cancel job");

        var request = RuleExecutionRequest.CreateCancelRequest(willowEnvironment.Id, model.CorrelationId, model.Id);

        await messageSender.RequestRuleExecution(request);

        return Ok(request.CorrelationId);
    }

    /// <summary>
    /// Process ADT twins for enabled instances for the rule
    /// </summary>
    [HttpPost("processCalcPoints", Name = "ProcessCalcPoints")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesDefaultResponseType]
    [Authorize(Policy = nameof(CanEditRules))]
    public async Task<IActionResult> ProcessCalcPoints(string ruleId)
    {
        auditLogger.LogInformation(User, new() { ["RuleId"] = ruleId }, "Requesting processor to process calculated points for {ruleId}", ruleId);

        var processCPsRequest = RuleExecutionRequest.CreateProcessCalculatedPointsRequest(
            customerEnvironmentId: willowEnvironment.Id,
            requestedBy: User.UserName(),
            ruleId: ruleId);

        await messageSender.RequestRuleExecution(processCPsRequest, CancellationToken.None);

        return Ok();
    }
}
