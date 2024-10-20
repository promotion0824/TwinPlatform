namespace Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateModule;

using Ardalis.GuardClauses;
using MediatR;
using Willow.IoTService.Deployment.Common;
using Willow.IoTService.Deployment.Dashboard.Application.HealthChecks;
using Willow.IoTService.Deployment.DataAccess.Services;

public class CreateModuleHandler(IModuleDataService moduleService, HealthCheckSql healthCheckSql)
    : IRequestHandler<CreateModuleCommand, ModuleDto>
{
    public Task<ModuleDto> Handle(CreateModuleCommand request, CancellationToken cancellationToken)
    {
        var applicationType = request.IsBaseModule
            ? BaseModuleDeploymentHelper.BaseModuleTypeString
            : request.ApplicationType;
        Guard.Against.NullOrEmpty(applicationType);
        var input = new ModuleUpsertInput(
            request.Name,
            applicationType,
            false,
            false);

        try
        {
            var moduleDto = moduleService.UpsertAsync(input, cancellationToken);
            healthCheckSql.Current = HealthCheckSql.Healthy;
            return moduleDto;
        }
        catch (Exception)
        {
            healthCheckSql.Current = HealthCheckSql.FailingCalls;
            throw;
        }
    }
}
