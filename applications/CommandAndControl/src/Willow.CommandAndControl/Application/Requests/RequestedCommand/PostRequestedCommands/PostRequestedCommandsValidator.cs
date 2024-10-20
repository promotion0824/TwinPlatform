namespace Willow.CommandAndControl.Application.Requests.RequestedCommand.PostRequestedCommands;

internal class PostRequestedCommandsValidator : AbstractValidator<PostRequestedCommandsDto>
{
    public PostRequestedCommandsValidator()
    {
        RuleFor(x => x.Commands).NotEmpty();

        RuleForEach(x => x.Commands)
            .ChildRules(command =>
            {
                command.RuleFor(x => x.CommandName).NotEmpty();
                command.RuleFor(x => x.ExternalId).NotEmpty();
                command.RuleFor(x => x.Value).NotNull();
                command.RuleFor(x => x.Unit).NotEmpty();
                command.When(c => c.EndTime.HasValue,
                    () =>
                    {
                        command.RuleFor(c => c).Must(y => y.StartTime <= y.EndTime).WithMessage("Start Time must be less than End Time");
                    });
            });
    }
}
