using System;
using Willow.Rules.Repository;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Willow.Rules.Model;

/// <summary>
/// Status of a currently executing rules engine run, or a completed run
/// </summary>
/// <remarks>
/// An existing rule execution can be extended forward in time, but cannot be extended backwards.
/// You would have to start again from the beginning replaying the data. If a rule changes
/// the RuleExecution record for that Id should be deleted and re-run as necessary.
/// 
/// For example, if we ask for one week back on a rule that will start a RuleExecution.
/// If we ask for one month back, that will reset the RuleExecution to a new one.
/// If we ask for an update for the last 24 hours, that may extend an existing RuleExecution. 
/// 
/// </remarks>
public class RuleExecution : IId
{
	/// <summary>
	/// Id for database (Guid) or customer environment + rule id ?
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// Id of the customer environment (allows for, but does not require, a multi-tenant rules engine)
	/// </summary>
	public string CustomerEnvironmentId { get; init; }

	/// <summary>
	/// The id of the rule or "" for all rules
	/// </summary>
	public string RuleId { get; init; }

	/// <summary>
	/// If the generation changes you can't write back to it any more and should abandon the current
	/// calculation. The generation changes when a request comes in with an earlier start time.
	/// </summary>
	public Guid Generation { get; set; }

	/// <summary>
	/// The percentage of execution complete
	/// </summary>
	public double Percentage { get; set; }

	/// <summary>
	/// The percentage of execution reported
	/// </summary>
	public double PercentageReported { get; set; } = 0.0;

	/// <summary>
	/// The start date
	/// </summary>
	/// <remarks>
	/// This never changes, a request starting after this start date is amalgamated into this request.
	/// A request starting before this date triggers a new calculation to start from scratch.
	/// </remarks>
	public DateTimeOffset StartDate { get; set; }

	/// <summary>
	/// The end date that has been calculated thus far
	/// </summary>
	/// <remarks>
	/// This tracks the calculation as it proceeds, but because it is only recorded occasionally it
	/// may be behind the point at which the rules engine reached. This isn't an issue as each rule
	/// instance will skip any old data presented to it until it catches up itself.
	/// </remarks>
	public DateTimeOffset CompletedEndDate { get; set; }

	/// <summary>
	/// The end date that we want to run rule evaluation up to. This may be extended any number of times
	/// and execution will resume from the <see cref="CompletedEndDate"/> going forward until this date is hit.
	/// </summary>
	/// <remarks>
	/// This can be bumped forward at any time, but if the start date goes backwards, it's a new job / generation.
	/// </remarks>
	public DateTimeOffset TargetEndDate { get; set; }

	/// <summary>
	/// Bumps the end date forward in time (or returns instance unchanged if attempted to bump backwards)
	/// </summary>
	public RuleExecution BumpEndDate(DateTimeOffset startDate, DateTimeOffset endDate)
	{
		if (startDate < this.StartDate) throw new ArgumentException("Cannot bump backwards in time, only forwards");
		// Already covering this end date
		if (endDate < this.TargetEndDate) return this;

		this.TargetEndDate = endDate;
		return this;
	}

	/// <summary>
	/// Does the execution engine need to restart from the new start date or can it continue?
	/// </summary>
	public bool NeedsToRestart(DateTimeOffset newStartDate, DateTimeOffset newEndDate) => newStartDate < this.StartDate;

	/// <summary>
	/// Do two execution requests overlap?
	/// </summary>
	public bool Overlaps(RuleExecution other)
	{
		if (this.TargetEndDate < other.StartDate) return false;
		if (this.StartDate > other.TargetEndDate) return false;
		return true;
	}

	/// <summary>
	/// Does this rule execution cover the other one?
	/// </summary>
	public bool Consumes(RuleExecution other)
	{
		if (this.TargetEndDate < other.TargetEndDate) return false;
		if (this.StartDate > other.StartDate) return false;
		return true;
	}
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
