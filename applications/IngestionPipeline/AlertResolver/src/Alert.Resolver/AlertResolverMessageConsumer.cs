using MassTransit;
using Willow.Alert.Resolver.ResolutionHandlers.Abstractions;
using Willow.Alert.Resolver.ResolutionHandlers.Implementation;
using Willow.Alert.Resolver.Services;
using Willow.IoTService.Monitoring.Contracts;
using Willow.IoTService.Monitoring.Services.DeploymentDashboard;

namespace Willow.Alert.Resolver;

internal sealed class AlertResolverMessageConsumer : IConsumer<IAlertResolverMessage>
{
    private readonly IConfiguration _config;
    private readonly IDeviceMappingService _deviceMappingService;
    private readonly IDeploymentDashboardApiService _deploymentDashboardApiService;
    private readonly IResolutionStepRunner<ResolutionRequest> _resolutionStepRunner;
    private readonly ILogger<AlertResolverMessageConsumer> _logger;

    public AlertResolverMessageConsumer(ILogger<AlertResolverMessageConsumer> logger,
                                        IConfiguration configuration,
                                        IDeviceMappingService deviceMappingService,
                                        IResolutionStepRunner<ResolutionRequest> resolutionStepRunner,
                                        IDeploymentDashboardApiService deploymentDashboardApiService)
    {
        _logger = logger;
        _config = configuration;
        _resolutionStepRunner = resolutionStepRunner;
        _deploymentDashboardApiService = deploymentDashboardApiService;
        _deviceMappingService = deviceMappingService;
    }

    public async Task Consume(ConsumeContext<IAlertResolverMessage> context)
    {
        _logger.LogInformation("Processing Alert for Connector {ConnectorId} with ConnectorType {ConnectorType}",
                               context.Message.ConnectorId,
                               context.Message.ConnectorType);

        string? deviceName;
        string? iotHubName;
        try
        {
            (deviceName, iotHubName) = await _deploymentDashboardApiService.GetDeviceProperties(context.Message.ConnectorName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting device properties for {ConnectorName}. Exception: {Message}", context.Message.ConnectorName, ex.Message);
            return;
        }

        if (string.IsNullOrEmpty(deviceName) || string.IsNullOrEmpty(iotHubName))
        {
            _logger.LogInformation("Device configuration not found for {ConnectorName}", context.Message.ConnectorName);
            return;
        }

        var deviceInfo = _deviceMappingService.GetDeviceInfo(deviceName);

        var ioTHubConnectionString = _config.GetValue<string>("IoTHubConnectionString");
        if (string.IsNullOrEmpty(ioTHubConnectionString))
        {
            _logger.LogError("IoTHub connection not found for customer {CustomerId}",
                             context.Message.CustomerId);
            return;
        }

        _logger.LogInformation("Connector {ConnectorId} is mapped to IoTHub {IoTHubName} and Device {DeviceId}",
                               context.Message.ConnectorId,
                               iotHubName,
                               deviceName);


        var resolutionRequest = new ResolutionRequest(context.Message.CustomerId,
                                                      context.Message.ConnectorId,
                                                      ioTHubConnectionString,
                                                      deviceName,
                                                      iotHubName,
                                                      context.Message.ConnectorName,
                                                      context.Message.ConnectorType,
                                                      deviceInfo?.ToList());

        var rContext = new ResolutionContext();
        await _resolutionStepRunner.RunAsync(resolutionRequest, rContext);
        _logger.LogInformation("Finished processing Alert for Connector {ConnectorId} with ConnectorType {ConnectorType}",
                               context.Message.ConnectorId,
                               context.Message.ConnectorType);
    }
}
