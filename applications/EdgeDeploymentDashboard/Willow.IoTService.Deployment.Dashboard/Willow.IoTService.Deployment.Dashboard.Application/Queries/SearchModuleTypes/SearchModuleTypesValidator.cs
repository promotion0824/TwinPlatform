namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.SearchModuleTypes;

using FluentValidation;

public class SearchModuleTypesValidator : AbstractValidator<SearchModuleTypesQuery>
{
    private const int MinCharSearchSize = 2;

    public SearchModuleTypesValidator()
    {
        this.RuleFor(x => x.ModuleType)
            .MinimumLength(MinCharSearchSize)
            .When(x => string.IsNullOrWhiteSpace(x.ModuleType));
        this.RuleFor(x => x.Page)
            .GreaterThan(0);
        this.RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PagedQuery.MaxPageSize);
    }
}
