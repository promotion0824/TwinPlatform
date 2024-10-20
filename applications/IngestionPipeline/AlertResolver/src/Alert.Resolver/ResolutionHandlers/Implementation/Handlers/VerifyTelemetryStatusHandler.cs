using Willow.Alert.Resolver.ResolutionHandlers.Abstractions;
using Willow.Alert.Resolver.ResolutionHandlers.Enumerations;
using Willow.Alert.Resolver.ResolutionHandlers.Extensions;
using Willow.IoTService.Monitoring.Enums;
using Willow.IoTService.Monitoring.Services.LiveDataCore;

namespace Willow.Alert.Resolver.ResolutionHandlers.Implementation.Handlers;

internal sealed  class VerifyTelemetryStatusHandler : RetryableResolutionHandlerBase<ResolutionRequest>
{
    private readonly ILogger<VerifyTelemetryStatusHandler> _logger;
    private readonly IConnectorStatsQueries _connectorStatsQueries;
    private const string Message = "Verifying Telemetry for connector {0} is {1}";

    public VerifyTelemetryStatusHandler(ILogger<VerifyTelemetryStatusHandler> logger,
                                        IConfiguration configuration,
                                        IConnectorStatsQueries connectorStatsQueries) : base(logger, configuration)
    {
        _logger = logger;
        _connectorStatsQueries = connectorStatsQueries;
    }

    public override async Task<bool> RunAsync(ResolutionRequest request, IResolutionContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Verifying telemetry status");
        // On resolving the issue, we expect data to start flowing back. So, use default interval of last 30 minutes
        // Wait 2 minutes before checking the status to allow the data to flow back
        await Task.Delay(TimeSpan.FromMinutes(2), cancellationToken);
        const int intervalInMinutes = 30;
        var end = DateTime.UtcNow;
        var start = end.AddMinutes(-intervalInMinutes);

        try
        {
            Guid? customerId = null;
            if (Guid.TryParse(request.CustomerId, out var value))
            {
                customerId = value;
            }
            var response = await _connectorStatsQueries.ConnectorStats(customerId, Guid.Parse(request.ConnectorId), start, end);

            var result = Enum.TryParse<ConnectorStatus>(response?.CurrentStatus, out var status) && status != ConnectorStatus.OFFLINE && status != ConnectorStatus.UNKNOWN;
            var message = string.Format(Message, request.ConnectorName, result ? "successful" : "failed");
            context.AddResponse(this, result.GetResolutionStatus(), message);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during VerifyTelemetryStatusHandler: {Message}", ex.Message );
            var message = string.Format(Message, request.ConnectorName, "failed");
            context.AddResponse(this, ResolutionStatus.Failed, message);

            return false;
        }
    }
}
