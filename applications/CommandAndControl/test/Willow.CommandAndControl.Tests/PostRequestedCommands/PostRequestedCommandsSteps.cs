namespace Willow.CommandAndControl.Tests;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Willow.CommandAndControl.Application.Models;
using Willow.CommandAndControl.Application.Requests.RequestedCommand.PostRequestedCommands;
using Willow.CommandAndControl.Application.Services.Abstractions;
using Willow.CommandAndControl.Data;
using Willow.CommandAndControl.Data.Enums;
using Willow.CommandAndControl.Data.Models;
using Willow.SpecFlow;
using Xunit;

[Binding]
public class PostRequestedCommandsSteps
{
    private readonly List<RequestedCommand> requestedCommands = [];
    private readonly List<RequestedCommand> newCommands = [];

    [Given(@"I have a command with")]
    public void GivenIHaveACommandWith(Table table)
    {
        var set = table.CreateSet<RequestedCommand>(() =>
        new RequestedCommand
        {
            CommandName = "Test Command",
            ConnectorId = "Connector",
            ExternalId = "EXT",
            IsCapabilityOf = "",
            IsHostedBy = "",
            Location = "",
            RuleId = "",
            SiteId = Guid.Empty.ToString(),
            StartTime = DateTimeOffset.MinValue,
            TwinId = "",
            Type = "",
            Unit = "degF",
            Value = 0,
            CreatedDate = DateTime.UtcNow,
            EndTime = null,
            IsDeleted = false,
            Id = Guid.NewGuid(),
            LastUpdated = DateTime.UtcNow,
            Locations = [],
            ReceivedDate = DateTime.UtcNow,
            Status = RequestedCommandStatus.Pending,
            StatusUpdatedBy = new()
            {
                Email = "test@willowinc.com",
                Name = "Test User"
            }
        });

        requestedCommands.AddRange(set);
    }

    [When(@"I receive a command with")]
    public async Task WhenIReceiveACommandWith(Table table)
    {
        Mock<IApplicationDbContext> dbContextMock = new();
        Mock<IDbContextTransaction> transactionMock = new();

        Mock<IDbTransactions> transactionsMock = new();
        transactionsMock.Setup(t => t.RunAsync(It.IsAny<IApplicationDbContext>(), It.IsAny<Func<IDbContextTransaction, Task<bool>>>())).Callback
            ((IApplicationDbContext context, Func<IDbContextTransaction, Task<bool>> act) =>
            {
                act(transactionMock.Object);
            });

        Mock<DbSet<RequestedCommand>> requestedCommandsMock = new();
        requestedCommandsMock.Setup(r => r.Add(It.IsAny<RequestedCommand>())).Callback<RequestedCommand>(newCommands.Add);
        dbContextMock.Setup(c => c.RequestedCommands).ReturnsDbSet(requestedCommands, requestedCommandsMock);
        dbContextMock.Setup(c => c.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(transactionMock.Object);

        var commands = table.CreateSet(() => new RequestedCommandDto
        (
            CommandName: "Test Command",
            ConnectorId: "Connector",
            ExternalId: "EXT",
            RuleId: "",
            Unit: "degF",
            TwinId: "",
            StartTime: DateTimeOffset.MinValue,
            EndTime: null,
            Value: 0,
            Type: "",
            Relationships: []
        )).ToList();

        PostRequestedCommandsDto request = new(commands);

        ILogger<PostRequestedCommandsHandler> logger = LoggerFactory.Create((config) => { }).CreateLogger<PostRequestedCommandsHandler>();

        Mock<IActivityLogger> activityLoggerMock = new();
        activityLoggerMock.Setup(a => a.LogAsync(It.IsAny<ActivityType>(), It.IsAny<RequestedCommand>(), It.IsAny<CancellationToken>()));
        Mock<ITwinInfoService> twinInfoServiceMock = new();
        twinInfoServiceMock.Setup(t => t.GetTwinInfoAsync(It.IsAny<IReadOnlyCollection<string>>())).ReturnsAsync((IEnumerable<string> inputs) =>
        {
            var validInputs = inputs.Where(i => !i.StartsWith("invalid"));
            return validInputs.Select(i => new KeyValuePair<string, TwinInfoModel>(i, new TwinInfoModel
            {
                ConnectorId = "Connector",
                ExternalId = "EXT",
                IsHostedBy = "",
                IsCapabilityOf = "",
                SiteId = Guid.Empty.ToString(),
                PresentValue = 0.1,
                Location = "",
                TwinId = i,
                Unit = "degF",
                Locations = [],
            })).ToDictionary();
        });

        await PostRequestedCommandsHandler.HandleAsync(request, dbContextMock.Object, transactionsMock.Object, logger, activityLoggerMock.Object, twinInfoServiceMock.Object);
    }

    [Then(@"Nothing Happens")]
    public void ThenNothingHappens()
    {
        Assert.Empty(newCommands);
    }

    [Then(@"A new command is created with")]
    public void ThenANewCommandIsCreatedWith(Table table)
    {
        var inspectors = table.Rows.Select<TableRow, Action<RequestedCommand>>(r =>
        {
            return (RequestedCommand command) =>
            {
                Assert.Equal(r["TwinId"], command.TwinId);
                Assert.Equal(r["RuleId"], command.RuleId);
                Assert.Equal(double.Parse(r["Value"]), command.Value);
                Assert.Equal(DateTimeExpression.Parse(r["StartTime"]) ?? DateTime.MinValue, command.StartTime);
                Assert.Equal(DateTimeExpression.Parse(r["EndTime"]), command.EndTime);
            };
        }).ToArray();

        Assert.Collection(newCommands, inspectors);
    }
}
