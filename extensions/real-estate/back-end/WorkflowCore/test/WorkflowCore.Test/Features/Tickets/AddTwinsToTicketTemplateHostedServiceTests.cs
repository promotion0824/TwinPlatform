using AutoFixture;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Willow.Common;
using Willow.Data;
using Willow.Tests.Infrastructure;
using WorkflowCore.Dto;
using WorkflowCore.Entities;
using WorkflowCore.Models;
using WorkflowCore.Repository;
using WorkflowCore.Services;
using WorkflowCore.Services.Apis;
using WorkflowCore.Services.Background;
using Xunit.Abstractions;
using Xunit;
using Newtonsoft.Json;
using WorkflowCore.Infrastructure.Helpers;
using Willow.Calendar;
using LazyCache.Mocks;

namespace WorkflowCore.Test.Features.Tickets;

public class AddTwinsToTicketTemplateHostedServiceTests : BaseInMemoryTest
{
    private readonly IConfiguration configuration;
    public AddTwinsToTicketTemplateHostedServiceTests(ITestOutputHelper output) : base(output)
    {
        var inMemorySettings = new Dictionary<string, string>
            {
                { "BackgroundJobOptions:TicketTemplate:EnableProcess", "true" },
                { "BackgroundJobOptions:TicketTEplate:BatchSize","2"}
            };

        configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact(Skip = "Causes pipeline test command to hang due to thread locks")]
    public async Task Verify_HostedService_Executes()
    {
        var ticketService = new Mock<IWorkflowService>();
        ticketService.Setup(c => c.AddTwinsToTicketTemplateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var serviceScope = new Mock<IServiceScope>();
        var logger = GetLogger<AddTwinsToTicketTemplateHostedService>();
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(c => c.GetService(typeof(IServiceScope))).Returns(serviceScope.Object);
        serviceProvider.Setup(c => c.GetService(typeof(IWorkflowService))).Returns(ticketService.Object);

        var hostedService = new AddTwinsToTicketTemplateHostedService(logger.Object, serviceProvider.Object, configuration);

        await hostedService.StartAsync(It.IsAny<CancellationToken>());
        await Task.Delay(1000);
        var task = hostedService.ExecuteTask;

        logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce, "the 'adding Twins to the TicketTemplatess' process is enabled and running");
        task.Should().NotBeNull();
    }

    [Fact(Skip = "Causes pipeline test command to hang due to thread locks")]
    public async Task TicketTemplatessWithoutTwins_TicketService_AddedTwinsToTheTicketTemplates()
    {
        var siteIdFirstGroup = Guid.NewGuid();

        var expectedTicketTemplateAssets = Fixture.Build<TicketAsset>().CreateMany(3);

        var expectedTicketTemplateEntitiesFirstGroup = Fixture.Build<TicketTemplateEntity>()
            .With(c => c.SiteId, siteIdFirstGroup)
            .With(c => c.Assets, JsonConvert.SerializeObject(expectedTicketTemplateAssets))
            .With(x => x.Recurrence, JsonConvert.SerializeObject(_event1))
            .With(x => x.OverdueThreshold, "3;3")
            .Without(c => c.Twins)
            .Without(c => c.Attachments)
            .Without(x => x.Tasks)
            .Without(x => x.DataValue)
            .CreateMany(2);

        var notExpectedTicketTemplatesEntities = Fixture.Build<TicketTemplateEntity>()
            .With(c => c.SiteId, siteIdFirstGroup)
            .Without(c => c.Assets)
            .Without(c => c.Twins)
            .CreateMany(2);

        var expectedTwinIds = expectedTicketTemplateAssets.Select(x => new TwinIdDto()
        {
            Id = x.AssetId.ToString(),
            UniqueId = x.AssetId.ToString()
        }).ToList();

        await using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDbWithTicketTemplateHostedJobSetting);

        var queryString = HttpHelper.ToQueryString(new { uniqueIds = expectedTicketTemplateAssets.Select(x => x.AssetId) });

        server.Arrange().GetDigitalTwinApi()
            .SetupRequest(HttpMethod.Get, $"admin/sites/{siteIdFirstGroup}/twins/byUniqueId/batch?{queryString}")
            .ReturnsJson(expectedTwinIds);

        var db = server.Arrange().CreateDbContext<WorkflowContext>();

        db.TicketTemplates.RemoveRange(db.TicketTemplates.ToList());
        await db.TicketTemplates.AddRangeAsync(expectedTicketTemplateEntitiesFirstGroup);
        await db.TicketTemplates.AddRangeAsync(notExpectedTicketTemplatesEntities);
        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();
        var logger = GetLogger<WorkflowService>();
        var workflowService = new WorkflowService(new Mock<IDateTimeService>().Object,
                              new WorkflowRepository(db,
                                                     new Mock<IDateTimeService>().Object,
                                                     new Mock<IDirectoryApiService>().Object,
                                                     new Mock<IMarketPlaceApiService>().Object,
                                                     new Mock<ITicketStatusTransitionsService>().Object,
                                                     new Mock<ITicketStatusService>().Object,
                                                     new MockCachingService()),
                                                     new Mock<IWorkflowNotificationService>().Object,
                                                     new Mock<IReadRepository<Guid, Site>>().Object,
                                                     logger.Object,
                                                     new Mock<IInsightServiceApi>().Object,
                                                     server.Arrange().MainServices.GetRequiredService<IDigitalTwinServiceApi>(),
                                                     new Mock<IWorkflowSequenceNumberService>().Object);

        await workflowService.AddTwinsToTicketTemplateAsync(10, CancellationToken.None);

        var ticketTemplates = db.TicketTemplates.Where(c => c.Twins != null).ToList();
        ticketTemplates.Should().NotBeNullOrEmpty();
        ticketTemplates.Count.Should().Be(2);
        ticketTemplates.All(x => x.Assets == null).Should().Be(true);
        ticketTemplates.All(x => x.Twins != null).Should().Be(true);
    }

    private static Mock<ILogger<T>> GetLogger<T>()
    {
        var logger = new Mock<ILogger<T>>();

        logger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
            .Callback(new InvocationAction(invocation =>
            {
                var logLevel = (LogLevel)invocation.Arguments[0]; // The first two will always be whatever is specified in the setup above
                var eventId = (EventId)invocation.Arguments[1];  // so I'm not sure you would ever want to actually use them
                var state = invocation.Arguments[2];
                var exception = (Exception)invocation.Arguments[3];
                var formatter = invocation.Arguments[4];

                var invokeMethod = formatter.GetType().GetMethod("Invoke");
                var logMessage = (string)invokeMethod?.Invoke(formatter, new[] { state, exception });

                Trace.WriteLine($"{logLevel} - {logMessage}");
            }));

        return logger;
    }

    private static Event _event1 = new Event
    {
        StartDate = DateTime.Parse("2021-01-14T00:00:00"),
        Occurs = Event.Recurrence.Monthly,
        Timezone = "Pacific Standard Time",
        DayOccurrences = new List<Event.DayOccurrence>
            {
                new Event.DayOccurrence
                {
                    Ordinal = 1,
                    DayOfWeek = DayOfWeek.Wednesday
                }
            }
    };
}
