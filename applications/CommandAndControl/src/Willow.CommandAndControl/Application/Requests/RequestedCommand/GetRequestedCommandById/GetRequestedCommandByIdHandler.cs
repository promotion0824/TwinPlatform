namespace Willow.CommandAndControl.Application.Requests.RequestedCommand.GetRequestedCommandById;

internal class GetRequestedCommandByIdHandler
{
    internal static async Task<Results<Ok<RequestedCommandResponseDto>, BadRequest<ProblemDetails>, NotFound>> HandleAsync(Guid id,
        IApplicationDbContext dbContext,
        ITwinInfoService twinInfoService,
        CancellationToken cancellationToken = default)
    {
        var command = await dbContext.RequestedCommands.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (command is null)
        {
            return TypedResults.NotFound();
        }

        (await twinInfoService.GetPresentValueAsync([command.ExternalId])).TryGetValue(command.ExternalId, out var presentValue);
        return TypedResults.Ok(command?.MapToCommandDto(presentValue));
    }
}
