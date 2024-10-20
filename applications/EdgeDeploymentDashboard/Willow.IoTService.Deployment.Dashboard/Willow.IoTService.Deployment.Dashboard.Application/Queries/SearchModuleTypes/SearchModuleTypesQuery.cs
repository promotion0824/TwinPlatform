namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.SearchModuleTypes;

using MediatR;
using Willow.IoTService.Deployment.DataAccess.Services;

public record SearchModuleTypesQuery(
    string? ModuleType,
    int Page = 1,
    int PageSize = 20) : PagedQuery(Page, PageSize), IRequest<PagedResult<SearchModuleTypeResponse>>;
