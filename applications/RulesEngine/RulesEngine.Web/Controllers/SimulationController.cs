using Microsoft.AspNetCore.Authorization;
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
using WillowRules.DTO;
using WillowRules.Services;

namespace Willow.Rules.Web.Controllers;

/// <summary>
/// Controller for rules simulation api
/// </summary>
[Route("api/[controller]")]
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
public partial class SimulationController : ControllerBase
{
    private readonly ILogger<SimulationController> logger;
    private readonly IRuleSimulationService ruleSimulationService;
    private readonly IRepositoryRules repositoryRules;
    private readonly IRepositoryActorState repositoryActorState;
    private readonly IRepositoryRuleInstances repositoryRuleInstances;
    private readonly IRepositoryInsightChange repositoryInsightChange;

    /// <summary>
    /// Creates a new <see cref="SimulationController"/>
    /// </summary>
    public SimulationController(
        IRuleSimulationService ruleSimulationService,
        IRepositoryRules repositoryRules,
        IRepositoryInsightChange repositoryInsightChange,
        IRepositoryActorState repositoryActorState,
        IRepositoryRuleInstances repositoryRuleInstances,
        ILogger<SimulationController> logger)
    {
        this.ruleSimulationService = ruleSimulationService ?? throw new ArgumentNullException(nameof(ruleSimulationService));
        this.repositoryRules = repositoryRules ?? throw new ArgumentNullException(nameof(repositoryRules));
        this.repositoryInsightChange = repositoryInsightChange ?? throw new ArgumentNullException(nameof(repositoryInsightChange));
        this.repositoryActorState = repositoryActorState ?? throw new ArgumentNullException(nameof(repositoryActorState));
        this.repositoryRuleInstances = repositoryRuleInstances ?? throw new ArgumentNullException(nameof(repositoryRuleInstances));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Simulates rule execution for a given ruleid and equipmentid
    /// </summary>
    [Route("execute-simulation-rule", Name = "ExecuteSimulationForRule")]
    [HttpPost]
    [Produces(typeof(SimulationResultDto))]
    [Authorize(Policy = nameof(CanViewRules))]
    public async Task<IActionResult> ExecuteSimulationForRule([FromBody] RuleSimulationRequest request)
    {
        var id = request.RuleId;
        var equipmentId = request.EquipmentId;
        var startTime = request.StartTime;
        var endTime = request.EndTime;
        var updateRule = request.UpdateRule;
        var data = request.Rule;
        var useExistingData = request.UseExistingData;
        var enableCompression = request.EnableCompression;
        var optimizeCompression = request.OptimizeCompression;
        var showAutoVariables = request.ShowAutoVariables;
        var optimizeExpression = request.OptimizeExpression;
        var applyLimits = request.ApplyLimits;
        var skipMaxPointLimit = request.SkipMaxPointLimit;

        using var disp = logger.BeginScope(new Dictionary<string, object> { ["RuleId"] = id, ["EquipmentId"] = equipmentId });

        logger.LogInformation("Execute simulation for rule {id} and equipment {equipment}", id, equipmentId);

        if (string.IsNullOrEmpty(equipmentId))
        {
            return Ok(new SimulationResultDto()
            {
                Error = "equipmentId required"
            });
        }

        TimeSpan timeout = TimeSpan.FromMinutes(3);

        startTime = startTime.Date;
        endTime = endTime.Date.AddDays(1);

        try
        {
            using (var tokenSource = new CancellationTokenSource(timeout))
            {
                Rule rule = null;
                GlobalVariable global = null;

                if (!string.IsNullOrEmpty(id))
                {
                    rule = await this.repositoryRules.GetOne(id, updateCache: false);
                }

                //Typically from rule page 
                if (updateRule && rule is not null)
                {
                    rule.Update(data, out var validations);

                    if (validations.Any())
                    {
                        return Ok(new SimulationResultDto()
                        {
                            Error = string.Join(",", validations.Select(v => $"{v.field}:{v.message}"))
                        });
                    }
                }

                //ADT calc points for example don't have rules
                if (rule is null)
                {
                    rule = new Rule()
                    {
                        Id = !string.IsNullOrEmpty(id) ? id : data.Id,
                        TemplateId = data.TemplateId
                    };

                    foreach (var parameter in data.Parameters)
                    {
                        rule.Parameters.Add(new RuleParameter(parameter.Name, parameter.FieldId, parameter.PointExpression, parameter.Units, parameter.CumulativeSetting));
                    }

                    rule.Update(data, out _);
                }

                if (request.Global is not null)
                {
                    global = new GlobalVariable();

                    global.Update(request.Global, out var validations);

                    if (validations.Any())
                    {
                        return Ok(new SimulationResultDto()
                        {
                            Error = $"Global validation failed: {string.Join(",", validations.Select(v => $"{v.field}:{v.message}"))}"
                        });
                    }
                }

                if (string.IsNullOrEmpty(rule.Id))
                {
                    return Ok(new SimulationResultDto()
                    {
                        Error = "rule id required"
                    });
                }

                (var pointLog, var ruleInstance, var actor, var insight, var commands, var template) = await ruleSimulationService.ExecuteRule(
                    rule,
                    equipmentId,
                    startTime,
                    endTime,
                    useExistingData,
                    enableCompression: enableCompression,
                    optimizeCompression: optimizeCompression,
                    global: global,
                    generatePointTracking: request.GeneratePointTracking,
                    optimizeExpressions: optimizeExpression,
                    applyLimits: applyLimits,
                    skipMaxPointLimit: skipMaxPointLimit,
                    token: tokenSource.Token);

                string error = string.Empty;
                string warning = string.Empty;

                RuleInstanceDto ruleInstanceDto = null;

                if (ruleInstance is not null)
                {
                    ruleInstanceDto = new RuleInstanceDto(ruleInstance, new RuleInstanceMetadata(), canViewRule: true);
                }

                InsightDto insightDto = null;

                if (insight is not null)
                {
                    insightDto = new InsightDto(insight);
                }

                var boundParams = ruleInstance.GetAllBoundParameters();

                var invalidParams = boundParams
                                       .Concat(ruleInstance.RuleFiltersBound)
                                       .Where(v => v.Status.HasFlag(RuleInstanceStatus.BindingFailed) || v.Status.HasFlag(RuleInstanceStatus.ArrayUnexpected))
                                       .Select(v => $"{v.FieldId}: {v.PointExpression}");

                if (invalidParams.Any())
                {
                    error = $"{ruleInstance.Status}\n {string.Join("\n", invalidParams)}";
                }
                else if (actor is null)
                {
                    error = $"No data found. Please check data range.";
                }
                else if (actor.TimedValues.Count() == 0)
                {
                    error = $"Invalid data. Please check occurrences output.";
                }

                if (!skipMaxPointLimit && ruleInstance.PointEntityIds.Count > RuleSimulationService.MaxPointLimit)
                {
                    warning += $"Results have been limited to 24hrs due to >{RuleSimulationService.MaxPointLimit} capabilities ({ruleInstance.PointEntityIds.Count}). ";
                }

                if (tokenSource.Token.IsCancellationRequested)
                {
                    warning += "Your request did not fully complete within the allowed duration. Please run a smaller time range. ";
                }

                if (!string.IsNullOrEmpty(error))
                {
                    return Ok(new SimulationResultDto()
                    {
                        Error = error,
                        Warning = warning,
                        RuleInstance = ruleInstanceDto,
                        Insight = insightDto
                    });
                }

                var insightChanges = await repositoryInsightChange.Get(v => v.InsightId == ruleInstance.Id);

                TimeSeriesDataDto timeSeriesData = null;

                if (ruleInstance is not null && actor is not null)
                {
                    timeSeriesData = ruleInstance.GetTimeseriesDataForRuleInstance(
                        actor,
                        startTime,
                        endTime,
                        template,
                        insight,
                        changes: insightChanges,
                        commands: commands,
                        pointLog: pointLog);
                }

                if (!showAutoVariables)
                {
                    timeSeriesData.Trendlines = timeSeriesData.Trendlines
                        .Where(t => !boundParams.Any(p => p.IsAutoGenerated && p.FieldId == t.Id))
                        .ToArray();
                }

                return Ok(new SimulationResultDto()
                {
                    Error = error,
                    Warning = warning,
                    RuleInstance = ruleInstanceDto,
                    Insight = insightDto,
                    TimeSeriesData = timeSeriesData,
                    Commands = commands is not null ? commands.Select(v => new CommandDto(v)).ToArray() : new CommandDto[] { },
                });
            }
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation timed out for {ruleId}. Simulations are limited to {time} minutes", id, timeout.TotalMinutes);

            return Ok(new SimulationResultDto()
            {
                Error = $"Operation timed out. Simulations are limited to {timeout.TotalMinutes} minutes"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Simulation failed for {ruleId}.", id);

            return Ok(new SimulationResultDto()
            {
                Error = ex.Message
            });
        }
    }
}
