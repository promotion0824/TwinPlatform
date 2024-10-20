namespace Willow.IoTService.Monitoring.Options
{
    public class AppInsightsOptions
    {
        public string? ApiBaseUrl { get; set; } = "https://api.applicationinsights.io/v1";

        public string? ApplicationId { get; set; }

        public string? ApiKey { get; set; }
    }
}