using AutoFixture;
using InsightCore.Dto;
using InsightCore.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using InsightCore.Entities;
using InsightCore.Services.Background;
using Microsoft.Extensions.Caching.Memory;
using Willow.Infrastructure.Services;
using Willow.Tests.Infrastructure;
using Workflow.Tests;
using Xunit.Abstractions;
using Xunit;
using Willow.Notifications.Interfaces;

namespace InsightCore.Test.HostedServices;

public class AddTwinNameToInsightHostedServiceTests:BaseInMemoryTest
{
    private readonly IConfiguration configuration;
    public AddTwinNameToInsightHostedServiceTests(ITestOutputHelper output) : base(output)
    {
        var inMemorySettings = new Dictionary<string, string>
            {
                { "BackgroundJobOptions:EnableProcess", "true" },
                { "BackgroundJobOptions:BatchSize","2"}
            };
        configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task Verify_HostedService_Executes()
    {
        var InsightService = new Mock<IInsightService>();
        InsightService.Setup(c => c.AddMissingTwinDetailsToInsightsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var serviceScope = new Mock<IServiceScope>();
        var logger = GetLogger<AddTwinNameToInsightHostedService>();
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(c => c.GetService(typeof(IServiceScope))).Returns(serviceScope.Object);
        serviceProvider.Setup(c => c.GetService(typeof(IInsightService))).Returns(InsightService.Object);

        var hostedService = new AddTwinNameToInsightHostedService(logger.Object, serviceProvider.Object, configuration);

        await hostedService.StartAsync(It.IsAny<CancellationToken>());
        await Task.Delay(1000);
        var task = hostedService.ExecuteTask;

        logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce, "The process for adding TwinName to the Insight enabled and running");
        task.Should().NotBeNull();

    }

    [Fact]
    public async Task InsightsWithoutTwinId_InsightService_AddedTwinIdToTheInsights()
    {
        var siteIdFirstGroup = Guid.NewGuid();

        var expectedInsightEntities = Fixture.Build<InsightEntity>()
            .With(c => c.SiteId, siteIdFirstGroup)
            .Without(i => i.TwinName)
            .Without(i=>i.PointsJson)
            .Without(i=>i.StatusLogs)
            .Without(i => i.ImpactScores)
            .Without(i => i.InsightOccurrences)
            .CreateMany(2).ToList();

        var siteIdSecondGroup = Guid.NewGuid();
        expectedInsightEntities .AddRange(Fixture.Build<InsightEntity>()
            .With(c => c.SiteId, siteIdSecondGroup)
            .Without(i => i.TwinName)
            .Without(i=>i.PointsJson)
            .Without(i=>i.StatusLogs)
            .Without(i => i.ImpactScores)
            .Without(i => i.InsightOccurrences)
            .CreateMany(2));

        var siteId3Group = Guid.NewGuid();

        expectedInsightEntities.AddRange( Fixture.Build<InsightEntity>()
            .With(c => c.SiteId, siteId3Group)
            .Without(i=>i.PrimaryModelId)
            .Without(i => i.PointsJson)
            .Without(i => i.StatusLogs)
            .Without(i => i.ImpactScores)
            .Without(i => i.InsightOccurrences)
            .CreateMany(2));

        var twinSimpleResponse = expectedInsightEntities.Select(c=>Fixture
            .Build<TwinSimpleDto>()
            .With(x => x.Id, c.TwinId)
            .With(x => x.SiteId, c.SiteId)
            .Create()).ToList();
        using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDbWithHostedJobSetting);

        var memoryCacheMock = new Mock<IMemoryCache>();

        memoryCacheMock.Setup(c => c.CreateEntry(It.IsAny<string>())).Returns(Mock.Of<ICacheEntry>);
      
        var db = server.Arrange().CreateDbContext<InsightDbContext>();
        db.Insights.RemoveRange(db.Insights.ToList());
        await db.Insights.AddRangeAsync(expectedInsightEntities);

        await db.SaveChangesAsync();
        server.Arrange().GetDigitalTwinApi().
            SetupRequest(HttpMethod.Post, "sites/Assets/names")
            .ReturnsJson(twinSimpleResponse);
        db.ChangeTracker.Clear();
        var logger = GetLogger<InsightService>();
        var InsightService = new InsightService(new Mock<IDateTimeService>().Object,new InsightRepository(db,new Mock<INotificationService>().Object, new Mock<IConfiguration>().Object), new Mock<IAnalyticsService>().Object,
            server.Arrange().MainServices.GetRequiredService<IDigitalTwinServiceApi>(), new Mock<IWorkflowServiceApi>().Object, logger.Object,null, memoryCacheMock.Object);

        await InsightService.AddMissingTwinDetailsToInsightsAsync(10, CancellationToken.None);

        var Insights = db.Insights.Where(c =>string.IsNullOrWhiteSpace(c.PrimaryModelId)).ToList();
        Insights.Should().BeNullOrEmpty();

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
