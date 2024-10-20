using Willow.Platform.Models;


namespace PlatformPortalXL.ServicesApi.DirectoryApi
{
    public class DirectoryCreatePortfolioRequest
    {
        public string Name { get; set; }
        public PortfolioFeatures Features { get; set; }
    }
}
