namespace Willow.Telemetry.Test.AuditLogger;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Willow.Telemetry;

public sealed class AuditLoggerFixture : IDisposable
{
    public AuditLoggerFixture()
    {
        FakeLoggerProvider = new FakeLoggerProvider();
        AuditLoggerFactory = new AuditLoggerFactory(LoggerFactory.Create(config => config.AddProvider(FakeLoggerProvider)));
        AuditLogger = new AuditLogger<AuditLoggerTests>(AuditLoggerFactory);
    }

    public FakeLoggerProvider FakeLoggerProvider { get; private set; }

    public AuditLoggerFactory AuditLoggerFactory { get; private set; }

    public AuditLogger<AuditLoggerTests> AuditLogger { get; private set; }

    public void Dispose()
    {
        AuditLoggerFactory.Dispose();
    }
}
