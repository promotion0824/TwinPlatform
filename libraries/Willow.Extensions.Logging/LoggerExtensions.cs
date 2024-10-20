using System;
using Microsoft.Extensions.Logging;

namespace Willow.Extensions.Logging;

/// <summary>
/// Extends <see cref="ILogger"/> with timed operations.
/// </summary>
public static class LoggerExtensions
{
    private static readonly Dictionary<string, object> auditScope = new Dictionary<string, object>
    {
        ["Audit"] = true
    };
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

    /// <summary>
	/// Log an Audit event
	/// </summary>
	/// <param name="logger">The logger through which the audit message will be recorded.</param>
	/// <param name="userId">The user id</param>
	/// <param name="messageTemplate">A log message describing the operation, in message template format.</param>
	/// <param name="args">Arguments to the log message. These will be stored and captured only when the
	/// operation completes, so do not pass arguments that are mutated during the operation.</param>
	public static void Audit(this ILogger logger, string userId, string messageTemplate, params object[] args)
    {
        using (var logScope = logger.BeginScope(auditScope))
        {
            messageTemplate = $"{messageTemplate} by {{userId}}";
            args = args.Append(userId).ToArray();
            logger.Log(LogLevel.Information, null, messageTemplate, args);
        }
    }
}
