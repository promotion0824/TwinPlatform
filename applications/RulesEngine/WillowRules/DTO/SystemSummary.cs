using System;
using System.Collections.Generic;
using System.Linq;
using Willow.HealthChecks;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

// EF
#nullable disable

namespace Willow.Rules.DTO;

/// <summary>
/// Information about the state of the Rules Engine system
/// </summary>
public class SystemSummary
{
	/// <summary>
	/// Creates a new instance of <see cref="SystemSummary"/>
	/// </summary>
	public SystemSummary()
	{
	}

	/// <summary>
	/// How many rules
	/// </summary>
	public int CountRules { get; set; }

	/// <summary>
	/// How many data quality reports being sent
	/// </summary>
	public int CountDataQualityReports { get; set; }

	/// <summary>
	/// Speed of execution
	/// </summary>
	public double Speed { get; set; }

	/// <summary>
	/// Last time stamp processed
	/// </summary>
	public DateTimeOffset? LastTimeStamp { get; set; }

	/// <summary>
	/// Command summary
	/// </summary>
	public CommandSummary CommandSummary { get; set; } = new CommandSummary();

	/// <summary>
	/// Insight summary
	/// </summary>
	public InsightSummary InsightSummary { get; set; } = new InsightSummary();

	/// <summary>
	/// Rule Instance summary
	/// </summary>
	public RuleInstanceSummary RuleInstanceSummary { get; set; } = new RuleInstanceSummary();

	/// <summary>
	/// Timeseries summary
	/// </summary>
	public TimeSeriesSummary TimeSeriesSummary { get; set; } = new TimeSeriesSummary();

	/// <summary>
	/// Add rules to summary
	/// </summary>
	public void AddToSummary(IEnumerable<Rule> rules)
	{
		CountRules = rules.Count();
	}

	/// <summary>
	/// Clear summaries that are continually updated
	/// </summary>
	public void ClearRunningSummaries()
	{
		CommandSummary = new CommandSummary();
		InsightSummary = new InsightSummary();
		TimeSeriesSummary = new TimeSeriesSummary();
	}

	/// <summary>
	/// Add Timeseries to summary
	/// </summary>
	public void AddToSummary(TimeSeries timeSeries)
	{
		TimeSeriesSummary.Total++;

		if(!string.IsNullOrEmpty(timeSeries.DtId))
		{
			TimeSeriesSummary.TotalWithTwins++;
		}
	}

	/// <summary>
	/// Add rules instance to summary
	/// </summary>
	public void AddToSummary(RuleInstance ruleInstance)
	{
		if(!string.IsNullOrEmpty(ruleInstance.TimeZone) &&
		   !string.Equals(ruleInstance.TimeZone, "UTC", StringComparison.OrdinalIgnoreCase))
		{
			RuleInstanceSummary.TimeZone = ruleInstance.TimeZone;
		}

		RuleInstanceSummary.Total++;

		if(ruleInstance.RuleTemplate == RuleTemplateCalculatedPoint.ID)
		{
			RuleInstanceSummary.TotalCalcPoints++;
		}			
	}

	/// <summary>
	/// Add command to summary
	/// </summary>
	public void AddToSummary(Command command)
	{
		CommandSummary.Total++;

		if (command.IsTriggered)
		{
			CommandSummary.TotalTriggered++;
		}

		CommandSummary.CommandsByModel.TryGetValue(command.PrimaryModelId, out int totalByModel);

		totalByModel++;

		CommandSummary.CommandsByModel[command.PrimaryModelId] = totalByModel;
	}

	/// <summary>
	/// Add insight to summary
	/// </summary>
	public void AddToSummary(Insight insight)
	{
		InsightSummary.Total++;

		if (insight.CommandEnabled)
		{
			InsightSummary.TotalEnabled++;
		}

		if (insight.CommandInsightId != Guid.Empty)
		{
			InsightSummary.TotalLinked++;
		}

		if (insight.CommandEnabled == false && insight.CommandInsightId != Guid.Empty)
		{
			InsightSummary.TotalNotSynced++;
		}

		if (insight.IsFaulty)
		{
			InsightSummary.TotalFaulted++;
		}

		if (!insight.IsValid)
		{
			InsightSummary.TotalInvalid++;
		}

		if (insight.IsValid == true && insight.IsFaulty == false)
		{
			InsightSummary.TotalValidNotFaulted++;
		}

		if (insight.IsValid == true && insight.IsFaulty == false)
		{
			InsightSummary.TotalValidNotFaulted++;
		}

		InsightSummary.InsightsByModel.TryGetValue(insight.PrimaryModelId, out int totalByModel);

		totalByModel++;

		InsightSummary.InsightsByModel[insight.PrimaryModelId] = totalByModel;
	}
}


/// <summary>
/// Represents <see cref="RuleInstance"/> Aggregations
/// </summary>
public class RuleInstanceSummary
{
	/// <summary>
	/// The last non UTC time zone
	/// </summary>
	public string TimeZone { get; set; }

	/// <summary>
	/// Total RuleInstance
	/// </summary>
	public int Total { get; set; }

	/// <summary>
	/// Total calc points
	/// </summary>
	public int TotalCalcPoints { get; set; }
}

/// <summary>
/// Represents Command Aggregations
/// </summary>
public class CommandSummary
{
	/// <summary>
	/// Total Commands
	/// </summary>
	public int Total { get; set; }

	/// <summary>
	/// Total Commands currently triggering
	/// </summary>
	public int TotalTriggered { get; set; }

	/// <summary>
	/// Commands by model type
	/// </summary>
	public Dictionary<string, int> CommandsByModel { get; set; } = new Dictionary<string, int>();
}

/// <summary>
/// Represnts Insight Aggregations
/// </summary>
public class InsightSummary
{
	/// <summary>
	/// Total Insights
	/// </summary>
	public int Total { get; set; }

	/// <summary>
	/// Insights marked not to sync but already pushed to Command
	/// </summary>
	public int TotalNotSynced { get; set; }

	/// <summary>
	/// Synced at least once
	/// </summary>
	public int TotalLinked { get; set; }

	/// <summary>
	/// Insights syncing to Command
	/// </summary>
	public int TotalEnabled { get; set; }

	/// <summary>
	/// Insights that are faulted
	/// </summary>
	public int TotalFaulted { get; set; }

	/// <summary>
	/// Insights that are not valid
	/// </summary>
	public int TotalInvalid { get; set; }

	/// <summary>
	/// Insights that are not faulted and are valid
	/// </summary>
	public int TotalValidNotFaulted { get; set; }

	/// <summary>
	/// Insights by model type
	/// </summary>
	public Dictionary<string, int> InsightsByModel { get; set; } = new Dictionary<string, int>();
}

/// <summary>
/// Represents TimeSeries Aggregations
/// </summary>
public class TimeSeriesSummary
{
	/// <summary>
	/// Total TimeSeries
	/// </summary>
	public int Total { get; set; }

	/// <summary>
	/// TimeSeries linked to twins
	/// </summary>
	public int TotalWithTwins { get; set; }
}
