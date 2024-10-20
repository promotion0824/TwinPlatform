namespace Willow.Email.SendGrid;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// The service collection extensions.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add the send grid email service to the service collection.
    /// </summary>
    /// <param name="services">The current services collection.</param>
    public static void AddSendGrid(this IServiceCollection services)
    {
        services
            .AddOptions<SendGridOptions>()
            .BindConfiguration("SendGrid");

        services.AddTransient<IEmailService, EmailService>();
    }
}
