namespace Willow.CommandAndControl.Application.Requests.ResolvedCommand.GetResolvedCommandById;

internal class GetResolvedCommandByIdValidator : AbstractValidator<GetResolvedCommandByIdRequestDto>
{
    public GetResolvedCommandByIdValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
