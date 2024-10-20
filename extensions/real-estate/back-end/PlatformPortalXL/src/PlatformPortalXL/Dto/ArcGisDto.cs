namespace PlatformPortalXL.Dto
{
    public class ArcGisDto
    {
        public string GisBaseUrl { get; set; } = "";
        public string Token { get; set; } = "";
        public string[] AuthRequiredPaths { get; set; }
        public string GisPortalPath { get; set; } = "";
    }
}
