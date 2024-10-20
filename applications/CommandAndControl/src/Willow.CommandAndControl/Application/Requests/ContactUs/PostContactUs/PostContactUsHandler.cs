namespace Willow.CommandAndControl.Application.Requests.ContactUs.PostContactUs;

using Willow.Email.SendGrid;

internal class PostContactUsHandler
{
    public static async Task<Results<NoContent, BadRequest<ProblemDetails>>> HandleAsync([FromBody] PostContactUsDto contactUs, [FromServices] IEmailService emailService, [FromServices] IOptions<Options.ContactUs> emailOptions, CancellationToken cancellationToken = default)
    {
        string message = $@"
            <p>Requester's Name: {contactUs.RequestersName}</p>
            <p>Requester's Email: {contactUs.RequestersEmail}</p>
            <p>URL: {contactUs.Url}</p>
            <p>Comment:<br/>{contactUs.Comment}</p>";

        // The email service does not support sending multiple emails in one call, so we need to loop through the recipients and send an email to each one.
        foreach (var recipient in emailOptions.Value.ToEmailAddresses)
        {
            await emailService.SendEmailAsync(recipient, "ActiveControl: Feedback", message, emailOptions.Value.FromEmail, "ActiveControl", cancellationToken);
        }

        return TypedResults.NoContent();
    }
}
