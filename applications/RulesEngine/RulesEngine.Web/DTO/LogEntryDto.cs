using System;
using Willow.Rules.Model;

namespace RulesEngine.Web;

/// <summary>
/// Represents a log entry
/// </summary>
public class LogEntryDto
{
    /// <summary>
    /// Constructor
    /// </summary>
    public LogEntryDto(LogEntry logEntry)
    {
        Message = logEntry.Message;
        Level = logEntry.Level;
        TimeStamp = logEntry.TimeStamp;
        Exception = logEntry.Exception;
        LogEvent = logEntry.LogEvent;
        CorrelationId = logEntry.CorrelationId;
        ProgressId = logEntry.ProgressId;
    }

    /// <summary>
    /// The log message
    /// </summary>
    public string Message { get; init; }

	/// <summary>
	/// The log level
	/// </summary>
	public string Level { get; init; }

	/// <summary>
	/// When log occured
	/// </summary>
	public DateTime TimeStamp { get; init; }

	/// <summary>
	/// Any exception stack trace
	/// </summary>
	public string Exception { get; init; }

	/// <summary>
	/// Structured json properties
	/// </summary>
	public string LogEvent { get; init; }

	/// <summary>
	/// Correlation Id id for logs
	/// </summary>
	public string CorrelationId { get; init; }

    /// <summary>
    /// Progress Id for logs
    /// </summary>
    public string ProgressId { get; init; }
}
