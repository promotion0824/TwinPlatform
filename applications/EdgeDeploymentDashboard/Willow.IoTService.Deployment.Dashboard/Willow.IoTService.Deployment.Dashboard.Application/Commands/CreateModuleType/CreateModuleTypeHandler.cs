namespace Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateModuleType;

using MediatR;
using Willow.IoTService.Deployment.Dashboard.Application.HealthChecks;
using Willow.IoTService.Deployment.DataAccess.Services;
using Willow.IoTService.Deployment.ManifestStorage;
using Willow.IoTService.WebApiErrorHandling.Contracts;

public class CreateModuleTypeHandler(
    IManifestStorageService manifestStorageService,
    IModuleDataService moduleDataService,
    HealthCheckSql healthCheckSql)
    : IRequestHandler<CreateModuleTypeCommand, string>
{
    public async Task<string> Handle(CreateModuleTypeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = request.Content.OpenReadStream();
            var (containerName, blobName) = await manifestStorageService.UploadTemplateAsync(
                request.ModuleType,
                request.Version,
                stream,
                false,
                cancellationToken);
            try
            {
                await moduleDataService.AddModuleTypeVersionsAsync(
                    request.ModuleType,
                    request.Version,
                    cancellationToken);
                healthCheckSql.Current = HealthCheckSql.Healthy;
            }
            catch (Exception)
            {
                healthCheckSql.Current = HealthCheckSql.FailingCalls;
                throw;
            }

            return $"{containerName}-{blobName}";
        }
        catch (TemplateExistsException)
        {
            throw new RequestInvalidException($"Template with version {request.Version} already exists");
        }
    }
}
