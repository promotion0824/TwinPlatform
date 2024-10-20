namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.GetModuleTypeTemplate;

using FluentValidation;

public class GetModuleTypeTemplateValidator : AbstractValidator<GetModuleTypeTemplateQuery>
{
    public GetModuleTypeTemplateValidator()
    {
        this.RuleFor(x => x.Version)
           .NotEmpty();

        this.RuleFor(x => x.ModuleType)
           .NotEmpty();
    }
}
