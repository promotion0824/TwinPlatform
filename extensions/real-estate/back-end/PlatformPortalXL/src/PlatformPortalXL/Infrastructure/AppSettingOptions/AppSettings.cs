using PlatformPortalXL.ServicesApi.ZendeskApi;

namespace PlatformPortalXL.Infrastructure.AppSettingOptions
{
    public class AppSettings
    {
        public string CommandPortalBaseUrl { get; set; }
        public ZendeskOptions ZendeskOptions { get; set; }
    }
}
