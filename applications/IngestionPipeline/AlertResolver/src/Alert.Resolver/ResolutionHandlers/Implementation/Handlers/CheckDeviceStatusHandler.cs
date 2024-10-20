using Willow.Alert.Resolver.ResolutionHandlers.Abstractions;
using Willow.Alert.Resolver.ResolutionHandlers.Extensions;
using Willow.Alert.Resolver.Services;

namespace Willow.Alert.Resolver.ResolutionHandlers.Implementation.Handlers;
internal sealed class CheckDeviceStatusHandler : RetryableResolutionHandlerBase<ResolutionRequest>
{
    private readonly ILogger<CheckDeviceStatusHandler> _logger;
    private readonly IDeviceService _deviceService;
    private const string Message = "Pinging to {0} device is {1}";

    public CheckDeviceStatusHandler(ILogger<CheckDeviceStatusHandler> logger,
                                    IDeviceService deviceService,
                                    IConfiguration configuration
                                    ) : base(logger, configuration)
    {
        _logger = logger;
        _deviceService = deviceService;
    }
    public override async Task<bool> RunAsync(ResolutionRequest request, IResolutionContext context, CancellationToken cancellationToken = default)
    {
        var result = await _deviceService.PingDeviceModuleAsync(request.ConnectionString,
                                                                request.DeviceId);
        var message = string.Format(Message, request.DeviceId, result ? "successful" : "failed");
        context.AddResponse(this, result.GetResolutionStatus(), message);
        return result;
    }
}

