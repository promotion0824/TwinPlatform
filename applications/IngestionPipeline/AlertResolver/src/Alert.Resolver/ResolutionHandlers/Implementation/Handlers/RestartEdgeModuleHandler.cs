using Willow.Alert.Resolver.ResolutionHandlers.Abstractions;
using Willow.Alert.Resolver.ResolutionHandlers.Extensions;
using Willow.Alert.Resolver.Services;

namespace Willow.Alert.Resolver.ResolutionHandlers.Implementation.Handlers;
internal sealed class RestartEdgeModuleHandler : RetryableResolutionHandlerBase<ResolutionRequest>
{
    private readonly ILogger<RestartEdgeModuleHandler> _logger;
    private readonly IDeviceService _deviceService;
    private const string Message = "Restarting a {0} module is {1}";

    public RestartEdgeModuleHandler(ILogger<RestartEdgeModuleHandler> logger,
                                    IDeviceService deviceService,
                                    IConfiguration configuration) : base(logger, configuration)
    {
        _logger = logger;
        _deviceService = deviceService;
    }
    public override async Task<bool> RunAsync(ResolutionRequest request, IResolutionContext context, CancellationToken cancellationToken = default)
    {
        var result = await _deviceService.RestartDeviceModuleAsync(request.ConnectionString,
                                                                   request.DeviceId,
                                                                   request.ConnectorType!,
                                                                   request.ConnectorName!);
        var message = string.Format(Message, request.ConnectorName, result ? "successful" : "failed");
        context.AddResponse(this, result.GetResolutionStatus(), message);
        return result;
    }
}
