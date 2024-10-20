namespace Willow.Email.SendGrid;

using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// The email service.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email.
    /// </summary>
    /// <param name="email">The target email address.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="message">The message of the email.</param>
    /// <param name="fromEmail">The email address to be used as the sender.</param>
    /// <param name="fromName">The name to be associated with the sender of the email.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task SendEmailAsync(string email, string subject, string message, string fromEmail, string fromName, CancellationToken cancellationToken = default);
}
