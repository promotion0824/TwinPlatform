using RulesEngine.Web;
using System;

namespace Willow.Rules.Web;

/// <summary>
/// Request object for simulation
/// </summary>
public class RuleSimulationRequest
{
    /// <summary>
    /// Rule id for existing rule
    /// </summary>
    public string RuleId { get; set; }
    /// <summary>
    /// Equipment id to run simuaiton against
    /// </summary>
    public string EquipmentId { get; set; }
    /// <summary>
    /// Start time for simulation
    /// </summary>
    public DateTime StartTime { get; set; }
    /// <summary>
    /// End time for simulation
    /// </summary>
    public DateTime EndTime { get; set; }
    /// <summary>
    /// Use existing actor data
    /// </summary>
    public bool UseExistingData { get; set; }
    /// <summary>
    /// Optionally update rule from rule dto in request before simulation
    /// </summary>
    public bool UpdateRule { get; set; }
    /// <summary>
    /// Optionally generate calc input values for point
    /// </summary>
    public bool GeneratePointTracking { get; set; }
    /// <summary>
    /// Optionally show auto generated variables
    /// </summary>
    public bool ShowAutoVariables { get; set; }
    /// <summary>
    /// Indicates whetehr compression optmisations are done
    /// </summary>
    public bool OptimizeCompression { get; set; } = true;
    /// <summary>
    /// Indicates wheter compression is enabled
    /// </summary>
    public bool EnableCompression { get; set; } = true;
    /// <summary>
    /// Optionally exclude optmisations, by defualt true
    /// </summary>
    public bool OptimizeExpression { get; set; } = true;
    /// <summary>
    /// Optionally applies lilmits to timeseries and variables
    /// </summary>
    public bool ApplyLimits { get; set; }
    /// <summary>
    /// Optionally ignore 24hr point limit
    /// </summary>
    public bool SkipMaxPointLimit { get; set; }
    /// <summary>
    /// Optionally rule property overrides
    /// </summary>
    public RuleDto Rule { get; set; }
    /// <summary>
    /// Optionally a global to test with
    /// </summary>
    public GlobalVariableDto Global { get; set; }
}
