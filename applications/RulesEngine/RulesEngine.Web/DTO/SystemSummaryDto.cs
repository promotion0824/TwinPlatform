using System;
using System.Collections.Generic;
using System.Linq;
using Willow.HealthChecks;
using Willow.Rules.Model;

// EF
#nullable disable

namespace RulesEngine.Web;

/// <summary>
/// Information about the state of the Rules Engine system
/// </summary>
public class SystemSummaryDto
{
    /// <summary>
    /// Creates a new instance of <see cref="SystemSummaryDto"/>
    /// </summary>
    public SystemSummaryDto()
    {
    }

    /// <summary>
    /// When twins and relationships counts were set
    /// </summary>
    public DateTimeOffset ADTAsOfDate { get; set; }

    /// <summary>
    /// How many twins
    /// </summary>
    public int CountTwins { get; set; }

    /// <summary>
    /// How many twins with trend Ids
    /// </summary>
    public int CountCapabilities { get; set; }

    /// <summary>
    /// How many relationships
    /// </summary>
    public int CountRelationships { get; set; }

    /// <summary>
    /// How many rules
    /// </summary>
    public int CountRules { get; set; }

    /// <summary>
    /// How many rule instances
    /// </summary>
    public int CountRuleInstances { get; set; }

    /// <summary>
    /// How many calculated points
    /// </summary>
    public int CountCalculatedPoints { get; set; }

    /// <summary>
    /// How many live data points are coming in
    /// </summary>
    public int CountLiveData { get; set; }

    /// <summary>
    /// How many time series buffers
    /// </summary>
    public int CountTimeSeriesBuffers { get; set; }

    /// <summary>
    /// How many commands in total
    /// </summary>
    public int CountCommands { get; set; }

    /// <summary>
    /// How many commands are triggering
    /// </summary>
    public int CountCommandsTriggering { get; set; }

    /// <summary>
    /// How many insights in the faulted state
    /// </summary>
    public int CountInsightsFaulted { get; set; }

    /// <summary>
    /// How many insights invalid
    /// </summary>
    public int CountInsightsInValid { get; set; }

    /// <summary>
    /// How many insights in healthy state
    /// </summary>
    public int CountInsightsHealthy { get; set; }

    /// <summary>
    /// How many insights flowing to command
    /// </summary>
    public int CountCommandInsights { get; set; }

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
    /// Health checks flattened
    /// </summary>
    public HealthCheckDto[] Health { get; set; }

    /// <summary>
    /// Insights by model type
    /// </summary>
    public Dictionary<string, int> InsightsByModel { get; set; }

    /// <summary>
    /// Commands by model type
    /// </summary>
    public Dictionary<string, int> CommandsByModel { get; set; }

    /// <summary>
    /// Model summary
    /// </summary>
    public ADTModelSummary[] ModelSummary { get; set; } = [];
}

/// <summary>
/// Model summary
/// </summary>
public class ADTModelSummary
{
    /// <summary>
    /// The full path property name
    /// </summary>
    public string ModelId { get; set; }

    /// <summary>
    /// Total Properties used in ADT
    /// </summary>
    public Dictionary<string, int> PropertiesUsed { get; set; }

    /// <summary>
    /// Total Properties declared in ADT
    /// </summary>
    public Dictionary<string, int> PropertiesDelared { get; set; }

    /// <summary>
    /// Get Summaries
    /// </summary>
    public static ADTModelSummary[] ModelSummaries(ADTSummary summary)
    {
        return summary.ExtensionData.ModelSummary.Select(v => new ADTModelSummary()
        {
            ModelId = v.ModelId,
            PropertiesDelared = v.PropertyReferences.ToDictionary(v => v.PropertyName, v => v.TotalDelcared),
            PropertiesUsed = v.PropertyReferences.Where(v => v.TotalUsed > 0).ToDictionary(v => v.PropertyName, v => v.TotalUsed),
        }).ToArray();
    }
}
