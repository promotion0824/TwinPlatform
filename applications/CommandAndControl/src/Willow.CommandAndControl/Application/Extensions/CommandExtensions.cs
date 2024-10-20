namespace Willow.CommandAndControl.Application.Extensions;

internal static class CommandExtensions
{
    internal static ResolvedCommandResponseDto MapToCommandDto(this ResolvedCommand command, double? presentValue = null)
    {
        var result = new ResolvedCommandResponseDto
        {
            Id = command.Id,
            CommandName = command.RequestedCommand.CommandName,
            Type = command.RequestedCommand.Type,
            RuleId = command.RequestedCommand.RuleId,
            Status = command.Status,
            TwinId = command.RequestedCommand.TwinId,
            ExternalId = command.RequestedCommand.ExternalId,
            ConnectorId = command.RequestedCommand.ConnectorId,
            SiteId = command.RequestedCommand.SiteId,
            Location = command.RequestedCommand.Location,
            Locations = [.. command.RequestedCommand.Locations],
            IsCapabilityOf = command.RequestedCommand.IsCapabilityOf,
            IsHostedBy = command.RequestedCommand.IsHostedBy,
            PresentValue = presentValue,
            Value = command.RequestedCommand.Value,
            Unit = command.RequestedCommand.Unit,
            RequestId = command.RequestedCommandId,
            StartTime = command.StartTime,
            EndTime = command.EndTime,
            CreatedDate = command.CreatedDate,
            StatusUpdatedBy = command.RequestedCommand.StatusUpdatedBy,
            LastUpdated = command.LastUpdated,
        };

        return result;
    }

    internal static RequestedCommandResponseDto MapToCommandDto(this RequestedCommand command, double? presentValue = null)
    {
        var result = new RequestedCommandResponseDto
        {
            Id = command.Id,
            Status = command.Status.ToString(),
            RuleId = command.RuleId,
            TwinId = command.TwinId,
            CommandName = command.CommandName,
            Type = command.Type,
            ConnectorId = command.ConnectorId,
            ExternalId = command.ExternalId,
            SiteId = command.SiteId,
            Location = command.Location,
            Locations = [.. command.Locations],
            IsCapabilityOf = command.IsCapabilityOf,
            IsHostedBy = command.IsHostedBy,
            PresentValue = presentValue,
            Value = command.Value,
            Unit = command.Unit,
            StartTime = command.StartTime,
            EndTime = command.EndTime,
            CreatedDate = command.CreatedDate,
            StatusUpdatedBy = command.StatusUpdatedBy,
            LastUpdated = command.LastUpdated,
        };

        return result;
    }

    internal static RequestedCommandResponseDto[] MapToDto(this RequestedCommand[] list, IDictionary<string, double?>? presentValueList = null)
    {
        return list.Select(x =>
        new RequestedCommandResponseDto
        {
            Status = x.Status.ToString(),
            CommandName = x.CommandName,
            Type = x.Type,
            ExternalId = x.ExternalId,
            ConnectorId = x.ConnectorId,
            TwinId = x.TwinId,
            SiteId = x.SiteId,
            Location = x.Location,
            Locations = [.. x.Locations],
            IsCapabilityOf = x.IsCapabilityOf,
            IsHostedBy = x.IsHostedBy,
            RuleId = x.RuleId,
            PresentValue = presentValueList?.ContainsKey(x.ExternalId) == true ? presentValueList[x.ExternalId] : null,
            CreatedDate = x.CreatedDate,
            EndTime = x.EndTime,
            Id = x.Id,
            StartTime = x.StartTime,
            Value = x.Value,
            Unit = x.Unit,
            StatusUpdatedBy = x.StatusUpdatedBy,
            LastUpdated = x.LastUpdated,
        }).ToArray();
    }

