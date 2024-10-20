using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using Willow.IoTService.Monitoring.Models;
using Willow.IoTService.Monitoring.Options;

namespace Willow.IoTService.Monitoring.Services
{
    public sealed class PagerDutyEmailNotificationChannel : IAlertNotificationChannel
    {
        private readonly PagerDutyEmailOptions _pagerDutyEmailOptions;
        private readonly ILogger<PagerDutyEmailNotificationChannel> _logger;

        public string ChannelName => nameof(PagerDutyEmailNotificationChannel);

        public bool FilterAlertsForChannel => true;

        public TimeSpan ActiveAlertTTL => TimeSpan.FromHours(12);

        public PagerDutyEmailNotificationChannel(PagerDutyEmailOptions pagerDutyEmailOptions,
                                                 ILogger<PagerDutyEmailNotificationChannel> logger)
        {
            _pagerDutyEmailOptions = pagerDutyEmailOptions;
            _logger = logger;
        }

        public async Task<IEnumerable<(Guid AlertId, string AlertKey, bool Success)>> Notify(IEnumerable<AlertNotification> alertNotifications)
        {
            var results = new List<(Guid, string, bool)>();

            if (_pagerDutyEmailOptions == null)
            {
                return results;
            }

            foreach (var notification in alertNotifications)
            {
                try
                {
                    var recipientEmails = _pagerDutyEmailOptions.RecipientEmailAddresses?.Trim()
                                                                                         .Replace(" ", "")
                                                                                         .Split(",");

                    await SendMessageAsync(notification.AlertName, GetEmailText(notification), GetEmailHtml(notification), recipientEmails, notification.Attachments);

                    results.Add((notification.Id, notification.AlertKey, true));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending PagerDuty notification.");
                    results.Add((notification.Id, notification.AlertKey, false));
                }
            }

            return results;
        }

        public async Task<IEnumerable<(Guid AlertId, string AlertKey, bool Success)>> Resolve(IEnumerable<AlertNotification> alertNotifications)
        {
            return await Notify(alertNotifications);
        }

        private async Task SendMessageAsync(string subject, string messageText, string messageHtml, IEnumerable<string>? recipients, IEnumerable<AlertAttachment>? attachments = null)
        {
            var client = new SendGridClient(_pagerDutyEmailOptions.SendGridApiToken);
            var fromAddress = new EmailAddress(_pagerDutyEmailOptions.FromEmailAddress, _pagerDutyEmailOptions.FromName);
            var recipientAddresses = recipients?.Select(s => new EmailAddress(s)).ToList();

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(fromAddress, recipientAddresses, subject, messageText, messageHtml);
            if (attachments?.Any() == true)
            {
                int count = 0;
                msg.AddAttachments(attachments.Select(x => new Attachment{
                    Filename = x.FileName,
                    Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(x.Content)),
                    ContentId = $"CSV File { ++count }",
                    Disposition = "attachment",
                    Type = "text/plain"
                }));
            }
            await client.SendEmailAsync(msg);
        }

        private static string GetEmailHtml(AlertNotification notification)
        {
            if (notification == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            sb.AppendLine($"<div>{notification.Message}</div><br />");
            foreach (var data in notification.Data)
            {
                sb.AppendLine($"<div>{data.Key} : {data.Value}</div>");
            }

            return sb.ToString();
        }

        private static string GetEmailText(AlertNotification notification)
        {
            if (notification == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            sb.AppendLine($"{notification.Message}");
            foreach (var data in notification.Data)
            {
                sb.AppendLine($"{data.Key} : {data.Value}");
            }

            return sb.ToString();
        }
    }
}
