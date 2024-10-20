using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using Willow.Api.Client;
using Willow.Batch;

namespace PlatformPortalXL.ServicesApi.InsightApi
{
    public interface IInsightApiService
    {
        Task<Insight> GetInsight(Guid siteId, Guid insightId);
        Task<Insight> GetInsight( Guid insightId);
        Task<BatchDto<Insight>> GetInsights(BatchRequestDto request, bool addFloor=false);
        Task<BatchDto<SkillDto>> GetSkills(BatchRequestDto request);
        Task<BatchDto<InsightCard>> GetInsightCards(BatchRequestDto request);
        Task<List<ImpactScore>> GetImpactScoresSummary(BatchRequestDto request);
        Task<List<InsightOccurrence>> GetInsightOccurrencesAsync(Guid insightId);
        Task<Insight> UpdateInsightAsync(Guid siteId, Guid insightId, UpdateInsightRequest request);
		Task<List<InsightActivity>> GetInsightActivitiesAsync(Guid siteId, Guid insightId);
        Task<List<InsightSourceDto>> GetInsightSourcesAsync(List<Guid> siteIds);
        Task<InsightPointsDto> GetInsightPointsAsync(Guid siteId, Guid insightId);
        Task UpdateBatchInsightStatusAsync(Guid siteId, BatchUpdateInsightStatusRequest request);
        Task<List<InsightMapViewResponse>> GetInsightListForMapViewAsync(IEnumerable<Guid> siteIds);
        Task<List<InsightDiagnosticDto>> GetInsightDiagnosticAsync(Guid insightId, DateTime start, DateTime end, string interval);
        Task<DiagnosticsSnapshotDto> GetDiagnosticsSnapshot(Guid insightId);
        Task<InsightFilterDto> GetInsightsFilterAsync(GetInsightFilterApiRequest getInsightFilterApiRequest);
        Task<InsightStatisticsResponse> GetInsightStatisticsBySiteIds(List<Guid> siteIds);
        Task<List<InsightSnackbarByStatus>> GetInsightSnackbarsByStatus(IEnumerable<FilterSpecificationDto> filters);
        Task<List<TwinInsightStatisticsDto>> GetTwinsInsightStatistics(TwinInsightStatisticsRequest request);
        Task<InsightOccurrencesCountByDateResponse> GetInsightOccurrencesByDateAsync(string spaceTwinId, DateTime startDate, DateTime endDate);
        Task<List<ActiveInsightCountByModelIdDto>> GetActiveInsightByModelId(string spaceTwinId, int limit);
    }

    public class InsightApiService : IInsightApiService
    {
        private readonly IRestApi _insightApi;

        public InsightApiService(IRestApi insightApi)
        {
            _insightApi = insightApi;
        }

        public Task<List<ActiveInsightCountByModelIdDto>> GetActiveInsightByModelId(string spaceTwinId, int limit)
        {
            return _insightApi.Get<List<ActiveInsightCountByModelIdDto>>(
                $"insights/twin/{spaceTwinId}/activeInsightCountsByTwinModel?limit={limit}");
        }
        public Task<InsightOccurrencesCountByDateResponse> GetInsightOccurrencesByDateAsync(string spaceTwinId,
            DateTime startDate, DateTime endDate)
        {
            return _insightApi.Get<InsightOccurrencesCountByDateResponse>(
                $"insights/twin/{spaceTwinId}/insightOccurrencesByDate?startDate={startDate.Date}&endDate={endDate.Date}");
        }
        public Task<List<InsightDiagnosticDto>> GetInsightDiagnosticAsync(Guid insightId, DateTime start, DateTime end,
            string interval)
        {
            var url = $"insights/{insightId}/occurrences/diagnostics?start={start}&end={end}&interval={interval}";
            return _insightApi.Get<List<InsightDiagnosticDto>>(url);
        }

        public Task<DiagnosticsSnapshotDto> GetDiagnosticsSnapshot(Guid insightId)
        {
            var url = $"insights/{insightId}/diagnostics/snapshot";
            return _insightApi.Get<DiagnosticsSnapshotDto>(url);
        }

