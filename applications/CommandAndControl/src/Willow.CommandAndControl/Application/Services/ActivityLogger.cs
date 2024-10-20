namespace Willow.CommandAndControl.Application.Services;

/// <summary>
/// Logs user activity.
/// </summary>
internal class ActivityLogger : IActivityLogger
{
    private readonly IApplicationDbContext dbContext;
    private readonly IUserInfoService userInfoService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActivityLogger"/> class.
    /// </summary>
    /// <param name="dbContext">Database context.</param>
    /// <param name="userInfoService">User information service.</param>
    public ActivityLogger(IApplicationDbContext dbContext, IUserInfoService userInfoService)
    {
        this.dbContext = dbContext;
        this.userInfoService = userInfoService;
    }

    /// <summary>
    /// Save activity log to database.
    /// </summary>
    /// <param name="type">The type of activity.</param>
    /// <param name="command">The requested command.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task.</returns>
    public async ValueTask LogAsync(ActivityType type, RequestedCommand command, CancellationToken cancellationToken = default)
    {
        await dbContext.ActivityLogs.AddAsync(new ActivityLog
        {
            Type = type,
            RequestedCommandId = command.Id,
            Timestamp = DateTimeOffset.UtcNow,
            UpdatedBy = userInfoService.GetUser() ?? command.StatusUpdatedBy,
        },
        cancellationToken);
    }

    /// <summary>
    /// Save activity log to database.
    /// </summary>
    /// <param name="type">The type of activity.</param>
    /// <param name="command">The resolved command.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task.</returns>
    public async ValueTask LogAsync(ActivityType type, ResolvedCommand command, CancellationToken cancellationToken = default)
    {
        await dbContext.ActivityLogs.AddAsync(new ActivityLog
        {
            Type = type,
            RequestedCommandId = command.RequestedCommand.Id,
            ResolvedCommandId = command.Id,
            Timestamp = DateTimeOffset.UtcNow,
            UpdatedBy = userInfoService.GetUser() ?? command.StatusUpdatedBy,
        },
        cancellationToken);
    }

    /// <summary>
    /// Save activity log to database.
    /// </summary>
    /// <param name="type">The type of activity.</param>
    /// <param name="command">The resolved command.</param>
    /// <param name="extraInfo">Extra Information to log.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task.</returns>
    public async ValueTask LogAsync(ActivityType type, ResolvedCommand command, string? extraInfo, CancellationToken cancellationToken = default)
    {
        await dbContext.ActivityLogs.AddAsync(new ActivityLog
        {
            Type = type,
            ExtraInfo = extraInfo,
            RequestedCommandId = command.RequestedCommand.Id,
            ResolvedCommandId = command.Id,
            Timestamp = DateTimeOffset.UtcNow,
            UpdatedBy = userInfoService.GetUser() ?? command.StatusUpdatedBy,
        },
        cancellationToken);
    }
}

internal static class ActivityLogSources
{
    public static readonly User EdgeDevice = new() { Name = "Edge Device" };
}
