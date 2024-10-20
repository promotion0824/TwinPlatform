
using Willow.Platform.Models;

namespace PlatformPortalXL.Features.Management
{
    public class CreatePortfolioRequest
    {
        public string Name { get; set; }
        public PortfolioFeatures Features { get; set; }
    }
}
