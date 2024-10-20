namespace Willow.IoTService.Deployment.Dashboard.Application.Tests.AuditLogging;

using Moq;
using TechTalk.SpecFlow;
using Willow.IoTService.Deployment.Dashboard.Application.AuditLogging;
using Willow.IoTService.Deployment.DataAccess.PortService;
using Microsoft.Extensions.Logging;

[Binding]
public class AuditLoggingSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly AuditLogPipeline<IAuditLog, object> _pipeline;
    private readonly Mock<IAuditLogger<IAuditLog>> _auditLogger;
    private readonly Mock<IUserInfoService> _userInfoService;
    private bool _hasPermission = false;

    public AuditLoggingSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _auditLogger = new Mock<IAuditLogger<IAuditLog>>();
        _userInfoService = new Mock<IUserInfoService>();
        _pipeline = new AuditLogPipeline<IAuditLog, object>(_auditLogger.Object, _userInfoService.Object);
    }

    [Given(@"the user has permission")]
    public void GivenTheUserHasPermission()
    {
        _hasPermission = true;
    }

    [When(@"the user performs action '(.*)'")]
    public async Task WhenTheUserPerformsAction(string action)
    {
        if (_hasPermission)
        {
            var request = new Mock<IAuditLog>();
            await _pipeline.Handle(request.Object, () => Task.FromResult(new object()), CancellationToken.None);
            _scenarioContext["Request"] = request.Object;
        }
    }

    [When(@"the user performs action '(.*)' and it throws an exception")]
    public async Task WhenTheUserPerformsActionAndItThrowsAnException(string action)
    {
        if (_hasPermission)
        {
            var request = new Mock<IAuditLog>();
            try
            {
                await _pipeline.Handle(request.Object, () => throw new Exception("Test exception"), CancellationToken.None);
            }
            catch (Exception ex)
            {
                _scenarioContext["CaughtException"] = ex;
            }
            _scenarioContext["Request"] = request.Object;
        }
    }

    [Then(@"the action is logged via IAuditLogger")]
    public void ThenTheActionIsLoggedViaIAuditLogger()
    {
        var request = _scenarioContext.Get<IAuditLog>("Request");
        _auditLogger.Verify(x => x.LogInformation(It.IsAny<string>(), It.IsAny<string>(), request), Times.Once);
    }

    [Then(@"the action is not logged via IAuditLogger")]
    public void ThenTheActionIsNotLoggedViaIAuditLogger()
    {
        var request = _scenarioContext.Get<IAuditLog>("Request");
        _auditLogger.Verify(x => x.LogInformation(It.IsAny<string>(), It.IsAny<string>(), request), Times.Never);
    }

    [Then(@"an exception is thrown")]
    public void ThenAnExceptionIsThrown()
    {
        var exception = _scenarioContext.Get<Exception>("CaughtException");
        if (exception == null)
        {
            throw new Exception("Expected an exception to be thrown, but none was caught.");
        }
    }
}
