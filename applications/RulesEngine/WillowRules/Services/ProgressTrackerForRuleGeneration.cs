using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Sources;

namespace Willow.Rules.Services;

/// <summary>
/// Tracks progress on rule generation and predicts ETA
/// </summary>
public class ProgressTrackerForRuleGeneration : ProgressTrackerBase
{
	private ProgressTrackerStage CalculatedPointsStage = new ProgressTrackerStage("Calculated points", 20.0, 1000);
	private ProgressTrackerStage RuleInstances2Stage = new ProgressTrackerStage("Instances", 20.0, 10000);
	private ProgressTrackerStage ExcludedStage = new ProgressTrackerStage("Excluded", 1.0, 0);

	public ProgressTrackerForRuleGeneration(IRepositoryProgress repositoryProgress, string correlationId, string requestedBy, DateTimeOffset dateRequested, string ruleId, ILogger logger)
		: base(Progress.RuleExpansionId, ProgressType.RuleGeneration, correlationId, repositoryProgress, requestedBy, dateRequested, logger, ruleId: ruleId)
	{
		this.stages = new()
		{
			[this.RuleInstances2Stage.Name] = this.RuleInstances2Stage,
			[this.CalculatedPointsStage.Name] = this.CalculatedPointsStage
		};
	}

	public async Task SetCalculatedPointsProcessed(int c, int total)
	{
		this.CalculatedPointsStage.Track(c, total);
		await ReportStats(false);
	}

	public async Task SetNoCalculatedPointsProcessed()
	{
		this.stages.TryRemove(this.CalculatedPointsStage.Name, out var _);
		await ReportStats(true);
	}

	public async Task SetNoRuleInstanceProcessed()
	{
		this.stages.TryRemove(this.RuleInstances2Stage.Name, out var _);
		await ReportStats(true);
	}

	public async Task SetInstancesProcessed2(int count, int total)
	{
		this.RuleInstances2Stage.Track(count, total);
		await ReportStats(false);
	}

	/// <summary>
	/// Sets how many twins have been excluded
	/// </summary>
	public async Task SetTwinsExcluded(int count)
	{
		this.stages[this.ExcludedStage.Name] = this.ExcludedStage;
		this.ExcludedStage.Track(count, count);
		await this.ReportStats(false);
	}
}
