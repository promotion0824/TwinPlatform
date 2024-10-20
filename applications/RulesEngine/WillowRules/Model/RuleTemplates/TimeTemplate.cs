using System;
using System.Linq;
using Willow.Rules.Services;

namespace Willow.Rules.Model.RuleTemplates;

/// <summary>
/// A time template specifies the temporal aspects of a rule, e.g. how long it has to be active
/// over what period of time
/// </summary>
/// <remarks>
/// The time template tells an actor how much state it needs to keep
/// Time templates are constructed from UI elements / parameters by matching known parameters
/// rather than any strongly-typed fashion
/// </remarks>
/// <example>
/// Active for >10% of the time measured over 6 hours
/// </example>
/// <example>
/// Active for >10% of the working day measured over two days
/// </example>
public class TimeTemplate
{
	private readonly bool hasPercentage;
	private readonly double percentage = 0.0;

	/// <summary>
	/// How much data to keep expressed as a time interval
	/// </summary>
	public TimeSpan WindowDuration { get; init; }

	/// <summary>
	/// How many hours of fault condition existed to trigger this (percentage x window)
	/// </summary>
	public TimeSpan AffectedTime => hasPercentage ? WindowDuration * percentage : WindowDuration;

	/// <summary>
	/// Percentage parameter, could be zero
	/// </summary>
	public double Percentage => hasPercentage ? percentage : 0.0;

	/// <summary>
	/// Creates a new <see cref="TimeTemplate"/> template from stored parameters
	/// </summary>
	public TimeTemplate(params RuleUIElement[] elements)
		: this((RuleUIElementCollection)elements)
	{
	}

	/// <summary>
	/// Creates a new <see cref="TimeTemplate"/> template from stored parameters
	/// </summary>
	public TimeTemplate(RuleUIElementCollection ui)
	{
		// default values
		percentage = 1.0;
		hasPercentage = ui.TryGetDoubleField(Fields.PercentageOfTime, out percentage);
		if (!hasPercentage) percentage = 1.0;

		// Hours was an integer field but now we allow either
		bool hasHours = ui.TryGetDoubleField(Fields.OverHowManyHours, out double hours);

		bool hasDays = ui.TryGetIntField(Fields.OverHowManyDays, out int days);
		if (hasDays) hours = hours + 24 * days;

		if (!hasHours && !hasDays) hours = 6.0;

		WindowDuration = TimeSpan.FromHours(hours);
	}

	private double getRange(TimeSeriesBuffer timeSeries) =>
		(timeSeries.GetLastSeen() - timeSeries.GetFirstSeen())
		.TotalHours;

	/// <summary>
	/// Get an earliest time for the window covered by this policy
	/// </summary>
	/// <param name="now"></param>
	/// <returns></returns>
	public DateTimeOffset WindowStart(DateTimeOffset now) => now.Add(-this.WindowDuration);

	/// <summary>
	/// Checks a list of timed BOOL values to see if there are enough to determine the result
	/// and if there are enough whether the value was true for the specified percentage of time
	/// </summary>
	/// <remarks>
	/// Series must be in time order, earliest first
	/// </remarks>
	public (SeriesCheckVolume enough, SeriesCheckResult result, double percentageFaulted)
		CheckTimeCriteria(TimeSeriesBuffer timeseries, DateTimeOffset now)
	{
		double hoursRange = getRange(timeseries);
		bool enoughData = (hoursRange >= this.WindowDuration.TotalHours);

		var start = now.Add(-this.WindowDuration);

		double percentageTrue = timeseries.Points.AverageTrue(start, now);

		var fault = percentageTrue > percentage ? SeriesCheckResult.Fault : SeriesCheckResult.Healthy;

		return (enoughData ? SeriesCheckVolume.Enough : SeriesCheckVolume.Unknown, fault, percentageTrue);
	}

	/// <summary>
	/// Is there enough data to complete the calculation
	/// </summary>
	/// <remarks>
	/// This needs to check that there are no gaps in the range too beyond some reasonable limit
	/// </remarks>
	internal bool EnoughData(RuleInstance ruleInstance, IRuleTemplateDependencies dependencies) =>
		dependencies.GetAllTimeSeries(ruleInstance).All(v => getRange(v.Value) >= this.WindowDuration.TotalHours);

	/// <summary>
	/// Description for UI
	/// </summary>
	/// <returns></returns>
	internal string Describe()
	{
		return $"{this.percentage:P0} of {this.WindowDuration.TotalHours:0.0} hours";
	}
}
