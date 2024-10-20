namespace Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateDeployment;

using MediatR;
using Willow.IoTService.Deployment.Common;
using Willow.IoTService.Deployment.Dashboard.Application.HealthChecks;
using Willow.IoTService.Deployment.Dashboard.Application.PortServices;
using Willow.IoTService.Deployment.DataAccess.PortService;
using Willow.IoTService.Deployment.DataAccess.Services;
using Willow.IoTService.WebApiErrorHandling.Contracts;

public class CreateDeploymentHandler(
    IDeploymentDataService deploymentService,
    IDeployModuleService deployModuleService,
    IUserInfoService userInfoService,
    HealthCheckSql healthCheckSql)
    : IRequestHandler<CreateDeploymentCommand, DeploymentDto>
{
    public async Task<DeploymentDto> Handle(CreateDeploymentCommand request, CancellationToken cancellationToken)
    {
        var input = new DeploymentCreateInput(
            request.ModuleId,
            DeploymentStatus.Pending.ToString(),
            string.Empty,
            request.Version,
            userInfoService.GetUserName(),
            DateTimeOffset.UtcNow);

        DeploymentDto deploymentDto;
        try
        {
            deploymentDto = await deploymentService.CreateAsync(input, cancellationToken);
            healthCheckSql.Current = HealthCheckSql.Healthy;
        }
        catch (ArgumentNullException)
        {
            healthCheckSql.Current = HealthCheckSql.FailingCalls;
            throw new NotFoundException("Cannot find related module");
        }

        await deployModuleService.SendDeployModuleMessageAsync(
            deploymentDto.Id,
            request.ModuleId,
            request.Version,
            request.ContainerConfigs,
            BaseModuleDeploymentHelper.IsBaseDeployment(deploymentDto.ModuleType),
            cancellationToken);

        await deployModuleService.SendStatusAsync(
            deploymentDto.Id,
            deploymentDto.ModuleId,
            DeploymentStatus.Pending,
            cancellationToken: cancellationToken);

        return deploymentDto;
    }
}
