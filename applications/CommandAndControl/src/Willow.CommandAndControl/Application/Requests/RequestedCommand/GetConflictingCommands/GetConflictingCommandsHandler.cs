namespace Willow.CommandAndControl.Application.Requests.RequestedCommand.GetConflictingCommands;

internal static class GetConflictingCommandsHandler
{
    internal static async Task<Results<Ok<BatchDto<ConflictingCommandsResponseDto>>, BadRequest<ProblemDetails>>> HandleAsync(
        [FromBody] GetConflictingCommandsRequestDto request,
        IApplicationDbContext dbContext,
        ITwinInfoService twinInfoService,
        CancellationToken cancellationToken = default)
    {
        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? 10;
        var utcNow = DateTime.UtcNow;

        var total = await dbContext.RequestedCommands
            .AsNoTracking()
            .FilterBy(request.FilterSpecifications)
            .Where(x => x.EndTime == null || x.EndTime > utcNow)
            .GroupBy(x => new { x.TwinId, x.IsCapabilityOf, x.IsHostedBy, x.Location, x.ConnectorId, x.ExternalId, x.Unit })
            .Where(x => !x.All(rc => rc.Status == RequestedCommandStatus.Approved || rc.Status == RequestedCommandStatus.Rejected))
            .Select(x => x.Key.TwinId)
            .Distinct()
            .CountAsync(cancellationToken);

        var twinIds = dbContext.RequestedCommands
            .AsNoTracking()
            .FilterBy(request.FilterSpecifications)
            .Where(x => x.EndTime == null || x.EndTime > utcNow)
            .GroupBy(x => new { x.TwinId, x.IsCapabilityOf, x.IsHostedBy, x.Location, x.ConnectorId, x.ExternalId, x.Unit })
            .Where(x => !x.All(rc => rc.Status == RequestedCommandStatus.Approved || rc.Status == RequestedCommandStatus.Rejected))
            .Select(x => new InterimDto
            {
                TwinId = x.Key.TwinId,
                ConnectorId = x.Key.ConnectorId,
                IsCapabilityOf = x.Key.IsCapabilityOf,
                IsHostedBy = x.Key.IsHostedBy,
                Location = x.Key.Location,
                ReceivedDate = x.Max(rc => rc.ReceivedDate),
                Commands = x.Count(),
                ApprovedCommands = x.Count(rc => rc.Status == RequestedCommandStatus.Approved),
                ExternalId = x.Key.ExternalId,
                Unit = x.Key.Unit,
            })
            .SortBy(request.SortSpecifications)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.TwinId)
            .Distinct();

        var queryable = dbContext.RequestedCommands
            .GroupBy(x => new { x.TwinId, x.IsCapabilityOf, x.IsHostedBy, x.SiteId, x.Location, x.ExternalId, x.ConnectorId, x.Unit })
            .Where(x => twinIds.Contains(x.Key.TwinId))
            .Select(g => new ConflictingCommandsResponseDto
            {
                TwinId = g.Key.TwinId,
                ConnectorId = g.Key.ConnectorId,
                IsCapabilityOf = g.Key.IsCapabilityOf,
                IsHostedBy = g.Key.IsHostedBy,
                SiteId = g.Key.SiteId,
                Location = g.Key.Location,
                ExternalId = g.Key.ExternalId,
                Unit = g.Key.Unit,
                ReceivedDate = g.Max(x => x.ReceivedDate),
                Commands = g.Count(),
                ApprovedCommands = g.Where(x => x.Status == RequestedCommandStatus.Approved).Count(),
            });

        var result = await queryable
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        if (result.Count == 0)
        {
            return TypedResults.Ok(new BatchDto<ConflictingCommandsResponseDto>());
        }

        var batchResult = new BatchDto<ConflictingCommandsResponseDto>()
        {
            Total = total,
            Items = result.ToArray(),
        };

        return TypedResults.Ok(batchResult);
    }
#pragma warning restore SA1513

    private record InterimDto : TwinInfoModel
    {
        public DateTimeOffset ReceivedDate { get; set; }

        public int Commands { get; set; }

        public int ApprovedCommands { get; set; }

        public override string ToString() =>
            $"{ConnectorId}|{TwinId}|{IsCapabilityOf}|{IsHostedBy}|{Location}|{ExternalId}|{Unit}";
    }
}
