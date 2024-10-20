namespace Willow.Telemetry;

using Microsoft.Extensions.Logging;

/// <summary>
/// An injectable factory for creating <see cref="ILogger"/> instances for use in <see cref="AuditLogger{T}"/>.
/// </summary>
public sealed class AuditLoggerFactory : ILoggerFactory
{
    private readonly ILoggerFactory innerLoggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditLoggerFactory"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory that this object wraps.</param>
    public AuditLoggerFactory(ILoggerFactory loggerFactory)
    {
        this.innerLoggerFactory = loggerFactory;
    }

    /// <inheritdoc/>
    public void AddProvider(ILoggerProvider provider)
        => this.innerLoggerFactory.AddProvider(provider);

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
        => this.innerLoggerFactory.CreateLogger(categoryName);

    /// <inheritdoc/>
    public void Dispose()
        => this.innerLoggerFactory.Dispose();
}
