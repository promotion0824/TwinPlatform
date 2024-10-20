
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using WorkflowCore.Models;

namespace WorkflowCore.Entities;

/// <summary>
/// Class implementing the IAuditTrail is eligible for audit trail logging
/// </summary>
interface IAuditTrail
{
	/// <summary>
	/// Entity Id
	/// </summary>
	Guid Id { get; set; }
    /// <summary>
    /// Columns to be tracked
    /// </summary>
    List<string> GetTrackedColumns();

}

[Table("WF_AuditTrail")]
public class AuditTrailEntity
{
	public Guid Id { get; set; }
	public Guid RecordID { get; set; }
	public string OperationType { get; set; }
	public DateTime Timestamp { get; set; }
	public string TableName { get; set; }
	public string ColumnName { get; set; }
	public SourceType? SourceType { get; set; }
	public Guid? SourceId { get; set; }
	public string OldValue { get; set; }
	public string NewValue { get; set; }
}
