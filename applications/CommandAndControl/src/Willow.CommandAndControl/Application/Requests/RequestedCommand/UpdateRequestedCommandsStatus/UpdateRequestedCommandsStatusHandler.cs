namespace Willow.CommandAndControl.Application.Requests.RequestedCommand.UpdateRequestedCommandsStatus;

internal class UpdateRequestedCommandsStatusHandler
{
    private static readonly string[] CommandIdError = ["Invalid command ID."];

    private static readonly CompositeFormat NotFoundError = CompositeFormat.Parse("Request with ID {0} not found");

    internal static async Task<Results<NoContent, BadRequest<string[]>, NotFound<string>>> HandleAsync([FromBody] UpdateRequestedCommandsStatusDto request,
    IActivityLogger activityLogger,
    IAuditLogger<UpdateRequestedCommandsStatusHandler> auditLogger,
    IApplicationDbContext dbContext,
    IDbTransactions dbTransactions,
    IUserInfoService userInfoService,
    IConfiguration configuration,
    CancellationToken cancellationToken = default)
    {
        List<(string UserId, string Message, object[] Parameters)> auditLogs = [];

        var userInfo = userInfoService.GetUser()!;
        var startAutoExecutionOnApproval = configuration.GetValue<bool>("StartAutoExecutionOnApproval", false);

        foreach (var id in request.ApproveIds)
        {
            if (!Guid.TryParse(id, out var commandId)) { return TypedResults.BadRequest(CommandIdError); }

            var requested = await dbContext.RequestedCommands.FirstOrDefaultAsync(x => x.Id == commandId, cancellationToken);

            if (requested == null)
            {
                return TypedResults.NotFound(GetNotFoundError(commandId));
            }

            // Already approved, nothing to do.
            if (requested.Status == RequestedCommandStatus.Approved)
            {
                var pendingResolvedCommands = await dbContext.ResolvedCommands
                    .AnyAsync(r => r.RequestedCommandId == commandId && r.Status < ResolvedCommandStatus.Executing, cancellationToken);
                if (pendingResolvedCommands) { continue; }
            }

            requested.Status = RequestedCommandStatus.Approved;

            //TODO: Resolve the RequestedCommand using Conflict resolver once we are confident to automate approval process.
            var resolved = requested.CreateResolvedCommand(userInfo);

            if (startAutoExecutionOnApproval)
            {
                resolved.Status = ResolvedCommandStatus.Scheduled;
            }

            await dbContext.ResolvedCommands.AddAsync(resolved, cancellationToken);

            auditLogs.Add((userInfo.Email!, "Request with {id} Approved", [requested.Id]));
            await activityLogger.LogAsync(ActivityType.Approved, requested, cancellationToken);

            requested.StatusUpdatedBy = userInfo;
            requested.LastUpdated = DateTimeOffset.UtcNow;
        }

        foreach (var id in request.RejectIds)
        {
            if (!Guid.TryParse(id, out var commandId)) { return TypedResults.BadRequest(CommandIdError); }

            var requested = await dbContext.RequestedCommands.FirstOrDefaultAsync(x => x.Id == commandId, cancellationToken);

            if (requested == null)
            {
                return TypedResults.NotFound(GetNotFoundError(commandId));
            }

            // Already rejected, nothing to do.
            if (requested.Status == RequestedCommandStatus.Rejected) { continue; }

            // Cancel any unexecuted resolved commands.
            var resolvedCommands = await dbContext.ResolvedCommands.Where(r => r.RequestedCommandId == commandId && r.Status < ResolvedCommandStatus.Executing).ToListAsync(cancellationToken: cancellationToken);
            resolvedCommands.ForEach(r =>
            {
                r.Status = ResolvedCommandStatus.Cancelled;
                r.StatusUpdatedBy = userInfo;
            });

            requested.Status = RequestedCommandStatus.Rejected;

            auditLogs.Add((userInfo.Email!, "Request with {id} Rejected", [requested.Id]));
            await activityLogger.LogAsync(ActivityType.Retracted, requested, cancellationToken);

            requested.StatusUpdatedBy = userInfo;
            requested.LastUpdated = DateTimeOffset.UtcNow;
        }

        await dbTransactions.RunAsync(dbContext, async _ =>
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            auditLogs.ForEach(a => auditLogger.LogInformation(a.UserId, a.Message, a.Parameters));
            return true;
        });

        return TypedResults.NoContent();
    }

    private static string GetNotFoundError(Guid id) => string.Format(System.Globalization.CultureInfo.InvariantCulture, NotFoundError, id);
}
