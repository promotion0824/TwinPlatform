namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.SearchModules;

using MediatR;
using Willow.IoTService.Deployment.DataAccess.Services;

public record SearchModulesQuery(
    string? Name = null,
    string? ModuleType = null,
    IEnumerable<Guid>? DeploymentIds = null,
    string? DeviceName = null,
    bool? IsArchived = null,
    int Page = 1,
    int PageSize = 20) : PagedQuery(Page, PageSize), IRequest<PagedResult<ModuleDto>>;
