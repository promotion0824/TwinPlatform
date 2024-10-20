namespace Willow.IoTService.Deployment.Dashboard.Application.Commands.CreateModuleTypeDeployments;

using System.Threading.Tasks.Dataflow;
using MediatR;
using Microsoft.Extensions.Logging;
using Willow.IoTService.Deployment.Common;
using Willow.IoTService.Deployment.Dashboard.Application.HealthChecks;
using Willow.IoTService.Deployment.Dashboard.Application.PortServices;
using Willow.IoTService.Deployment.DataAccess.PortService;
using Willow.IoTService.Deployment.DataAccess.Services;
using Willow.IoTService.WebApiErrorHandling.Contracts;

public class CreateModuleTypeDeploymentsHandler(
    IModuleDataService moduleDataService,
    IDeploymentDataService deploymentDataService,
    IDeployModuleService deployModuleService,
    IUserInfoService userInfoService,
    ILogger<CreateModuleTypeDeploymentsHandler> logger,
    HealthCheckSql healthCheckSql)
    : IRequestHandler<CreateModuleTypeDeploymentsCommand, CreateModuleTypeDeploymentsResponse>
{
    private const int CreateBatchSize = 50;
    private readonly BufferBlock<ModuleDto> moduleBuffer = new();

    public async Task<CreateModuleTypeDeploymentsResponse> Handle(
        CreateModuleTypeDeploymentsCommand request,
        CancellationToken cancellationToken)
    {
        var versions = await moduleDataService.GetModuleTypeVersionsAsync(request.ModuleType, cancellationToken);
        if (!versions.Any(x => x.Equals(request.Version)))
        {
            throw new NotFoundException("Module type version not found");
        }

        var searchModulesTask = this.SearchModulesAsync(request.ModuleType, cancellationToken);
        var createDeploymentsTask = this.CreateDeploymentsAsync(request.Version, cancellationToken);
        await searchModulesTask;
        var count = await createDeploymentsTask;

        return new CreateModuleTypeDeploymentsResponse(count);
    }

    private async Task<int> CreateDeploymentsAsync(string version, CancellationToken cancellationToken)
    {
        var moduleToSend = new List<ModuleDto>();
        var count = 0;
        while (await this.moduleBuffer.OutputAvailableAsync(cancellationToken))
        {
            var module = await this.moduleBuffer.ReceiveAsync(cancellationToken);
            moduleToSend.Add(module);
            if (moduleToSend.Count < CreateBatchSize)
            {
                continue;
            }

            try
            {
                var success = await this.CreateDeploymentsBatchAsync(
                    version,
                    cancellationToken,
                    moduleToSend);
                count += success;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while creating deployments");
            }
            finally
            {
                moduleToSend.Clear();
            }
        }

        if (moduleToSend.Any())
        {
            try
            {
                var success = await this.CreateDeploymentsBatchAsync(
                    version,
                    cancellationToken,
                    moduleToSend);
                count += success;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while creating deployments");
            }
        }

        return count;
    }

    private async Task<int> CreateDeploymentsBatchAsync(
        string version,
        CancellationToken cancellationToken,
        IEnumerable<ModuleDto> modules)
    {
        var count = 0;
        var creatInputs = modules.Select(
            x => new DeploymentCreateInput(
                x.Id,
                DeploymentStatus.Pending.ToString(),
                string.Empty,
                version,
                userInfoService.GetUserName(),
                DateTimeOffset.UtcNow));
        try
        {
            var deployments = await deploymentDataService.CreateMultipleAsync(creatInputs, cancellationToken);
            healthCheckSql.Current = HealthCheckSql.Healthy;
            foreach (var deployment in deployments)
            {
                await deployModuleService.SendDeployModuleMessageAsync(
                    deployment.Id,
                    deployment.ModuleId,
                    version,
                    isBaseDeployment: BaseModuleDeploymentHelper.IsBaseDeployment(deployment.ModuleType),
                    cancellationToken: cancellationToken);

                await deployModuleService.SendStatusAsync(
                    deployment.Id,
                    deployment.ModuleId,
                    DeploymentStatus.Pending,
                    cancellationToken: cancellationToken);
                count++;
            }

            return count;
        }
        catch (Exception)
        {
            healthCheckSql.Current = HealthCheckSql.FailingCalls;
            throw;
        }
    }

    // search all modules by module type with pagination, send to buffer
    private async Task SearchModulesAsync(string moduleType, CancellationToken cancellationToken)
    {
        var totalCount = 0;
        var currentPage = 1;
        var total = 0;
        do
        {
            PagedResult<(ModuleDto, IEnumerable<DeploymentDto>?)> current;
            try
            {
                current = await moduleDataService.SearchAsync(
                    new ModuleSearchInput(ModuleType: moduleType, Page: currentPage),
                    cancellationToken);
                healthCheckSql.Current = HealthCheckSql.Healthy;
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Error while getting modules by module type");
                healthCheckSql.Current = HealthCheckSql.FailingCalls;
                continue;
            }

            totalCount = current.TotalCount;
            foreach (var (module, _) in current.Items)
            {
                await this.moduleBuffer.SendAsync(module, cancellationToken);
                total++;
            }

            currentPage++;
        }
        while (total < totalCount);

        this.moduleBuffer.Complete();
    }
}
