using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using Testcontainers.MsSql;
using Willow.CommandAndControl.Application.Services;
using Willow.CommandAndControl.Application.Services.Abstractions;
using Willow.CommandAndControl.Data;
using Willow.CommandAndControl.Data.Enums;
using Willow.CommandAndControl.Data.Models;
using Willow.CommandAndControl.Tests.Mock;
using Xunit;

namespace Willow.CommandAndControl.Tests;

public class OverlappingCommandsTests : IAsyncLifetime
{
    private readonly IActivityLogger _activityLoggerMock = Substitute.For<IActivityLogger>();

    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder().Build();
    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
    }


    [Fact]
    public async Task CommandManager_GetOverlappingRequestedCommands_With_Pending_Status_Returns_None()
    {
        //Arrange
        var httpContextAccessor = NSubstitute.Substitute.For<IHttpContextAccessor>();
        var userInfoService = new UserInfoService(httpContextAccessor);
        var interceptor = new BaseEntitySaveChangesInterceptor(userInfoService);
        var dbContext = await MockedDbFactory.CreateAsync(_msSqlContainer.GetConnectionString());
        var logger = NSubstitute.Substitute.For<ILogger<CommandManager>>();
        var conflictDetector = new ConflictDetector();
        var conflictResolver = new ConflictResolver(conflictDetector);
        var mappedMock = new Mock<IMappedGatewayService>();
        var dbTransactionsMock = new Mock<IDbTransactions>();
        var commandManager = new CommandManager(logger,
                                                dbContext,
                                                dbTransactionsMock.Object,
                                                mappedMock.Object,
                                                conflictResolver);


        var requestCommandId = Guid.NewGuid();
        var future = DateTime.Now.AddDays(2);

        var requestedCommands = new List<RequestedCommand>(){
            new()
            {
                Id = requestCommandId,
                Status = RequestedCommandStatus.Pending,
                ConnectorId = "b5dd9d50-8fd5-45a4-a9bf-42fff3287bbb",
                CommandName = "atLeast",
                ExternalId = "13018",
                Value = 70,
                Unit = "F",
                StartTime = new DateTimeOffset(future.Year,future.Month,future.Day,10,0,0,TimeSpan.FromHours(-2)),
                EndTime = new DateTimeOffset(future.Year,future.Month,future.Day,13,0,0,TimeSpan.FromHours(-2)),
StatusUpdatedBy = new User { Name = "test" },
                CreatedDate = DateTimeOffset.UtcNow,
                LastUpdated = DateTimeOffset.UtcNow,
                IsCapabilityOf = "test",
                IsHostedBy = "test",
                Location = "test",
                RuleId = "test",
                SiteId = "test",
                TwinId = "test",
                Type = "atLeast",
                ReceivedDate = DateTimeOffset.UtcNow,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Status = RequestedCommandStatus.Pending,
                ConnectorId = "b5dd9d50-8fd5-45a4-a9bf-42fff3287bbb",
                CommandName = "atLeast",
                ExternalId = "13018",
                Value = 70,
                Unit = "F",
                StartTime = new DateTimeOffset(future.Year,future.Month,future.Day,11,0,0,TimeSpan.FromHours(-2)),
                EndTime = new DateTimeOffset(future.Year,future.Month,future.Day,12,0,0,TimeSpan.FromHours(-2)),
                StatusUpdatedBy = new User { Name = "test" },
                CreatedDate = DateTimeOffset.UtcNow,
                LastUpdated = DateTimeOffset.UtcNow,
                IsCapabilityOf = "test",
                IsHostedBy = "test",
                Location = "test",
                RuleId = "test",
                SiteId = "test",
                TwinId = "test",
                Type = "atLeast",
                ReceivedDate = DateTimeOffset.UtcNow,
            }
        };

        //Act
        //await commandManager.CreateRequestedCommandsAsync(requestedCommands, CancellationToken.None);
        dbContext.RequestedCommands.AddRange(requestedCommands);
        await dbContext.SaveChangesAsync();

        var overlappingCommands = await commandManager.GetOverlappingRequestedCommandsAsync(requestCommandId, CancellationToken.None);

        //Assert
        overlappingCommands.Should().NotBeNull();
        overlappingCommands.Count.Should().Be(0);

    }

    [Fact]
    public async Task CommandManager_GetOverlappingRequestedCommands_With_NonOverlaping_Commands_Returns_None()
    {
        //Arrange
        var httpContextAccessor = NSubstitute.Substitute.For<IHttpContextAccessor>();
        var userInfoService = new UserInfoService(httpContextAccessor);
        var interceptor = new BaseEntitySaveChangesInterceptor(userInfoService);
        var dbContext = await MockedDbFactory.CreateAsync(_msSqlContainer.GetConnectionString());
        var logger = NSubstitute.Substitute.For<ILogger<CommandManager>>();
        var conflictDetector = new ConflictDetector();
        var conflictResolver = new ConflictResolver(conflictDetector);
        var mappedMock = new Mock<IMappedGatewayService>();
        var dbTransactionsMock = new Mock<IDbTransactions>();
        var commandManager = new CommandManager(logger,
                                                dbContext,
                                                dbTransactionsMock.Object,
                                                mappedMock.Object,
                                                conflictResolver);


        var requestCommandId = Guid.NewGuid();
        var future = DateTime.Now.AddDays(2);


        List<RequestedCommand> requestedCommands = [
            new RequestedCommand
            {
                Id = requestCommandId,
                Status = RequestedCommandStatus.Approved,
                ConnectorId = "b5dd9d50-8fd5-45a4-a9bf-42fff3287bbb",
                CommandName = "atLeast",
                ExternalId = "13018",
                Value = 70,
                Unit = "F",
                StartTime = new DateTimeOffset(future.Year, future.Month, future.Day, 10, 0, 0, TimeSpan.FromHours(-2)),
                EndTime = new DateTimeOffset(future.Year, future.Month, future.Day, 13, 0, 0, TimeSpan.FromHours(-2)),
                StatusUpdatedBy = new User { Name = "test" },
                CreatedDate = DateTimeOffset.UtcNow,
                LastUpdated = DateTimeOffset.UtcNow,
                IsCapabilityOf = "test",
                IsHostedBy = "test",
                Location = "test",
                RuleId = "test",
                SiteId = "test",
                TwinId = "test",
                Type = "atLeast",
                ReceivedDate = DateTimeOffset.UtcNow,
            },
            new RequestedCommand
            {
                Id = Guid.NewGuid(),
                Status = RequestedCommandStatus.Approved,

                ConnectorId = "b5dd9d50-8fd5-45a4-a9bf-42fff3287bbb",
                CommandName = "atLeast",
                ExternalId = "13018",
                Value = 70,
                Unit = "F",
                StartTime = new DateTimeOffset(future.Year, future.Month, future.Day, 13, 0, 1, TimeSpan.FromHours(-2)),
                EndTime = new DateTimeOffset(future.Year, future.Month, future.Day, 14, 0, 0, TimeSpan.FromHours(-2)),
                StatusUpdatedBy = new User { Name = "test" },
                CreatedDate = DateTimeOffset.UtcNow,
                LastUpdated = DateTimeOffset.UtcNow,
                IsCapabilityOf = "test",
                IsHostedBy = "test",
                Location = "test",
                RuleId = "test",
                SiteId = "test",
                TwinId = "test",
                Type = "atLeast",
                ReceivedDate = DateTimeOffset.UtcNow,
            }
        ];

        //Act
        //var list = await commandManager.CreateRequestedCommandsAsync(requestedCommands, CancellationToken.None);
        dbContext.RequestedCommands.AddRange(requestedCommands);
        await dbContext.SaveChangesAsync();

        var overlappingCommands = await commandManager.GetOverlappingRequestedCommandsAsync(requestCommandId, CancellationToken.None);

        //Assert
        overlappingCommands.Should().NotBeNull();
        overlappingCommands.Count.Should().Be(0);
    }

    [Fact]
    public async Task CommandManager_GetOverlappingRequestedCommands_With_Approved_Status_Returns_OverlappingCommands()
    {
        //Arrange
        var httpContextAccessor = NSubstitute.Substitute.For<IHttpContextAccessor>();
        var userInfoService = new UserInfoService(httpContextAccessor);
        var interceptor = new BaseEntitySaveChangesInterceptor(userInfoService);
        var dbContext = await MockedDbFactory.CreateAsync(_msSqlContainer.GetConnectionString());
        var logger = NSubstitute.Substitute.For<ILogger<CommandManager>>();
        var conflictDetector = new ConflictDetector();
        var conflictResolver = new ConflictResolver(conflictDetector);
        var mappedMock = new Mock<IMappedGatewayService>();
        var dbTransactionsMock = new Mock<IDbTransactions>();
        var commandManager = new CommandManager(logger,
                                                dbContext,
                                                dbTransactionsMock.Object,
                                                mappedMock.Object,
                                                conflictResolver);


        var requestCommandId = Guid.NewGuid();
        var future = DateTime.Now.AddDays(2);

        var requestedCommands = new List<RequestedCommand>(){
            new()
            {
                Id = requestCommandId,
                Status = RequestedCommandStatus.Approved,
                ConnectorId = "b5dd9d50-8fd5-45a4-a9bf-42fff3287bbb",
                CommandName = "atLeast",
                ExternalId = "13018",
                Value = 70,
                Unit = "F",
                 StartTime = new DateTimeOffset(future.Year,future.Month,future.Day,10,0,0,TimeSpan.FromHours(-2)),
                EndTime = new DateTimeOffset(future.Year,future.Month,future.Day,13,0,0,TimeSpan.FromHours(-2)),
                StatusUpdatedBy = new User { Name = "test" },
                CreatedDate = DateTimeOffset.UtcNow,
                LastUpdated = DateTimeOffset.UtcNow,
                IsCapabilityOf = "test",
                IsHostedBy = "test",
                Location = "test",
                RuleId = "test",
                SiteId = "test",
                TwinId = "test",
                Type = "atLeast",
                ReceivedDate = DateTimeOffset.UtcNow,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Status = RequestedCommandStatus.Approved,
                ConnectorId = "b5dd9d50-8fd5-45a4-a9bf-42fff3287bbb",
                CommandName = "atLeast",
                ExternalId = "13018",
                Value = 70,
                Unit = "F",
                 StartTime = new DateTimeOffset(future.Year,future.Month,future.Day,11,0,0,TimeSpan.FromHours(-2)),
                EndTime = new DateTimeOffset(future.Year,future.Month,future.Day,12,0,0,TimeSpan.FromHours(-2)),
                StatusUpdatedBy = new User { Name = "test" },
                CreatedDate = DateTimeOffset.UtcNow,
                LastUpdated = DateTimeOffset.UtcNow,
                IsCapabilityOf = "test",
                IsHostedBy = "test",
                Location = "test",
                RuleId = "test",
                SiteId = "test",
                TwinId = "test",
                Type = "atLeast",
                ReceivedDate = DateTimeOffset.UtcNow,
            }
        };

        //Act
        //   var list = await commandManager.CreateRequestedCommandsAsync(requestedCommands, CancellationToken.None);
        dbContext.RequestedCommands.AddRange(requestedCommands);
        await dbContext.SaveChangesAsync();

        var overlappingCommands = await commandManager.GetOverlappingRequestedCommandsAsync(requestCommandId, CancellationToken.None);

        //Assert
        overlappingCommands.Should().NotBeNullOrEmpty();
        overlappingCommands.Count.Should().Be(2);
    }
}
