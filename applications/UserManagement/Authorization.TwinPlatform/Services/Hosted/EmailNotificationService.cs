using Authorization.TwinPlatform.Services.Hosted.Request;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Authorization.TwinPlatform.Services.Hosted;

/// <summary>
/// Email Notification background service for sending email requested via background channel.
/// </summary>
/// <param name="logger">Logger Instance.</param>
/// <param name="backgroundChannelReceiver">IBackgroundChannelReceiver of type EmailNotificationRequest.</param>
/// <param name="option">Email Notification Service Option instance.</param>
public class EmailNotificationService(ILogger<EmailNotificationService> logger,
    IBackgroundChannelReceiver<EmailNotificationRequest> backgroundChannelReceiver,
    IOptions<EmailNotificationServiceOption> option) : BackgroundService
{
    readonly EmailNotificationServiceOption notificationOption = option.Value;

    /// <summary>
    /// Execute background service.
    /// </summary>
    /// <param name="stoppingToken">Cancellation Token.</param>
    /// <returns></returns>
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!notificationOption.Enabled)
        {
            logger.LogWarning("Hosted Email Notification Service disabled. Returning without execution.");
            return;
        }

        // activate background channel, so that the channel start to accept the messages
        backgroundChannelReceiver.SetStatus(status: true);

        logger.LogInformation("Started Email Notification Service with Refresh Interval: {Interval} Minutes.", notificationOption.RefreshInterval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            // Wait for the specified interval before refreshing cache again
            await Task.Delay(notificationOption.RefreshInterval, stoppingToken);

            await SendEmailNotification(stoppingToken);
        }
        logger.LogInformation("Stopping Email Notification Service.");
    }


    private async Task SendEmailNotification(CancellationToken stoppingToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(notificationOption.SendGridAPIKey))
            {
                logger.LogError("SendGrid API key cannot be empty.");
                return;
            }

            logger.LogDebug("Entering [Send email notification from channel].");

            var client = new SendGridClient(notificationOption.SendGridAPIKey);

            
            await foreach (var emailNotificationRequest in backgroundChannelReceiver.ReadEnumerableAsync(stoppingToken))
            {
                try
                {
                    var message = new SendGridMessage()
                    {
                        From = new EmailAddress(notificationOption.SenderEmail, notificationOption.SenderName),                        
                        TemplateId = emailNotificationRequest.TemplateId,
                    };

                    message.AddTo(new EmailAddress(emailNotificationRequest.Receiver.Email, emailNotificationRequest.Receiver.Name), 0,
                        new Personalization()
                        {
                            TemplateData = emailNotificationRequest.TemplateData,
                        });

                    var response = await client.SendEmailAsync(message,stoppingToken);

                    if(response == null)
                    {
                        logger.LogError("Failed to send email. No response from the email provider.");
                    }
                    else if(!response.IsSuccessStatusCode) {

                        var errorMessage = await response!.Body.ReadAsStringAsync(stoppingToken);
                        logger.LogError("SendGrid Error Message : {Message}",errorMessage);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,"Error while sending email notification: {TemplateId}:{Receiver}", emailNotificationRequest.TemplateId, emailNotificationRequest.Receiver);
                }
            }

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while scheduling email notification from the channel.");
        }
    }
}
