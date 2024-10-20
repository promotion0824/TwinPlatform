namespace Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateDeployment;

using FluentValidation;

public class CreateDeploymentValidator : AbstractValidator<CreateDeploymentCommand>
{
    public CreateDeploymentValidator()
    {
        this.RuleFor(c => c.ModuleId)
            .NotEmpty();
        this.RuleFor(c => c.Version)
            .NotEmpty();
    }
}
