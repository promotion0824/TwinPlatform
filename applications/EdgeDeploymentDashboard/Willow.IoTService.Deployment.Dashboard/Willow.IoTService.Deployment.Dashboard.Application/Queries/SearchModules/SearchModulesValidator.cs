namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.SearchModules;

using FluentValidation;

public class SearchModulesValidator : AbstractValidator<SearchModulesQuery>
{
    private const int MinCharSearchSize = 2;

    public SearchModulesValidator()
    {
        this.RuleFor(x => x.Name)
            .MinimumLength(MinCharSearchSize)
            .When(x => string.IsNullOrWhiteSpace(x.Name));
        this.RuleFor(x => x.ModuleType)
            .MinimumLength(MinCharSearchSize)
            .When(x => string.IsNullOrWhiteSpace(x.ModuleType));
        this.RuleFor(x => x.DeviceName)
            .MinimumLength(MinCharSearchSize)
            .When(x => string.IsNullOrWhiteSpace(x.DeviceName));
        this.RuleFor(x => x.Page)
            .GreaterThan(0);
        this.RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PagedQuery.MaxPageSize);
    }
}
