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
using Xunit.Abstractions;
using Xunit;
using SiteCore.Services;
using SiteCore.Services.Background;
using SiteCore.Entities;
using SiteCore.Services.DigitalTwinCore;
using SiteCore.Tests;
using SiteCore.Dto;

namespace WorkflowCore.Test.Features.Inspections;

public class AddScopeIdToWidgetsHostedServiceTests : BaseInMemoryTest
{
    private readonly IConfiguration configuration;
    public AddScopeIdToWidgetsHostedServiceTests(ITestOutputHelper output) : base(output)
    {
        var widgetBackgroundJobOptions = new WidgetBackgroundJobOptions() { EnableProcess = true, BatchSize = 10 };

        var inMemorySettings = new Dictionary<string, string>
            {
                { "BackgroundJobOptions:Widget:EnableProcess", "true" },
                { "BackgroundJobOptions:Widget:BatchSize","2"}
            };

        configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact(Skip = "Causes pipeline test command to hang due to thread locks")]
    public async Task Verify_HostedService_Executes()
    {
        var service = new Mock<IWidgetService>();
        service.Setup(c => c.AddScopedFromSiteWidgetsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var serviceScope = new Mock<IServiceScope>();
        var logger = GetLogger<AddScopeIdToWidgetsHostedService>();
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(c => c.GetService(typeof(IServiceScope))).Returns(serviceScope.Object);
        serviceProvider.Setup(c => c.GetService(typeof(IWidgetService))).Returns(service.Object);

        var hostedService = new AddScopeIdToWidgetsHostedService(logger.Object, serviceProvider.Object, null);

        await hostedService.StartAsync(It.IsAny<CancellationToken>());
        await Task.Delay(1000);
        var task = hostedService.ExecuteTask;

        logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce, $"the {AddScopeIdToWidgetsHostedService.ProcessName} process is enabled and running");
        task.Should().NotBeNull();
    }

    [Fact(Skip = "Causes pipeline test command to hang due to thread locks")]
    public async Task WidgetService_AddedScopedWidgets()
    {
        var siteId = Guid.NewGuid();
        var widgetId = Guid.NewGuid();
        var scopeId = "TestScopeId";
        var expectedWidgetEntities = Fixture.Build<ScopeWidgetEntity>()
            .With(c => c.ScopeId, scopeId)
            .With(c => c.WidgetId, widgetId)
            .CreateMany(2);

        var expectedTwinIds = new List<TwinDto>()
        {
            Fixture.Build<TwinDto>()
                .With(c => c.UniqueId, siteId.ToString())

                .Create(),
            Fixture.Build<TwinDto>()
                .With(c => c.UniqueId, siteId.ToString())
                .Create()
        };

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        {
            server.Arrange().GetDigitalTwinApi()
            .SetupRequest(HttpMethod.Get, $"admin/sites/{siteId}/twins/byUniqueId/batch?uniqueIds={siteId}")
            .ReturnsJson(expectedTwinIds);

            var db = server.Arrange().CreateDbContext<SiteDbContext>();
            db.ScopeWidgets.RemoveRange(db.ScopeWidgets.ToList());
            await db.ScopeWidgets.AddRangeAsync(expectedWidgetEntities);
            await db.SaveChangesAsync();

            db.ChangeTracker.Clear();
            var logger = GetLogger<WidgetService>();
            var service = new WidgetService(db, server.Arrange().MainServices.GetRequiredService<IDigitalTwinCoreApiService>(), logger.Object);

            await service.AddScopedFromSiteWidgetsAsync(10, CancellationToken.None);

            var scopeWidhgets = db.ScopeWidgets.Where(c => c.ScopeId == scopeId).ToList();
            scopeWidhgets.Should().NotBeNullOrEmpty();
            scopeWidhgets.Count.Should().Be(2);
        }
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
