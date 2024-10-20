using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Model;
using Willow.Rules.Repository;

namespace RulesEngine.Web;

#nullable disable

/// <summary>
/// Metadata for a rule, updated in real-time as rule instances are created by the back-end
/// </summary>
public class RuleMetadataDto : IId
{
    /// <summary>
    /// RuleMetadataDto constructor
    /// </summary>
    public RuleMetadataDto(RuleMetadata ruleMetadata)
        : this(ruleMetadata, new List<string>())
    {

    }

    /// <summary>
    /// RuleMetadataDto constructor
    /// </summary>
    public RuleMetadataDto(RuleMetadata ruleMetadata, List<string> modelIds)
    {
        Id = ruleMetadata.Id;
        ScanState = ruleMetadata.ScanState;
        ScanStarted = ruleMetadata.ScanStarted;
        ScanComplete = ruleMetadata.ScanComplete;
        ScanStateAsOf = ruleMetadata.ScanStateAsOf;
        RuleInstanceCount = ruleMetadata.RuleInstanceCount;
        ValidInstanceCount = ruleMetadata.ValidInstanceCount;
        InsightsGenerated = ruleMetadata.InsightsGenerated;
        EarliestExecutionDate = ruleMetadata.EarliestExecutionDate;
        ScanError = ruleMetadata.ScanError;
        HasExecuted = EarliestExecutionDate < DateTime.Now;
        ModelIds = modelIds;
        Created = ruleMetadata.Created;
        CreatedBy = ruleMetadata.CreatedBy;
        ModifiedBy = ruleMetadata.ModifiedBy;
        LastModified = ruleMetadata.LastModified;
        Version = ruleMetadata.Version;
        RuleInstanceStatus = ruleMetadata.RuleInstanceStatus;
        CommandsGenerated = ruleMetadata.CommandsGenerated;
        Logs = ruleMetadata.ExtensionData.Logs.Select(v => new AuditLogEntryDto(v)).ToArray();
    }

    /// <summary>
    /// Constructor for serialization 
    /// </summary>
    [JsonConstructor]
    private RuleMetadataDto()
    {
    }

    /// <summary>
	///The version of the rule. Usually increments for rule expression updates
	/// </summary>
	public int Version { get; set; }

    /// <summary>
    /// Count of how many commands were generated
    /// </summary>
    public int CommandsGenerated { get; set; }

    /// <summary>
    /// The ID of the rule for which this metadata was created
    /// </summary>
    public string Id { get; init; }

    /// <summary>
	/// The user who created the rule
	/// </summary>
	public string CreatedBy { get; init; }

    /// <summary>
    /// The user who last modified the rule
    /// </summary>
    public string ModifiedBy { get; set; }

    /// <summary>
    /// The last time the rule was modified
    /// </summary>
    public DateTimeOffset LastModified { get; set; }

    /// <summary>
    /// When the rule was created
    /// </summary>
    public DateTimeOffset Created { get; init; }

    /// <summary>
    /// The overall rule instance status
    /// </summary>
    public RuleInstanceStatus RuleInstanceStatus { get; set; }

    /// <summary>
    /// Scan started
    /// </summary>
    public bool ScanStarted { get; set; }

    /// <summary>
    /// Counts are updated
    /// </summary>
    public bool ScanComplete { get; set; }

    /// <summary>
    /// State of being scanned, completed, failed
    /// </summary>
    public ScanState ScanState { get; set; }

    /// <summary>
    /// The last scan error
    /// </summary>
    public string ScanError { get; set; }

    /// <summary>
    /// Datetimeoffset when the scan state was last updated
    /// </summary>
    public DateTimeOffset ScanStateAsOf { get; set; }

    /// <summary>
    /// The count of instances that the rule expanded to
    /// </summary>
    public int RuleInstanceCount { get; set; }

    /// <summary>
    /// The count of valid instances of this rule
    /// </summary>
    public int ValidInstanceCount { get; set; }

    /// <summary>
    /// Count of how many FAULTED insights this rule has generated
    /// </summary>
    /// <remarks>
    /// Replace this with a more useful statistics object tracking day by day for some window
    /// </remarks>
    public int InsightsGenerated { get; set; }

    /// <summary>
    /// The earliest date the rule has ever executed against
    /// </summary>
    public DateTimeOffset EarliestExecutionDate { get; set; }

    /// <summary>
	/// Indicates wheter execution has ever run on this rule
	/// </summary>
	public bool HasExecuted { get; set; }

    /// <summary>
    /// A list of model ids found in the rule expressions
    /// </summary>
    public List<string> ModelIds { get; set; }

    /// <summary>
    /// Audit logs
    /// </summary>
    public AuditLogEntryDto[] Logs { get; set; }
}
