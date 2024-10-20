namespace Willow.ServiceHealthAggregator.Snowflake;

using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Willow.AppContext;
using Willow.Email.SendGrid;
using Willow.ServiceHealthAggregator.Snowflake.Options;

internal class MultiMessageForwarder(IOptions<WillowContextOptions> contextOptions, IEmailService emailService, HttpClient httpClient, IOptions<SnowflakeOptions> snowflakeOptions, ILogger<MultiMessageForwarder> logger) : IMessageForwarder
{
    private readonly WillowContextOptions context = contextOptions.Value;
    private readonly SnowflakeOptions options = snowflakeOptions.Value;

    public async Task ForwardAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        if (options.Teams.SendTeamsMessage)
        {
            await PostToTeamsChannelAsync(notification, cancellationToken);
        }

        if (options.Email.SendEmail)
        {
            await SendEmailToOpsPlatform(notification, cancellationToken);
        }
    }

    private async Task PostToTeamsChannelAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        string webhookUrl = options.Teams.WebhookUrl;

        string teamsMessage = notification.ToTeamsMessageString(context.FullCustomerInstanceName);

        var content = new StringContent(teamsMessage, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(webhookUrl, content, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation("Message posted to Teams successfully.");
        }
        else
        {
            logger.LogError("Failed to post message to Teams. Status code: {statusCode}", response.StatusCode);
        }
    }

    private async Task SendEmailToOpsPlatform(Notification notification, CancellationToken cancellationToken = default)
    {
        string recipientEmail = options.Email.RecipientEmail;

        string senderEmail = options.Email.SenderEmail;

        string emailSubject = "Snowflake Failure Notification";
        string emailBody = notification.ToEmailBodyString(context.FullCustomerInstanceName);

        await emailService.SendEmailAsync(recipientEmail, emailSubject, emailBody, senderEmail, "Willow (Health)", cancellationToken);
    }
}
