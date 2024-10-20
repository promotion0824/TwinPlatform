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
using Willow.Tests.Infrastructure;
using WorkflowCore.Dto;
using WorkflowCore.Entities;
using WorkflowCore.Repository;
using WorkflowCore.Services;
using WorkflowCore.Services.Apis;
using WorkflowCore.Services.Background;
using Xunit.Abstractions;
using Xunit;

namespace WorkflowCore.Test.Features.Inspections;

public class AddTwinIdToInspectionHostedServiceTests : BaseInMemoryTest
{
    private readonly IConfiguration configuration;
    public AddTwinIdToInspectionHostedServiceTests(ITestOutputHelper output) : base(output)
    {
        var inMemorySettings = new Dictionary<string, string>
            {
                { "BackgroundJobOptions:Inspection:EnableProcess", "true" },
                { "BackgroundJobOptions:Inspection:BatchSize","2"}
            };
        configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact(Skip = "Causes pipeline test command to hang due to thread locks")]
    public async Task Verify_HostedService_Executes()
    {
        var InspectionService = new Mock<IInspectionService>();
        InspectionService.Setup(c => c.AddTwinIdToInspectionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var serviceScope = new Mock<IServiceScope>();
        var logger = GetLogger<AddTwinIdToInspectionHostedService>();
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(c => c.GetService(typeof(IServiceScope))).Returns(serviceScope.Object);
        serviceProvider.Setup(c => c.GetService(typeof(IInspectionService))).Returns(InspectionService.Object);

        var hostedService = new AddTwinIdToInspectionHostedService(logger.Object, serviceProvider.Object, configuration);

        await hostedService.StartAsync(It.IsAny<CancellationToken>());
        await Task.Delay(1000);
        var task = hostedService.ExecuteTask;

        logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce, "the 'adding TwinId to the Inspections' process is enabled and running");
        task.Should().NotBeNull();

    }

    [Fact(Skip = "Causes pipeline test command to hang due to thread locks")]
    public async Task InspectionsWithoutTwinId_InspectionService_AddedTwinIdToTheInspections()
    {
        var siteIdFirstGroup = Guid.NewGuid();
        var assetIdFirstGroup = Guid.NewGuid();
        var expectedInspectionEntitiesFirstGroup = Fixture.Build<InspectionEntity>()
            .With(c => c.SiteId, siteIdFirstGroup)
            .With(c => c.AssetId, assetIdFirstGroup)
            .Without(i => i.FrequencyDaysOfWeekJson)
            .Without(x => x.Zone)
            .Without(c => c.TwinId)
            .Without(i => i.Checks)
            .Without(x => x.LastRecord)
            .CreateMany(2);

        var siteIdSecondGroup = Guid.NewGuid();
        var assetIdSecondGroup = Guid.NewGuid();
        var expectedInspectionEntitiesSecondGroup = Fixture.Build<InspectionEntity>()
            .With(c => c.SiteId, siteIdSecondGroup)
            .With(c => c.AssetId, assetIdSecondGroup)
            .Without(i => i.FrequencyDaysOfWeekJson)
            .Without(x => x.Zone)
            .Without(c => c.TwinId)
            .Without(i => i.Checks)
            .Without(x => x.LastRecord)
            .CreateMany(2);

        var siteId3Group = Guid.NewGuid();
        var assetId3Group = Guid.NewGuid();
        var expectedInspectionEntities3Group = Fixture.Build<InspectionEntity>()
            .With(c => c.SiteId, siteId3Group)
            .With(c => c.AssetId, assetId3Group)
            .Without(i => i.FrequencyDaysOfWeekJson)
            .Without(x => x.Zone)
            .Without(c => c.TwinId)
            .Without(i => i.Checks)
            .Without(x => x.LastRecord)
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

        await using var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDbWithInspectionHostedJobSetting);
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
        db.Inspections.RemoveRange(db.Inspections.ToList());
        await db.Inspections.AddRangeAsync(expectedInspectionEntitiesFirstGroup);
        await db.Inspections.AddRangeAsync(expectedInspectionEntitiesSecondGroup);
        await db.Inspections.AddRangeAsync(expectedInspectionEntities3Group);
        await db.SaveChangesAsync();

        db.ChangeTracker.Clear();
        var logger = GetLogger<InspectionService>();
        var inspectionService = new InspectionService(new InspectionRepository(db), new Mock<ISiteService>().Object, new Mock<IDateTimeService>().Object,
            server.Arrange().MainServices.GetRequiredService<IDigitalTwinServiceApi>(), logger.Object,null);

        await inspectionService.AddTwinIdToInspectionsAsync(10, CancellationToken.None);

        var inspections = db.Inspections.Where(c => c.TwinId == null).ToList();
        inspections.Should().NotBeNullOrEmpty();
        inspections.Count.Should().Be(2);
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
