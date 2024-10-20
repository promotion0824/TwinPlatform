namespace Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateBatchDeployments;

using MediatR;
using Willow.IoTService.Deployment.Common;
using Willow.IoTService.Deployment.Dashboard.Application.HealthChecks;
using Willow.IoTService.Deployment.Dashboard.Application.PortServices;
using Willow.IoTService.Deployment.DataAccess.PortService;
using Willow.IoTService.Deployment.DataAccess.Services;

public class CreateBatchDeploymentsHandler(
    IUserInfoService userInfoService,
    IDeploymentDataService dataService,
    IDeployModuleService deployModuleService,
    HealthCheckSql healthCheckSql)
    : IRequestHandler<CreateBatchDeploymentsCommand, IEnumerable<Guid>>
{
    public async Task<IEnumerable<Guid>> Handle(CreateBatchDeploymentsCommand request, CancellationToken cancellationToken)
    {
        var commands = request.CreateDeploymentCommands.ToList();
        var inputs = commands.Select(x => new DeploymentCreateInput(x.ModuleId,
                                                                    DeploymentStatus.Pending.ToString(),
                                                                    string.Empty,
                                                                    x.Version,
                                                                    userInfoService.GetUserName(),
                                                                    DateTimeOffset.UtcNow));

        try
        {
            var dtos = await dataService.CreateMultipleAsync(inputs, cancellationToken);
            healthCheckSql.Current = HealthCheckSql.Healthy;

            // we only allow distinct module ids, so we can use it to map the deployment dto to the command
            var dict = dtos.ToDictionary(dto => dto, dto => commands.First(c => c.ModuleId == dto.ModuleId));
            foreach (var (deploymentDto, command) in dict)
            {
                await deployModuleService.SendDeployModuleMessageAsync(deploymentDto.Id,
                                                                       deploymentDto.ModuleId,
                                                                       command.Version,
                                                                       command.ContainerConfigs,
                                                                       BaseModuleDeploymentHelper.IsBaseDeployment(deploymentDto.ModuleType),
                                                                       cancellationToken);

                await deployModuleService.SendStatusAsync(deploymentDto.Id,
                                                          deploymentDto.ModuleId,
                                                          DeploymentStatus.Pending,
                                                          cancellationToken: cancellationToken);
            }

            return dict.Keys.Select(x => x.Id);
        }
        catch (Exception)
        {
            healthCheckSql.Current = HealthCheckSql.FailingCalls;
            throw;
        }
    }
}
