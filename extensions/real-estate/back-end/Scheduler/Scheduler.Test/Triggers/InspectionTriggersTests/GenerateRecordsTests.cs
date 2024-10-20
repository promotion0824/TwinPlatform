using Microsoft.Extensions.Logging;
using NSubstitute;
using Scheduler.Services;
using Scheduler.Test.Infrastructure;
using Scheduler.Triggers;
using System.Net;

namespace Scheduler.Test.Triggers.InspectionTriggersTests;
public class GenerateRecordsTests
{
    private readonly IHttpClientFactory _mockHttpClientFactory = Substitute.For<IHttpClientFactory>();
    [Fact]
    public async Task GenerateRecordsFunction_RunGenerateRecords_Success()
    {

        // Arrange
        var url = "/inspectionRecords/generate";
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
        var inspectionTriggersFakeLogger = Substitute.For<ILogger<InspectionTriggers>>();
        var fakeLoggerFactory = Substitute.For<ILoggerFactory>();
        fakeLoggerFactory.CreateLogger<WorkflowApi>().Returns(workflowApiFakeLogger);
        fakeLoggerFactory.CreateLogger<InspectionTriggers>().Returns(inspectionTriggersFakeLogger);

        var _workflowApi = new WorkflowApi(_mockHttpClientFactory, fakeLoggerFactory.CreateLogger<WorkflowApi>());
        var mockInspectionTriggers = new InspectionTriggers(_workflowApi, fakeLoggerFactory.CreateLogger<InspectionTriggers>());
        // Act
        await mockInspectionTriggers.GenerateRecords(null);

        // Assert
        var workflowApiLogCalls = workflowApiFakeLogger.ReceivedCalls().Where(call => call.GetMethodInfo().Name == "Log").ToList();
        var inspectionTriggersLogCalls = inspectionTriggersFakeLogger.ReceivedCalls().Where(call => call.GetMethodInfo().Name == "Log").ToList();
        Assert.Equal(2, workflowApiLogCalls.Count);
        Assert.Single(inspectionTriggersLogCalls);

        // Check the second log message from api call
        var args2 = workflowApiLogCalls[0].GetArguments();
        Assert.Equal(LogLevel.Information, args2[0]);
        Assert.Contains("Status Code: OK", args2[2].ToString());

    } 
}


