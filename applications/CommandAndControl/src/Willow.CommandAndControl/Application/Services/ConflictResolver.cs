namespace Willow.CommandAndControl.Application.Services;

using Willow.CommandAndControl.Application.Helpers;

internal class ConflictResolver : IConflictResolver
{
    private readonly IConflictDetector conflictDetector;

    public ConflictResolver(IConflictDetector conflictDetector)
    {
        this.conflictDetector = conflictDetector;
    }

    public Task<List<ResolvedCommand>> ResolveAsync(RequestedCommand approvedCommand, IReadOnlyCollection<RequestedCommand>? overlappingCommands)
    {
        var resolvedCommands = new List<ResolvedCommand>();

        if (AnyOverlapping(overlappingCommands))
        {
            resolvedCommands.Add(CreateFromRequested(approvedCommand));
        }

        return Task.FromResult(resolvedCommands);
    }

    private static ResolvedCommand CreateFromRequested(RequestedCommand approvedCommand)
    {
        return new ResolvedCommand
        {
            RequestedCommandId = approvedCommand.Id,
            StartTime = approvedCommand.StartTime,
            EndTime = approvedCommand.EndTime,
            Status = ResolvedCommandStatus.Scheduled,
            RequestedCommand = approvedCommand,
        };
    }

    private static RequestedCommand CreateClone(RequestedCommand approvedCommand)
    {
        return new RequestedCommand
        {
            CommandName = approvedCommand.CommandName,
            Type = approvedCommand.Type,
            Status = approvedCommand.Status,
            ConnectorId = approvedCommand.ConnectorId,
            ExternalId = approvedCommand.ExternalId,
            TwinId = approvedCommand.TwinId,
            RuleId = approvedCommand.RuleId,
            StartTime = approvedCommand.StartTime,
            EndTime = approvedCommand.EndTime,
            Unit = approvedCommand.Unit,
            Value = approvedCommand.Value,
            IsCapabilityOf = approvedCommand.IsCapabilityOf,
            IsHostedBy = approvedCommand.IsHostedBy,
            Location = approvedCommand.Location,
            SiteId = approvedCommand.SiteId,
            CreatedDate = approvedCommand.CreatedDate,
        };
    }

    private static bool AnyOverlapping(IReadOnlyCollection<RequestedCommand>? overlappingCommands)
    {
        return overlappingCommands != null && overlappingCommands.Count > 0;
    }

    public Task<List<ResolvedCommand>> ResolveAsync(List<RequestedCommand> overlappingCommands)
    {
        var resolvedCommands = new List<ResolvedCommand>();

        //check if the overlappingCommands are have unique external id
        if (overlappingCommands.GroupBy(group => new { group.ExternalId }).Count() > 1)
        {
            throw new Exception("ExternalID is not unique");
        }

        var resolvedRequestedCommands = new List<RequestedCommand>();
        var sortedCommand = overlappingCommands.OrderBy(x => x.StartTime)
            .ThenBy(x => x.EndTime != null ? x.EndTime : DateTimeOffset.MaxValue)
        /*    .ThenByDescending(x => x.Value)
            .ThenByDescending(x => x.CreatedDateUtc).ToList()*/
        ;

        foreach (var newCommand in sortedCommand)
        {
            resolvedRequestedCommands.RemoveAll(existing => conflictDetector.AreConflicting(existing, newCommand));

            if (resolvedRequestedCommands.Exists(existingCommand => conflictDetector.HaveSameIntervalAndValue(newCommand, existingCommand)))
            {
                continue;
            }
            else if (resolvedRequestedCommands.Exists(existing => conflictDetector.HaveSameInterval(newCommand, existing) &&
                ((newCommand.Value <= existing.Value && newCommand.CommandName == existing.CommandName && newCommand.CommandName.Equals(nameof(SetPointCommandName.atLeast)))
                || (newCommand.Value >= existing.Value && newCommand.CommandName == existing.CommandName && newCommand.CommandName.Equals(nameof(SetPointCommandName.atMost))))))
            {
                continue;
            }
            else if (resolvedRequestedCommands.Exists(existing => conflictDetector.AreContained(newCommand, existing)))
            {
                var containing = resolvedRequestedCommands.First(existing => conflictDetector.AreContained(newCommand, existing));
                if (containing.Value != newCommand.Value)
                {
                    resolvedRequestedCommands.Add(CreateClone(newCommand));
                    var another = CreateClone(containing);
                    another.StartTime = newCommand.EndTime!.Value;
                    containing.EndTime = newCommand.StartTime;
                    resolvedRequestedCommands.Add(another);
                }
            }
            else if (resolvedRequestedCommands.Exists(existing => conflictDetector.IsOverlapping(existing, newCommand)))
            {
                var containing = resolvedRequestedCommands.First(existing => conflictDetector.IsOverlapping(existing, newCommand));
                if (containing.Value == newCommand.Value)
                {
                    // Merge the overlapping intervals with the same value
                    containing.EndTime = newCommand.EndTime;
                }
                else
                {
                    // Split the existing interval at the start of the new interval
                    var split = CreateClone(newCommand);
                    resolvedRequestedCommands.Add(split);
                    containing.EndTime = split.StartTime;
                }
            }
            else
            {
                resolvedRequestedCommands.Add(CreateClone(newCommand));
            }
        }

        foreach (var command in resolvedRequestedCommands)
        {
            resolvedCommands.Add(CreateFromRequested(command));
        }

        return Task.FromResult(resolvedCommands);
    }
}
