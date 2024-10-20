using System;
using PlatformPortalXL.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.Workflow;
using PlatformPortalXL.Features.Controllers;

namespace PlatformPortalXL.Features.Insights
{
    public interface IInsightService
	{
        Task<List<InsightSimpleDto>> GetInsightStatistics(List<InsightSimpleDto> insights);
        Task<List<InsightTicketStatistics>> GetSiteInsightStatistics(List<Guid> siteIds);

        Task<List<InsightOccurrenceDto>> GetInsightOccurrencesAsync(Guid insightId);
		Task<List<InsightActivityDto>> GetInsightActivitiesAsync(Guid siteId, Guid insightId);
		Task<List<InsightSimpleDto>> MapFloorData(List<InsightSimpleDto> insights);
        Task<InsightPointsDto> GetInsightPointsAsync(Guid siteId, Guid insightId);
        Task<ImpactScoresLiveData> GetLiveDataByExternalId(Guid customerId, string externalId, DateTime start, DateTime end, string selectedInterval);
        Task<List<TwinInsightStatisticsResponseDto>> GetTwinsWithGeometryIdInsightStatisticsAsync(InsightTwinStatisticsRequest request);
        Task<List<TwinInsightStatisticsDto>> GetTwinInsightStatisticsAsync(InsightTwinStatisticsRequest request);
    }
}
