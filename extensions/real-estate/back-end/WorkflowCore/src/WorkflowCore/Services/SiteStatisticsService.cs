using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Dto;
using WorkflowCore.Repository;

namespace WorkflowCore.Services
{
    public interface ISiteStatisticsService
    {
        Task<List<SiteStatistics>> GetSiteStatisticsList(IList<Guid> siteIds);
        Task<SiteStatistics> GetSiteStatistics(Guid siteId, string floorId);
        Task<SiteTicketStatisticsByStatus> GetSiteTicketStatisticsByStatus(Guid siteId);
        Task<TicketStatisticsDto> GetTicketStatisticsBySiteIdsAsync(List<Guid> siteIds);
    }

    public class SiteStatisticsService : ISiteStatisticsService
    {
        private readonly IWorkflowRepository _repository;

        public SiteStatisticsService(IWorkflowRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<SiteStatistics>> GetSiteStatisticsList(IList<Guid> siteIds)
        {
            return await _repository.GetSiteStatisticsList(siteIds);
        }

        public async Task<SiteStatistics> GetSiteStatistics(Guid siteId, string floorId)
        {
            return await _repository.GetSiteStatistics(siteId, floorId);
        }

        public async Task<SiteTicketStatisticsByStatus> GetSiteTicketStatisticsByStatus(Guid siteId)
        {
            return (await _repository.GetSiteTicketStatisticsByStatus([siteId])).FirstOrDefault();
        }

        public async Task<TicketStatisticsDto> GetTicketStatisticsBySiteIdsAsync(List<Guid> siteIds)
        {
            siteIds = siteIds.Distinct().ToList();
            var statisticsByPriority = await _repository.GetSiteStatistics(siteIds);
            var statisticsByStatus = await  _repository.GetSiteTicketStatisticsByStatus(siteIds);

            return new TicketStatisticsDto
            {
                StatisticsByPriority =  statisticsByPriority,
                StatisticsByStatus =  statisticsByStatus
            };

        }
    }
}
