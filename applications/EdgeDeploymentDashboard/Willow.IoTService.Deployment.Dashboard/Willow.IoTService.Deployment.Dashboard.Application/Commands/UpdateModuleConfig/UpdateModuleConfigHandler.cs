namespace Willow.IoTService.Deployment.Dashboard.Application.Commands.UpdateModuleConfig;

using MediatR;
using Willow.IoTService.Deployment.Dashboard.Application.HealthChecks;
using Willow.IoTService.Deployment.DataAccess.Services;
using Willow.IoTService.WebApiErrorHandling.Contracts;

public class UpdateModuleConfigHandler(IModuleDataService moduleService, HealthCheckSql healthCheckSql)
    : IRequestHandler<UpdateModuleConfigCommand, ModuleDto>
{
    public async Task<ModuleDto> Handle(UpdateModuleConfigCommand request, CancellationToken cancellationToken)
    {
        var input = new ModuleUpdateConfigurationInput(
            request.Id,
            request.IsAutoDeploy,
            request.DeviceName,
            request.IoTHubName,
            request.Environment,
            request.Platform);

        try
        {
            var result = await moduleService.UpdateConfigurationAsync(input, cancellationToken);
            healthCheckSql.Current = HealthCheckSql.Healthy;
            return result;
        }
        catch (ArgumentNullException)
        {
            healthCheckSql.Current = HealthCheckSql.FailingCalls;
            throw new NotFoundException("Cannot find module");
        }
    }
}
