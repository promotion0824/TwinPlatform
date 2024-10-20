namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.SearchDeployments;

using FluentValidation;

public class SearchDeploymentsValidator : AbstractValidator<SearchDeploymentsQuery>
{
    private const int MinCharSearchSize = 2;

    public SearchDeploymentsValidator()
    {
        this.RuleFor(x => x.DeviceName)
            .MinimumLength(MinCharSearchSize)
            .When(x => string.IsNullOrWhiteSpace(x.DeviceName));
        this.RuleFor(x => x.Page)
            .GreaterThan(0);
        this.RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PagedQuery.MaxPageSize);
    }
}
