using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Willow.IoTService.Monitoring.Extensions;
using Willow.IoTService.Monitoring.Models;
using Willow.IoTService.Monitoring.Persistence.AzureDataTables;
using Willow.IoTService.Monitoring.Queries;
using Willow.IoTService.Monitoring.Services;
using Willow.IoTService.Monitoring.Utils;

[assembly: InternalsVisibleTo("Willow.IoTService.Monitoring.UnitTests")]
namespace Willow.IoTService.Monitoring.Ports
{
    public interface IAlertsService
    {
        Task ProcessAlerts(List<ConnectorConfigInfo> connectorConfigInfos, CancellationToken cancellationToken);
    }

    public class AlertsService : IAlertsService
    {
        private readonly IAlertsFactory _alertsFactory;
        private readonly IAlertNotificationChannelFactory _alertNotificationChannelFactory;
        private readonly IAlertRunHistoryRepository _alertRunHistoryService;
        private readonly ILogger<AlertsService> _logger;
        private readonly IActiveAlertRepository _activeAlertRepository;

        public AlertsService(
            IAlertsFactory alertsFactory,
            IAlertNotificationChannelFactory alertNotificationChannelFactory,
            IAlertRunHistoryRepository alertRunHistoryService,
            ILogger<AlertsService> logger,
            IActiveAlertRepository activeAlertRepository
            )
        {
            _alertsFactory = alertsFactory;
            _alertNotificationChannelFactory = alertNotificationChannelFactory;
            _alertRunHistoryService = alertRunHistoryService;
            _logger = logger;
            _activeAlertRepository = activeAlertRepository;
        }

