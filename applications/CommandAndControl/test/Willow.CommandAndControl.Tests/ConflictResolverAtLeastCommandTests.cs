using FluentAssertions;
using Willow.CommandAndControl.Application.Helpers;
using Willow.CommandAndControl.Application.Services;
using Willow.CommandAndControl.Data.Models;
using Willow.CommandAndControl.Tests.Helpers;
using Xunit;

namespace Willow.CommandAndControl.Tests;

public class ConflictResolverAtLeastCommandTests
{
    [Fact]
    public async Task ConflictResolver_Scenario1_NonConflicting_RequestedCommand_Returns_Single_ResolvedCommand()
    {
        //Arrange
        var conflictDetector = new ConflictDetector();
        var conflictResolver = new ConflictResolver(conflictDetector);
        var inNext2Days = DateTimeOffset.UtcNow.AddDays(2).StartOfDay();
        var overlappingCommands = new List<RequestedCommand>
        {
            CommandRandomizer.GetRequestedCommand("13018",
                                                  70,
                                                  nameof(SetPointCommandName.atLeast),
                                                  inNext2Days.AddHours(1),
                                                  inNext2Days.AddHours((1 + 1))), //01:00-02:00,
        };

        //Act
        List<ResolvedCommand> resolvedCommands = await conflictResolver.ResolveAsync(overlappingCommands);


        //Assert
        resolvedCommands.Count.Should().Be(1);
        //01:00-02:00
        resolvedCommands[0].StartTime.Should().Be(inNext2Days.AddHours(1));
        resolvedCommands[0].EndTime.Should().Be(inNext2Days.AddHours(1 + 1));
    }

    [Fact]
    public async Task ConflictResolutionManager_Scenario2a_Conflicting_RequestedCommand_On_Same_Interval__Different_Value_Returns_Single_MaxValue_ResolvedCommand()
    {
        //Arrange
        var conflictDetector = new ConflictDetector();
        var conflictResolver = new ConflictResolver(conflictDetector);
        var inNext2Days = DateTimeOffset.UtcNow.AddDays(2).StartOfDay();
        var overlappingCommands = new List<RequestedCommand>
        {
            CommandRandomizer.GetRequestedCommand("13018",
                                                  70,
                                                  nameof(SetPointCommandName.atLeast),
                                                  inNext2Days.AddHours(1),
                                                  inNext2Days.AddHours((1 + 1))), //01:00-02:00,
            CommandRandomizer.GetRequestedCommand("13018",
                                                  75,
                                                  nameof(SetPointCommandName.atLeast),
                                                  inNext2Days.AddHours(1),
                                                  inNext2Days.AddHours((1 +1)),
                                                  DateTimeOffset.UtcNow.AddMinutes(5))//01:00-02:00
        };


        //Act
        List<ResolvedCommand> resolvedCommands = await conflictResolver.ResolveAsync(overlappingCommands);


        //Assert
        resolvedCommands.Count.Should().Be(1);

        //01:00 - 02:00
        resolvedCommands[0].StartTime.Should().Be(inNext2Days.AddHours(1));
        resolvedCommands[0].EndTime.Should().Be(inNext2Days.AddHours(1 + 1));
        resolvedCommands[0].RequestedCommand.Value.Should().Be(75);
    }

    [Fact]
    public async Task ConflictResolutionManager_Scenario2b_Conflicting_RequestedCommand_On_Same_Interval__Different_Value_Returns_Single_MaxValue_ResolvedCommand()
    {
        //Arrange
        var conflictDetector = new ConflictDetector();
        var conflictResolver = new ConflictResolver(conflictDetector);
        var inNext2Days = DateTimeOffset.UtcNow.AddDays(2).StartOfDay();
        var now = DateTimeOffset.Now;
        var overlappingCommands = new List<RequestedCommand>
        {
            CommandRandomizer.GetRequestedCommand("13018",
                                                  75,
                                                  nameof(SetPointCommandName.atLeast),
                                                  inNext2Days.AddHours(1),
                                                  inNext2Days.AddHours((1 + 1)),
                                                  now), //01:00-02:00,
            CommandRandomizer.GetRequestedCommand("13018",
                                                  70,
                                                  nameof(SetPointCommandName.atLeast),
                                                  inNext2Days.AddHours(1),
                                                  inNext2Days.AddHours((1 +1)),
                                                  now.AddMinutes(5))//01:00-02:00
        };


        //Act
        List<ResolvedCommand> resolvedCommands = await conflictResolver.ResolveAsync(overlappingCommands);


        //Assert
        resolvedCommands.Count.Should().Be(1);

        //01:00 - 02:00
        resolvedCommands[0].StartTime.Should().Be(inNext2Days.AddHours(1));
        resolvedCommands[0].EndTime.Should().Be(inNext2Days.AddHours(1 + 1));
        resolvedCommands[0].RequestedCommand.Value.Should().Be(75);
        resolvedCommands[0].RequestedCommand.CreatedDate.Should().Be(now);
    }

