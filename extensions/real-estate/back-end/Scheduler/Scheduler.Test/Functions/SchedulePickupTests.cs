using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Scheduler.Services;
using Scheduler.Test.Infrastructure;
using System.Net;
using Willow.Scheduler.Functions;

namespace Scheduler.Test.Functions
{
    public class SchedulePickupTests
    {
        private readonly IHttpClientFactory _mockHttpClientFactory = Substitute.For<IHttpClientFactory>();

        [Fact]
        public async Task SchedulePickupFunction_RunSchedulePickup_Success ()
        {

            // Arrange
            var url = "/schedules/check";
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
            var schedulePickupLogger = Substitute.For<ILogger<SchedulePickup>>();
            var fakeLoggerFactory = Substitute.For<ILoggerFactory>();
            fakeLoggerFactory.CreateLogger<WorkflowApi>().Returns(workflowApiFakeLogger);
            fakeLoggerFactory.CreateLogger<SchedulePickup>().Returns(schedulePickupLogger);

            var _workflowApi = new WorkflowApi(_mockHttpClientFactory, fakeLoggerFactory.CreateLogger<WorkflowApi>());
            var schedulePickupFunction = new SchedulePickup(_workflowApi, fakeLoggerFactory.CreateLogger<SchedulePickup>());
            // Act
            await schedulePickupFunction.Run(null);

            // Assert
            var workflowApiLogCalls = workflowApiFakeLogger.ReceivedCalls().Where(call => call.GetMethodInfo().Name == "Log").ToList();
            var schedulePickupLogCall = schedulePickupLogger.ReceivedCalls().Where(call => call.GetMethodInfo().Name == "Log").ToList();

            Assert.Single(workflowApiLogCalls);
            Assert.Single(schedulePickupLogCall);

            // Check the second log message 
            var args2 = workflowApiLogCalls[0].GetArguments();
            Assert.Equal(LogLevel.Information, args2[0]);
            Assert.Contains("Status Code: OK", args2[2].ToString());

        }


        [Fact]
        public async Task SchedulePickupFunction_RunSchedulePickup_Fail()
        {

            // Arrange
            var url = "/schedules/check";
            var fakeHttpMessageHandler = new MockHttpMessageHandler(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
            },url);
            var fakeHttpClient = new HttpClient(fakeHttpMessageHandler)
            {
                BaseAddress = new Uri("https://localhost")
            };

            _mockHttpClientFactory.CreateClient(ApiServiceNames.WorkflowCore).Throws(new Exception("Test Exception"));
            var workflowApiFakeLogger = Substitute.For<ILogger<WorkflowApi>>();
            var schedulePickupLogger = Substitute.For<ILogger<SchedulePickup>>();
            var fakeLoggerFactory = Substitute.For<ILoggerFactory>();
            fakeLoggerFactory.CreateLogger<WorkflowApi>().Returns(workflowApiFakeLogger);
            fakeLoggerFactory.CreateLogger<SchedulePickup>().Returns(schedulePickupLogger);

            var _workflowApi = new WorkflowApi(_mockHttpClientFactory, fakeLoggerFactory.CreateLogger<WorkflowApi>());
            var schedulePickupFunction = new SchedulePickup(_workflowApi, fakeLoggerFactory.CreateLogger<SchedulePickup>());
            // Act
            await schedulePickupFunction.Run(null);

            // Assert
            var workflowApiLogCalls = workflowApiFakeLogger.ReceivedCalls().Where(call => call.GetMethodInfo().Name == "Log").ToList();
            var schedulePickupLogCall = schedulePickupLogger.ReceivedCalls().Where(call => call.GetMethodInfo().Name == "Log").ToList();

            Assert.Equal(2, schedulePickupLogCall.Count);

            // Check the second log message 
            var args2 = schedulePickupLogCall[1].GetArguments();
            Assert.Equal(LogLevel.Error, args2[0]);
            Assert.Contains("SchedulePickup function failed", args2[2].ToString());

        }
    }
}
