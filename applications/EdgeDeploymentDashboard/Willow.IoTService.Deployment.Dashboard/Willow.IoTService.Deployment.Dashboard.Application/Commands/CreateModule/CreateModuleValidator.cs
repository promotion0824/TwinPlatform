namespace Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateModule;

using FluentValidation;

public class CreateModuleValidator : AbstractValidator<CreateModuleCommand>
{
    public CreateModuleValidator()
    {
        this.RuleFor(c => c.Name)
            .NotEmpty();
        this.RuleFor(c => c.ApplicationType)
            .NotEmpty()
            .When(c => !c.IsBaseModule)
            .WithMessage("'Application Type' must not be empty when 'Is Base Module' is false.");
    }
}
