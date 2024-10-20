namespace Willow.IoTService.Deployment.Service.Application.Deployments;

using Ardalis.GuardClauses;
using Azure.Core;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Contrib.WaitAndRetry;

internal sealed class DeploymentService : IDeploymentService, IDisposable
{
    private const int MinDeployments = 1;
    private const int MaxDeployments = 5;
    private const int IotHubMaxDeployments = 100;
    private const string ModuleTypeLabelName = "moduleType";
    private const string ModuleNameLabelName = "moduleName";
    private const int RetryCount = 3;
    private static readonly TimeSpan MedianRetryDelay = TimeSpan.FromSeconds(5);
    private readonly ILogger<DeploymentService> logger;
    private readonly RegistryManager manager;

    internal DeploymentService(RegistryManager manager, ILogger<DeploymentService> logger)
    {
        this.Hostname = string.Empty;
        this.manager = manager;
        this.logger = logger;
    }

    internal DeploymentService(string hostname, ILogger<DeploymentService> logger, TokenCredential tokenCredential)
    {
        this.Hostname = hostname;
        this.logger = logger;
        this.logger.LogInformation("Creating RegistryManager for {Hostname}", hostname);
        this.manager = RegistryManager.Create(hostname, tokenCredential);
    }

    public string Hostname { get; }

    public async Task<string> DeployAsync(DeploymentConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var added = await this.manager.AddConfigurationAsync(configuration.Configuration, cancellationToken);
        this.logger.LogInformation("Added configuration id {Id}", added.Id);
        try
        {
            // if there are multiple configuration being applied to the same device
            // DeviceAlreadyExistsExceptions will be thrown
            // in this case we will retry the deployment 3 times
            // with a median delay of 5 seconds using Jittered Back-off
            var delay = Backoff.DecorrelatedJitterBackoffV2(MedianRetryDelay, RetryCount);
            var retryPolicy = Policy.Handle<DeviceAlreadyExistsException>()
                                    .WaitAndRetryAsync(delay);

            await retryPolicy.ExecuteAsync(
                                           async () =>
                                           {
                                               await this.manager.ApplyConfigurationContentOnDeviceAsync(
                                                                                                         configuration.DeviceId,
                                                                                                         added.Content,
                                                                                                         cancellationToken);
                                           });
            this.logger.LogInformation(
                                       "Applied configuration id {Id} on device {DeviceId}",
                                       added.Id,
                                       configuration.DeviceId);
        }
        catch (Exception)
        {
            // try to clean added configuration due to any exception when applying
            await this.manager.RemoveConfigurationAsync(added.Id, cancellationToken);
            this.logger.LogInformation(
                                       "Apply operation on device {DeviceId} failed, configuration removed, id {ConfigurationId}",
                                       configuration.DeviceId,
                                       added.Id);
            throw;
        }

        return added.Id;
    }

    public Task RemoveHistoricalDeploymentsAsync(
        string configurationId,
        int deploymentsLeft = 2,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.OutOfRange(
                                 deploymentsLeft,
                                 nameof(deploymentsLeft),
                                 MinDeployments,
                                 MaxDeployments);

        return this.RemoveHistoricalDeploymentsInternalAsync(
                                                             configurationId,
                                                             deploymentsLeft,
                                                             cancellationToken);
    }

    public void Dispose()
    {
        this.manager.Dispose();
    }

    private async Task RemoveHistoricalDeploymentsInternalAsync(
        string configurationId,
        int deploymentsLeft,
        CancellationToken cancellationToken = default)
    {
        var configurations = (await this.manager.GetConfigurationsAsync(IotHubMaxDeployments, cancellationToken)).ToList();
        var matching = configurations.FirstOrDefault(x => x.Id == configurationId);
        if (matching is null || !matching.Labels.TryGetValue(ModuleTypeLabelName, out var moduleType) ||
            !matching.Labels.TryGetValue(ModuleNameLabelName, out var moduleName))
        {
            return;
        }

        this.logger.LogInformation(
                                   "Removing historical deployments for target condition {TargetCondition}, moduleType {ModuleType}, moduleName {ModuleName}",
                                   matching.TargetCondition,
                                   moduleType,
                                   moduleName);
        var historical = configurations.Where(
                                              x =>

                                                  // same device
                                                  x.TargetCondition == matching.TargetCondition &&

                                                  // same module type
                                                  x.Labels.TryGetValue(ModuleTypeLabelName, out var type) &&
                                                  type == moduleType &&
                                                  x.Labels.TryGetValue(ModuleNameLabelName, out var name) &&
                                                  name == moduleName)
                                       .OrderByDescending(x => x.LastUpdatedTimeUtc)
                                       .Skip(deploymentsLeft);

        var count = 0;
        foreach (var config in historical)
        {
            try
            {
                await this.manager.RemoveConfigurationAsync(config, cancellationToken);
                count++;
            }
            catch (Exception e)
            {
                this.logger.LogWarning(
                                       e,
                                       "Failed to remove configuration {ConfigurationId} from {Host}",
                                       config.Id,
                                       this.Hostname);
            }
        }

        this.logger.LogInformation("Removed {Count} historical deployments", count);
    }
}