    [Fact]
    public async Task ConflictResolver_Scenario3_Conflicting_RequestedCommand_On_Same_Interval_Same_Value_Returns_Single_First_ResolvedCommand()
    {
        //Arrange
        var conflictDetector = new ConflictDetector();
        var conflictResolver = new ConflictResolver(conflictDetector);
        var inNext2Days = DateTimeOffset.UtcNow.AddDays(2).StartOfDay();
        var now = DateTimeOffset.Now;
        var overlappingCommands = new List<RequestedCommand>
        {
            CommandRandomizer.GetRequestedCommand("13018",
                                                  70,
                                                  nameof(SetPointCommandName.atLeast),
                                                  inNext2Days.AddHours(1),
                                                  inNext2Days.AddHours((1 + 1)),
                                                  now), //01:00-02:00,
            CommandRandomizer.GetRequestedCommand("13018",
                                                  70,
                                                  nameof(SetPointCommandName.atLeast),
                                                  inNext2Days.AddHours(1),
                                                  inNext2Days.AddHours((1 +1)),
                                                  now.AddMinutes(5))//01:00-02:00
        };


        //Act
        List<ResolvedCommand> resolvedCommands = await conflictResolver.ResolveAsync(overlappingCommands);


        //Assert
        resolvedCommands.Count.Should().Be(1);

        //01:00 - 02:00
        resolvedCommands[0].StartTime.Should().Be(inNext2Days.AddHours(1));
        resolvedCommands[0].EndTime.Should().Be(inNext2Days.AddHours(1 + 1));
        resolvedCommands[0].RequestedCommand.Value.Should().Be(70);
        resolvedCommands[0].RequestedCommand.CreatedDate.Should().Be(now);
    }


    [Fact]
    public async Task ConflictResolver_Scenario4_Conflicting_RequestedCommand_On_Overlapping_With_Different_Value_Returns_Split_ResolvedCommands()
    {
        //Arrange
        var conflictDetector = new ConflictDetector();
        var conflictResolver = new ConflictResolver(conflictDetector);
        var inNext2Days = DateTimeOffset.UtcNow.AddDays(2).StartOfDay();
        var overlappingCommands = new List<RequestedCommand>
        {
            CommandRandomizer.GetRequestedCommand("13018",
                                                  70,
                                                  nameof(SetPointCommandName.atLeast),
                                                  inNext2Days.AddHours(1),
                                                  inNext2Days.AddHours((1 + 4))), //01:00-05:00,
            CommandRandomizer.GetRequestedCommand("13018",
                                                  75,
                                                  nameof(SetPointCommandName.atLeast),
                                                  inNext2Days.AddHours(1 + 3),
                                                  inNext2Days.AddHours((1 + 6)),
                                                  DateTime.UtcNow.AddMinutes(5))//04:00-07:00
        };



        //Act
        List<ResolvedCommand> resolvedCommands = await conflictResolver.ResolveAsync(overlappingCommands);


        //Assert
        resolvedCommands.Count.Should().Be(2);

        //01:00-04:00
        resolvedCommands[0].StartTime.Should().Be(inNext2Days.AddHours(1));
        resolvedCommands[0].EndTime.Should().Be(inNext2Days.AddHours(1 + 3));
        resolvedCommands[0].RequestedCommand.Value.Should().Be(70);

        //04:07:00
        resolvedCommands[1].StartTime.Should().Be(inNext2Days.AddHours(1 + 3));
        resolvedCommands[1].EndTime.Should().Be(inNext2Days.AddHours(1 + 6));
        resolvedCommands[1].RequestedCommand.Value.Should().Be(75);
    }

