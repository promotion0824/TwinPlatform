using Microsoft.Extensions.Logging;
using NSubstitute;
using Scheduler.Services;
using Scheduler.Test.Infrastructure;
using Scheduler.Triggers;
using System.Net;

namespace Scheduler.Test.Triggers.InspectionTriggersTests;

public class SendDailyReportTests
{
    private readonly IHttpClientFactory _mockHttpClientFactory = Substitute.For<IHttpClientFactory>();
    [Fact]
    public async Task SendDailyReportFunction_RunSendDailyReport_Success()
    {

        // Arrange
        var url = "/inspections/reports";
        var fakeHttpMessageHandler = new MockHttpMessageHandler(new HttpResponseMessage()
        {
            StatusCode = HttpStatusCode.OK,
        }, url);
        var fakeHttpClient = new HttpClient(fakeHttpMessageHandler)
        {
            BaseAddress = new Uri("https://localhost")
        };

        _mockHttpClientFactory.CreateClient(ApiServiceNames.WorkflowCore).Returns(fakeHttpClient);

        var workflowApiFakeLogger = Substitute.For<ILogger<WorkflowApi>>();
        var inspectionTriggersFakeLogger2 = Substitute.For<ILogger<InspectionTriggers>>();
        var fakeLoggerFactory = Substitute.For<ILoggerFactory>();
        fakeLoggerFactory.CreateLogger<WorkflowApi>().Returns(workflowApiFakeLogger);
        fakeLoggerFactory.CreateLogger<InspectionTriggers>().Returns(inspectionTriggersFakeLogger2);

        var _workflowApi = new WorkflowApi(_mockHttpClientFactory, fakeLoggerFactory.CreateLogger<WorkflowApi>());
        var mockInspectionTriggers = new InspectionTriggers(_workflowApi, fakeLoggerFactory.CreateLogger<InspectionTriggers>());
        // Act
        await mockInspectionTriggers.SendDailyReport(null);

        // Assert
        var workflowApiLogCalls = workflowApiFakeLogger.ReceivedCalls().Where(call => call.GetMethodInfo().Name == "Log").ToList();
        var inspectionTriggersLogCalls = inspectionTriggersFakeLogger2.ReceivedCalls().Where(call => call.GetMethodInfo().Name == "Log").ToList();
        Assert.Single(workflowApiLogCalls);
        Assert.Single(inspectionTriggersLogCalls);

        // Check the second log message from api call
        var args = workflowApiLogCalls[0].GetArguments();
        Assert.Equal(LogLevel.Information, args[0]);
        Assert.Contains("Status Code: OK", args[2].ToString());

    }
}

