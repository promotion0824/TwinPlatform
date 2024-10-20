namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.SearchDeployments;

using MediatR;
using Willow.IoTService.Deployment.DataAccess.Services;

public record SearchDeploymentsQuery(
    IEnumerable<Guid>? Ids,
    Guid? ModuleId = null,
    string? DeviceName = null,
    int Page = 1,
    int PageSize = 20) : PagedQuery(Page, PageSize), IRequest<PagedResult<DeploymentDto>>;
