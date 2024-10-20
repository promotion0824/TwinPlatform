using Ardalis.GuardClauses;
using Willow.Alert.Resolver.ResolutionHandlers.Abstractions;
using Willow.IoTService.Monitoring.Enums;
using Willow.IoTService.Monitoring.Models;
using Willow.IoTService.Monitoring.Services.AppInsights;

namespace Willow.Alert.Resolver.ResolutionHandlers.Implementation.Handlers;
internal sealed class SendFeedbackHandler : RetryableResolutionHandlerBase<ResolutionRequest>
{
    private readonly ILogger<SendFeedbackHandler> _logger;
    private readonly IAlertNotificationChannel _alertNotificationChannel;
    private readonly bool _channelIsEnabled;

    public SendFeedbackHandler(ILogger<SendFeedbackHandler> logger,
                               IAlertNotificationChannel alertNotificationChannel,
                               IMonitorEventTracker monitorEventTracker,
                               IConfiguration configuration) : base(logger, configuration, monitorEventTracker)
    {
        _logger = logger;
        _alertNotificationChannel = alertNotificationChannel;
        _channelIsEnabled = configuration.GetValue($"Alerting:{_alertNotificationChannel.ChannelName}Options:Enabled", true);
    }
    public override async Task<bool> RunAsync(ResolutionRequest request,IResolutionContext context, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(request.ConnectorName);
        Guard.Against.NullOrEmpty(request.ConnectorType);

        if (!_channelIsEnabled)
        {
            _logger.LogInformation("{ChannelName} is disabled", _alertNotificationChannel.ChannelName);
            return true;
        }

        var result = (await _alertNotificationChannel.Notify(GetAlertNotification(request,context))).ToList();
        Guard.Against.NullOrEmpty(result);
        Guard.Against.Zero(result.Count);

        return result[0].Success;
    }

    private IEnumerable<AlertNotification> GetAlertNotification(ResolutionRequest request, IResolutionContext context)
    {
        Dictionary<string, object> data = new();
        data.Add("ConnectorName", request.ConnectorName!);
        data.Add("ConnectorType", request.ConnectorType!);
        data.Add("IoTHub", request.IoTHubName);
        data.Add("DeviceId", request.DeviceId);

        foreach (var item in context.Responses)
        {
            data.Add(item.Key, item.Value.Message);
        }

        return new List<AlertNotification>() { new()
        {
            AlertKey = "Alert-Feedback",
            AlertName = $"AlertFeedback:{request.ConnectorName}",
            AutoResolve = true,
            Data = data,
            Message = GetMessage(request),
            Severity = AlertSeverity.Critical,
            Source = "Alert.Resolver",
            Timestamp = DateTime.UtcNow
        } };
    }

    private string GetMessage(ResolutionRequest request)
    {
        return $"Alert Resolver has finished running for {request.ConnectorName}'s alert";
    }
}
