using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using RulesEngine.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.HealthChecks;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Sources;

namespace Willow.Rules.Web.Controllers;

/// <summary>
/// Controller for customer environment summaries
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = nameof(CanViewModels))]
[ApiExplorerSettings(GroupName = "v1")]
public class SummaryController : ControllerBase
{
    private readonly ILogger<SummaryController> logger;
    private readonly WillowEnvironment willowEnvironment;
    private readonly IMemoryCache memoryCache;
    private readonly IRepositoryADTSummary repositorySummary;
    private readonly IRepositoryProgress repositoryProgress;
    private readonly IHealthCheckPublisher healthCheckPublisher;

    /// <summary>
    /// Creates a new <see cref="SummaryController"/>
    /// </summary>
    public SummaryController(
        ILogger<SummaryController> logger,
        WillowEnvironment willowEnvironment,
        IMemoryCache memoryCache,
        IRepositoryADTSummary repositorySummary,
        IRepositoryProgress repositoryProgress,
        IHealthCheckPublisher healthCheckPublisher)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
        this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        this.repositorySummary = repositorySummary ?? throw new ArgumentNullException(nameof(repositorySummary));
        this.repositoryProgress = repositoryProgress ?? throw new ArgumentNullException(nameof(repositoryProgress));
        this.healthCheckPublisher = healthCheckPublisher ?? throw new ArgumentNullException(nameof(healthCheckPublisher));
    }

    /// <summary>
    /// Get a summary of execution progress
    /// </summary>
    /// <returns></returns>
    [HttpGet("Progress", Name = "Progress")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProgressSummaryDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetProgress()
    {
        var all = await repositoryProgress.Get();
        var result = new ProgressSummaryDto(all);

        return Ok(result);
    }

    /// <summary>
    /// Get a summary of twin counts
    /// </summary>
    /// <returns></returns>
    [HttpGet("Summary", Name = "Summary")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ADTSummaryDto[]))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetSummary()
    {
        List<ADTSummary> all = new();
        await foreach (var summary in this.repositorySummary.GetAll())
        {
            all.Add(summary);
        }
        var top20 = all.OrderByDescending(x => x.AsOfDate).Take(20);
        return Ok(top20.Select(v => new ADTSummaryDto(v)).ToArray());
    }

    /// <summary>
    /// Get a summary of the whole system
    /// </summary>
    /// <returns></returns>
    [HttpGet("SystemSummary", Name = "SystemSummary")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SystemSummaryDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetSystemSummary()
    {
        var result = await memoryCache.GetOrCreateAsync<SystemSummaryDto>("systemSummary", async (c) =>
        {
            c.SetAbsoluteExpiration(TimeSpan.FromSeconds(10));  // prevent DOS

            HealthCheckDto[] health = null;
            if (healthCheckPublisher is HealthCheckPublisher healthPub)  // nasty cast to concrete type
            {
                health = healthPub.GetAll().ToArray();  // flattened
            }

            var adtSummary = await this.repositorySummary.GetLatest();
            var systemSummary = adtSummary.SystemSummary ?? new Rules.DTO.SystemSummary();

            var ruleInstanceSummary = systemSummary.RuleInstanceSummary;
            var timeSeriesSummary = systemSummary.TimeSeriesSummary;
            var insightSummary = systemSummary.InsightSummary;
            var commandSummary = systemSummary.CommandSummary;

            var result = new SystemSummaryDto
            {
                ADTAsOfDate = adtSummary.AsOfDate,
                CountTwins = adtSummary.CountTwins,
                CountRelationships = adtSummary.CountRelationships,
                CountCalculatedPoints = ruleInstanceSummary.TotalCalcPoints,
                CountRules = systemSummary.CountRules,
                CountRuleInstances = ruleInstanceSummary.Total - ruleInstanceSummary.TotalCalcPoints,
                CountCapabilities = adtSummary.CountCapabilities,
                CountCommandInsights = insightSummary.TotalEnabled,
                CountDataQualityReports = 0,
                CountInsightsFaulted = insightSummary.TotalFaulted,
                CountInsightsHealthy = insightSummary.TotalValidNotFaulted,
                CountInsightsInValid = insightSummary.TotalInvalid,
                InsightsByModel = insightSummary.InsightsByModel,
                CountCommands = commandSummary.Total,
                CountCommandsTriggering = commandSummary.TotalTriggered,
                CommandsByModel = commandSummary.CommandsByModel,
                CountLiveData = timeSeriesSummary.Total,
                CountTimeSeriesBuffers = timeSeriesSummary.TotalWithTwins,
                Speed = systemSummary.Speed,
                LastTimeStamp = systemSummary.LastTimeStamp,
                Health = health,
                ModelSummary = ADTModelSummary.ModelSummaries(adtSummary)
            };

            return result;
        });

        return Ok(result);
    }
}
