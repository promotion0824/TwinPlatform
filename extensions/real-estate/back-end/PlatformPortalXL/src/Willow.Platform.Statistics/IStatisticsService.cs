using Willow.Platform.Models;

namespace Willow.Platform.Statistics
{
    public class SiteStatisticsRequest
    {
        public Guid   SiteId  { get; init; }
        public string FloorId { get; init; }

        public override string ToString()
        {
            return SiteId.ToString() + "_" + FloorId ?? "";
        }
    }
    
    public interface IStatisticsService
    {
	    public Task<InsightsStats> GetPortfolioInsights(Guid customerId, Guid portfolioId);
        public Task<TicketStats> GetSiteTickets(SiteStatisticsRequest request);
        public Task<TicketStats> GetPortfolioTickets(Guid customerId, Guid portfolioId);
        public Task<TicketStatsByStatus> GetSiteTicketsStatsByStatus(Guid siteId);
    }
}
