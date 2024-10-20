namespace Microsoft.Extensions.Logging;

using Willow.Telemetry;

/// <summary>
/// Logs security audit events.
/// </summary>
/// <remarks>
/// Exposes a subset of the methods normally available on <see cref="ILogger"/>.
/// The intention is to duplicate the methods available for ILogger but to also
/// include a UserId. Does not allow Debug, Trace or None log levels.
/// </remarks>
public interface IAuditLogger
{
    /// <summary>
    /// Logs an information audit event.
    /// </summary>
    /// <param name="userId">The user's object or client ID.</param>
    /// <param name="messageTemplate">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    void LogInformation(string userId, string? messageTemplate, params object[] args);

    /// <summary>
    /// Logs a warning audit event.
    /// </summary>
    /// <param name="userId">The user's object or client ID.</param>
    /// <param name="messageTemplate">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    void LogWarning(string userId, string? messageTemplate, params object[] args);

    /// <summary>
    /// Logs a warning audit event.
    /// </summary>
    /// <param name="userId">The user's object or client ID.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="messageTemplate">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    void LogWarning(string userId, Exception? exception, string? messageTemplate, params object[] args);

    /// <summary>
    /// Logs an error audit event.
    /// </summary>
    /// <param name="userId">The user's object or client ID.</param>
    /// <param name="messageTemplate">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    void LogError(string userId, string? messageTemplate, params object[] args);

    /// <summary>
    /// Logs an error audit event.
    /// </summary>
    /// <param name="userId">The user's object or client ID.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="messageTemplate">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    void LogError(string userId, Exception? exception, string? messageTemplate, params object[] args);

    /// <summary>
    /// Logs an critical audit event.
    /// </summary>
    /// <param name="userId">The user's object or client ID.</param>
    /// <param name="messageTemplate">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    void LogCritical(string userId, string? messageTemplate, params object[] args);

    /// <summary>
    /// Logs an critical audit event.
    /// </summary>
    /// <param name="userId">The user's object or client ID.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="messageTemplate">Format string of the log message.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    void LogCritical(string userId, Exception? exception, string? messageTemplate, params object[] args);

    /// <summary>
    /// Begins a logical operation scope.
    /// </summary>
    /// <param name="state">The identifier for the scope.</param>
    /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
    /// <returns>An <see cref="IDisposable"/> that ends the logical operation scope on dispose.</returns>
    IDisposable? BeginScope<TState>(TState state)
            where TState : notnull;
}

/// <summary>
/// Logs security audit events.
/// </summary>
/// <typeparam name="TCategoryName">The type whose name is used for the logger category name.</typeparam>
public interface IAuditLogger<out TCategoryName> : IAuditLogger
{
}

/// <inheritdoc/>
public sealed class AuditLogger<T>
    : IAuditLogger<T>
{
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLogger{T}"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create the inner logger.</param>
    public AuditLogger(AuditLoggerFactory loggerFactory)
    {
        logger = loggerFactory.CreateLogger(typeof(T).FullName!);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "This is a wrapper for ILogger")]
    private void Log(LogLevel logLevel, string userId, Exception? exception, string? messageTemplate, params object[] args)
    {
        messageTemplate += @" by {userId}";
        args = args.Append(userId).ToArray();

        using (logger.BeginScope(new Dictionary<string, object?>() { ["Audit"] = true }))
        {
            logger.Log(logLevel, exception, messageTemplate, args);
        }
    }

    /// <inheritdoc/>
    public void LogCritical(string userId, string? messageTemplate, params object[] args) =>
        Log(LogLevel.Critical, userId, null, messageTemplate, args);

    /// <inheritdoc/>
    public void LogCritical(string userId, Exception? exception, string? messageTemplate, params object[] args) =>
        Log(LogLevel.Critical, userId, exception, messageTemplate, args);

    /// <inheritdoc/>
    public void LogError(string userId, string? messageTemplate, params object[] args) =>
        Log(LogLevel.Error, userId, null, messageTemplate, args);

    /// <inheritdoc/>
    public void LogError(string userId, Exception? exception, string? messageTemplate, params object[] args) =>
        Log(LogLevel.Error, userId, exception, messageTemplate, args);

    /// <inheritdoc/>
    public void LogInformation(string userId, string? messageTemplate, params object[] args) =>
        Log(LogLevel.Information, userId, null, messageTemplate, args);

    /// <inheritdoc/>
    public void LogWarning(string userId, Exception? exception, string? messageTemplate, params object[] args) =>
        Log(LogLevel.Warning, userId, exception, messageTemplate, args);

    /// <inheritdoc/>
    public void LogWarning(string userId, string? messageTemplate, params object[] args) =>
        Log(LogLevel.Warning, userId, null, messageTemplate, args);

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return logger.BeginScope(state);
    }
}
