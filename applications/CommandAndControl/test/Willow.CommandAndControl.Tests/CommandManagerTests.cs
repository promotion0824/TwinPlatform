using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Willow.CommandAndControl.Application.Services;
using Willow.CommandAndControl.Application.Services.Abstractions;
using Willow.CommandAndControl.Data.Enums;
using Willow.CommandAndControl.Data.Models;
using Willow.CommandAndControl.Data;
using Xunit;
using Testcontainers.MsSql;
using Willow.CommandAndControl.Tests.Mock;
using Moq;

namespace Willow.CommandAndControl.Tests;

public class CommandManagerTests : IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder().Build();
    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
    }
    /* [Fact]
     public async Task CommandManager_CreateRequestedCommand_ReturnsSuccess()
     {
         //Arrange
         var httpContextAccessor = NSubstitute.Substitute.For<IHttpContextAccessor>();
         var userInfoService = new UserInfoService(httpContextAccessor);
         var interceptor = new BaseEntitySaveChangesInterceptor(userInfoService);
         var dbContext = new ApplicationDbContext(_dbContextOptions, interceptor);
         var logger = NSubstitute.Substitute.For<ILogger<CommandManager>>();
         var conflictDetector = new ConflictDetector();
         var conflictResolver = new ConflictResolver(conflictDetector);
         var commandManager = new CommandManager(logger,
                                                 dbContext,
                                                 null,
                                                 conflictResolver);


         var requestCommandId = Guid.NewGuid();

         var requestedCommands = new List<RequestedCommand>(){
             new RequestedCommand
             {
                 Id = requestCommandId,
                 Status = RequestedCommandStatus.Pending,
                 //CommandSourceId = commandSource.Id,
                 ConnectorId = "b5dd9d50-8fd5-45a4-a9bf-42fff3287bbb",
                 CommandName = "atLeast",
                 ExternalId = "13018",
                 Value = 70,
                 Unit = "F",
                 StartTime = DateTimeOffset.UtcNow.AddMinutes(10),
                 StatusUpdatedBy = new User { Name = "test" },
                 CreatedDate = DateTimeOffset.UtcNow,
                 LastUpdated = DateTimeOffset.UtcNow
             }
         };

         //Act
         var result = await commandManager.CreateRequestedCommandsAsync(requestedCommands, CancellationToken.None);

         //Assert
         Assert.NotNull(result);
         Assert.Equal(1, result?.Count());
         Assert.Equal(requestCommandId.ToString(), result?.FirstOrDefault());


     }

     [Fact]
     public async Task CommandManager_CreateRequestedCommand_With_Empty_RequestedCommand_ReturnsException()
     {
         //Arrange
         var httpContextAccessor = NSubstitute.Substitute.For<IHttpContextAccessor>();
         var userInfoService = new UserInfoService(httpContextAccessor);
         var interceptor = new BaseEntitySaveChangesInterceptor(userInfoService);
         var dbContext = new ApplicationDbContext(_dbContextOptions, interceptor);
         var conflictDetector = new ConflictDetector();
         var conflictResolver = new ConflictResolver(conflictDetector);
         var logger = NSubstitute.Substitute.For<ILogger<CommandManager>>();
         var commandManager = new CommandManager(logger,
                                                 dbContext,
                                                 null,
                                                 conflictResolver);

         var requestedCommands = new List<RequestedCommand>();

         //Act
         await Assert.ThrowsAsync<ArgumentException>(() => commandManager.CreateRequestedCommandsAsync(requestedCommands, CancellationToken.None));

     }

     [Fact]
     public async Task CommandManager_CreateRequestedCommand_With_TimeZone_ReturnsSuccess()
     {
         //Arrange
         var httpContextAccessor = NSubstitute.Substitute.For<IHttpContextAccessor>();
         var userInfoService = new UserInfoService(httpContextAccessor);
         var interceptor = new BaseEntitySaveChangesInterceptor(userInfoService);
         var dbContext = new ApplicationDbContext(_dbContextOptions, interceptor);
         var logger = NSubstitute.Substitute.For<ILogger<CommandManager>>();
         var conflictDetector = new ConflictDetector();
         var conflictResolver = new ConflictResolver(conflictDetector);
         var commandManager = new CommandManager(logger,
                                                 dbContext,
                                                 null,
                                                 conflictResolver);


         var requestCommandId = Guid.NewGuid();

         List<RequestedCommand> requestedCommands = [
             new() {
                 Id = requestCommandId,
                 Status = RequestedCommandStatus.Pending,
                 ConnectorId = "b5dd9d50-8fd5-45a4-a9bf-42fff3287bbb",
                 CommandName = "atLeast",
                 ExternalId = "13018",
                 Value = 70,
                 Unit = "F",
                 StartTime = DateTimeOffset.Parse("2023-09-04 10:46:09 AM -02:00", CultureInfo.InvariantCulture),
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
         await commandManager.CreateRequestedCommandsAsync(requestedCommands, CancellationToken.None);

         var commandResult = await commandManager.GetRequestedCommandByIdAsync(requestCommandId.ToString(), CancellationToken.None);


         //Assert
         Assert.NotNull(commandResult);
         Assert.Equal(12, commandResult.StartTime.UtcDateTime.Hour);
     }*/

    [Fact]
    public async Task CommandManager_GetOverlappingRequestedCommands_ReturnsSuccess()
    {
        //Arrange
        var httpContextAccessor = NSubstitute.Substitute.For<IHttpContextAccessor>();
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
                StartTime = DateTimeOffset.Parse("2023-09-04 10:00:00 AM -02:00", CultureInfo.InvariantCulture),
                EndTime = DateTimeOffset.Parse("2023-09-04 01:00:00 PM -02:00", CultureInfo.InvariantCulture),
                StatusUpdatedBy = new User { Name = "test", Email = "test@example.com" },
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
                StartTime = DateTimeOffset.Parse("2023-09-04 11:00:00 AM -02:00", CultureInfo.InvariantCulture),
                EndTime = DateTimeOffset.Parse("2023-09-04 12:00:00 PM -02:00", CultureInfo.InvariantCulture),
                StatusUpdatedBy = new User { Name = "test", Email = "test@example.com" },
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
        dbContext.RequestedCommands.AddRange(requestedCommands);
        await dbContext.SaveChangesAsync();
        //await commandManager.CreateRequestedCommandsAsync(requestedCommands, CancellationToken.None);

        var overlappingCommands = await commandManager.GetOverlappingRequestedCommandsAsync(requestCommandId, CancellationToken.None);

        //Assert
        Assert.NotNull(overlappingCommands);
        Assert.Equal(2, overlappingCommands.Count);
    }
}
