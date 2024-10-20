using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InsightCore.Controllers.Requests;
using InsightCore.Dto;
using Willow.Batch;
using Willow.ExceptionHandling.Exceptions;

namespace InsightCore.Services
{
    public interface IInsightStatisticsService
    {

        Task<InsightStatisticsResponse> GetInsightStatisticsBySiteIds(IList<Guid> siteIds);
        Task<List<InsightSnackbarByStatus>> GetInsightSnackbarsByStatus(IEnumerable<FilterSpecificationDto> filters);
        Task<List<TwinInsightStatisticsDto>> GetInsightStatisticsByTwinIds(TwinInsightStatisticsRequest request);
        Task<InsightOccurrencesCountByDateResponse> GetInsightOccurrencesByDate(string spaceTwinId, DateTime startDate, DateTime endDate);
        Task<List<ActiveInsightByModelIdDto>> GetActiveInsightByModelId(string spaceTwinId, int limit);
    }

    public class InsightStatisticsService : IInsightStatisticsService
    {
        private readonly IInsightRepository _repository;
        private readonly IInsightService _insightService;
        public InsightStatisticsService(IInsightRepository repository,IInsightService insightService)
        {
            _repository = repository;
            _insightService=insightService;
        }

        public async Task<List<ActiveInsightByModelIdDto>> GetActiveInsightByModelId(string spaceTwinId, int limit)
        {
            return await _repository.GetActiveInsightByModelId(spaceTwinId, limit <= 0 ? 5 : limit);
        }
        public async Task<InsightOccurrencesCountByDateResponse> GetInsightOccurrencesByDate(string spaceTwinId,
            DateTime startDate, DateTime endDate)
        {
            var insightOccurrencesCount = await _repository.GetInsightOccurrencesByDate(spaceTwinId, startDate, endDate);
            if(insightOccurrencesCount == null || !insightOccurrencesCount.Any())
            {
                throw new NotFoundException("No insight occurrences found for the given date range");
            }
            return new InsightOccurrencesCountByDateResponse()
            {
                Counts = insightOccurrencesCount
                    .Select(c => new InsightOccurrencesCountDto() { Count = c.Count, Date = c.Date }).ToList(),
                AverageDuration =(int) insightOccurrencesCount.Average(c => c.AverageDuration)
            };
        }
        public async Task<InsightStatisticsResponse> GetInsightStatisticsBySiteIds(IList<Guid> siteIds)
        {
            if (siteIds == null || !siteIds.Any())
            {
                throw new BadRequestException("The siteIds are required");
            }

            siteIds = siteIds.Distinct().ToList();

            var statisticsByPriority = await _repository.GetInsightStatisticsByPriorityList(siteIds);
            var statisticsByStatus = await _repository.GetInsightStatisticsByStatusList(siteIds);

            return new InsightStatisticsResponse
            {
                StatisticsByPriority =  statisticsByPriority,
                StatisticsByStatus =  statisticsByStatus
            };
        }

        public async Task<List<TwinInsightStatisticsDto>> GetInsightStatisticsByTwinIds(TwinInsightStatisticsRequest request)
        {
            if (request.TwinIds == null || !request.TwinIds.Any())
            {
                throw new BadRequestException("The twinIds are required");
            }

            return await _repository.GetInsightStatisticsByTwinIds(
                request.TwinIds.Distinct().ToList(),
                request.IncludeRuleId,
                request.ExcludeRuleId,
                request.IncludePriorityCounts);
        }

        public async Task<List<InsightSnackbarByStatus>> GetInsightSnackbarsByStatus(IEnumerable<FilterSpecificationDto> filters)
        {
            var insightSnackbar=await _repository.GetInsightSnackbarByStatus(filters);
            insightSnackbar.ForEach(c=>c.SourceName=c.SourceType.HasValue?  _insightService.GetSourceName(c.SourceType.Value,c.SourceId):null);
            return insightSnackbar;
        }
    }
}
