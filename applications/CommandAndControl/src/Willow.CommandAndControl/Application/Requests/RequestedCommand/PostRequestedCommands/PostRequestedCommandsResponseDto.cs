namespace Willow.CommandAndControl.Application.Requests.RequestedCommand.PostRequestedCommands;

internal record PostRequestedCommandsResponseDto(List<InvalidRequestCommand> InvalidCommands);

internal record RequestCommandError
{
    public required string Property { get; init; }

    public required string Value { get; init; }

    public required string Message { get; init; }
}

internal record InvalidRequestCommand
{
    public required string Field { get; init; }

    public IEnumerable<RequestCommandError> Errors { get; init; } = [];
}
