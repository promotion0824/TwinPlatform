namespace Willow.CommandAndControl.Application.Requests.RequestedCommand.PostRequestedCommands;

using RequestedCommand = Willow.CommandAndControl.Data.Models.RequestedCommand;

internal class PostRequestedCommandsHandler
{
    internal static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync([FromBody] PostRequestedCommandsDto request,
        IApplicationDbContext dbContext,
        IDbTransactions dbTransactions,
        ILogger<PostRequestedCommandsHandler> logger,
        IActivityLogger activityLogger,
        ITwinInfoService twinInfoService,
        CancellationToken cancellationToken = default)
    {
        var twinIds = request.Commands.Select(x => x.TwinId).Distinct().ToList();
        var twinInfos = await twinInfoService.GetTwinInfoAsync(twinIds);

        var (validCommands, invalidCommands) = ValidateCommands(request.Commands, twinInfos);

        if (invalidCommands.Count != 0)
        {
            return TypedResults.Problem(title: "One or more validation errors occurred.",
                extensions: new Dictionary<string, object?>
                {
                    { "errors", new PostRequestedCommandsResponseDto(invalidCommands) },
                },
                statusCode: StatusCodes.Status400BadRequest);
        }

        // Get grouped result by external id so that we can easily resolve conflicting commands
        var commandsGroup = MapToRequestedCommandsGroup(validCommands, twinInfos);
        var commandGroupKeys = commandsGroup.Select(y => y.Key);

        //TODO: Find more efficient way to do this as a bulk operation
        var existingCommandsDictionary = await dbContext.RequestedCommands
            .AsNoTracking()
            .Join(commandGroupKeys, command => command.TwinId, key => key, (command, key) => command)
            .GroupBy(x => x.TwinId)
            .ToDictionaryAsync(x => x.Key, x => x.ToList(), cancellationToken);

        await dbTransactions.RunAsync(dbContext, async _ =>
        {
            foreach (var requestedCommands in commandsGroup)
            {
                var group = existingCommandsDictionary.GetValueOrDefault(requestedCommands.Key);
                foreach (var requestedCommand in requestedCommands.Value)
                {
                    var command = group?.FirstOrDefault(x => IsMatchingCommand(x, requestedCommand));

                    // Rules Engine sends command every 15 min. We only create a new command if there is a change in
                    // value or Type or if the command is outside the bound of existing command's start and end time.
                    if (command is null)
                    {
                        dbContext.RequestedCommands.Add(requestedCommand);
                        await activityLogger.LogAsync(ActivityType.Received, requestedCommand, cancellationToken);
                        if (group == null)
                        {
                            group = new List<RequestedCommand>();
                            existingCommandsDictionary.Add(requestedCommand.TwinId, group);
                        }

                        group.Add(requestedCommand);
                        continue;
                    }

                    if (command.Status == RequestedCommandStatus.Approved)
                    {
                        //TODO: Run conflict resolution
                    }

                    // Only create an activity log entry if the value has changed.f
                    // TODO: We don't have a way of tracking value changes, the log screen only shows the most recent value.
                    /*if (valueChanged)
                    {
                        await activityLogger.LogAsync(ActivityType.Received, command, cancellationToken);
                    }*/

                    //TODO: Uncomment the below section once we are confident to use auto resolver/approver
                    // if status is approved we don't have to reapprove it and can directly generate resolved commands
                    //if (command.Status == RequestedCommandStatus.Approved)
                    //{
                    //  commandsToBeResolved.Add(requestedCommand);
                    //}
                }

                // the conflict resolution is done to fix multiple rules trying to apply different values to same point or ExternalId
                //if (commandsToBeResolved.Any())
                //{
                //  await commandManager.ResolveAsync(requestedCommands.Key, valueChanged, commandsToBeResolved, cancellationToken);
                //}
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        });

        return TypedResults.NoContent();
    }

    private static (List<RequestedCommandDto> ValidCommands, List<InvalidRequestCommand> InvalidCommands) ValidateCommands(List<RequestedCommandDto> commands, IDictionary<string, TwinInfoModel> twinInfos)
    {
        var twins = twinInfos.Values.ToList();
        var externalIdMaps = twins.Select(twin => twin.ExternalId).ToHashSet();
        var twinIdMaps = twins.Select(twin => twin.TwinId).ToHashSet();
        var connectorIdMaps = twins.Select(twin => twin.ConnectorId).ToHashSet();

        var validCommands = new List<RequestedCommandDto>();
        var invalidCommands = new List<InvalidRequestCommand>();

        for (var index = 0; index < commands.Count; index++)
        {
            var command = commands[index];
            var errors = new List<RequestCommandError>();

            if (!twinIdMaps.Contains(command.TwinId))
            {
                errors.Add(new RequestCommandError
                {
                    Property = nameof(command.TwinId),
                    Value = command.TwinId,
                    Message = $"{nameof(command.TwinId)} is not valid.",
                });
            }

            if (!externalIdMaps.Contains(command.ExternalId))
            {
                errors.Add(new RequestCommandError
                {
                    Property = nameof(command.ExternalId),
                    Value = command.ExternalId,
                    Message = $"{nameof(command.ExternalId)} is not valid.",
                });
            }

            if (!connectorIdMaps.Contains(command.ConnectorId))
            {
                errors.Add(new RequestCommandError
                {
                    Property = nameof(command.ConnectorId),
                    Value = command.ConnectorId,
                    Message = $"{nameof(command.ConnectorId)} is not valid.",
                });
            }

            if (errors.Count != 0)
            {
                invalidCommands.Add(new InvalidRequestCommand
                {
                    Field = $"Commands[{index}]",
                    Errors = errors,
                });
                continue;
            }

            validCommands.Add(command);
        }

        return (validCommands, invalidCommands);
    }

    private static bool IsMatchingCommand(RequestedCommand command, RequestedCommand requestedCommand)
    {
        return command.RuleId == requestedCommand.RuleId
            && command.Value == requestedCommand.Value
            && command.Type == requestedCommand.Type
            && requestedCommand.StartTime >= command.StartTime
            && (command.EndTime == null || requestedCommand.EndTime <= command.EndTime);
    }

    private static Dictionary<string, IGrouping<string, RequestedCommand>> MapToRequestedCommandsGroup(
        IEnumerable<RequestedCommandDto> request,
        IDictionary<string, TwinInfoModel> twinInfos)
    {
        return request.Select(x =>
        {
            twinInfos.TryGetValue(x.TwinId, out var twinInfo);
            return new RequestedCommand
            {
                CommandName = x.CommandName,
                Type = x.Type,
                TwinId = x.TwinId,
                ExternalId = x.ExternalId,
                RuleId = x.RuleId,
                Value = x.Value,
                Unit = x.Unit,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                ReceivedDate = DateTimeOffset.UtcNow,
                ConnectorId = twinInfo?.ConnectorId ?? x.ConnectorId,
                IsCapabilityOf = twinInfo?.IsCapabilityOf ?? string.Empty,
                IsHostedBy = twinInfo?.IsHostedBy ?? string.Empty,
                Location = twinInfo?.Location ?? string.Empty,
                SiteId = twinInfo?.SiteId ?? string.Empty,
                Locations = x.Relationships?.Select((c, index) => new LocationTwin
                {
                    Order = index,
                    TwinName = c.TwinName,
                    LocationTwinName = c.TwinId,
                    LocationModelName = c.ModelId,
                }).ToList() ?? [],
            };
        }).GroupBy(x => x.TwinId)
        .ToDictionary(x => x.Key, x => x);
    }
}
