using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KPI.Service.Models;

/// <summary>
/// Kpi Building daily performance scores
/// </summary>
public class KPIBuildingPerformanceScoresDto
{
    /// <summary>
    /// metric name e.g. Energy, Comfort, etc.
    /// </summary>
    public string MetricName { get; set; }
    /// <summary>
    /// The Date of the Performance score
    /// </summary>
    public DateTime Date { get; set; }
    /// <summary>
    /// Performance score
    /// </summary>
    public double Value { get; set; }
}

/// <summary>
/// Building Daily Performance Scores
/// </summary>
/// <param name="Date">The Date of the Performance score</param>
/// <param name="Value">Performance score</param>
public record BuildingPerformanceScoresResponse(DateTime Date, double Value);

