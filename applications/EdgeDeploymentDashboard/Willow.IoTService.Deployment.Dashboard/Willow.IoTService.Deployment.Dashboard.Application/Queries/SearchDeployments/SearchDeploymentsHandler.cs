namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.SearchDeployments;

using MediatR;
using Willow.IoTService.Deployment.DataAccess.Services;

public class SearchDeploymentsHandler(IDeploymentDataService deploymentService)
    : IRequestHandler<SearchDeploymentsQuery, PagedResult<DeploymentDto>>
{
    public Task<PagedResult<DeploymentDto>> Handle(SearchDeploymentsQuery request, CancellationToken cancellationToken)
    {
        return deploymentService.SearchAsync(
            new DeploymentSearchInput(
                request.Ids,
                request.ModuleId,
                request.DeviceName,
                request.Page,
                request.PageSize),
            cancellationToken);
    }
}
