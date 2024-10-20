
using Willow.Platform.Models;

namespace PlatformPortalXL.Features.Management
{
    public class UpdatePortfolioRequest
    {
        public string Name { get; set; }
        public PortfolioFeatures Features { get; set; }
    }
}
