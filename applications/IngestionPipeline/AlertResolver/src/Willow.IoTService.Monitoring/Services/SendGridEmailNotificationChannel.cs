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
    public sealed class SendGridEmailNotificationChannel : IAlertNotificationChannel
    {
        private readonly SendGridOptions _channelOptions;
        private readonly ILogger<SendGridEmailNotificationChannel> _logger;

        public string ChannelName => nameof(SendGridEmailNotificationChannel);

        public bool FilterAlertsForChannel => false;

        public TimeSpan ActiveAlertTTL => TimeSpan.FromHours(12);

        public SendGridEmailNotificationChannel(SendGridOptions channelOptions,
                                                ILogger<SendGridEmailNotificationChannel> logger)
        {
            _channelOptions = channelOptions;
            _logger = logger;
        }

        public async Task<IEnumerable<(Guid AlertId, string AlertKey, bool Success)>> Notify(IEnumerable<AlertNotification> alertNotifications)
        {
            var results = new List<(Guid, string, bool)>();

            foreach (var notification in alertNotifications)
            {
                try
                {
                    var recipientEmails = _channelOptions.RecipientEmails?.Trim()
                                                         .Replace(" ", "")
                                                         .Split(",");

                    if (recipientEmails == null) continue;
                    await SendMessageAsync(notification.AlertName, GetEmailText(notification), GetEmailHtml(notification), recipientEmails, notification.Attachments);
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

        private async Task SendMessageAsync(string subject, string messageText, string messageHtml, IEnumerable<string> recipients, IEnumerable<AlertAttachment>? attachments = null)
        {
            var client = new SendGridClient(_channelOptions.ApiToken);
            var fromAddress = new EmailAddress(_channelOptions.FromEmail, _channelOptions.FromName);
            var recipientAddresses = recipients.Select(s => new EmailAddress(s)).ToList();

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(fromAddress, recipientAddresses, subject, messageText, messageHtml);
            var attachmentList = attachments?.ToList();
            if (attachmentList?.Count > 0)
            {
                int count = 0;
                msg.AddAttachments(attachmentList.Select(x => new Attachment
                {
                    Filename = x.FileName,
                    Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(x.Content)),
                    ContentId = $"CSV File {++count}",
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