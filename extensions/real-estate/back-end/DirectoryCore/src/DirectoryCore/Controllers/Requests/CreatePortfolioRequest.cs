using DirectoryCore.Domain;

namespace DirectoryCore.Controllers.Requests
{
    public class CreatePortfolioRequest
    {
        public string Name { get; set; }
        public PortfolioFeatures Features { get; set; }
    }
}
