namespace Willow.DataQuality.Model.Validation;

// NOTE: Update the DataQualityAdxService.cs methods
//		GetTwinDataQualityResultsByIdAsync(), GetDefaultColumns() and ValidationResults.cs
//	if these properties are updated

public class ValidationResults
{
    /// <summary>
    /// The ADT $dtid of the twin 
    /// </summary>
    /// <remarks>
    /// It can be "UNASSIGNED" which means no capability is assigned for that time-series yet
    /// </remarks>
    public string? TwinDtId { get; set; }

    /// <summary>
    /// A JSON map of alternate identifiers or context for this twin.  
    /// </summary>
    /// <remarks>
    /// ConnectorId and ExternalId should be set for RulesEngineCapabilityStatus rows, at least when the TwinDtid is "UNASSIGNED" 
    /// </remarks>
    public object? TwinIdentifiers { get; set; }

    /// <summary>
    /// The model of twin
    /// </summary>
    /// <remarks>
    /// For rules that validate multiple twins, this is the “root” twin.
    /// </remarks>
    public string? ModelId { get; set; }

    /// <summary>
    /// The component or engine that was the source of the result 
    /// </summary>
    /// <remarks>
    /// StaticDataQuality | RulesEngineCapabilityStatus 
    /// </remarks>
    public string? ResultSource { get; set; }

    /// <summary>
    /// The full description of the error
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The classification of result/errors
    /// </summary>
    public Result ResultType { get; set; }

    /// <summary>
    /// The classification of checks : DataQualityRule, Properties, Relationships, Telemetry, IsValueOutOfRange
    /// </summary>
    public CheckType CheckType { get; set; }

    /// <summary>
    /// A JSON map of properties/attributes and result of the validation  
    /// </summary>
    public object? ResultInfo { get; set; }

    /// <summary>
    /// This property describes the scope of the rule 
    /// </summary>
    public object? RuleScope { get; set; }

    /// <summary>
    /// The name of the rule/rule-template
    /// </summary>
    public string? RuleId { get; set; }

    /// <summary>
    /// A JSON string with the following fields: CheckTime, BatchTime
    /// </summary>
    public object? RunInfo { get; set; }

    /// <summary>
    /// Information relating to the twin, such as its “location context”
    /// </summary>
    public object? TwinInfo { get; set; }

    /// <summary>
    /// A number between >= 0 and less than 100
    /// </summary>
    public int Score { get; set; }
}

public enum Result
{
    /// <summary>
    /// All tests have passed for this twin
    /// </summary>
    Ok,

    /// <summary>
    /// Error 
    /// </summary>
    Error
}

public enum CheckType
{
    /// <summary>
    /// Error in the rule template, such as checking a property that does not exist on a twin
    /// </summary>
    DataQualityRule,

    /// <summary>
    /// Error dealing with the attributes/properties of a twin (required, range, format, etc.)
    /// </summary>
    Properties,

    /// <summary>
    /// Error dealing with the relationships where twin does not have an allowable graph context
    /// </summary>
    Relationships,

    /// <summary>
    /// Error dealing with the missing or invalid incoming telemetry related to a Capability
    /// </summary>
    Telemetry,
}

