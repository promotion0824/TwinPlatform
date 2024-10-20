using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Sources;

namespace Willow.Rules.Services;

/// <summary>
/// Progress tracker base class handling when to send and the dictionary of progress fragments
/// </summary>
public abstract class ProgressTrackerBase
{
	protected bool completed = false;
	protected bool failed = false;
	protected string ruleId = "";
	protected string failedReason = string.Empty;

	private long lastSent = DateTimeOffset.Now.Ticks;

	protected IEnumerable<ProgressInner> innerProgress =>
		this.stages.Values.Select(s => new ProgressInner(s.Name, (int)s.Count, (int)s.Total));

	/// <summary>
	/// The stages that we will progress through, each has a count and total
	/// and each may handle multiple sources
	/// </summary>
	protected ConcurrentDictionary<string, ProgressTrackerStage> stages = new();

	protected readonly ILogger logger;

	protected DateTimeOffset StartTime;

	private TimeSpan Elapsed => DateTimeOffset.Now - this.StartTime;

	public string Id { get; }

	public ProgressType ProgressType { get; }

	protected readonly string correlationId;

	protected readonly string requestedBy;

	protected readonly DateTimeOffset dateRequested;

	private readonly IRepositoryProgress repositoryProgress;

	protected async Task Upsert(
		DateTimeOffset startTimeUtc, DateTimeOffset currentTimeUtc, DateTimeOffset endTimeUtc,
		double speed,
		CancellationToken cancellationToken = default)
	{
		var status = completed ? ProgressStatus.Completed : (failed ? ProgressStatus.Failed : ProgressStatus.InProgress);

		var progress = new Progress(this.Id, DateTimeOffset.Now, this.ProgressType, this.Percentage,
			this.innerProgress.ToList(), this.correlationId, "", requestedBy, dateRequested)
		{
			Speed = speed,
			RuleId = ruleId,
			RuleName = "",
			StartTime = this.StartTime,
			Eta = this.Eta,
			StartTimeSeriesTime = startTimeUtc,
			CurrentTimeSeriesTime = currentTimeUtc,
			EndTimeSeriesTime = endTimeUtc,
			Status = status,
			FailedReason = status == ProgressStatus.Failed ? failedReason : string.Empty
		};

		await this.repositoryProgress.UpsertOne(progress, updateCache: false, cancellationToken);
	}

	protected ProgressTrackerBase(
		string id,
		ProgressType progressType,
		string correlationId,
		IRepositoryProgress repositoryProgress,
		string requestedBy,
		DateTimeOffset dateRequested,
		ILogger logger,
		string ruleId = "")
	{
		this.Id = id ?? throw new ArgumentNullException(nameof(id));
		this.ProgressType = progressType;
		this.repositoryProgress = repositoryProgress ?? throw new ArgumentNullException(nameof(repositoryProgress));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.correlationId = correlationId;
		this.StartTime = DateTimeOffset.Now;
		this.requestedBy = requestedBy;
		this.dateRequested = dateRequested;
		this.ruleId = ruleId;
	}

	/// <summary>
	/// Get the total progress percentage estimated from the weighted stage totals
	/// </summary>
	protected virtual double Percentage =>
        this.completed ? 1.0 :
        this.stages.Values.Where(x => !x.IsIgnored).Sum(x => x.Count * x.Weight) /
		Math.Max(0.00001, this.stages.Values.Where(x => !x.IsIgnored).Sum(x => x.Total * x.Weight));

	/// <summary>
	/// Expected completion time
	/// </summary>
	protected DateTimeOffset Eta => this.StartTime + (DateTimeOffset.Now - this.StartTime) /
		Math.Min(1.0, Math.Max(this.Percentage, 0.01));

	/// <summary>
	/// Goes true once every 5 seconds
	/// </summary>
	protected bool TimeToSend()
	{
		DateTimeOffset now = DateTimeOffset.Now;

		long lastTick = lastSent;

		if (lastTick + TimeSpan.FromSeconds(5).Ticks > now.Ticks) return false;

		// If multiple threads make it to here, only one gets to send
		if (Interlocked.CompareExchange(ref lastSent, now.Ticks, lastTick) == lastTick)
		{
			// successfully we are the one
			return true;
		}
		return false;
	}

	/// <summary>
	/// Report the stats if it's time to send (or forced)
	/// </summary>
	protected async Task ReportStats(bool force)
	{
		if (TimeToSend() || force)
		{
			try
			{
				await this.Upsert(DateTimeOffset.Now, DateTimeOffset.Now, DateTimeOffset.Now, 0.0);
			}
			catch (Exception ex)
			{
				// Do not quit for a service bus exception, just keep processing
				logger.LogError(ex, ex.Message);
			}
		}
	}

	/// <summary>
	/// Report the stats if it's time to send (or forced) with real time datetimes too
	/// </summary>
	protected async Task ReportStats(DateTimeOffset startTimeUtc, DateTimeOffset currentTimeUtc, DateTimeOffset endTimeUtc,
		double speed,
		bool force)
	{
		if (TimeToSend() || force)
		{
			try
			{
				await this.Upsert(startTimeUtc, currentTimeUtc, endTimeUtc, speed);
			}
			catch (Exception ex)
			{
				// Do not quit for a service bus exception, just keep processing
				logger.LogError(ex, ex.Message);
			}
		}
	}

	/// <summary>
	/// Report initial empty state
	/// </summary>
	public async Task Start()
	{
		await ReportStats(true);
	}

	/// <summary>
	/// Force percentage to 100%
	/// </summary>
	public async Task Completed()
	{
		this.completed = true;
		await ReportStats(true);
	}

	/// <summary>
	/// Unexpected error or cancellation?
	/// </summary>
	public Task Cancelled(string? reason = null)
	{
		return Failed("Cancelled");
	}

	/// <summary>
	/// Unexpected error or cancellation?
	/// </summary>
	public async Task Failed(string? reason = null)
	{
		this.failed = true;
		this.failedReason = reason ?? "Unexpected Error";
		await ReportStats(true);
	}

	public async Task SetValues(string item, int count, int total, bool isIgnored = true, bool force = false)
	{
		var stage = this.stages.AddOrUpdate(item, k => new ProgressTrackerStage(item, 1) { IsIgnored = isIgnored }, (k, u) => u);
		stage.Track(count, total);
		await ReportStats(force);
	}
}
