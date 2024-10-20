namespace Willow.CommandAndControl.Application.Requests.ResolvedCommand.UpdateStatusCommand;

using Willow.CommandAndControl.Application.Helpers;

internal class UpdateResolvedCommandStatusHandler
{
    internal static async Task<Results<Ok<UpdateResolvedCommandStatusResponseDto>, BadRequest<ProblemDetails>, NotFound<string>>> HandleAsync([FromRoute] Guid id,
                                                                                                       [FromBody] UpdateResolvedCommandStatusDto request,
                                                                                                       IApplicationDbContext dbContext,
                                                                                                       IActivityLogger activityLogger,
                                                                                                       IAuditLogger<UpdateResolvedCommandStatusHandler> auditLogger,
                                                                                                       IUserInfoService userInfoService,
                                                                                                       ILogger<UpdateResolvedCommandStatusHandler> logger,
                                                                                                       CancellationToken cancellationToken = default)
    {
        var user = userInfoService.GetUser()!;

        var command = await dbContext.ResolvedCommands.Include(rc => rc.RequestedCommand).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (command is null)
        {
            return TypedResults.NotFound($"Command with ID {id} not found");
        }

        command.Comment = request.Comment;
        command.StatusUpdatedBy = user;

        bool result;

        ProblemDetails problemDetails = null!;

        switch (request.Action)
        {
            case ResolvedCommandAction.Cancel:

                if (command.Status > ResolvedCommandStatus.Failed)
                {
                    logger.LogWarning("{status} Command cannot be cancelled", command.Status);
                    problemDetails = CreateProblemDetails($"{command.Status} Command cannot be cancelled");
                    result = false;
                    break;
                }

                command.Status = ResolvedCommandStatus.Cancelled;
                result = true;
                await activityLogger.LogAsync(ActivityType.Cancelled, command, cancellationToken);
                break;
            case ResolvedCommandAction.Suspend:
                if (command.Status != ResolvedCommandStatus.Scheduled)
                {
                    logger.LogWarning("{status} Command cannot be suspended", command.Status);
                    problemDetails = CreateProblemDetails($"{command.Status} Command cannot be suspended");
                    result = false;
                    break;
                }

                command.Status = ResolvedCommandStatus.Suspended;
                result = true;
                await activityLogger.LogAsync(ActivityType.Suspended, command, cancellationToken);
                break;
            case ResolvedCommandAction.Unsuspend:
                if (command.Status != ResolvedCommandStatus.Suspended || command.EndTime <= DateTime.UtcNow)
                {
                    logger.LogWarning("{status} Command with end time {endTime} cannot be unsuspended", command.Status, command.EndTime);
                    problemDetails = CreateProblemDetails($"{command.Status} Command with end time {command.EndTime} cannot be unsuspended");
                    result = false;
                    break;
                }

                command.Status = ResolvedCommandStatus.Scheduled;
                result = true;
                await activityLogger.LogAsync(ActivityType.Executed, command, cancellationToken);
                break;
            case ResolvedCommandAction.Execute:
                if ((command.Status != ResolvedCommandStatus.Approved && command.Status != ResolvedCommandStatus.Failed) || command.EndTime <= DateTime.UtcNow)
                {
                    logger.LogWarning("{status} Command with end time {endTime} cannot be executed", command.Status, command.EndTime);
                    problemDetails = CreateProblemDetails($"{command.Status} Command with end time {command.EndTime} cannot be executed");
                    result = false;
                    break;
                }

                command.Status = ResolvedCommandStatus.Scheduled;

                result = true;
                await activityLogger.LogAsync(ActivityType.Executed, command, cancellationToken);
                break;
            case ResolvedCommandAction.Retry:
                if (command.Status != ResolvedCommandStatus.Failed || command.EndTime <= DateTime.UtcNow)
                {
                    logger.LogWarning("{status} Command with end time {endTime} cannot be retried", command.Status, command.EndTime);
                    problemDetails = CreateProblemDetails($"{command.Status} Command with end time {command.EndTime} cannot be retried");
                    result = false;
                    break;
                }

                command.Status = ResolvedCommandStatus.Scheduled;
                result = true;
                await activityLogger.LogAsync(ActivityType.Retried, command, cancellationToken);
                break;
            default:
                logger.LogWarning("Invalid action type {action}", request.Action);
                problemDetails = CreateProblemDetails($"Invalid action type {request.Action}");
                result = false;
                break;
        }

        if (result)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            auditLogger.LogInformation(user.Email!, "Command with {id} {status}", id, command.Status);
        }

        return result ? TypedResults.Ok(new UpdateResolvedCommandStatusResponseDto(id)) : TypedResults.BadRequest<ProblemDetails>(problemDetails);
    }

    private static ProblemDetails CreateProblemDetails(string message, params object[] args) =>
        new()
        {
            Title = "Cannot update command status",
            Detail = message,
            Type = "https://httpstatuses.com/400",
        };
}
