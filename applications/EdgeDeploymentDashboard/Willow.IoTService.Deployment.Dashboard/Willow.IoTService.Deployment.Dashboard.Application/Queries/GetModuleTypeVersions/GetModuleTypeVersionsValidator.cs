namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.GetModuleTypeVersions;

using FluentValidation;

public class GetModuleTypeVersionsValidator : AbstractValidator<GetModuleTypeVersionsQuery>
{
    public GetModuleTypeVersionsValidator()
    {
        this.RuleFor(x => x.ModuleType)
           .NotEmpty();
    }
}
