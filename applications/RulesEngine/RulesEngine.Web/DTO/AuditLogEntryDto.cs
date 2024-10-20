using Newtonsoft.Json;
using System;
using Willow.Rules.Model;

// EF
#nullable disable

namespace RulesEngine.Web;


/// <summary>
/// Audit Log entry for rule metadata
/// </summary>
public class AuditLogEntryDto
{
    /// <summary>
    /// Constructor
    /// </summary>
    public AuditLogEntryDto(AuditLogEntry auditLogEntry)
    {
        Date = auditLogEntry.Date;
        Message = auditLogEntry.Message;
        User = auditLogEntry.User;
    }

    /// <summary>
    /// Constructor for json
    /// </summary>
    [JsonConstructor]
    public AuditLogEntryDto()
    {
    }

    /// <summary>
    /// Date of log entry
    /// </summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>
    /// Message for log
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Which user the log is for
    /// </summary>
    public string User { get; set; }
}
