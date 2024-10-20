namespace Willow.CommandAndControl.Application.Services.Abstractions;

internal interface IConflictResolver
{
    Task<List<ResolvedCommand>> ResolveAsync(RequestedCommand approvedCommand, IReadOnlyCollection<RequestedCommand> overlappingCommands);

    Task<List<ResolvedCommand>> ResolveAsync(List<RequestedCommand> overlappingCommands);
}
