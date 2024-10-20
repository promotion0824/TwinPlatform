namespace Willow.CommandAndControl.Application.Services;

using Polly;

/// <summary>
/// Provides command management functionality for Requested/Resolved Commands.
/// </summary>
internal class CommandManager(ILogger<CommandManager> logger,
                          IApplicationDbContext dbContext,
                          IDbTransactions dbTransactions,
                          IMappedGatewayService mappedGatewayService,
                          IConflictResolver conflictResolver)
    : ICommandManager
{
    public async Task ResolveAsync(string externalId, bool valueChanged, List<RequestedCommand> commands, CancellationToken cancellationToken = default)
    {
        if (valueChanged && commands.Count == 1)
        {
            RemoveExistingResolvedCommands(externalId);
            await dbContext.ResolvedCommands.AddAsync(commands.First().CreateResolvedCommand(), cancellationToken);
        }

        var resolvedCommands = await conflictResolver.ResolveAsync(commands);
        await dbContext.ResolvedCommands.AddRangeAsync(resolvedCommands, cancellationToken);
    }

    private void RemoveExistingResolvedCommands(string externalId)
    {
        dbContext.ResolvedCommands.RemoveRange(dbContext.ResolvedCommands.Where(x => x.RequestedCommand.ExternalId == externalId));
    }

    public async Task<List<RequestedCommand>> GetOverlappingRequestedCommandsAsync(Guid id,
                                                                                   CancellationToken cancellationToken = default)
    {
        var command = await dbContext.RequestedCommands.SingleOrDefaultAsync(x => x.Id == id, cancellationToken) ??
            throw new InvalidOperationException("Requested command not found");

        //TODO: Use repository to fetch only the future commands for ExternalId
        var existingRequestedCommands = dbContext.RequestedCommands
       .Where(command1 => dbContext.RequestedCommands.Any(command2 => command2.ExternalId == command.ExternalId
                      && command2.Status == RequestedCommandStatus.Approved
                      && command1.ExternalId == command.ExternalId
                      && command1.Status == RequestedCommandStatus.Approved
                      && command1.StartTime < command2.EndTime
                      && command1.EndTime > command2.StartTime
                      && command1 != command2))
       .Distinct()
       .ToList();

        return existingRequestedCommands;
    }

    public async Task<List<RequestedCommand>> GetOverlappingRequestedCommandsAsync(string externalId,
                                                                                   DateTimeOffset startTime,
                                                                                   DateTimeOffset? endTime,
                                                                                   CancellationToken cancellationToken = default)
    {
        // TODO: Need to check this logic
        var existingRequestedCommands = await dbContext.RequestedCommands.Where(x =>
                                                                            ((x.StartTime >= startTime &&
                                                                                    endTime.HasValue &&
                                                                                    x.StartTime <= endTime.Value) ||
                                                                            (x.EndTime != null &&
                                                                                    endTime.HasValue &&
                                                                                    x.EndTime <= endTime.Value)) &&
                                                                            x.Status == RequestedCommandStatus.Approved &&
                                                                            x.ExternalId == externalId)
                                                                          .ToListAsync(cancellationToken: cancellationToken);
        return existingRequestedCommands;
    }

    public async Task<List<RequestedCommand>> GetOverlappingRequestedCommandsAsync(Guid requestedCommandId,
                                                                                   string externalId,
                                                                                   DateTimeOffset startTime,
                                                                                   DateTimeOffset? endTime,
                                                                                   CancellationToken cancellationToken = default)
    {
        // TODO: This looks wrong "x.EndTime >= endTime.Value && x.EndTime <= endTime.Value"
        var existingRequestedCommands = await dbContext.RequestedCommands.Where(x =>
                                                                    x.StartTime >= startTime &&
                                                                            endTime.HasValue &&
                                                                            x.StartTime <= endTime.Value &&
                                                                    x.EndTime != null &&
                                                                            endTime.HasValue &&
                                                                            x.EndTime >= endTime.Value &&
                                                                            x.EndTime <= endTime.Value &&
                                                                    x.Status == RequestedCommandStatus.Approved &&
                                                                    x.ExternalId == externalId &&
                                                                    x.Id != requestedCommandId)
                                                                  .ToListAsync(cancellationToken: cancellationToken);
        return existingRequestedCommands;
    }

    public async Task<bool> ApproveRequestedCommandAsync(Guid id, CancellationToken cancellationToken)
    {
        var success = true;
        try
        {
            await dbTransactions.RunAsync(dbContext, async _ =>
            {
                var requested = await dbContext.RequestedCommands.SingleOrDefaultAsync(x => x.Id == id, cancellationToken) ??
                    throw new InvalidOperationException("Requested command not found");

                ValidateCommandStatus(requested);

                //TODO: Resolve the RequestedCommand using IConflictResolutionManager
                var resolved = requested.CreateResolvedCommand();
                await dbContext.ResolvedCommands.AddAsync(resolved);
                await dbContext.SaveChangesAsync(cancellationToken);
                return true;
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error approving the requested commanded: {message}", ex.Message);
            throw;
        }

        return success;
    }

    public async Task<bool> RejectRequestedCommandAsync(Guid id, CancellationToken cancellationToken)
    {
        var command = await dbContext.RequestedCommands.SingleOrDefaultAsync(x => x.Id == id, cancellationToken) ??
            throw new InvalidOperationException("Requested command not found");

        if (command.Status == RequestedCommandStatus.Approved)
        {
            logger.LogError("Approved commands cant be rejected");
            return false;
        }

        if (command.Status == RequestedCommandStatus.Rejected)
        {
            logger.LogError("RequestedCommand has already been rejected");
            return false;
        }

        command.Status = RequestedCommandStatus.Rejected;
        var result = await dbContext.SaveChangesAsync(cancellationToken);
        return result == 1;
    }

    private void ValidateCommandStatus(RequestedCommand requestedCommand)
    {
        if (requestedCommand.Status == RequestedCommandStatus.Rejected)
        {
            logger.LogError("Rejected commands cant be approved");
            throw new Exception("Rejected commands cant be approved");
        }

        if (requestedCommand.Status == RequestedCommandStatus.Approved)
        {
            logger.LogError("RequestedCommand has already been approved");
            throw new Exception("RequestedCommand has already been approved");
        }

        requestedCommand.Status = RequestedCommandStatus.Approved;
    }

    public async Task ExecuteResolvedCommandAsync(Guid id, CancellationToken cancellationToken)
    {
        ResolvedCommand? resolvedCommand = await dbContext.ResolvedCommands.Include(x => x.RequestedCommand).FirstOrDefaultAsync(x => x.Id == id, cancellationToken) ??
            throw new InvalidOperationException("Resolved Command not found");

        await UpdateResolvedCommandAsync(resolvedCommand, ResolvedCommandStatus.Executing, cancellationToken);
        SetValueResponse result;
        try
        {
            result = await InvokeMappedGatewayServiceWithRetryAsync(resolvedCommand.RequestedCommand.ExternalId, resolvedCommand.RequestedCommand.Value);
            if (result is not null && result.StatusCode == HttpStatusCode.OK)
            {
                await UpdateResolvedCommandAsync(resolvedCommand, ResolvedCommandStatus.Executed, cancellationToken);
            }
            else
            {
                await UpdateResolvedCommandAsync(resolvedCommand, ResolvedCommandStatus.Failed, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            await UpdateResolvedCommandAsync(resolvedCommand, ResolvedCommandStatus.Failed, cancellationToken);
            logger.LogError(ex, "Error executing command to Mapped Gateway");
        }
        finally
        {
            //TODO: Save failure result into MappedRequestLog
        }
    }

    private async Task<SetValueResponse> InvokeMappedGatewayServiceWithRetryAsync(string pointId, double value)
    {
        var policy = Policy
            .Handle<Exception>()
            .OrResult<SetValueResponse>(x => x.StatusCode != HttpStatusCode.OK)
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        return await policy.ExecuteAsync(async () =>
        {
            return await mappedGatewayService.SendSetValueCommandAsync(pointId, value);
        });
    }

    private async Task UpdateResolvedCommandAsync(ResolvedCommand command, ResolvedCommandStatus status, CancellationToken cancellationToken = default)
    {
        command.Status = status;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<ResolvedCommand>> GetDueResolvedCommandsAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.Now;
        Expression<Func<ResolvedCommand, bool>> predicate = command =>
        !command.IsDeleted &&
        command.Status != ResolvedCommandStatus.Executed && command.Status != ResolvedCommandStatus.Cancelled &&
        command.Status != ResolvedCommandStatus.Executing &&
        now > command.StartTime &&
        command.EndTime.HasValue && now < command.EndTime;

        var list = await dbContext.ResolvedCommands.Where(predicate).Paginate(1, 50);
        return list.Items.Where(x => x.IsExpired).ToList();
    }
}
