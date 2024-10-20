namespace Willow.IoTService.Deployment.Service.Application.Deployments;

using System.Text;
using FluentValidation;
using MassTransit;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Willow.IoTService.Deployment.Common;
using Willow.IoTService.Deployment.Common.Messages;
using Willow.IoTService.Deployment.DataAccess.Services;
using Willow.IoTService.Deployment.ManifestStorage;
using Willow.IoTService.Deployment.Service.Application.HealthChecks;
using Willow.Telemetry;

/// <summary>
///     Consumer for <see cref="IDeployModule" /> messages.
/// </summary>
/// <param name="logger">Logger.</param>
/// <param name="bus">MassTransit Bus interface.</param>
/// <param name="validator">Fluent Validator.</param>
/// <param name="deploymentServiceFactory">Deployment service implementation.</param>
/// <param name="manifestStorageService">Storage service implementation.</param>
/// <param name="deploymentData">Deployment Database data handler.</param>
/// <param name="moduleData">Module Data service implementation.</param>
/// <param name="contentService">Module config service implementation.</param>
/// <param name="deploymentConfigurationCreator">IoT Hub deployment configuration creator.</param>
/// <param name="metricsCollector">Metrics collector.</param>
/// <param name="healthCheckServiceBus">Healthcheck service for Service Bus.</param>
/// <param name="healthCheckSql">Healthcheck service for SQL.</param>
public sealed class DeployModuleConsumer(
    ILogger<DeployModuleConsumer> logger,
    IPublishEndpoint bus,
    IValidator<IDeployModule> validator,
    IDeploymentServiceFactory deploymentServiceFactory,
    IManifestStorageService manifestStorageService,
    IDeploymentDataService deploymentData,
    IModuleDataService moduleData,
    IModuleConfigContentService contentService,
    IDeploymentConfigurationCreator deploymentConfigurationCreator,
    IMetricsCollector metricsCollector,
    HealthCheckServiceBus healthCheckServiceBus,
    HealthCheckSql healthCheckSql)
    : IConsumer<IDeployModule>
{
    private readonly DeploymentContext deploymentContext = new();
    private const string IncomingRequestDesc = "The number of module deployment requests received";
    private const string SuccessfulRequestDesc = "The number of module deployment requests that were successful";
    private const string FailedRequestDesc = "The number of module deployment requests that failed";

    /// <summary>
    ///     Consumes the service bus message and deploys the module to the IoT Hub.
    /// </summary>
    /// <param name="context">Context containing details of the module to be deployed.</param>
    /// <returns>A <see cref="Task" /> representing consuming the service bus message.</returns>
    public async Task Consume(ConsumeContext<IDeployModule> context)
    {
        // init
        var cancellationToken = context.CancellationToken;
        var request = context.Message;
        deploymentContext.DeploymentId = request.DeploymentId;
        deploymentContext.ModuleId = request.ModuleId;

        healthCheckServiceBus.Current = HealthCheckServiceBus.Healthy;
        metricsCollector.TrackMetric("IncomingRequests", 1, MetricType.Counter, IncomingRequestDesc);
        if (!await ValidateRequest(context.Message, cancellationToken) || !await ValidateThenSetupContextAsync(cancellationToken))
        {
            return;
        }

        logger.LogInformation(
                              "Deployment {DeploymentId} - Module {ModuleId} - Starting",
                              this.deploymentContext.DeploymentId,
                              this.deploymentContext.ModuleId);
        await this.UpdateDeploymentStatusAsync(DeploymentStatus.InProgress, cancellationToken: cancellationToken);

        // pre deployment
        ConfigurationContent content;
        try
        {
            logger.LogInformation("Generating deployment manifest");
            content = await contentService.GetContent(
                                                      new GetConfigContentRequest(this.deploymentContext.Module, request.Version)
                                                      {
                                                          ContainerConfigs = request.ContainerConfigs,
                                                          IsBaseDeployment = request.IsBaseDeployment,
                                                      },
                                                      cancellationToken);

            var manifest = JsonConvert.SerializeObject(content);

            logger.LogInformation("Uploading manifest to blob storage");
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(manifest));
            await manifestStorageService.UploadManifestAsync(
                                                             this.deploymentContext.DeploymentId,
                                                             stream,
                                                             cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Deploy failed generating config");
            metricsCollector.TrackMetric("FailedRequests", 1, MetricType.Counter, FailedRequestDesc);
            await this.UpdateDeploymentStatusAsync(
                                                   DeploymentStatus.Failed,
                                                   message: $"Deploy failed generating config: {e.Message}",
                                                   cancellationToken: cancellationToken);
            return;
        }

        logger.LogInformation(
                              "Deploying module {ModuleId} to IoTHub {IoTHub}, device {Device}",
                              this.deploymentContext.ModuleId,
                              this.deploymentContext.Module.IoTHubName,
                              this.deploymentContext.Module.DeviceName);

        // deploy the module
        var service = deploymentServiceFactory.Create(this.deploymentContext.Module.IoTHubName!);
        DeploymentConfiguration config;
        try
        {
            config = deploymentConfigurationCreator.Create(
                                                           this.deploymentContext.DeploymentId,
                                                           content,
                                                           request.IsBaseDeployment,
                                                           this.deploymentContext.Module.ModuleType,
                                                           this.deploymentContext.Module.DeviceName!,
                                                           this.deploymentContext.Module.Name);
            await service.DeployAsync(config, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Deploy failed applying config");
            metricsCollector.TrackMetric("FailedRequests", 1, MetricType.Counter, FailedRequestDesc);
            await this.UpdateDeploymentStatusAsync(
                                                   DeploymentStatus.Failed,
                                                   message: $"Deploy failed applying config: {e.Message}",
                                                   cancellationToken: cancellationToken);
            return;
        }

        healthCheckSql.Current = HealthCheckSql.Healthy;
        logger.LogInformation(
                              "Deployment {DeploymentId} - Module {ModuleId} - Complete",
                              this.deploymentContext.DeploymentId,
                              this.deploymentContext.ModuleId);
        metricsCollector.TrackMetric("SuccessfulRequests", 1, MetricType.Counter, SuccessfulRequestDesc);
        await this.UpdateDeploymentStatusAsync(
                                               DeploymentStatus.Succeeded,
                                               DateTimeOffset.UtcNow,
                                               cancellationToken: cancellationToken);

        try
        {
            await service.RemoveHistoricalDeploymentsAsync(config.Configuration.Id, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to remove historical deployments");
        }
    }

    private async Task<bool> ValidateThenSetupContextAsync(CancellationToken cancellationToken)
    {
        DeploymentDto? deployment;
        ModuleDto? module;
        try
        {
            deployment = await deploymentData.GetAsync(this.deploymentContext.DeploymentId, cancellationToken);
            module = await moduleData.GetAsync(this.deploymentContext.ModuleId, cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(
                            "Getting deployment {DeploymentId} or module {ModuleId} failed - {Message}",
                            this.deploymentContext.DeploymentId,
                            this.deploymentContext.ModuleId,
                            e.Message);
            healthCheckSql.Current = HealthCheckSql.FailingCalls;
            metricsCollector.TrackMetric("FailedRequests", 1, MetricType.Counter, FailedRequestDesc);
            await this.UpdateDeploymentStatusAsync(
                                                   DeploymentStatus.Failed,
                                                   message: $"Getting module or deployment failed: {e.Message}",
                                                   cancellationToken: cancellationToken);
            return false;
        }

        if (deployment == null || module == null)
        {
            logger.LogError(
                            "Cannot find deployment {DeploymentId} or module {ModuleId}",
                            this.deploymentContext.DeploymentId,
                            this.deploymentContext.ModuleId);
            metricsCollector.TrackMetric("FailedRequests", 1, MetricType.Counter, FailedRequestDesc);
            await this.UpdateDeploymentStatusAsync(
                                                   DeploymentStatus.Failed,
                                                   message: "Deployment or module not found",
                                                   cancellationToken: cancellationToken);
            return false;
        }

        if (string.IsNullOrWhiteSpace(module.DeviceName) || string.IsNullOrWhiteSpace(module.IoTHubName))
        {
            metricsCollector.TrackMetric("FailedRequests", 1, MetricType.Counter, FailedRequestDesc);
            await this.UpdateDeploymentStatusAsync(
                                                   DeploymentStatus.Failed,
                                                   message: "Module not setup for deployment",
                                                   cancellationToken: cancellationToken);
            return false;
        }

        this.deploymentContext.Deployment = deployment;
        this.deploymentContext.Module = module;
        return true;
    }

    private async Task<bool> ValidateRequest(IDeployModule request, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (validationResult.IsValid)
        {
            return true;
        }

        var errorMessage = JsonConvert.SerializeObject(validationResult.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage.Replace("\'", string.Empty)));
        logger.LogError(
                        "Request {@Request} failed validation: {@ErrorMessage}",
                        request,
                        errorMessage);
        metricsCollector.TrackMetric("FailedRequests", 1, MetricType.Counter, FailedRequestDesc);
        await this.UpdateDeploymentStatusAsync(
                                               DeploymentStatus.Failed,
                                               message: errorMessage,
                                               cancellationToken: cancellationToken);

        return false;
    }

    // publish deployment status to the bus
    private async Task UpdateDeploymentStatusAsync(
        DeploymentStatus status,
        DateTimeOffset? timestamp = null,
        string? message = null,
        CancellationToken cancellationToken = default)
    {
        var now = timestamp ?? DateTimeOffset.UtcNow;
        try
        {
            await deploymentData.UpdateStatusAsync(
                                                   new DeploymentStatusUpdateInput(
                                                                                   this.deploymentContext.DeploymentId,
                                                                                   status.ToString(),
                                                                                   message ?? string.Empty,
                                                                                   now),
                                                   cancellationToken);
        }
        catch (Exception)
        {
            healthCheckSql.Current = HealthCheckSql.FailingCalls;
            throw;
        }

        var result = new
        {
            this.deploymentContext.DeploymentId,
            this.deploymentContext.ModuleId,
            AppliedDateTime = now,
            Status = status,
            Message = message,
        };
        try
        {
            await bus.Publish<IDeploymentStatus>(
                                                 result,
                                                 context => { context.TimeToLive = TimeSpan.FromMinutes(1); },
                                                 cancellationToken);
        }
        catch (Exception)
        {
            healthCheckServiceBus.Current = HealthCheckServiceBus.FailingCalls;
            throw;
        }
    }

    private sealed record DeploymentContext
    {
        public Guid DeploymentId { get; set; }

        public Guid ModuleId { get; set; }

        // dtos won't be null if the context is setup correctly
        public DeploymentDto Deployment { get; set; } = null!;

        public ModuleDto Module { get; set; } = null!;
    }
}
