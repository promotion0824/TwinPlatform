namespace Willow.CommandAndControl.Application.Requests.RequestedCommand.GetConflictingCommandsByTwinId;

internal class GetConflictingCommandsByTwinIdValidator : AbstractValidator<GetConflictingCommandsByTwinIdDto>
{
    public GetConflictingCommandsByTwinIdValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
