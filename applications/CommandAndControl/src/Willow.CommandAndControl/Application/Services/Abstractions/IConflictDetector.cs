namespace Willow.CommandAndControl.Application.Services.Abstractions;

/// <summary>
/// Provides conditions for identifying conflicts in commands.
/// </summary>
public interface IConflictDetector
{
    /// <summary>
    /// Returns True when one command is fully contained in another command.
    /// </summary>
    /// <param name="inner">The first command.</param>
    /// <param name="outer">The second command.</param>
    /// <returns>true if one command is inside the other, otherwise false.</returns>
    bool AreContained(RequestedCommand inner, RequestedCommand outer);

    /// <summary>
    /// Returns True when both commands have Start/End time .
    /// </summary>
    /// <param name="inner">The first command.</param>
    /// <param name="outer">The second command.</param>
    /// <returns>true if the commands have the same interval, otherwise false.</returns>
    bool HaveSameInterval(RequestedCommand inner, RequestedCommand outer);

    /// <summary>
    /// Returns True when both commands have same Start/End time and same Value.
    /// </summary>
    /// <param name="inner">The first command.</param>
    /// <param name="outer">The second command.</param>
    /// <returns>true if the commands have the same interval and value, otherwise false.</returns>
    bool HaveSameIntervalAndValue(RequestedCommand inner, RequestedCommand outer);

    /// <summary>
    /// Returns True when new command is overlapping existing command.
    /// </summary>
    /// <param name="existing">The existing command.</param>
    /// <param name="newCommand">The new command.</param>
    /// <returns>true if the commands overlap, otherwise false.</returns>
    bool IsOverlapping(RequestedCommand existing, RequestedCommand newCommand);

    /// <summary>
    ///  Returns True when new command has overlapping interval with existing one, and values less than or greater than existing command depending on the type of command.
    /// </summary>
    /// <param name="existing">The existing command.</param>
    /// <param name="newCommand">The new command.</param>
    /// <returns>true if the commands conflict, otherwise false.</returns>
    bool AreConflicting(RequestedCommand existing, RequestedCommand newCommand);
}
