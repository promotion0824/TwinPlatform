namespace Willow.CommandAndControl.Application.Requests.RequestedCommand.GetConflictingCommands;

internal static class GetConflictingCommandPresentValuesHandler
{
    internal static async Task<Results<Ok<GetConflictingCommandPresentValuesResponseDto>, BadRequest<ProblemDetails>>> HandleAsync(
    [FromBody] GetConflictingCommandPresentValuesRequestDto request,
    ITwinInfoService twinInfoService,
    CancellationToken cancellationToken = default)
    {
        var presentValues = await twinInfoService.GetPresentValueAsync(request.ExternalIds);

        var uniqueIds = request.ExternalIds.Distinct();

        var presentValuesDict = uniqueIds
            .ToDictionary(
                id => id,
                id => presentValues.TryGetValue(id, out double? value) ? value : null);

        return TypedResults.Ok(new GetConflictingCommandPresentValuesResponseDto { PresentValues = presentValuesDict });
    }
}