        public Task<List<InsightMapViewResponse>> GetInsightListForMapViewAsync(IEnumerable<Guid> siteIds)
        {
            var url = $"insights/mapview";
            foreach (var siteId in siteIds)
            {
                url = QueryHelpers.AddQueryString(url, "siteIds", siteId.ToString());
            }

            return _insightApi.Get<List<InsightMapViewResponse>>(url);

        }

        public Task<List<InsightOccurrence>> GetInsightOccurrencesAsync(Guid insightId)
        {
	        return _insightApi.Get<List<InsightOccurrence>>($"insights/{insightId}/occurrences");
		}

		public Task<Insight> GetInsight(Guid siteId, Guid insightId)
        {
            return _insightApi.Get<Insight>($"sites/{siteId}/insights/{insightId}");
        }
        public Task<Insight> GetInsight(Guid insightId)
        {
            return _insightApi.Get<Insight>($"insights/{insightId}");
        }
        public async Task UpdateBatchInsightStatusAsync(Guid siteId, BatchUpdateInsightStatusRequest request)
        {
            await _insightApi.PutCommand<BatchUpdateInsightStatusRequest>($"sites/{siteId}/insights/status", request);
        }

        public Task<Insight> UpdateInsightAsync(Guid siteId, Guid insightId, UpdateInsightRequest request)
		{
			return _insightApi.Put<UpdateInsightRequest, Insight>($"sites/{siteId}/insights/{insightId}", request);
		}

        public Task<InsightFilterDto> GetInsightsFilterAsync(
            GetInsightFilterApiRequest request)
        {
            return _insightApi.Post<GetInsightFilterApiRequest, InsightFilterDto>($"insights/filters", request);
        }
        public async Task<BatchDto<Insight>> GetInsights(BatchRequestDto request, bool addFloor = false)
        {
            return await _insightApi.Post<BatchRequestDto, BatchDto<Insight>>($"insights?addFloor={addFloor}", request);
        }
        public async Task<BatchDto<SkillDto>> GetSkills(BatchRequestDto request)
		{
			return await _insightApi.Post<BatchRequestDto, BatchDto<SkillDto>>($"skills", request);
		}

        public async Task<BatchDto<InsightCard>> GetInsightCards(BatchRequestDto request)
        {
            return await _insightApi.Post<BatchRequestDto, BatchDto<InsightCard>>("insights/cards", request);
        }

        public async Task<List<ImpactScore>> GetImpactScoresSummary(BatchRequestDto request)
        {
            return await _insightApi.Post<BatchRequestDto, List<ImpactScore>>("insights/impactscores/summary", request);
        }

        public Task<InsightStatisticsResponse> GetInsightStatisticsBySiteIds(List<Guid> siteIds)
        {
            return _insightApi.Post<List<Guid>,InsightStatisticsResponse>("insights/statistics", siteIds);
        }

        public Task<List<TwinInsightStatisticsDto>> GetTwinsInsightStatistics(TwinInsightStatisticsRequest request)
        {
            return _insightApi.Post<TwinInsightStatisticsRequest, List<TwinInsightStatisticsDto>>("insights/twins/statistics", request);
        }

        public Task<List<InsightSnackbarByStatus>> GetInsightSnackbarsByStatus(IEnumerable<FilterSpecificationDto> filters)
        {
            return _insightApi.Post<IEnumerable<FilterSpecificationDto>, List<InsightSnackbarByStatus>>("insights/snackbars/status", filters);
        }

        public async Task<List<InsightActivity>> GetInsightActivitiesAsync(Guid siteId, Guid insightId)
		{
			var url = $"sites/{siteId}/insights/{insightId}/activities";
			return await _insightApi.Get<List<InsightActivity>>(url);
		}

        public async Task<List<InsightSourceDto>> GetInsightSourcesAsync(List<Guid> siteIds)
        {
            var url = $"sources";
            foreach (var siteId in siteIds)
            {
                url = QueryHelpers.AddQueryString(url, "siteIds", siteId.ToString());
            }

            return await _insightApi.Get<List<InsightSourceDto>>(url);
        }

        public async Task<InsightPointsDto> GetInsightPointsAsync(Guid siteId, Guid insightId)
        {
            var url = $"sites/{siteId}/insights/{insightId}/points";

            return await _insightApi.Get<InsightPointsDto>(url);
        }
    }
}
