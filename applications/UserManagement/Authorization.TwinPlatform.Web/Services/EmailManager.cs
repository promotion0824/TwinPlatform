using Authorization.Common;
using Authorization.Common.Abstracts;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Services.Hosted;
using Authorization.TwinPlatform.Services.Hosted.Request;

namespace Authorization.TwinPlatform.Web.Services;
/// <summary>
/// Email Manager.
/// </summary>
public class EmailManager(ILogger<EmailManager> logger,
    IBackgroundChannelSender<EmailNotificationRequest> backgroundChannelSender) : IRecordChangeListener
{
    public async Task Notify(object targetRecord, RecordAction recordAction)
    {
        if (recordAction == RecordAction.Create && targetRecord is IUser user)
        {
            await QueueWelcomeEmailForUser(user);
        }
    }

    private async Task QueueWelcomeEmailForUser(IUser user)
    {
        try
        {
            var request = new EmailNotificationRequest()
            {
                Id = $"Welcome_Email_{user.Email}",
                Receiver = new(user.Email, user.FullName),
                TemplateId = "d-48541a4d8ac0441fb09a7db4b1a3f4bf",
                TemplateData = new Dictionary<string, object> {
                    {nameof(user.Email),user.Email },
                    {nameof(user.FullName),user.FullName }
                }
            };

            backgroundChannelSender.Write(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while sending welcome email for user:{Email}", user.Email);
        }

        await Task.CompletedTask;
    }
}
