namespace Willow.CommandAndControl.Application.Requests.RequestedCommand.GetRequestedCommandById;

internal class GetRequestedCommandByIdValidator : AbstractValidator<GetRequestedCommandByIdDto>
{
    public GetRequestedCommandByIdValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
