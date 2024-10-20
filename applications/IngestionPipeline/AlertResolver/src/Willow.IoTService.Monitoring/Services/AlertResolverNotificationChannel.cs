using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.IoTService.Monitoring.Contracts;
using Willow.IoTService.Monitoring.Enums;
using Willow.IoTService.Monitoring.Models;
using Willow.IoTService.Monitoring.Services.AppInsights;

namespace Willow.IoTService.Monitoring.Services;

public sealed class AlertResolverNotificationChannel : IAlertNotificationChannel
{
    private readonly ILogger<AlertResolverNotificationChannel> _logger;
    private readonly IBus _bus;
    private readonly IMonitorEventTracker _monitorEventTracker;

    private readonly List<string> _customMetrics = new()
    {
        "ActiveCapabilities",
        "TrendingCapabilities",
        "MinimumPercentage",
        "ActualPercentage"
    };

    public AlertResolverNotificationChannel(ILogger<AlertResolverNotificationChannel> logger,
                                            IBus bus,
                                            IMonitorEventTracker monitorEventTracker)
    {
        _logger = logger;
        _bus = bus;
        _monitorEventTracker = monitorEventTracker;
    }

    public string ChannelName => nameof(AlertResolverNotificationChannel);
    public bool FilterAlertsForChannel => false;
    public TimeSpan ActiveAlertTTL => TimeSpan.FromHours(12);

    public async Task<IEnumerable<(Guid AlertId, string AlertKey, bool Success)>> Notify(IEnumerable<AlertNotification> alertNotifications)
    {
        var results = new List<(Guid, string, bool)>();

        foreach (var notification in alertNotifications)
        {
            try
            {
                _logger.LogDebug("Alert {AlertId} with key {AlertKey} will be sent to alert service bus queue", notification.Id, notification.AlertKey);
                results.Add((notification.Id, notification.AlertKey, true));
                // Log the alert to AppInsights for dashboard
                try
                {
                    LogAlertToAppInsights(notification);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending message to app insights: {Message}", ex.Message);
                }

                // Don't send degraded alerts to alert resolver
                if (notification.OriginalAlert?.AlertType.Contains("Degraded") ?? false) continue;
                
                var connectorId = notification.Data["ConnectorId"].ToString() ?? string.Empty;
                var connectorType = notification.Data["ConnectorType"].ToString() ?? string.Empty;
                var customerId = notification.Data["CustomerId"].ToString() ?? string.Empty;
                var connectorName = notification.Data["ConnectorName"].ToString() ?? string.Empty;

                await _bus.Send<IAlertResolverMessage>(new
                {
                    connectorId,
                    connectorType,
                    connectorName,
                    customerId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to alert service bus queue");
                results.Add((notification.Id, notification.AlertKey, false));
            }
        }
        return results;
    }

    public Task<IEnumerable<(Guid AlertId, string AlertKey, bool Success)>> Resolve(IEnumerable<AlertNotification> alertNotifications)
    {
        return Notify(alertNotifications);
    }

    private void LogAlertToAppInsights(AlertNotification alertNotification)
    {
        var metrics = _customMetrics.Where(metric => alertNotification.Data.ContainsKey(metric))
                                    .ToDictionary(metric => metric,
                                                  metric => Convert.ToDouble(alertNotification.Data[metric]));
        metrics.Add((alertNotification.OriginalAlert?.AlertType ?? string.Empty) + "Alert", 1);

        var properties = alertNotification.Data.Where(prop => !_customMetrics.Contains(prop.Key))
                                          .ToDictionary(prop => prop.Key,
                                                        prop => prop.Value.ToString() ?? string.Empty);

        var monitorSource = alertNotification.OriginalAlert?.AlertType?.Contains("Degraded") ?? false
            ? MonitorSource.PartialOutageAlert
            : MonitorSource.FullOutageAlert;

        var monitorEvent = new MonitorEvent
        {
            MonitorSource = monitorSource,
            CustomProperties = properties,
            Metrics = metrics
        };
        _monitorEventTracker.Execute(monitorEvent);
    }
}
