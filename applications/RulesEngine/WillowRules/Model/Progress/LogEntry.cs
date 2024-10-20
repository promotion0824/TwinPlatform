using System;
using System.Collections.Generic;
using Willow.Rules.Repository;

// EF
#nullable disable

namespace Willow.Rules.Model;

/// <summary>
/// Represents Database logs
/// </summary>
public class LogEntry : IId
{
	/// <summary>
	/// The Id stays empty for a log entry. It is faster to insert entries if there's no primary key
	/// </summary>
	public string Id { get; init; }

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
	/// Correlation Id that ran on processor for logs
	/// </summary>
	public string CorrelationId { get; init; }

	/// <summary>
	/// Progress Id that ran on processor for logs
	/// </summary>
	public string ProgressId { get; init; }
}
