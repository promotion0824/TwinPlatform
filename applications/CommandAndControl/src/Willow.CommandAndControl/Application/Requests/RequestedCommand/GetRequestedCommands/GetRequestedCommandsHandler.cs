namespace Willow.CommandAndControl.Application.Requests.RequestedCommand.GetRequestedCommands;

internal static class GetRequestedCommandsHandler
{
    internal static async Task<Results<Ok<BatchDto<RequestedCommandResponseDto>>, BadRequest<ProblemDetails>>> HandleAsync(
        [FromBody] GetRequestedCommandsDto request,
        IApplicationDbContext dbContext,
        ITwinInfoService twinInfoService,
        CancellationToken cancellationToken = default)
    {
        var list = await dbContext.RequestedCommands.FilterBy(request.FilterSpecifications).SortBy(request.SortSpecifications).Paginate(request.Page, request.PageSize);
        if (list?.Items == null || list.Items.Length == 0)
        {
            return TypedResults.Ok(new BatchDto<RequestedCommandResponseDto>());
        }

        var twinInfoList = await twinInfoService.GetPresentValueAsync(list.Items.Select(x => x.ExternalId));
        var batchResult = new BatchDto<RequestedCommandResponseDto>()
        {
            Items = list.Items.MapToDto(twinInfoList),
            Total = list.Total,
        };

        return TypedResults.Ok(batchResult);
    }
}
