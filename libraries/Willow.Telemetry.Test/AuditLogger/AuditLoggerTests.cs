namespace Willow.Telemetry.Test.AuditLogger;

using Microsoft.Extensions.Logging;
using Xunit;

public class AuditLoggerTests(AuditLoggerFixture fixture) : IClassFixture<AuditLoggerFixture>
{
    [Fact]
    public void LogInformation_SimpleMessage_DefaultScopeCreated()
    {
        fixture.AuditLogger.LogInformation("test@willowinc.com", "Login");

        var logMessage = fixture.FakeLoggerProvider.Collector.LatestRecord;

        Assert.Equal("Login by test@willowinc.com", logMessage.Message);
        Assert.Equal(LogLevel.Information, logMessage.Level);
        Assert.Collection(logMessage.Scopes, scope =>
        {
            Assert.IsType<Dictionary<string, object?>>(scope);
            Assert.Contains((Dictionary<string, object?>)scope, p => p.Key == "Audit" && Convert.ToBoolean(p.Value) == true);
        });
    }

    [Fact]
    public void LogInformation_StructuredMessageWithScope_ScopePropertiesArePreserved()
    {
        using (fixture.AuditLogger.BeginScope(new Dictionary<string, object?> { ["Permissions"] = "CanReadRules, CanEditRules" }))
        {
            fixture.AuditLogger.LogInformation("test@willowinc.com", "Login to {SiteName}", "Willow");
        }

        var logMessage = fixture.FakeLoggerProvider.Collector.LatestRecord;

        Assert.Equal("Login to Willow by test@willowinc.com", logMessage.Message);
        Assert.Equal(LogLevel.Information, logMessage.Level);
        Assert.Collection(logMessage.Scopes,
            scope => Assert.Contains((Dictionary<string, object?>)scope!, p => p.Key == "Permissions" && p.Value?.ToString() == "CanReadRules, CanEditRules"),
            scope => Assert.Contains((Dictionary<string, object?>)scope!, p => p.Key == "Audit" && Convert.ToBoolean(p.Value) == true));

        var structuredState = logMessage.StructuredState;

        Assert.NotNull(structuredState);
        Assert.Contains(structuredState, tag => tag.Key == "SiteName" && tag.Value == "Willow");
        Assert.Contains(structuredState, tag => tag.Key == "userId" && tag.Value == "test@willowinc.com");
    }

    [Fact]
    public void LogWarning_SimpleMessage_DefaultScopeCreated()
    {
        fixture.AuditLogger.LogWarning("test@willowinc.com", "Message");

        var logMessage = fixture.FakeLoggerProvider.Collector.LatestRecord;

        Assert.Equal(LogLevel.Warning, logMessage.Level);
        Assert.Single(logMessage.Scopes);
    }

    [Fact]
    public void LogWarning_WithException_LoggedCorrectly()
    {
        fixture.AuditLogger.LogWarning("test@willowinc.com", new Exception("TestException"), "Message");

        var logMessage = fixture.FakeLoggerProvider.Collector.LatestRecord;

        Assert.Equal(LogLevel.Warning, logMessage.Level);
        Assert.NotNull(logMessage.Exception);
    }

    [Fact]
    public void LogError_SimpleMessage_DefaultScopeCreated()
    {
        fixture.AuditLogger.LogError("test@willowinc.com", "Message");

        var logMessage = fixture.FakeLoggerProvider.Collector.LatestRecord;

        Assert.Equal(LogLevel.Error, logMessage.Level);
        Assert.Single(logMessage.Scopes);
    }

    [Fact]
    public void LogError_WithException_LoggedCorrectly()
    {
        fixture.AuditLogger.LogError("test@willowinc.com", new Exception("TestException"), "Message");

        var logMessage = fixture.FakeLoggerProvider.Collector.LatestRecord;

        Assert.Equal(LogLevel.Error, logMessage.Level);
        Assert.NotNull(logMessage.Exception);
    }

    [Fact]
    public void LogCritical_SimpleMessage_DefaultScopeCreated()
    {
        fixture.AuditLogger.LogCritical("test@willowinc.com", "Message");

        var logMessage = fixture.FakeLoggerProvider.Collector.LatestRecord;

        Assert.Equal(LogLevel.Critical, logMessage.Level);
        Assert.Single(logMessage.Scopes);
    }

    [Fact]
    public void LogCritical_WithException_LoggedCorrectly()
    {
        fixture.AuditLogger.LogCritical("test@willowinc.com", new Exception("TestException"), "Message");

        var logMessage = fixture.FakeLoggerProvider.Collector.LatestRecord;

        Assert.Equal(LogLevel.Critical, logMessage.Level);
        Assert.NotNull(logMessage.Exception);
    }
}
