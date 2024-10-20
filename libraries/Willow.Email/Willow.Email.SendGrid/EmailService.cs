namespace Willow.Email.SendGrid;

using System;
using System.Threading;
using System.Threading.Tasks;
using global::SendGrid;
using global::SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// The email service.
/// </summary>
public class EmailService : IEmailService
{
    private readonly SendGridClient sendGridClient;
    private readonly ILogger<EmailService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailService"/> class.
    /// </summary>
    /// <param name="options">The configuration options for SendGrid.</param>
    /// <param name="logger">An ILogger instance.</param>
    public EmailService(IOptions<SendGridOptions> options, ILogger<EmailService> logger)
    {
        sendGridClient = new SendGridClient(options.Value.ApiKey);
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task SendEmailAsync(string email, string subject, string message, string fromEmail, string fromName, CancellationToken cancellationToken = default)
    {
        var fromAddress = new EmailAddress(fromEmail, fromName);
        var sendGridMessage = new SendGridMessage()
        {
            From = fromAddress,
            Subject = subject,
            HtmlContent = message,
        };

        sendGridMessage.AddTo(new EmailAddress(email.Trim()));

        logger.LogInformation("Attempting email send of type {Subject}", subject);

        try
        {
            var response = await sendGridClient.SendEmailAsync(sendGridMessage, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                logger.LogInformation("Email sent.");
            }
            else
            {
                var responseBody = await response.Body.ReadAsStringAsync(cancellationToken);
                logger.LogError("Email send failed with {StatusCode} response {Response}", response.StatusCode, responseBody);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while sending email of type {Subject}", subject);
        }
    }
}
