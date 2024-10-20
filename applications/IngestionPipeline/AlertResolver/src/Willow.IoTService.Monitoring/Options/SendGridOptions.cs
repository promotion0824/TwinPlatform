namespace Willow.IoTService.Monitoring.Options
{
    public class SendGridOptions
    {
        public string? ApiToken { get; set; }

        public string? FromEmail { get; set; }

        public string? FromName { get; set; }

        public bool Enabled { get; set; }

        /// <summary>
        /// Comma delimited list of recipient email addresses
        /// </summary>
        public string? RecipientEmails { get; set; }
    }
}