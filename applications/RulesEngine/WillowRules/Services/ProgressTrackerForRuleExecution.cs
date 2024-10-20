using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Willow.Rules.DTO;
using Willow.Rules.Model;
using Willow.Rules.Repository;

namespace Willow.Rules.Services;

/// <summary>
/// Tracks progress on rule execution and predicts ETA
/// </summary>
public class ProgressTrackerForRuleExecution : ProgressTrackerBase
{
	private readonly ProgressTrackerStage running = new ProgressTrackerStage("Lines read", 1, 1000000) { IsIgnored = true };
	private readonly ProgressTrackerStage flushing = new ProgressTrackerStage("Flushing", 1, 100000) { };
	private readonly ProgressTrackerStage loadingActors = new ProgressTrackerStage("Actors", 1, 10000) { };
	private readonly ProgressTrackerStage loadingRuleInstances = new ProgressTrackerStage("Skill Instances", 1, 10000) { };

	public ProgressTrackerForRuleExecution(
		string ruleId,
		string progressId,
		ProgressType progressType,
		string correlationId,
		IRepositoryProgress repositoryProgress,
		string requestedBy,
		DateTimeOffset dateRequested,
		ILogger logger) :
			base(progressId, progressType, correlationId, repositoryProgress, requestedBy, dateRequested, logger, ruleId: ruleId)
	{
		this.stages["Running"] = running;
	}

	public async Task Starting(DateTimeOffset startRealtime, DateTimeOffset endRealtime)
	{
		await base.ReportStats(startRealtime, startRealtime, endRealtime, 0.0, true);
	}

	private double percentageRealtime = 0.0;

	// Guess for now that flushing is 10% of real-time, base percentage uses only flushing
	protected override double Percentage => this.completed ? 1.0 : (this.percentageRealtime * 0.9 + base.Percentage * 0.1);

	private double speed = 0.0;

	private long linesProcessed = 0;

	DateTimeOffset startRealtime;
	DateTimeOffset endRealtime;
	DateTimeOffset now;

	public void AddToSummary(SystemSummary summary)
	{
		summary.Speed = speed;
		summary.LastTimeStamp = endRealtime;
	}

	public async Task ReportProgress(DateTimeOffset startRealtime, DateTimeOffset now, DateTimeOffset endRealtime,
		long linesRead, double speed)
	{
		this.startRealtime = startRealtime;
		this.now = now;
		this.endRealtime = endRealtime;

		this.linesProcessed = linesRead;
		this.speed = speed;

		this.percentageRealtime =
			endRealtime > startRealtime ? (now - startRealtime).TotalHours / (endRealtime - startRealtime).TotalHours
			: 0.0;

		// TODO: Really need a way for stages to also be doubles and to track realtime as a stage

		this.running.Track("", linesRead, linesRead);
		this.stages.TryRemove("Actors", out var _);
		this.stages.TryRemove("Instances", out var _);
		await base.ReportStats(startRealtime, now, endRealtime, speed, false);
	}

	public async Task ReportFlushingInsights(int flushCount, int flushTotal)
	{
		this.stages["Flushing"] = flushing;
		this.flushing.Track("Insights", flushCount, flushTotal);
		await this.ReportStats(this.startRealtime, this.now, this.endRealtime, this.speed, false);
	}

	public async Task ReportFlushingCommands(int flushCount, int flushTotal)
	{
		this.stages["Flushing"] = flushing;
		this.flushing.Track("Commands", flushCount, flushTotal);
		await this.ReportStats(this.startRealtime, this.now, this.endRealtime, this.speed, false);
	}

	public async Task ReportFlushingTimeseries(int count, int total)
	{
		if (count == 0 || count == total || count % 100 == 0)  // optimization
		{
			this.stages["Flushing"] = flushing;
			this.flushing.Track("TimeSeries", count, total);
			await this.ReportStats(this.startRealtime, this.now, this.endRealtime, this.speed, false);
		}
	}

	public async Task ReportFlushingActors(int count, int total)
	{
		if (count == 0 || count == total || count % 100 == 0)  // optimization
		{
			this.stages["Flushing"] = flushing;
			this.flushing.Track("Actors", count, total);
			await this.ReportStats(this.startRealtime, this.now, this.endRealtime, this.speed, false);
		}
	}

	public async Task ReportLoadingActors(int count, int total)
	{
		if (count == 0 || count == total || count % 100 == 0)  // optimization
		{
			this.stages["Actors"] = loadingActors;
			this.loadingActors.Track(count, total);
			await this.ReportStats(this.startRealtime, this.now, this.endRealtime, this.speed, false);
		}
	}

	public async Task ReportLoadingRuleInstances(int count, int total)
	{
		if (count == 0 || count == total || count % 100 == 0)  // optimization
		{
			this.stages["Instances"] = loadingRuleInstances;
			this.loadingRuleInstances.Track(count, total);
			await this.ReportStats(this.startRealtime, this.now, this.endRealtime, this.speed, false);
		}
	}

	public async Task ReportFinished(long linesRead)
	{
		this.completed = true;
		var finishedStage = new ProgressTrackerStage("Finished", 1.0);
		finishedStage.Track("", linesRead, linesRead);
		this.stages["Finished"] = finishedStage;
		this.stages.TryRemove("Running", out var st);
		this.stages.TryRemove("Flushing", out var st2);
		await this.ReportStats(this.startRealtime, this.now, this.endRealtime, this.speed, true);
	}

	public async Task ReportWaitingForSync(int count)
	{
		var flushingStage = new ProgressTrackerStage("Flushing Insight Queue", 1.0);
		this.completed = false;
		flushingStage.Track("", count, count);
		this.stages["Flushing"] = flushingStage;
		await this.ReportStats(this.startRealtime, this.now, this.endRealtime, this.speed, false);
	}
}
