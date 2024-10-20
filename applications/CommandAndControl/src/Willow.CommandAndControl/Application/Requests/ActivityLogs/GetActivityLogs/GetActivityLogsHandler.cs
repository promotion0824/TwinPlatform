namespace Willow.CommandAndControl.Application.Requests.ActivityLogs.GetActivityLogs;

internal static class GetActivityLogsHandler
{
    internal static async Task<Results<Ok<BatchDto<ActivityLogsResponseDto>>, BadRequest<ProblemDetails>>> HandleAsync([FromBody] ActivityLogsRequestDto request,
        IApplicationDbContext dbContext,
        ITwinInfoService twinInfoService,
        CancellationToken cancellationToken = default)
    {
        var list = await dbContext.ActivityLogs.Include(a => a.RequestedCommand).FilterBy(request.FilterSpecifications).SortBy(request.SortSpecifications).Paginate(request.Page, request.PageSize);
        var result = list.Items.Select(x => new ActivityLogsResponseDto
        {
            Id = x.Id,
            RequestedCommandId = x.RequestedCommandId,
            ResolvedCommandId = x.ResolvedCommandId,
            ConnectorId = x.RequestedCommand.ConnectorId,
            ExternalId = x.RequestedCommand.ExternalId,
            Location = x.RequestedCommand.Location,
            RuleId = x.RequestedCommand.RuleId,
            SiteId = x.RequestedCommand.SiteId,
            TwinId = x.RequestedCommand.TwinId,
            Value = x.RequestedCommand.Value,
            IsCapabilityOf = x.RequestedCommand.IsCapabilityOf,
            IsHostedBy = x.RequestedCommand.IsHostedBy,
            StartTime = x.RequestedCommand.StartTime,
            EndTime = x.RequestedCommand.EndTime,
            CommandName = x.RequestedCommand.CommandName,
            Unit = x.RequestedCommand.Unit,
            ExtraInfo = x.ExtraInfo,
            UpdatedBy = x.UpdatedBy,
            Type = x.Type,
            Timestamp = x.Timestamp,
        });

        var batchResult = new BatchDto<ActivityLogsResponseDto>
        {
            Items = result.ToArray(),
            Total = list.Total,
        };

        return TypedResults.Ok(batchResult);
    }
}
