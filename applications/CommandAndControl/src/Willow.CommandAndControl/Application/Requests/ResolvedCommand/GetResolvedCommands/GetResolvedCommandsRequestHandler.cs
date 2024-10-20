namespace Willow.CommandAndControl.Application.Requests.ResolvedCommand.GetResolvedCommands;

internal static class GetResolvedCommandsRequestHandler
{
    internal static async Task<Results<Ok<BatchDto<ResolvedCommandResponseDto>>, BadRequest<ProblemDetails>>> HandleAsync([FromBody] GetResolvedCommandsDto request,
        IApplicationDbContext dbContext,
        ITwinInfoService twinInfoService,
        CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        var list = await dbContext.ResolvedCommands
            .Where(command => command.Status != ResolvedCommandStatus.Executed ||
                              command.Status != ResolvedCommandStatus.Cancelled || (
                              (command.EndTime == null || command.EndTime > utcNow) &&
                               command.Status != ResolvedCommandStatus.Executing)).FilterBy(request.FilterSpecifications).SortBy(request.SortSpecifications)
            .FilterBy(request.FilterSpecifications)
            .SortBy(request.SortSpecifications)
            .GetPagedResultAsync(
                x => x.Include(command => command.RequestedCommand),
                request.Page,
                request.PageSize,
                cancellationToken);

        if (list?.Items == null || list.Items.Length == 0) { return TypedResults.Ok(new BatchDto<ResolvedCommandResponseDto>()); }

        var presentValueList = await twinInfoService.GetPresentValueAsync(list.Items.Select(x => x.RequestedCommand.ExternalId));
        List<ResolvedCommandResponseDto> result = list.Items.ToList().MapToDto(presentValueList);
        result.ForEach(command =>
        {
            command.Actions = command.Status switch
            {
                ResolvedCommandStatus.Approved => ["Execute", "Cancel"],
                ResolvedCommandStatus.Scheduled => ["Suspend", "Cancel"],
                ResolvedCommandStatus.Suspended => ["Unsuspend", "Cancel"],
                ResolvedCommandStatus.Failed => ["Retry"],
                _ => [],
            };
        });
        var batchResult = new BatchDto<ResolvedCommandResponseDto>()
        {
            Items = result.ToArray(),
            Total = list.Total,
        };
        return TypedResults.Ok(batchResult);
    }
}
