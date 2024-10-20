using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Willow.IoTService.Monitoring.Models;
using Willow.IoTService.Monitoring.Options;
using Willow.IoTService.Monitoring.Utils;

namespace Willow.IoTService.Monitoring.Services
{
    public sealed class MicrosoftTeamsNotificationChannel : IAlertNotificationChannel
    {
        private readonly MicrosoftTeamsNotificationChannelOptions _channelOptions;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger _logger;

        public string ChannelName => nameof(MicrosoftTeamsNotificationChannel);

        public bool FilterAlertsForChannel => false;

        public TimeSpan ActiveAlertTTL => TimeSpan.FromHours(12);

        public MicrosoftTeamsNotificationChannel(MicrosoftTeamsNotificationChannelOptions channelOptions, IHttpClientFactory clientFactory, ILogger<MicrosoftTeamsNotificationChannel> logger)
        {
            _channelOptions = channelOptions;
            _clientFactory = clientFactory;
            _logger = logger;
        }

        private static HttpRequestMessage NewTeamsMessageFor(string? webhookEndpoint, AlertNotification notification)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, webhookEndpoint);

            request.Content = new StringContent(TeamsMessageConverter.FromAlertNotification(notification).ToJson(), System.Text.Encoding.UTF8, "application/json");

            return request;
        }

        public async Task<IEnumerable<(Guid AlertId, string AlertKey, bool Success)>> Notify(IEnumerable<AlertNotification> alertNotifications)
        {
            var client = _clientFactory.CreateClient();

            var results = new List<(Guid, string, bool)>();

            foreach (var notification in alertNotifications)
            {
                try
                {
                    var request = NewTeamsMessageFor(_channelOptions.WebhookEndpoint, notification);

                    await client.SendAsync(request);

                    results.Add((notification.Id, notification.AlertKey, true));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending notification");
                    results.Add((notification.Id, notification.AlertKey, false));
                }
            }

            return results;
        }

        public Task<IEnumerable<(Guid AlertId, string AlertKey, bool Success)>> Resolve(IEnumerable<AlertNotification> alertNotifications)
        {
            return Notify(alertNotifications);
        }
    }
}