using Ardalis.GuardClauses;
using Willow.Alert.Resolver.ResolutionHandlers.Abstractions;
using Willow.Alert.Resolver.ResolutionHandlers.Extensions;
using Willow.IoTService.Monitoring.Enums;
using Willow.IoTService.Monitoring.Services.LiveDataCore;

namespace Willow.Alert.Resolver.ResolutionHandlers.Implementation.Handlers;
internal sealed class VerifyDeviceStatusHandler : RetryableResolutionHandlerBase<ResolutionRequest>
{
    private readonly ILogger<VerifyDeviceStatusHandler> _logger;
    private readonly IConnectorStatsQueries _connectorStatsQueries;
    private readonly int _delayInSeconds = 300;
    private readonly string _message = "{0}logs found since {1} for {2}";

    public VerifyDeviceStatusHandler(ILogger<VerifyDeviceStatusHandler> logger,
                                     IConnectorStatsQueries connectorStatsQueries,
                                     IConfiguration configuration) : base(logger, configuration)
    {
        _logger = logger;
        _connectorStatsQueries = connectorStatsQueries;
        _delayInSeconds = configuration.GetValue("ResolutionSettings:Verification:FirstTimeDelayInSec", _delayInSeconds);
        //Task.Delay(_delayInSeconds * 1000).Wait();
    }
    public override async Task<bool> RunAsync(ResolutionRequest request, IResolutionContext context, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(request, nameof(request));
        var result = false;
        Guid? customerId = null;
        if (Guid.TryParse(request.CustomerId, out var value))
        {
            customerId = value;
        }
        var connectorStats = await _connectorStatsQueries.ConnectorStats(customerId,
                                             Guid.Parse(request.ConnectorId),
                                             request.RequestTime,
                                             DateTime.UtcNow);

        if (Enum.TryParse<ConnectorStatus>(connectorStats?.CurrentStatus, out var status))
        {
            result = status != ConnectorStatus.OFFLINE && status != ConnectorStatus.UNKNOWN;
        }
        var message = string.Format(_message, connectorStats != null ? "" : "No ", request.RequestTime.ToString("u"), request.ConnectorName);
        context.AddResponse(this, result.GetResolutionStatus(), message);
        _logger.LogInformation("{Message}", message);
        Guard.Against.Null(connectorStats);
        return result;
    }
}