        public async Task ProcessAlerts(List<ConnectorConfigInfo> connectorConfigInfos, CancellationToken cancellationToken)
        {
            try
            {
                var notifications = new List<AlertNotification>();

                var alerts = await GetAlerts(connectorConfigInfos);

                foreach (var alert in alerts)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var notification = await GetNotification(alert, _logger);

                    if (notification == null) continue;
                    // Ensure notification source is set.
                    if (string.IsNullOrEmpty(notification.Source))
                    {
                        notification.Source = "Alarm Function";
                    }

                    notifications.Add(notification);
                }

                 if (notifications.Any())
                {
                    var notificationChannels = _alertNotificationChannelFactory.CreateChannels();

                    foreach (var channel in notificationChannels.Where(ch => ch.IsEnabled()))
                    {
                        await SendNotifications(notifications, channel, _logger);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing ProcessAlerts");
            }
        }

        private async Task<IEnumerable<IAlert>> GetAlerts(IList<ConnectorConfigInfo> connectorConfigInfos)
        {
            var timeNow = DateTime.UtcNow;

            var alerts = new List<IAlert>();

            var allAlerts = _alertsFactory.CreateAlerts(connectorConfigInfos);

            foreach (var alert in allAlerts)
            {
                // Interval is in seconds. For connectors with interval less than 30 minutes (1800 seconds) we use 30 minutes interval
                // For connectors with interval greater than or equal to 30 minutes, we use interval + 5 minutes (300 seconds)
                var timespan = alert.ConnectorConfigInfo.TimeInterval < 1800 ? TimeSpan.FromSeconds(1800) : TimeSpan.FromSeconds(alert.ConnectorConfigInfo.TimeInterval + 300);
                var duration = $"{timespan.TotalMinutes} minutes";
                var mins = timespan.Hours * 60;
                var updateAt = alert.ConnectorConfigInfo.LastUpdatedAt.AddMinutes(timespan.Minutes + mins).ToUniversalTime();
                if (updateAt > timeNow)
                {
                    _logger.LogWarning("{ConnectorId} was updated within last {Duration}. Last updated at {LastUpdatedAt}. Skipping alert", alert.ConnectorConfigInfo.ConnectorId, duration, alert.ConnectorConfigInfo.LastUpdatedAt);
                    continue;
                }

                var alertLastEvaluatedAt = await _alertRunHistoryService.GetLastAlertRun(alert.AlertType, alert.ConnectorConfigInfo.SiteId, alert.ConnectorConfigInfo.ConnectorId);

                if (alertLastEvaluatedAt.GetValueOrDefault().AddMinutes(alert.AlertFrequencyInMins) > timeNow) continue;
                alerts.Add(alert);

                await _alertRunHistoryService.SaveAlertRun(alert.AlertType, alert.ConnectorConfigInfo.SiteId, alert.ConnectorConfigInfo.ConnectorId);
            }

            return alerts;
        }

        private static async Task<AlertNotification?> GetNotification(IAlert alert, ILogger logger)
        {
            try
            {
                if (await alert.IsActive())
                {
                    var result = await alert.Evaluate();

                    if (result != null)
                    {
                        result.OriginalAlert = alert;
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failure evaluating alert {AlertType}, siteId {SiteId}, connectorId {ConnectorId}",alert.AlertType, alert.ConnectorConfigInfo.SiteId, alert.ConnectorConfigInfo.ConnectorId);
            }

            return null;
        }

        private async Task<bool> ShouldSkipAlert(IAlert? alert, IEnumerable<AlertNotification> channelNotifications, IAlertNotificationChannel notificationChannel)
        {
            // 1. If alert is null - do not skip.
            if (alert == null)
            {
                return false;
            }

            // 2. Check if there's any alert active (in current list OR existing in storage) which will cause this alert to be skipped.
            if (!alert.SkipAlerts.Any())
            {
                return false;
            }

            var alertNotifications = channelNotifications as AlertNotification[] ?? channelNotifications.ToArray();
            foreach (var skipAlert in alert.SkipAlerts)
            {
                var alertKey = AlertUtils.CreateAlertKey(skipAlert, alert.ConnectorConfigInfo.SiteId, alert.ConnectorConfigInfo.ConnectorId.ToString());

                var isInCurrentList = alertNotifications.Any(x => x.OriginalAlert != null && alertKey == AlertUtils.CreateAlertKey(x.OriginalAlert.AlertType, x.OriginalAlert.ConnectorConfigInfo.SiteId, x.OriginalAlert.ConnectorConfigInfo.ConnectorId.ToString()));
                if (isInCurrentList)
                {
                    return true;
                }

                var alertLastOccurence = await _activeAlertRepository.AlertLastOccurence(GetAlertKeyForChannel(alertKey, notificationChannel));
                
                var isActive = !ShouldRaiseAlert(notificationChannel, alertLastOccurence);
                if (isActive)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task SendNotifications(IEnumerable<AlertNotification> notifications, IAlertNotificationChannel notificationChannel, ILogger logger)
        {
            try
            {
                var channelNotifications = notifications.ToList();

                if (notificationChannel.FilterAlertsForChannel)
                {
                    channelNotifications = channelNotifications.Where(n => n.OriginalAlert != null && n.OriginalAlert.IsEnabledForChannel(notificationChannel)).ToList();
                }

                await RaiseAlerts(notificationChannel, channelNotifications);

                await ResolveAlerts(notificationChannel, channelNotifications);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending alerts to {Channel}", notificationChannel);
            }
        }

        internal async Task RaiseAlerts(IAlertNotificationChannel notificationChannel, IEnumerable<AlertNotification> notifications)
        {
            var channelNotifications = notifications.Where(i => !i.AutoResolve).ToList();

            if (!channelNotifications.Any())
            {
                return;
            }

            var alertKeys = channelNotifications.Select(i => GetAlertKeyForChannel(i.AlertKey, notificationChannel));

            var alertLastOccurrences = await _activeAlertRepository.AlertLastOccurence(alertKeys);

            channelNotifications = channelNotifications.Where(n =>
            {
                var lastOccurence = alertLastOccurrences.First(a => a.AlertKey == GetAlertKeyForChannel(n.AlertKey, notificationChannel)).LastOccurence;
                return ShouldRaiseAlert(notificationChannel, lastOccurence);
            }).ToList();

            var channelNotificationsToRaise = await GetChannelNotificationsToRaise(channelNotifications, notificationChannel);

            var notificationsToRaise = channelNotificationsToRaise as AlertNotification[] ?? channelNotificationsToRaise.ToArray();
            if (notificationsToRaise.Any())
            {
                var result = await notificationChannel.Notify(notificationsToRaise);

                var successfulKeys = result.Where(i => i.Success)
                                           .Select(i => i.AlertKey);

                await _activeAlertRepository.LogAlert(successfulKeys.Select(k => GetAlertKeyForChannel(k, notificationChannel)));
            }
        }

        private async Task<IEnumerable<AlertNotification>> GetChannelNotificationsToRaise(IEnumerable<AlertNotification> channelNotifications, IAlertNotificationChannel alertNotificationChannel)
        {
            var channelNotificationsToRaise = new List<AlertNotification>();

            var alertNotifications = channelNotifications as AlertNotification[] ?? channelNotifications.ToArray();
            foreach (var channelNotification in alertNotifications)
            {
                var shouldSkip = await ShouldSkipAlert(channelNotification.OriginalAlert, alertNotifications, alertNotificationChannel);

                if (shouldSkip)
                {
                    continue;
                }

                channelNotificationsToRaise.Add(channelNotification);
            }

            return channelNotificationsToRaise;
        }

        private async Task ResolveAlerts(IAlertNotificationChannel notificationChannel, IEnumerable<AlertNotification> notifications)
        {
            var channelNotifications = notifications.Where(i => i.AutoResolve).ToList();

            if (!channelNotifications.Any())
            {
                return;
            }

            var alertKeys = channelNotifications.Select(i => GetAlertKeyForChannel(i.AlertKey, notificationChannel));

            var alertStatuses = await _activeAlertRepository.IsAlertActive(alertKeys);

            var activeAlertKeys = alertStatuses.Where(i => i.IsActive).Select(i => i.AlertKey);

            channelNotifications = channelNotifications.Where(i => activeAlertKeys.Contains(GetAlertKeyForChannel(i.AlertKey, notificationChannel))).ToList();

            if (channelNotifications.Any())
            {
                var result = await notificationChannel.Resolve(channelNotifications);

                var successfulKeys = result.Where(i => i.Success)
                                           .Select(i => i.AlertKey);

                await _activeAlertRepository.DeleteAlert(successfulKeys.Select(k => GetAlertKeyForChannel(k, notificationChannel)));
            }
        }

        private static string GetAlertKeyForChannel(string alertKey, IAlertNotificationChannel notificationChannel)
        {
            return $"{alertKey}:{notificationChannel.ChannelName}";
        }

        private static bool ShouldRaiseAlert(IAlertNotificationChannel notificationChannel, DateTime? alertLastOccurrence)
        {
            if (!alertLastOccurrence.HasValue)
            {
                return true;
            }

            var timeSinceLastOccurrence = DateTime.UtcNow - alertLastOccurrence;

            return timeSinceLastOccurrence > notificationChannel.ActiveAlertTTL;
        }
    }
}