namespace Willow.CommandAndControl.Application.Requests.ResolvedCommand.GetResolvedCommandById;

internal class GetResolvedCommandByIdHandler
{
    internal static async Task<Results<Ok<ResolvedCommandResponseDto>, BadRequest<ProblemDetails>, NotFound>> HandleAsync([AsParameters] GetResolvedCommandByIdRequestDto request,
        IApplicationDbContext dbContext,
        ITwinInfoService twinInfoService,
        CancellationToken cancellationToken = default)
    {
        var command = await dbContext.ResolvedCommands
            .Include(x => x.RequestedCommand)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (command is null) { return TypedResults.NotFound(); }

        (await twinInfoService.GetPresentValueAsync([command.RequestedCommand.ExternalId])).TryGetValue(command.RequestedCommand.ExternalId, out var value);
        return TypedResults.Ok(command?.MapToCommandDto(value));
    }
}
