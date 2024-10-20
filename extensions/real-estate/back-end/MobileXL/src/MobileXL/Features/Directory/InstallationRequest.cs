namespace MobileXL.Features.Directory
{
    public class InstallationRequest
    {
        public NotificationPlatform Platform { get; set; }
        public string Handle { get; set; }
    }

    public enum NotificationPlatform
    {
        Apns = 2,
        Fcm = 4
    }
}
