namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.GetDeployment;

using MediatR;
using Willow.IoTService.Deployment.DataAccess.Services;
using Willow.IoTService.WebApiErrorHandling.Contracts;

public class GetDeploymentHandler(IDeploymentDataService deploymentDataService) : IRequestHandler<GetDeploymentQuery, DeploymentDto>
{
    public async Task<DeploymentDto> Handle(GetDeploymentQuery request, CancellationToken cancellationToken)
    {
        return await deploymentDataService.GetAsync(request.Id, cancellationToken)
               ?? throw new NotFoundException("Deployment not found");
    }
}
