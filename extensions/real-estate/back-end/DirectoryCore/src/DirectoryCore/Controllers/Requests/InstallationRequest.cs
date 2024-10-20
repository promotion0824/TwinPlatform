using Microsoft.Azure.NotificationHubs;

namespace DirectoryCore.Dto.Requests
{
    public class InstallationRequest
    {
        /// <summary>
        /// Gets or sets notification platform for the installation
        /// </summary>
        public NotificationPlatform Platform { get; set; }

        /// <summary>
        /// Gets or set registration id, token or URI obtained from platform-specific notification service
        /// </summary>
        public string Handle { get; set; }
    }
}