    internal static List<RequestedCommandResponseDto> MapToDto(this List<RequestedCommand> list, double? presentValue = null)
    {
        return list.Select(x =>
        new RequestedCommandResponseDto
        {
            Status = x.Status.ToString(),
            CommandName = x.CommandName,
            Type = x.Type,
            ExternalId = x.ExternalId,
            ConnectorId = x.ConnectorId,
            TwinId = x.TwinId,
            SiteId = x.SiteId,
            Location = x.Location,
            Locations = [.. x.Locations],
            IsCapabilityOf = x.IsCapabilityOf,
            IsHostedBy = x.IsHostedBy,
            RuleId = x.RuleId,
            PresentValue = presentValue,
            CreatedDate = x.CreatedDate,
            EndTime = x.EndTime,
            Id = x.Id,
            StartTime = x.StartTime,
            Value = x.Value,
            Unit = x.Unit,
            StatusUpdatedBy = x.StatusUpdatedBy,
            LastUpdated = x.LastUpdated,
        }).ToList();
    }

    internal static List<ResolvedCommandResponseDto> MapToDto(this List<ResolvedCommand> list, IDictionary<string, double?>? presentValueList = null)
    {
        return list.Select(x =>
        new ResolvedCommandResponseDto
        {
            Id = x.Id,
            CommandName = x.RequestedCommand.CommandName,
            Type = x.RequestedCommand.Type,
            TwinId = x.RequestedCommand.TwinId,
            RuleId = x.RequestedCommand.RuleId,
            ExternalId = x.RequestedCommand.ExternalId,
            ConnectorId = x.RequestedCommand.ConnectorId,
            SiteId = x.RequestedCommand.SiteId,
            Location = x.RequestedCommand.Location,
            Locations = [.. x.RequestedCommand.Locations],
            IsCapabilityOf = x.RequestedCommand.IsCapabilityOf,
            IsHostedBy = x.RequestedCommand.IsHostedBy,
            PresentValue = presentValueList?.ContainsKey(x.RequestedCommand.ExternalId) == true ? presentValueList[x.RequestedCommand.ExternalId] : null,
            Value = x.RequestedCommand.Value,
            Unit = x.RequestedCommand.Unit,
            RequestId = x.RequestedCommandId,
            Status = x.Status,
            Comment = x.Comment,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            CreatedDate = x.CreatedDate,
            StatusUpdatedBy = x.StatusUpdatedBy,
            LastUpdated = x.LastUpdated,
        }).ToList();
    }

    internal static ResolvedCommand CreateResolvedCommand(this RequestedCommand requestedCommand, User? userInfo = null)
    {
        return new ResolvedCommand
        {
            RequestedCommandId = requestedCommand.Id,
            StartTime = requestedCommand.StartTime,
            EndTime = requestedCommand.EndTime,
            Status = ResolvedCommandStatus.Approved,
            StatusUpdatedBy = userInfo ?? User.Empty,
            RequestedCommand = requestedCommand,
        };
    }

    public static async Task<BatchDto<TEntity>> GetPagedResultAsync<TEntity>(this IQueryable<TEntity> dbSet,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeFunc = default,
        int? page = 1,
        int? pageSize = 10,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        page ??= 1;
        pageSize ??= 10;

        IQueryable<TEntity> queryable = dbSet.AsQueryable();

        if (includeFunc != null)
        {
            queryable = includeFunc(queryable) ?? queryable;
        }

        return await queryable.Paginate(page, pageSize);
    }

    public static async Task<(int Total, List<TEntity> Entities)> GetResultAsync<TEntity>(this IQueryable<TEntity> dbSet,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? includeFunc = default,
    CancellationToken cancellationToken = default)
        where TEntity : class
    {
        IQueryable<TEntity> queryable = dbSet.AsQueryable();

        if (includeFunc != null)
        {
            queryable = includeFunc(queryable) ?? queryable;
        }

        var total = await queryable.CountAsync(cancellationToken);
        var list = await queryable.ToListAsync(cancellationToken);
        return (total, list);
    }
}
