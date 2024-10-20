using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Dto;
using WorkflowCore.Repository;

namespace WorkflowCore.Services
{
    public interface IInsightStatisticsService
    {
        Task<List<InsightStatistics>> GetInsightStatisticsList(IList<Guid> insightIds, IList<int> statuses, bool? isScheduled);
        Task<List<InsightStatistics>> GetSiteInsightStatisticsList(IList<Guid> siteIds, IList<int> statuses, bool? isScheduled);
        Task<bool> HasInsightOpenTicketsAsync(Guid insightId);
    }

    public class InsightStatisticsService : IInsightStatisticsService
    {
        private readonly IWorkflowRepository _repository;

        public InsightStatisticsService(IWorkflowRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<InsightStatistics>> GetInsightStatisticsList(IList<Guid> insightIds, IList<int> statuses, bool? isScheduled)
        {
            return await _repository.GetInsightStatisticsList(insightIds, statuses, isScheduled);
        }

        public async Task<List<InsightStatistics>> GetSiteInsightStatisticsList(IList<Guid> siteIds, IList<int> statuses, bool? isScheduled)
        {
            return await _repository.GetSiteInsightStatisticsList(siteIds, statuses, isScheduled);
        }

        public async Task<bool> HasInsightOpenTicketsAsync(Guid insightId)
        {
	        return  await _repository.HasInsightOpenTicketsAsync(insightId);
        }
    }
}
