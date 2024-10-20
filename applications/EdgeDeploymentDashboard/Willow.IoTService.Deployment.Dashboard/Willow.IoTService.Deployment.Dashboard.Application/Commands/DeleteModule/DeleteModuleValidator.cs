namespace Willow.IoTService.Deployment.Dashboard.Application.Commands.DeleteModule;

using FluentValidation;

public class DeleteModuleValidator : AbstractValidator<DeleteModuleCommand>
{
    public DeleteModuleValidator()
    {
        this.RuleFor(x => x.Id).NotEmpty();
    }
}
