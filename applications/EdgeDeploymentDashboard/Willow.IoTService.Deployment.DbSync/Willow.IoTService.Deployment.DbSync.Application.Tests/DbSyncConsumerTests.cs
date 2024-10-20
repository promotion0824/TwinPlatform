namespace Willow.IoTService.Deployment.DbSync.Application.Tests;

using ConnectorCore.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Willow.IoTService.Deployment.DataAccess.Entities;
using Willow.IoTService.Deployment.DataAccess.Services;
using Willow.IoTService.Deployment.DbSync.Application.HealthChecks;
using Willow.IoTService.Deployment.DbSync.Application.Infrastructure;
using Willow.IoTService.Deployment.DbSync.Application.Tests.Infrastructure;
using Xunit;

public class DbSyncConsumerTests
{
    [Fact]
    public async Task ConnectorSyncConsumer_Consume_Calls_UpdateModule_Methods_Once()
    {
        var validator = new ConnectorMessageValidator();
        var message = new ConnectorSyncMessageMock
        {
            Archived = false,
            ConnectorId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Enabled = true,
            Status = ConnectorUpdateStatus.Enable,
            Timestamp = DateTime.UtcNow,
        };

        var context = Substitute.For<ConsumeContext<IConnectorMessage>>();
        context.Message.Returns(message);

        var updateModuleServiceMock = Substitute.For<IUpdateModuleService>();
        var healthCheckSqlMock = Substitute.For<HealthCheckSql>();
        var healthCheckServiceBusMock = Substitute.For<HealthCheckServiceBus>();
        updateModuleServiceMock.UpdateModuleAsync(Arg.Any<IConnectorMessage>(), Arg.Any<CancellationToken>())
                               .Returns(GenerateModule(message.ConnectorId));
        updateModuleServiceMock.UpdateModuleConfigAsync(Arg.Any<IConnectorMessage>(), Arg.Any<CancellationToken>())
                               .Returns(GenerateModule(message.ConnectorId));

        var consumer = new ConnectorSyncConsumer(new NullLogger<ConnectorSyncConsumer>(),
                                                 validator,
                                                 updateModuleServiceMock,
                                                 healthCheckSqlMock,
                                                 healthCheckServiceBusMock);

        await consumer.Consume(context);

        await updateModuleServiceMock.Received(1).UpdateModuleAsync(Arg.Any<IConnectorMessage>(), Arg.Any<CancellationToken>());
        await updateModuleServiceMock.Received(1).UpdateModuleConfigAsync(Arg.Any<IConnectorMessage>(), Arg.Any<CancellationToken>());
    }

    // ... rest of the tests
    private static ModuleDto GenerateModule(Guid? moduleId = null)
    {
        return new ModuleDto(moduleId ?? Guid.NewGuid(),
                             "Test",
                             "BACNET",
                             Status: "Passed",
                             StatusMessage: string.Empty,
                             IsArchived: false,
                             IsSynced: true,
                             IsAutoDeployment: false,
                             DeviceName: "TestDevice",
                             IoTHubName: "TestIoTHub",
                             Environment: "DEV",
                             Version: "1.0.0",
                             Platform: Platforms.arm64v8,
                             AssignedBy: "TestAssignee",
                             DateTimeApplied: DateTimeOffset.UtcNow);
    }
}
