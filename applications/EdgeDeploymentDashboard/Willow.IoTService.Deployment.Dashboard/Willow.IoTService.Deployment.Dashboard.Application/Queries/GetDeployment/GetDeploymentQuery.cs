namespace Willow.IoTService.Deployment.Dashboard.Application.Queries.GetDeployment;

using MediatR;
using Willow.IoTService.Deployment.DataAccess.Services;

public record GetDeploymentQuery(Guid Id) : IRequest<DeploymentDto>;
