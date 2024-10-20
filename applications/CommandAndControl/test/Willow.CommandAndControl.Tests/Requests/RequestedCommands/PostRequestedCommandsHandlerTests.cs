using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Testcontainers.MsSql;
using Willow.CommandAndControl.Application.Models;
using Willow.CommandAndControl.Application.Requests.RequestedCommand.PostRequestedCommands;
using Willow.CommandAndControl.Application.Services.Abstractions;
using Willow.CommandAndControl.Data;
using Willow.CommandAndControl.Data.Enums;
using Willow.CommandAndControl.Tests.Mock;
using Xunit;

namespace Willow.CommandAndControl.Tests.Requests.RequestedCommands;

public class PostRequestedCommandsHandlerTests : IAsyncLifetime
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

    [Fact]
    public async Task PostRequestedCommandsHandler_WithSomeValidDto_SavesCommands()
    {
        // Arrange
        var dbContext = await MockedDbFactory.CreateAsync(_msSqlContainer.GetConnectionString());
        var dbTransactions = new DbTransactions();
        var logger = Substitute.For<ILogger<PostRequestedCommandsHandler>>();
        var activityLogger = Substitute.For<IActivityLogger>();
        var twinInfoService = Substitute.For<ITwinInfoService>();
        twinInfoService.GetTwinInfoAsync(Arg.Any<IReadOnlyCollection<string>>()).Returns(new Dictionary<string, TwinInfoModel>
        {
            {
                "Wil-Twin-1", new TwinInfoModel
                {
                    TwinId = "Wil-Twin-1",
                    ConnectorId = "Connector1",
                    ExternalId = "ExternalId1"
                }
            },
            {
                "Wil-Twin-2", new TwinInfoModel
                {
                    TwinId = "Wil-Twin-2",
                    ConnectorId = "Connector2",
                    ExternalId = "ExternalId2"
                }
            },
            {
                "Wil-Twin-3", new TwinInfoModel
                {
                    TwinId = "Wil-Twin-3",
                    ConnectorId = "Connector3",
                    ExternalId = "ExternalId3"
                }
            }
        });
        var request = new PostRequestedCommandsDto(new List<RequestedCommandDto>
        {
            new("Connector1", "Command1", "Type1", "Wil-Twin-1", "ExternalId1", Guid.NewGuid().ToString(), 1.0, "Unit1",
                DateTimeOffset.Now, DateTimeOffset.Now.AddDays(1), []),
            new("Connector1", "Command1", "Type1", string.Empty, "ExternalId1", Guid.NewGuid().ToString(), 1.0, "Unit1",
                DateTimeOffset.Now, DateTimeOffset.Now.AddDays(1), []),
            new("invalid-connector-id", "Command1", "Type1", "Wil-Twin-1", "ExternalId1", Guid.NewGuid().ToString(), 2.0, "Unit1",
                DateTimeOffset.Now, DateTimeOffset.Now.AddDays(1), []),
            new("Connector1", "Command1", "Type1", "invalid-twin-id", "ExternalId1", Guid.NewGuid().ToString(), 3.0, "Unit1",
                DateTimeOffset.Now, DateTimeOffset.Now.AddDays(1), []),
            new("Connector1", "Command1", "Type1", "Wil-Twin-1", "invalid-external-id", Guid.NewGuid().ToString(), 4.0, "Unit1",
                DateTimeOffset.Now, DateTimeOffset.Now.AddDays(1), []),
        });
        var previousCount = dbContext.RequestedCommands.Count();

        // Act
        var result = await PostRequestedCommandsHandler.HandleAsync(request, dbContext, dbTransactions, logger, activityLogger, twinInfoService);

        Assert.IsType<ProblemHttpResult>(result.Result);
        var jsonResult = (ProblemHttpResult)result.Result;
        var errors = (PostRequestedCommandsResponseDto)jsonResult.ProblemDetails.Extensions["errors"]!;
        errors.InvalidCommands.Count.Should().Be(4);
        dbContext.RequestedCommands.Count().Should().Be(previousCount);
    }


    [Fact]
    public async Task PostRequestedCommandsHandler_WithValidDto_SavesCommands()
    {
        // Arrange
        var dbContext = await MockedDbFactory.CreateAsync(_msSqlContainer.GetConnectionString());
        var dbTransactions = new DbTransactions();
        var logger = Substitute.For<ILogger<PostRequestedCommandsHandler>>();
        var activityLogger = Substitute.For<IActivityLogger>();
        var twinInfoService = Substitute.For<ITwinInfoService>();
        twinInfoService.GetTwinInfoAsync(Arg.Any<IReadOnlyCollection<string>>()).Returns(new Dictionary<string, TwinInfoModel>
        {
            {
                "Wil-Twin-1", new TwinInfoModel
                {
                    TwinId = "Wil-Twin-1",
                    ConnectorId = "Connector1",
                    ExternalId = "ExternalId1"
                }
            }
        });
        var request = new PostRequestedCommandsDto(new List<RequestedCommandDto>
        {
            new("Connector1", "Command1", "Type1", "Wil-Twin-1", "ExternalId1", Guid.NewGuid().ToString(), 1.0, "Unit1",
                DateTimeOffset.Now, DateTimeOffset.Now.AddDays(1), [])
        });
        var previousCount = dbContext.RequestedCommands.Count();

        // Act
        var result = await PostRequestedCommandsHandler.HandleAsync(request, dbContext, dbTransactions, logger, activityLogger, twinInfoService);

        Assert.IsType<NoContent>(result.Result);
        dbContext.RequestedCommands.Count().Should().Be(previousCount + 1);
    }

    [Fact]
    public async Task PostRequestedCommandsHandler_WithEmptyDto_SavesNoCommands()
    {
        // Arrange
        var dbContext = await MockedDbFactory.CreateAsync(_msSqlContainer.GetConnectionString());
        var dbTransactions = new DbTransactions();
        var logger = Substitute.For<ILogger<PostRequestedCommandsHandler>>();
        var activityLogger = Substitute.For<IActivityLogger>();
        var twinInfoService = Substitute.For<ITwinInfoService>();
        var request = new PostRequestedCommandsDto(new List<RequestedCommandDto>());

        // Act
        var result = await PostRequestedCommandsHandler.HandleAsync(request, dbContext, dbTransactions, logger, activityLogger, twinInfoService);

        // Assert
        Assert.IsType<NoContent>(result.Result);
    }

    [Fact]
    public async Task PostRequestedCommandsHandler_WithDuplicateCommands_SavesUniqueCommandsOnly()
    {
        // Arrange
        var dbContext = await MockedDbFactory.CreateAsync(_msSqlContainer.GetConnectionString());
        var dataTimeNow = DateTimeOffset.Now;
        var dbTransactions = new DbTransactions();
        var logger = Substitute.For<ILogger<PostRequestedCommandsHandler>>();
        var activityLogger = Substitute.For<IActivityLogger>();
        var twinInfoService = Substitute.For<ITwinInfoService>();
        twinInfoService.GetTwinInfoAsync(Arg.Any<IReadOnlyCollection<string>>()).Returns(new Dictionary<string, TwinInfoModel>
        {
            {
                "Wil-Twin-1", new TwinInfoModel
                {
                    TwinId = "Wil-Twin-1",
                    ConnectorId = "Connector1",
                    ExternalId = "ExternalId1"
                }
            },
        });
        var request = new PostRequestedCommandsDto(new List<RequestedCommandDto>
        {
            new("Connector1", "Command1", "Type1", "Wil-Twin-1", "ExternalId1", "RuleId1", 1.0, "Unit1", dataTimeNow,
                null, []),
            new("Connector1", "Command1", "Type1", "Wil-Twin-1", "ExternalId1", "RuleId1", 1.0, "Unit1", dataTimeNow,
                null, []),
            new("Connector1", "Command1", "Type1", "Wil-Twin-1", "ExternalId1", "RuleId1", 1.0, "Unit1", dataTimeNow,
                null, [])
        });

        // Act
        var result = await PostRequestedCommandsHandler.HandleAsync(request, dbContext, dbTransactions, logger, activityLogger, twinInfoService);

        // Assert
        Assert.IsType<NoContent>(result.Result);
        dbContext.RequestedCommands.Where(x => x.TwinId == "Wil-Twin-1" && x.RuleId == "RuleId1").Count().Should().Be(1);
    }

    [Fact]
    public async Task PostRequestedCommandsHandler_WithPreApprovedOrRejectedCommands_DoesNotchangeStatus()
    {
        // Arrange
        var dbContext = await MockedDbFactory.CreateAsync(_msSqlContainer.GetConnectionString());
        var dbTransactions = new DbTransactions();
        var logger = Substitute.For<ILogger<PostRequestedCommandsHandler>>();
        var activityLogger = Substitute.For<IActivityLogger>();
        var twinInfoService = Substitute.For<ITwinInfoService>();
        twinInfoService.GetTwinInfoAsync(Arg.Any<IReadOnlyCollection<string>>()).Returns(new Dictionary<string, TwinInfoModel>
        {
            {
                "Wil-Twin-1", new TwinInfoModel
                {
                    TwinId = "Wil-Twin-1",
                    ConnectorId = "Connector1",
                    ExternalId = "ExternalId1"
                }
            }
        });
        var request = new PostRequestedCommandsDto(new List<RequestedCommandDto>
        {
            new("Connector1", "Command1", "Type1", "Wil-Twin-1", "ExternalId1", "RuleId1", 1.0, "Unit1",
                DateTimeOffset.Now, DateTimeOffset.Now.AddDays(1), []),
            new("Connector1", "Command1", "Type1", "Wil-Twin-1", "ExternalId1", "RuleId2", 2.0, "Unit1",
                DateTimeOffset.Now, DateTimeOffset.Now.AddDays(2), []),
            new("Connector1", "Command1", "Type1", "Wil-Twin-1", "ExternalId1", "RuleId3", 3.0, "Unit1",
                DateTimeOffset.Now, DateTimeOffset.Now.AddDays(3), [])
        });

        _ = await PostRequestedCommandsHandler.HandleAsync(request, dbContext, dbTransactions, logger, activityLogger, twinInfoService);

        var command2 = dbContext.RequestedCommands.First(x => x.TwinId == "Wil-Twin-1" && x.RuleId == "RuleId2");
        var command3 = dbContext.RequestedCommands.First(x => x.TwinId == "Wil-Twin-1" && x.RuleId == "RuleId3");

        command2.Status = RequestedCommandStatus.Approved;
        command3.Status = RequestedCommandStatus.Rejected;

        await dbContext.SaveChangesAsync();

        // Act
        var result = await PostRequestedCommandsHandler.HandleAsync(request, dbContext, dbTransactions, logger, activityLogger, twinInfoService);

        // Assert
        Assert.IsType<NoContent>(result.Result);
        dbContext.RequestedCommands.First(x => x.TwinId == "Wil-Twin-1" && x.RuleId == "RuleId1").Status.Should()
            .Be(RequestedCommandStatus.Pending);
        dbContext.RequestedCommands.First(x => x.TwinId == "Wil-Twin-1" && x.RuleId == "RuleId2").Status.Should()
            .Be(RequestedCommandStatus.Approved);
        dbContext.RequestedCommands.First(x => x.TwinId == "Wil-Twin-1" && x.RuleId == "RuleId3").Status.Should()
            .Be(RequestedCommandStatus.Rejected);
    }
}
