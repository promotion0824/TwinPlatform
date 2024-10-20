namespace Willow.CommandAndControl.Application.Requests.RequestedCommand.UpdateStatusCommand;

using Willow.CommandAndControl.Application.Helpers;

internal class UpdateRequestedCommandStatusHandler
{
    internal static async Task<Results<NoContent, BadRequest<string[]>>> HandleAsync(
        [FromQuery] string id,
        [FromBody] UpdateRequestedCommandStatusDto request,
        IActivityLogger activityLogger,
        IAuditLogger<UpdateRequestedCommandStatusHandler> auditLogger,
        IApplicationDbContext dbContext,
        IDbTransactions dbTransactions,
        IUserInfoService userInfoService,
        CancellationToken cancellationToken = default)
    {
        var userInfo = userInfoService.GetUser()!;

        var commandId = Guid.Parse(id);
        if (Enum.TryParse(request.Action, true, out RequestedCommandAction requestedAction))
        {
            var requested = await dbContext.RequestedCommands.SingleOrDefaultAsync(x => x.Id == commandId, cancellationToken) ??
                throw new InvalidOperationException("Requested Command not found");

            if (!Validate(requested, out string errorMessage))
            {
                return TypedResults.BadRequest(new[] { errorMessage });
            }

            await dbTransactions.RunAsync(dbContext, async _ =>
            {
                var commandsToReject = await dbContext.RequestedCommands.Where(x =>
                    x.Id != requested.Id &&
                    x.ExternalId == requested.ExternalId &&
                    x.Status == RequestedCommandStatus.Pending).ToListAsync(cancellationToken);

                commandsToReject.ForEach(x =>
                {
                    x.Status = RequestedCommandStatus.Rejected;
                    x.StatusUpdatedBy = userInfo;
                    x.LastUpdated = DateTimeOffset.UtcNow;
                });

                if (requestedAction == RequestedCommandAction.Approve)
                {
                    requested.Status = RequestedCommandStatus.Approved;

                    //TODO: Resolve the RequestedCommand using Conflict resolver once we are confident to automate approval process.
                    var resolved = requested.CreateResolvedCommand(userInfo);
                    await dbContext.ResolvedCommands.AddAsync(resolved);
                    await activityLogger.LogAsync(ActivityType.Approved, requested, cancellationToken);
                }
                else if (requestedAction == RequestedCommandAction.Reject)
                {
                    requested.Status = RequestedCommandStatus.Rejected;
                    await activityLogger.LogAsync(ActivityType.Retracted, requested, cancellationToken);
                }

                requested.StatusUpdatedBy = userInfo;
                requested.LastUpdated = DateTimeOffset.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
                auditLogger.LogInformation(userInfo.Email!, "Request with {id} {status}", requested.Id, requestedAction);
                return true;
            });

            return TypedResults.NoContent();
        }

        return TypedResults.BadRequest(new[] { "Invalid action type." });
    }

    private static bool Validate(Data.Models.RequestedCommand requested, out string errorMessage)
    {
        if (requested.Status == RequestedCommandStatus.Approved)
        {
            errorMessage = "Requested command is already approved.";
            return false;
        }

        if (requested.Status == RequestedCommandStatus.Rejected)
        {
            errorMessage = "Requested command is already rejected.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }
}
