namespace Willow.CommandAndControl.Application.Services;

using Willow.CommandAndControl.Application.Helpers;

internal class ConflictDetector : IConflictDetector
{
    public bool AreContained(RequestedCommand inner, RequestedCommand outer)
    {
        return (inner.StartTime > outer.StartTime && inner.EndTime < outer.EndTime)
                || (outer.StartTime > inner.StartTime && outer.EndTime < inner.EndTime);
    }

    public bool HaveSameInterval(RequestedCommand inner, RequestedCommand outer)
    {
        return inner.StartTime == outer.StartTime && inner.EndTime == outer.EndTime;
    }

    public bool HaveSameIntervalAndValue(RequestedCommand inner, RequestedCommand outer)
    {
        return HaveSameInterval(inner, outer) && HaveSameValue(inner, outer);
    }

    public bool HaveSameValue(RequestedCommand inner, RequestedCommand outer)
    {
        return inner.Value == outer.Value;
    }

    public bool IsAtleastCommand(RequestedCommand existing, RequestedCommand newCommand)
    {
        return newCommand.Value <= existing.Value
            && newCommand.CommandName == existing.CommandName
            && newCommand.CommandName.Equals(nameof(SetPointCommandName.atLeast));
    }

    public bool IsAtMostCommand(RequestedCommand existing, RequestedCommand newCommand)
    {
        return newCommand.Value >= existing.Value
            && newCommand.CommandName == existing.CommandName
            && newCommand.CommandName.Equals(nameof(SetPointCommandName.atMost));
    }

    public bool IsOverlapping(RequestedCommand existing, RequestedCommand newCommand)
    {
        return existing.StartTime < newCommand.EndTime
            && existing.EndTime > newCommand.StartTime;
    }

    public bool AreConflicting(RequestedCommand existing, RequestedCommand newCommand)
    {
        return existing.StartTime == newCommand.StartTime && existing.EndTime == newCommand.EndTime &&
               ((existing.Value < newCommand.Value && existing.CreatedDate < newCommand.CreatedDate && existing.CommandName == newCommand.CommandName && existing.CommandName.Equals(nameof(SetPointCommandName.atLeast)))
               ||
               (existing.Value > newCommand.Value && existing.CreatedDate < newCommand.CreatedDate && existing.CommandName == newCommand.CommandName && existing.CommandName.Equals(nameof(SetPointCommandName.atMost))));
    }
}
