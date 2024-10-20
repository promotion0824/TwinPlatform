using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Model;

namespace RulesEngine.Web;

/// <summary>
/// Tracks progress by the processor back-end, may be rendered by the front-end
/// </summary>
public class ProgressDto
{
	/// <summary>
	/// Creates a <see cref="ProgressDto" /> from an <see cref="Progress" />
	/// </summary>
	public ProgressDto(Progress progress)
	{
		this.Id = progress.Id;
		this.Type = progress.Type;
		this.EntityId = progress.EntityId;
		this.CorrelationId = progress.CorrelationId;
		this.LastUpdated = progress.LastUpdated;
		this.Percentage = progress.Percentage;
		this.StartTime = progress.StartTime;
		this.Eta = progress.Eta;
		this.RuleId = progress.RuleId;
		this.Speed = progress.Speed;
		this.StartTimeSeriesTime = progress.StartTimeSeriesTime;
		this.CurrentTimeSeriesTime = progress.CurrentTimeSeriesTime;
		this.EndTimeSeriesTime = progress.EndTimeSeriesTime;
		this.IsRealtime = progress.Id == Progress.RealtimeExecutionId;
		this.InnerProgress = progress.InnerProgress?.Select(x => new ProgressInner(x.ItemName, x.CurrentCount, x.TotalCount)).ToList() ?? new();
        this.RequestedBy = progress.RequestedBy;
        this.DateRequested = progress.DateRequested;
        this.Status = progress.Status;
        this.FailedReason = progress.FailedReason;
        this.Timeout = Status == ProgressStatus.InProgress && (DateTime.Now - LastUpdated).TotalMinutes > 8;
        this.CanCancel = !IsRealtime && Status == ProgressStatus.InProgress;
    }

    /// <summary>
	/// Creates a <see cref="ProgressDto" /> from an <see cref="Progress" />
	/// </summary>
	public ProgressDto(RuleExecutionRequest request)
    {
        this.Id = request.ProgressId;
        this.Type = request.ProgressType;
        this.CorrelationId = request.CorrelationId;
        this.RuleId = request.RuleId;
        this.InnerProgress = new List<ProgressInner>();
        this.RequestedBy = request.RequestedBy;
        this.DateRequested = request.RequestedDate;
        this.LastUpdated = request.RequestedDate;
        this.IsRealtime = request.ProgressId == Progress.RealtimeExecutionId;

        if (request.StartDate.HasValue)
        {
            this.StartTimeSeriesTime = request.StartDate.Value;
            this.StartTime = request.StartDate.Value;
            this.Eta = request.StartDate.Value;
            this.StartTimeSeriesTime = request.StartDate.Value;
            this.CurrentTimeSeriesTime = request.StartDate.Value;
            this.EndTimeSeriesTime = request.StartDate.Value;
        }
        this.CorrelationId = request.CorrelationId;
        this.Queued = true;
        this.CanCancel = true;
    }

    [System.Text.Json.Serialization.JsonConstructor]
	[Newtonsoft.Json.JsonConstructor]
	private ProgressDto()
	{
	}

    /// <summary>
    /// Is this a queued request
    /// </summary>
    public bool Queued { get; set; }

    /// <summary>
    /// The status of the progress
    /// </summary>
    public ProgressStatus Status { get; set; }

    /// <summary>
	/// The reason for failure
	/// </summary>
	public string FailedReason { get; set; }

    /// <summary>
    /// The Id
    /// </summary>
    public string Id { get; set; }

	/// <summary>
	/// Type of the progress item
	/// </summary>
	public ProgressType Type { get; set; }

	/// <summary>
	/// Whether it's realtime execution
	/// </summary>
	public bool IsRealtime { get; set; }

    /// <summary>
	/// Is timeout
	/// </summary>
	public bool Timeout { get; set; }

    /// <summary>
    /// Can be cancelled
    /// </summary>
    public bool CanCancel { get; set; }

    /// <summary>
    /// Progress results for the inner calulations, e.g. Twins, Relationships, ...
    /// </summary>
    public List<ProgressInner> InnerProgress { get; }

	/// <summary>
	/// If the progress is for a specific entity, that's specified here, otherwise ""
	/// </summary>
	public string EntityId { get; set; }

	/// <summary>
	/// Correlation Id for progress that is in response to a request to do work
	/// </summary>
	public string CorrelationId { get; set; }

	/// <summary>
	/// Timestamp for sorting to find most recent progress for any type
	/// </summary>
	/// <remarks>
	/// This field should be indexed
	/// </remarks>
	public DateTimeOffset LastUpdated { get; set; }

	/// <summary>
	/// Percentage progress
	/// </summary>
	public double Percentage { get; set; }

	/// <summary>
	/// Server started at this time
	/// </summary>
	public DateTimeOffset StartTime { get; set; }

	/// <summary>
	/// Server expects to end at this time
	/// </summary>
	public DateTimeOffset Eta { get; set; }

	/// <summary>
	/// Rule Id
	/// </summary>
	public string RuleId { get; set; }

	/// <summary>
	/// Rule Name
	/// </summary>
	public string RuleName { get; set; }

	/// <summary>
	/// Rule execution speed as a x on real-time
	/// </summary>
	public double Speed { get; init; }

	/// <summary>
	/// Start of tTime series execution time
	/// </summary>
	public DateTimeOffset StartTimeSeriesTime { get; set; }

	/// <summary>
	/// Time series execution time
	/// </summary>
	public DateTimeOffset CurrentTimeSeriesTime { get; set; }

	/// <summary>
	/// End of time series execution time
	/// </summary>
	public DateTimeOffset EndTimeSeriesTime { get; set; }

    /// <summary>
	/// The user that requested the job
	/// </summary>
	public string RequestedBy { get; set; }

    /// <summary>
    /// The time the user requested
    /// </summary>
    public DateTimeOffset DateRequested { get; set; }

}

/// <summary>
/// Used in a progress update to represent inner details of the state
/// </summary>
/// <remarks>
/// Counts and totals for any components of the overall progress
/// </remarks>
public class ProgressInnerDto
{
	/// <summary>
	/// Creates a new <see cref="ProgressInnerDto" />
	/// </summary>
	public ProgressInnerDto(string name, int count, int total)
	{
		this.ItemName = name;
		this.CurrentCount = count;
		this.TotalCount = total;
	}

	/// <summary>
	/// Serialization constructor
	/// </summary>
	public ProgressInnerDto()
	{
	}

	/// <summary>
	/// Name of the item, e.g. Twins, Relationships, RuleInstances, ...
	/// </summary>
	public string ItemName { get; set; } = "NOT SET";

	/// <summary>
	/// Count of how many we have processed so far
	/// </summary>
	public int CurrentCount { get; set; }

	/// <summary>
	/// Count of how many there are in total
	/// </summary>
	public int TotalCount { get; set; }
}
