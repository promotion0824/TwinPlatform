using Microsoft.Extensions.Logging;
using System;

namespace Willow.Rules.Logging;

/// <summary>
/// Extends <see cref="ILogger"/> with timed operations.
/// </summary>
public static class LoggerExtensions
{
	/// <summary>
	/// Begin a new timed operation. The return value must be disposed to complete the operation.
	/// </summary>
	/// <param name="logger">The logger through which the timing will be recorded.</param>
	/// <param name="messageTemplate">A log message describing the operation, in message template format.</param>
	/// <param name="args">Arguments to the log message. These will be stored and captured only when the
	/// operation completes, so do not pass arguments that are mutated during the operation.</param>
	/// <returns>An <see cref="Operation"/> object.</returns>
	public static Operation TimeOperation(this ILogger logger, string messageTemplate, params object[] args)
	{
		logger.LogInformation(messageTemplate + " starting", args);
		return new Operation(logger, messageTemplate, args, TimedLogCompletion.Complete, LogLevel.Information, LogLevel.Warning);
	}

	/// <summary>
	/// Begin a new timed operation. The return value must be disposed to complete the operation.
	/// </summary>
	/// <param name="logger">The logger through which the timing will be recorded.</param>
	/// <param name="messageTemplate">A log message describing the operation, in message template format.</param>
	/// <param name="args">Arguments to the log message. These will be stored and captured only when the
	/// operation completes, so do not pass arguments that are mutated during the operation.</param>
	/// <returns>An <see cref="Operation"/> object.</returns>
	public static Operation TimeOperation(this ILogger logger, TimeSpan warningThreshold, string messageTemplate, params object[] args)
	{
		logger.LogInformation(messageTemplate + " starting", args);
		return new Operation(logger, messageTemplate, args, TimedLogCompletion.Complete, LogLevel.Information, LogLevel.Warning, warningThreshold);
	}

	/// <summary>
	/// Begin a new timed operation, but only report overages. The return value must be disposed to complete the operation.
	/// </summary>
	/// <param name="logger">The logger through which the timing will be recorded.</param>
	/// <param name="messageTemplate">A log message describing the operation, in message template format.</param>
	/// <param name="args">Arguments to the log message. These will be stored and captured only when the
	/// operation completes, so do not pass arguments that are mutated during the operation.</param>
	/// <returns>An <see cref="Operation"/> object.</returns>
	public static Operation TimeOperationOver(this ILogger logger, TimeSpan warningThreshold, string messageTemplate, params object[] args)
	{
		return new Operation(logger, messageTemplate, args, TimedLogCompletion.Complete, LogLevel.None, LogLevel.Warning, warningThreshold);
	}

	/// <summary>
	/// Creates a new logger that only writes once every timespan
	/// </summary>
	public static ILogger Throttle(this ILogger logger, TimeSpan timespan)
	{
		return new LoggerEvery(logger, timespan);
	}
}
