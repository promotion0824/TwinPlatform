using System.Net.NetworkInformation;
using Willow.Alert.Resolver.ResolutionHandlers.Abstractions;
using Willow.Alert.Resolver.ResolutionHandlers.Extensions;

namespace Willow.Alert.Resolver.ResolutionHandlers.Implementation.Handlers;

internal sealed class CheckEdgeNetworkStatusHandler : RetryableResolutionHandlerBase<ResolutionRequest>
{
    private const string Message = "Pinging to address {0} is {1}";
    private readonly ILogger<CheckEdgeNetworkStatusHandler> _logger;

    public CheckEdgeNetworkStatusHandler(IConfiguration configuration,
                                         ILogger<CheckEdgeNetworkStatusHandler> logger
                                         ) : base(logger, configuration)
    {
        _logger = logger;
    }

    public override async Task<bool> RunAsync(ResolutionRequest request, IResolutionContext context, CancellationToken cancellationToken = default)
    {
        var result = true;
        var responseMessage = string.Empty;
        var ping = new Ping();

        foreach (var ipAddress in request.IpAddresses)
        {
            // Ping the address
            _logger.LogInformation("Pinging to address {IpAddress}", ipAddress);
            try
            {
                var pingResult = await ping.SendPingAsync(ipAddress);
                result &= pingResult.Status == IPStatus.Success;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while pinging to address {IpAddress}. Message: {Message}", ipAddress, e.Message);
                result = false;
            }
            var message = string.Format(Message, ipAddress, result ? "successful" : "failed");
            responseMessage += message + ",\n";
        }

        //Remove the last comma
        responseMessage = responseMessage.TrimEnd(',', '\n');
        if (string.IsNullOrEmpty(responseMessage))
        {
            responseMessage = $"No IP addresses configured for device {request.DeviceId}";
            result = false;
        }

        context.AddProperty("Log", responseMessage);
        context.AddResponse(this, result.GetResolutionStatus(), responseMessage);
        return result;
    }
}
