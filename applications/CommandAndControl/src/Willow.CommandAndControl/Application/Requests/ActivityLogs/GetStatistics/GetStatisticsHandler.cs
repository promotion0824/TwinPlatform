namespace Willow.CommandAndControl.Application.Requests.ActivityLogs.GetStatistics;

internal class GetStatisticsHandler
{
    public static async Task<Ok<GetStatisticsResponseDto>> HandleAsync([FromBody] GetStatisticsRequestDto request, IApplicationDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var startDate = request.StartDate ?? DateTime.Now.AddDays(-7);
        var endDate = request.EndDate ?? DateTime.Now;

        var top10Activities = await dbContext.ActivityLogs
            .Include(x => x.RequestedCommand)
            .Where(x => x.Timestamp.Date >= startDate && x.Timestamp.Date <= endDate)
            .Where(x => string.IsNullOrWhiteSpace(request.SiteId) || x.RequestedCommand.SiteId == request.SiteId)
            .OrderByDescending(x => x.Timestamp)
            .Take(10)
            .Select(x => new ActivityLogsResponseDto
            {
                Id = x.Id,
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
            }).ToListAsync(cancellationToken);

        var resolvedCommandCounts = dbContext.ResolvedCommands
            .Include(x => x.RequestedCommand)
            .Where(a => a.LastUpdated >= startDate && a.LastUpdated <= endDate)
            .Where(x => string.IsNullOrWhiteSpace(request.SiteId) || x.RequestedCommand.SiteId == request.SiteId)
            .GroupBy(a => new { a.LastUpdated.Date, a.Status })
            .Select(g => new DbRecord { Date = g.Key.Date.ToString("yyyy-MM-dd"), Status = g.Key.Status.ToString(), Count = g.Count() });

        var resolvedRes = await resolvedCommandCounts.ToDictionaryAsync(x => $"{x.Status}-{x.Date}", cancellationToken);

        int daysCount = (int)(endDate - startDate).TotalDays + 1;
        var categories = Enumerable.Range(0, daysCount).Select(i => startDate.AddDays(i).ToString("yyyy-MM-dd")).ToList();
        var commandTrends = new CommandTrendsDto
        {
            Categories = categories,
            Dataset = GetDataSet(categories, resolvedRes),
        };
        var queryableRequestedCommands = dbContext.RequestedCommands
            .Where(x => string.IsNullOrWhiteSpace(request.SiteId) || x.SiteId == request.SiteId)
            .Where(x => x.LastUpdated.Date >= startDate && x.LastUpdated.Date <= endDate);

        var queryableResolvedCommands = dbContext.ResolvedCommands
            .Include(x => x.RequestedCommand)
            .Where(x => string.IsNullOrWhiteSpace(request.SiteId) || x.RequestedCommand.SiteId == request.SiteId)
            .Where(x => x.LastUpdated.Date >= startDate && x.LastUpdated.Date <= endDate);

        return TypedResults.Ok(new GetStatisticsResponseDto
        {
            CommandsCount = new CommandsCountStatisticsDto
            {
                TotalRequestedCommands = await queryableRequestedCommands.CountAsync(cancellationToken),
                TotalApprovedCommands = await queryableResolvedCommands.CountAsync(x => x.Status == ResolvedCommandStatus.Approved, cancellationToken),
                TotalCancelledCommands = await queryableResolvedCommands.CountAsync(x => x.Status == ResolvedCommandStatus.Cancelled, cancellationToken),
                TotalExecutedCommands = await queryableResolvedCommands.CountAsync(x => x.Status == ResolvedCommandStatus.Executed, cancellationToken),
                TotalFailedCommands = await queryableResolvedCommands.CountAsync(x => x.Status == ResolvedCommandStatus.Failed, cancellationToken),
                TotalSuspendedCommands = await queryableResolvedCommands.CountAsync(x => x.Status == ResolvedCommandStatus.Suspended, cancellationToken),
            },
            CommandTrends = commandTrends,
            RecentActivities = top10Activities,
        });
    }

    private static IDictionary<string, CommandsTrendDataSetDto> GetDataSet(List<string> categories, IDictionary<string, DbRecord> resolvedRes)
    {
        var result = new Dictionary<string, CommandsTrendDataSetDto>();

        // Status that we are interested to show in the chart
        List<string> status = [
            ResolvedCommandStatus.Approved.ToString(),
            ResolvedCommandStatus.Cancelled.ToString(),
            ResolvedCommandStatus.Executed.ToString(),
            ResolvedCommandStatus.Failed.ToString(),
            ResolvedCommandStatus.Suspended.ToString()
            ];

        foreach (var item in status)
        {
            var data = new List<int>();
            result.Add(item, new CommandsTrendDataSetDto { Name = item, Data = data });
            foreach (var category in categories)
            {
                var key = $"{item}-{category}";
                data.Add(resolvedRes.ContainsKey(key) ? resolvedRes[key].Count : 0);
            }
        }

        return result;
    }

    private class DbRecord
    {
        public string Date { get; set; } = default!;

        public string Status { get; set; } = default!;

        public int Count { get; set; }
    }
}
