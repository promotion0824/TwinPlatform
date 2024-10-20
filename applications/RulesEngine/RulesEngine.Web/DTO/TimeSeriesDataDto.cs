using RulesEngine.Web;
using System;
using System.Collections.Generic;

namespace WillowRules.DTO;

/// <summary>
/// Time series data for multiple trends for a chart
/// </summary>
public class TimeSeriesDataDto
{
	/// <summary>
	/// An Id for the time series (fix name)
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// The start time for the timeseries data
	/// </summary>
	public DateTimeOffset StartTime { get; set; }

	/// <summary>
	/// The end time for the timeseries data
	/// </summary>
	public DateTimeOffset EndTime { get; set; }

	/// <summary>
	/// The time series data
	/// </summary>
	public TrendlineDto[] Trendlines { get; set; }

    /// <summary>
    /// Insights for this trendline
    /// </summary>
    public List<TrendlineInsightDto> Insights { get; set; } = new List<TrendlineInsightDto>();

    /// <summary>
    /// Names of axes (y, y2, y3, ...)
    /// </summary>
    public IReadOnlyList<AxisDto> Axes { get; internal set; }
}


/// <summary>
/// An axis
/// </summary>
public class AxisDto
{
	/// <summary>
	/// Group key, all trends with same key share an axis (normally unit)
	/// </summary>
	public string Key { get; set; }

	/// <summary>
	/// Short name like y2
	/// </summary>
	public string ShortName { get; set; }

	/// <summary>
	/// Long name like yaxis2
	/// </summary>
	public string LongName { get; set; }

	/// <summary>
	/// Title
	/// </summary>
	public string Title { get; set; }
}
