using Willow.Alert.Resolver.ResolutionHandlers.Abstractions;
using Willow.Alert.Resolver.Services;

namespace Willow.Alert.Resolver.ResolutionHandlers.Implementation.Handlers;
internal sealed class CheckEdgeHubStatusHandler : RetryableResolutionHandlerBase<ResolutionRequest>
{
    private readonly ILogger<CheckEdgeHubStatusHandler> _logger;
    private readonly IDeviceService _deviceService;

    public CheckEdgeHubStatusHandler(ILogger<CheckEdgeHubStatusHandler> logger,
                                     IDeviceService deviceService,
                                     IConfiguration configuration) : base(logger, configuration)
    {
        _logger = logger;
        _deviceService = deviceService;
    }
    public override async Task<bool> RunAsync(ResolutionRequest request, IResolutionContext context, CancellationToken cancellationToken = default)
    {
        var result = await _deviceService.PingDeviceModuleAsync(request.ConnectionString,
                                                                request.DeviceId);
        _logger.LogInformation("{DeviceName} is successful", GetType().Name);
        return result;
    }
}
