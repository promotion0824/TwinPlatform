using RulesEngine.Web;
using System.Collections.Generic;
using WillowRules.DTO;

namespace RulesEngine.Web;

/// <summary>
/// A single time series line
/// </summary>
public class TrendlineDto
{
	/// <summary>
	/// Id of the time series
	/// </summary>
	public string Id { get; set; }

	/// <summary>
	/// Name of the time series
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Unit of measure
	/// </summary>
	public string Unit { get; set; }

	/// <summary>
	/// Is this value an output value? They group together
	/// </summary>
	public bool IsOutput { get; set; }

    /// <summary>
	/// Is this value calculated by the rule, otherwise this is a user defined expression
	/// </summary>
	public bool IsSystemGenerated { get; set; }

    /// <summary>
    /// Is this a ranking value
    /// </summary>
    public bool IsRanking { get; set; }

    /// <summary>
    /// Is this line for a trigger
    /// </summary>
    public bool IsTrigger { get; init; }

    /// <summary>
    /// The timed values
    /// </summary>
    public List<TimedValueDto> Data { get; set; }

    /// <summary>
    /// Status changes for the time series
    /// </summary>
    public List<TrendlineStatusDto> Statuses { get; set; }

    /// <summary>
    /// Annotations for this trendline
    /// </summary>
    public List<TrendlineAnnotationDto> Annotations { get; set; } = new List<TrendlineAnnotationDto>();

    /// <summary>
    /// The axis name: y, y2, y3, ...
    /// </summary>
    public string Axis { get; set; }

	/// <summary>
	/// One of the D3 shapes, hv (stepped data), spline or linear
	/// </summary>
	public string Shape { get; set; }
}
