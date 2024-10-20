using System;
using Microsoft.Extensions.Logging;

namespace Willow.Extensions.Logging;

/// <summary>
/// Logs every N seconds, skipping all log writes in-between
/// </summary>
/// <remarks>
/// Wrap an existing ILogger in a LogEvery and it will adjust the
/// logging rate to no more than once every timespan
/// </remarks>
public class LoggerEvery : ILogger
{
    private DateTimeOffset lastReport = new DateTimeOffset();

    private readonly ILogger logger;
    private readonly TimeSpan timeSpan;

    /// <summary>
    /// Creates a new <see cref="LoggerEvery" />
    /// </summary>
    /// <param name="logger">Existing logger</param>
    /// <param name="timeSpan">Minimum interval between log messages</param>
    /// <exception cref="ArgumentNullException"></exception>
    public LoggerEvery(ILogger logger, TimeSpan timeSpan)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.timeSpan = timeSpan;
        //"- timeSpan" guarantees at least one write
        lastReport = DateTimeOffset.Now - timeSpan;
    }

    private static object lockTimer = new object();

    /// <summary>
    /// Logs an exception or trace
    /// </summary>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (DateTimeOffset.Now > lastReport.Add(this.timeSpan))
        {
            //lock log writing else it writes the same type of log info across the threads bloating the logs
            lock (lockTimer)
            {
                // double check in case the time has now passed
                if (DateTimeOffset.Now > lastReport.Add(this.timeSpan))
                {
                    lastReport = DateTimeOffset.Now;
                    logger.Log<TState>(logLevel, eventId, state, exception, formatter);
                }
            }
        }
    }

    /// <summary>
    /// Tests if the log is enabled for a given level
    /// </summary>
    public bool IsEnabled(LogLevel logLevel)
    {
        return logger.IsEnabled(logLevel);
    }

    /// <summary>
    /// Begins a logging scope
    /// </summary>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return logger.BeginScope<TState>(state);
    }
}
