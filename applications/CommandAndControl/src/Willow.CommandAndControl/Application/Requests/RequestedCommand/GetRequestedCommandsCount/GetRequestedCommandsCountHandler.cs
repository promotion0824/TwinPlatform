namespace Willow.CommandAndControl.Application.Requests.RequestedCommand.GetRequestedCommandsCount;

internal static class GetRequestedCommandsCountHandler
{
    internal static async Task<Ok<int>> HandleAsync(
        [FromBody] GetRequestedCommandsCountDto request,
        IApplicationDbContext dbContext)
    {
        var utcNow = DateTime.UtcNow;

        int count = await dbContext.RequestedCommands
            .FilterBy(request.FilterSpecifications)
            .Where(command => command.EndTime == null || command.EndTime > utcNow)
            .GroupBy(command => new { command.TwinId, command.IsCapabilityOf, command.IsHostedBy, command.Location, command.ConnectorId, command.ExternalId, command.Unit })
            .Where(command => !command.All(rc => rc.Status == RequestedCommandStatus.Approved || rc.Status == RequestedCommandStatus.Rejected))
            .CountAsync();

        return TypedResults.Ok(count);
    }
}
