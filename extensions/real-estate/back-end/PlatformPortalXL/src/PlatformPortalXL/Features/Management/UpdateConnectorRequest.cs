namespace PlatformPortalXL.Features.Management
{
    public class UpdateConnectorRequest
    {
        public string Configuration { get; set; }
        public bool? IsLoggingEnabled { get; set; }
        public int? ErrorThreshold { get; set; }
        public string Name { get; set; }
    }
}
