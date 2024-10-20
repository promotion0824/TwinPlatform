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
using LazyCache.Mocks;

namespace WorkflowCore.Test.Features.Tickets;

public class AddTwinIdToTicketHostedServiceTests : BaseInMemoryTest
{
    private readonly IConfiguration configuration;
    public AddTwinIdToTicketHostedServiceTests(ITestOutputHelper output) : base(output)
    {
        var inMemorySettings = new Dictionary<string, string>
            {
                { "BackgroundJobOptions:Ticket:EnableProcess", "true" },
                { "BackgroundJobOptions:Ticket:BatchSize","2"}
            };
        configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact(Skip = "Causes pipeline test command to hang due to thread locks")]
    public async Task Verify_HostedService_Executes()
    {
        var ticketService = new Mock<IWorkflowService>();
        ticketService.Setup(c => c.AddTwinIdToTicketAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var serviceScope = new Mock<IServiceScope>();
        var logger = GetLogger<AddTwinIdToTicketHostedService>();
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(c => c.GetService(typeof(IServiceScope))).Returns(serviceScope.Object);
        serviceProvider.Setup(c => c.GetService(typeof(IWorkflowService))).Returns(ticketService.Object);

        var hostedService = new AddTwinIdToTicketHostedService(logger.Object, serviceProvider.Object, configuration);

        await hostedService.StartAsync(It.IsAny<CancellationToken>());
        await Task.Delay(1000);
        var task = hostedService.ExecuteTask;

        logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce, "the 'adding TwinId to the Tickets' process is enabled and running");
        task.Should().NotBeNull();

    }

    [Fact(Skip = "Causes pipeline test command to hang due to thread locks")]
    public async Task TicketsWithoutTwinId_TicketService_AddedTwinIdToTheTickets()
    {
        var siteIdFirstGroup = Guid.NewGuid();
        var assetIdFirstGroup = Guid.NewGuid();
        var expectedTicketEntitiesFirstGroup = Fixture.Build<TicketEntity>()
            .With(c => c.SiteId, siteIdFirstGroup)
            .With(c => c.IssueId, assetIdFirstGroup)
            .Without(c => c.TwinId)
            .Without(x => x.JobType)
            .Without(x => x.Diagnostics)
            .Without(i => i.Attachments)
            .Without(x => x.Comments)
            .Without(x => x.Category)
            .Without(x => x.Tasks)
            .CreateMany(2);

        var siteIdSecondGroup = Guid.NewGuid();
        var assetIdSecondGroup = Guid.NewGuid();
        var expectedTicketEntitiesSecondGroup = Fixture.Build<TicketEntity>()
            .With(c => c.SiteId, siteIdSecondGroup)
            .With(c => c.IssueId, assetIdSecondGroup)
            .Without(c => c.TwinId)
            .Without(i => i.Attachments)
            .Without(x => x.JobType)
            .Without(x => x.Diagnostics)
            .Without(x => x.Comments)
            .Without(x => x.Category)
            .Without(x => x.Tasks)
            .CreateMany(2);

        var siteId3Group = Guid.NewGuid();
        var assetId3Group = Guid.NewGuid();
        var expectedTicketEntities3Group = Fixture.Build<TicketEntity>()
            .With(c => c.SiteId, siteId3Group)
            .With(c => c.IssueId, assetId3Group)
            .Without(c => c.TwinId)
            .Without(i => i.Attachments)
            .Without(x => x.Comments)
            .Without(x => x.JobType)
            .Without(x => x.Diagnostics)
            .Without(x => x.Category)
            .Without(x => x.Tasks)
            .CreateMany(2);

        var notExpectedTicketEntities = Fixture.Build<TicketEntity>()
            .With(c => c.SiteId, siteId3Group)
            .Without(c => c.IssueId)
            .Without(c => c.TwinId)
            .Without(i => i.Attachments)
            .Without(x => x.JobType)
            .Without(x => x.Diagnostics)
            .Without(x => x.Category)
            .Without(x => x.Comments)
            .Without(x => x.Tasks)
            .CreateMany(2);
        var expectedTwinIds = new List<TwinIdDto>()
            {
                Fixture.Build<TwinIdDto>()
                    .With(c => c.UniqueId, assetIdFirstGroup.ToString())
                    .Create(),
                Fixture.Build<TwinIdDto>()
                    .With(c => c.UniqueId, assetIdSecondGroup.ToString())
                    .Create()
            };

        await using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDbWithTicketHostedJobSetting);
        server.Arrange().GetDigitalTwinApi()
            .SetupRequest(HttpMethod.Get, $"admin/sites/{siteIdFirstGroup}/twins/byUniqueId/batch?uniqueIds={assetIdFirstGroup}")
            .ReturnsJson(expectedTwinIds);
        server.Arrange().GetDigitalTwinApi()
            .SetupRequest(HttpMethod.Get, $"admin/sites/{siteIdSecondGroup}/twins/byUniqueId/batch?uniqueIds={assetIdSecondGroup}")
            .ReturnsJson(expectedTwinIds);

        server.Arrange().GetDigitalTwinApi()
            .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId3Group}/twins/byUniqueId/batch?uniqueIds={assetId3Group}")
            .ReturnsJson(new List<TwinIdDto>());

        var db = server.Arrange().CreateDbContext<WorkflowContext>();
        db.Tickets.RemoveRange(db.Tickets.ToList());
        await db.Tickets.AddRangeAsync(expectedTicketEntitiesFirstGroup);
        await db.Tickets.AddRangeAsync(expectedTicketEntitiesSecondGroup);
        await db.Tickets.AddRangeAsync(expectedTicketEntities3Group);
        await db.Tickets.AddRangeAsync(notExpectedTicketEntities);
        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();
        var logger = GetLogger<WorkflowService>();
        var TicketService = new WorkflowService(new Mock<IDateTimeService>().Object,
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

        await TicketService.AddTwinIdToTicketAsync(10, CancellationToken.None);

        var Tickets = db.Tickets.Where(c => c.IssueId.HasValue && c.TwinId == null).ToList();
        Tickets.Should().NotBeNullOrEmpty();
        Tickets.Count.Should().Be(2);
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

}
