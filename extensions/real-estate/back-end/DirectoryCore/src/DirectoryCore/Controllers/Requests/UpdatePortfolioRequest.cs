using DirectoryCore.Domain;

namespace DirectoryCore.Controllers.Requests
{
    public class UpdatePortfolioRequest
    {
        public string Name { get; set; }
        public PortfolioFeatures Features { get; set; }
    }
}
