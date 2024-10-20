using Willow.Data;
using Willow.Platform.Models;

namespace Willow.Platform.Statistics
{
    public class StatisticsService : IStatisticsService
    {
	    private readonly IReadRepository<(Guid CustomerId, Guid PortfolioId), InsightsStats> _portfolioInsightsRepo;
        private readonly IReadRepository<SiteStatisticsRequest, TicketStats> _siteTicketRepo;
        private readonly IReadRepository<(Guid CustomerId, Guid PortfolioId), TicketStats> _portfolioTicketRepo;
        private readonly IReadRepository<Guid, TicketStatsByStatus> _siteTicketStatsByStatusRepo;

        public StatisticsService(IReadRepository<(Guid CustomerId, Guid PortfolioId), InsightsStats> portfolioInsightsRepo,
                                 IReadRepository<SiteStatisticsRequest, TicketStats> siteTicketRepo,
                                 IReadRepository<(Guid CustomerId, Guid PortfolioId), TicketStats> portfolioTicketRepo,
                                 IReadRepository<Guid, TicketStatsByStatus> siteTicketStatsByStatusRepo)
        {
	        _portfolioInsightsRepo = portfolioInsightsRepo;
            _siteTicketRepo        = siteTicketRepo;
            _portfolioTicketRepo   = portfolioTicketRepo;
            _siteTicketStatsByStatusRepo = siteTicketStatsByStatusRepo;
        }

        public Task<InsightsStats> GetPortfolioInsights(Guid customerId, Guid portfolioId)
        {
            return _portfolioInsightsRepo.Get((customerId, portfolioId));
        }

        public Task<TicketStats> GetPortfolioTickets(Guid customerId, Guid portfolioId)
        {
            return _portfolioTicketRepo.Get((customerId, portfolioId));
        }

        public Task<TicketStats> GetSiteTickets(SiteStatisticsRequest request)
        {
            return _siteTicketRepo.Get(request);
        }

        public Task<TicketStatsByStatus> GetSiteTicketsStatsByStatus(Guid siteId)
        {
            return _siteTicketStatsByStatusRepo.Get(siteId);
        }
    }
}
