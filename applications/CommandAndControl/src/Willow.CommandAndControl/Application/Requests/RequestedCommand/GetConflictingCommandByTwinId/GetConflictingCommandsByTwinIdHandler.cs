namespace Willow.CommandAndControl.Application.Requests.RequestedCommand.GetConflictingCommandsByTwinId;

internal class GetConflictingCommandsByTwinIdHandler
{
    internal static async Task<Results<Ok<ConflictingCommandsResponseDto>, BadRequest<ProblemDetails>, NotFound>> HandleAsync(
        string connectorId,
        string twinId,
        IApplicationDbContext dbContext,
        ITwinInfoService twinInfoService,
        CancellationToken cancellationToken = default)
    {
        double? presentValue = null;

        var queryable = dbContext.RequestedCommands
            .Where(x => x.ConnectorId == connectorId && x.TwinId == twinId)
            .OrderByDescending(x => x.LastUpdated)
            .AsNoTracking()
            .AsQueryable();

        var command = await queryable.ToListAsync(cancellationToken);
        if (command is null || command.Count == 0)
        {
            return TypedResults.NotFound();
        }

        var firstRequest = command.OrderByDescending(r => r.ReceivedDate).First();

        try
        {
            presentValue = await twinInfoService.GetPresentValueAsync(firstRequest.ConnectorId, firstRequest.ExternalId);
        }
        catch
        {
            // If we can't get the present value, we'll just return null.
        }

        return TypedResults.Ok(new ConflictingCommandsResponseDto
        {
            ConnectorId = firstRequest.ConnectorId,
            ExternalId = firstRequest.ExternalId,
            TwinId = firstRequest.TwinId,
            IsCapabilityOf = firstRequest.IsCapabilityOf,
            IsHostedBy = firstRequest.IsHostedBy,
            Location = firstRequest.Location,
            SiteId = firstRequest.SiteId,
            PresentValue = presentValue,
            ReceivedDate = firstRequest.ReceivedDate,
            Unit = firstRequest.Unit,
            Requests = command.MapToDto(presentValue),
        });
    }
}
