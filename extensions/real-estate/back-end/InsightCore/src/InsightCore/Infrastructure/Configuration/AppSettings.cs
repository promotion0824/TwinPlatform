namespace InsightCore.Infrastructure.Configuration
{
    public class AppSettings
    {
        public InspectionOptions InspectionOptions { get; set; }
        public WillowActivateOptions WillowActivateOptions { get; set; }
        public MappedIntegrationConfiguration MappedIntegrationConfiguration { get; set; }
        /// <summary>
        /// Service bus options for notification
        /// </summary>
        public ServiceBusOptions ServiceBusOptions { get; set; }
        /// <summary>
        /// Enable or disable notification
        /// this is a temporary solution to enable notification for testing
        /// and will be removed in the future once the notification is fully implemented
        /// </summary>
        public bool IsNotificationEnabled { get; set; }
    }
}
