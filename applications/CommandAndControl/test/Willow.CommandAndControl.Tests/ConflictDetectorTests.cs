using FluentAssertions;
using Willow.CommandAndControl.Application.Helpers;
using Willow.CommandAndControl.Application.Services;
using Willow.CommandAndControl.Tests.Helpers;
using Xunit;

namespace Willow.CommandAndControl.Tests;

public class ConflictDetectorTests
{
    [Fact]
    public void ConflictDetector_AreContainedCommands_With_Second_Command_Contained_In_First_Returns_True()
    {
        // Arrange
        var conflictDetector = new ConflictDetector();
        var future = DateTimeOffset.UtcNow.AddDays(2);
        var commandWithSource1 = CommandRandomizer.GetRequestedCommandWithSource("13018",
                                                                       70,
                                                                       nameof(SetPointCommandName.atMost),
                                                                       new DateTimeOffset(future.Year, future.Month, future.Day, 0, 0, 0, TimeSpan.Zero),
                                                                       new DateTimeOffset(future.Year, future.Month, future.Day, 12, 0, 0, TimeSpan.Zero));
        var commandWithSource2 = CommandRandomizer.GetRequestedCommandWithSource("13018",
                                                                       75,
                                                                       nameof(SetPointCommandName.atMost),
                                                                       new DateTimeOffset(future.Year, future.Month, future.Day, 2, 0, 0, TimeSpan.Zero),
                                                                       new DateTimeOffset(future.Year, future.Month, future.Day, 8, 0, 0, TimeSpan.Zero));


        // Act
        var result = conflictDetector.AreContained(commandWithSource1.Item2, commandWithSource2.Item2);

        // Assert
        result.Should().Be(true);

    }
    [Fact]
    public void ConflictDetector_AreContainedCommands_With_First_Command_Contained_In_Second_Returns_True()
    {
        // Arrange
        var conflictDetector = new ConflictDetector();
        var future = DateTimeOffset.UtcNow.AddDays(2);
        var commandWithSource1 = CommandRandomizer.GetRequestedCommandWithSource("13018",
                                                                       70,
                                                                       nameof(SetPointCommandName.atMost),
                                                                       new DateTimeOffset(future.Year, future.Month, future.Day, 0, 0, 0, TimeSpan.Zero),
                                                                       new DateTimeOffset(future.Year, future.Month, future.Day, 12, 0, 0, TimeSpan.Zero));
        var commandWithSource2 = CommandRandomizer.GetRequestedCommandWithSource("13018",
                                                                       75,
                                                                       nameof(SetPointCommandName.atMost),
                                                                       new DateTimeOffset(future.Year, future.Month, future.Day, 2, 0, 0, TimeSpan.Zero),
                                                                       new DateTimeOffset(future.Year, future.Month, future.Day, 8, 0, 0, TimeSpan.Zero));


        // Act
        var result = conflictDetector.AreContained(commandWithSource2.Item2, commandWithSource1.Item2);

        // Assert
        result.Should().Be(true);

    }

    [Fact]
    public void ConflictDetector_AreContainedCommands_With_Overlapping_Commands_From_End_Returns_False()
    {
        // Arrange
        var conflictDetector = new ConflictDetector();
        var future = DateTimeOffset.UtcNow.AddDays(2);
        var commandWithSource1 = CommandRandomizer.GetRequestedCommandWithSource("13018",
                                                                       70,
                                                                       nameof(SetPointCommandName.atMost),
                                                                       new DateTimeOffset(future.Year, future.Month, future.Day, 0, 0, 0, TimeSpan.Zero),
                                                                       new DateTimeOffset(future.Year, future.Month, future.Day, 12, 0, 0, TimeSpan.Zero));
        var commandWithSource2 = CommandRandomizer.GetRequestedCommandWithSource("13018",
                                                                       75,
                                                                       nameof(SetPointCommandName.atMost),
                                                                       new DateTimeOffset(future.Year, future.Month, future.Day, 2, 0, 0, TimeSpan.Zero),
                                                                       new DateTimeOffset(future.Year, future.Month, future.Day, 14, 0, 0, TimeSpan.Zero));


        // Act
        var result = conflictDetector.AreContained(commandWithSource1.Item2, commandWithSource2.Item2);

        // Assert
        result.Should().Be(false);

    }
    [Fact]
    public void ConflictDetector_AreContainedCommands_With_Overlapping_Commands_From_Start_Returns_False()
    {
        // Arrange
        var conflictDetector = new ConflictDetector();
        var future = DateTimeOffset.UtcNow.AddDays(2);
        var commandWithSource1 = CommandRandomizer.GetRequestedCommandWithSource("13018",
                                                                       70,
                                                                       nameof(SetPointCommandName.atMost),
                                                                       new DateTimeOffset(future.Year, future.Month, future.Day, 0, 0, 0, TimeSpan.Zero),
                                                                       new DateTimeOffset(future.Year, future.Month, future.Day, 12, 0, 0, TimeSpan.Zero));
        var commandWithSource2 = CommandRandomizer.GetRequestedCommandWithSource("13018",
                                                                       75,
                                                                       nameof(SetPointCommandName.atMost),
                                                                       new DateTimeOffset(future.Year, future.Month, future.Day, 2, 0, 0, TimeSpan.Zero),
                                                                       new DateTimeOffset(future.Year, future.Month, future.Day, 14, 0, 0, TimeSpan.Zero));


        // Act
        var result = conflictDetector.AreContained(commandWithSource2.Item2, commandWithSource1.Item2);

        // Assert
        result.Should().Be(false);

    }

    [Fact]
    public void ConflictDetector_AreContainedCommands_With_Non_Conflicting_Commands_Returns_False()
    {
        // Arrange
        var conflictDetector = new ConflictDetector();
        var future = DateTimeOffset.UtcNow.AddDays(2);
        var commandWithSource1 = CommandRandomizer.GetRequestedCommandWithSource("13018",
                                                                       70,
                                                                       nameof(SetPointCommandName.atMost),
                                                                       new DateTimeOffset(future.Year, future.Month, future.Day, 0, 0, 0, TimeSpan.Zero),
                                                                       new DateTimeOffset(future.Year, future.Month, future.Day, 12, 0, 0, TimeSpan.Zero));
        var commandWithSource2 = CommandRandomizer.GetRequestedCommandWithSource("13018",
                                                                       75,
                                                                       nameof(SetPointCommandName.atMost),
                                                                       new DateTimeOffset(future.Year, future.Month, future.Day, 12, 0, 1, TimeSpan.Zero),
                                                                       new DateTimeOffset(future.Year, future.Month, future.Day, 14, 0, 0, TimeSpan.Zero));


        // Act
        var result = conflictDetector.AreContained(commandWithSource1.Item2, commandWithSource2.Item2);

        // Assert
        result.Should().Be(false);

    }

}