    [Fact]
    public async Task ConflictResolver_Scenario5_Conflicting_RequestedCommand_On_Overlapping_With_Same_Value_Returns_Single_Extended_ResolvedCommand()
    {
        //Arrange
        var conflictDetector = new ConflictDetector();
        var conflictResolver = new ConflictResolver(conflictDetector);
        var inNext2Days = DateTimeOffset.UtcNow.AddDays(2).StartOfDay();
        var overlappingCommands = new List<RequestedCommand>
        {
            CommandRandomizer.GetRequestedCommand("13018",
                                                  70,
                                                  nameof(SetPointCommandName.atLeast),
                                                  inNext2Days.AddHours(1),
                                                  inNext2Days.AddHours((1 + 4))), //01:00-05:00,
            CommandRandomizer.GetRequestedCommand("13018",
                                                  70,
                                                  nameof(SetPointCommandName.atLeast),
                                                  inNext2Days.AddHours(1 + 3),
                                                  inNext2Days.AddHours((1 + 6)),
                                                  DateTimeOffset.UtcNow.AddMinutes(5))//04:00-07:00
        };



        //Act
        List<ResolvedCommand> resolvedCommands = await conflictResolver.ResolveAsync(overlappingCommands);


        //Assert
        resolvedCommands.Count.Should().Be(1);

        //01:00-07:00
        resolvedCommands[0].StartTime.Should().Be(inNext2Days.AddHours(1));
        resolvedCommands[0].EndTime.Should().Be(inNext2Days.AddHours(1 + 6));
        resolvedCommands[0].RequestedCommand.Value.Should().Be(70);
    }

    [Fact]
    public async Task ConflictResolver_Scenario6_Conflicting_RequestedCommand_On_Contained_With_Same_Value_Returns_Single_Larger_ResolvedCommand()
    {
        //Arrange
        var conflictDetector = new ConflictDetector();
        var conflictResolver = new ConflictResolver(conflictDetector);
        var inNext2Days = DateTimeOffset.UtcNow.AddDays(2).StartOfDay();
        var overlappingCommands = new List<RequestedCommand>
        {
            CommandRandomizer.GetRequestedCommand("13018",
                                                  70,
                                                  nameof(SetPointCommandName.atLeast),
                                                  inNext2Days.AddHours(1),
                                                  inNext2Days.AddHours((1 + 10))), //01:00-12:00,
            CommandRandomizer.GetRequestedCommand("13018",
                                                  70,
                                                  nameof(SetPointCommandName.atLeast),
                                                  inNext2Days.AddHours(1 + 3),
                                                  inNext2Days.AddHours((1 + 6)),
                                                  DateTimeOffset.UtcNow.AddMinutes(5))//04:00-07:00
        };



        //Act
        List<ResolvedCommand> resolvedCommands = await conflictResolver.ResolveAsync(overlappingCommands);


        //Assert
        resolvedCommands.Count.Should().Be(1);

        //01:00-12:00
        resolvedCommands[0].StartTime.Should().Be(inNext2Days.AddHours(1));
        resolvedCommands[0].EndTime.Should().Be(inNext2Days.AddHours(1 + 10));
        resolvedCommands[0].RequestedCommand.Value.Should().Be(70);
    }


    [Fact]
    public async Task ConflictResolver_Scenario7_Conflicting_RequestedCommand_On_Contained_With_Different_Value_Returns_3_Split_ResolvedCommands()
    {
        //Arrange
        var conflictDetector = new ConflictDetector();
        var conflictResolver = new ConflictResolver(conflictDetector);
        var inNext2Days = DateTimeOffset.UtcNow.AddDays(2).StartOfDay();
        var overlappingCommands = new List<RequestedCommand>
        {
            CommandRandomizer.GetRequestedCommand("13018",
                                                  70,
                                                  nameof(SetPointCommandName.atLeast),
                                                  inNext2Days.AddHours(1),
                                                  inNext2Days.AddHours((1 + 10))), //01:00-12:00,
            CommandRandomizer.GetRequestedCommand("13018",
                                                  75,
                                                  nameof(SetPointCommandName.atLeast),
                                                  inNext2Days.AddHours(1 + 3),
                                                  inNext2Days.AddHours((1 + 6)),
                                                  DateTimeOffset.UtcNow.AddMinutes(5))//04:00-07:00
        };



        //Act
        List<ResolvedCommand> resolvedCommands = await conflictResolver.ResolveAsync(overlappingCommands);


        //Assert
        resolvedCommands.Count.Should().Be(3);

        //01:00-04:00
        resolvedCommands[0].StartTime.Should().Be(inNext2Days.AddHours(1));
        resolvedCommands[0].EndTime.Should().Be(inNext2Days.AddHours(1 + 3));
        resolvedCommands[0].RequestedCommand.Value.Should().Be(70);


        //04:00-07:000
        resolvedCommands[1].StartTime.Should().Be(inNext2Days.AddHours(1 + 3));
        resolvedCommands[1].EndTime.Should().Be(inNext2Days.AddHours(1 + 6));
        resolvedCommands[1].RequestedCommand.Value.Should().Be(75);

        //07:00-12:00
        resolvedCommands[2].StartTime.Should().Be(inNext2Days.AddHours(1 + 6));
        resolvedCommands[2].EndTime.Should().Be(inNext2Days.AddHours(1 + 10));
        resolvedCommands[2].RequestedCommand.Value.Should().Be(70);
    }

}
