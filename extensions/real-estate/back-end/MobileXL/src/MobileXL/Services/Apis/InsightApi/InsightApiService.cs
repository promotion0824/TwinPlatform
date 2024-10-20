using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MobileXL.Models;

namespace MobileXL.Services.Apis.InsightApi
{
    public interface IInsightApiService
    {
        Task<Insight> CreateInsight(Guid siteId, CreateInsightCoreRequest request);
        Task<Insight> UpdateInsightAsync(Guid siteId, Guid insightId, Guid currentUserId, InsightStatus status);

    }

    public class InsightApiService : IInsightApiService
    {
        private readonly HttpClient _client;

        public InsightApiService(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient(ApiServiceNames.InsightCore);
        }

        public async Task<Insight> CreateInsight(Guid siteId, CreateInsightCoreRequest request)
        {
            var response = await _client.PostAsJsonAsync($"sites/{siteId}/insights", request);
            response.EnsureSuccessStatusCode(ApiServiceNames.InsightCore);
            return await response.Content.ReadAsAsync<Insight>();
        }
        public async Task<Insight> UpdateInsightAsync(Guid siteId, Guid insightId, Guid currentUserId, InsightStatus status )
        {
	        var response=await _client.PutAsJsonAsync($"sites/{siteId}/insights/{insightId}",
		        new
		        {
			        LastStatus = status,
			        UpdatedByUserId = currentUserId
		        });
	        response.EnsureSuccessStatusCode(ApiServiceNames.InsightCore);
	        return await response.Content.ReadAsAsync<Insight>();
		}
	}

    public class CreateInsightCoreRequest
    {
        public Guid CustomerId { get; set; }
        public string SequenceNumberPrefix { get; set; }
        public string TwinId { get; set; }
		public InsightType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public InsightState State { get; set; }
        public DateTime OccurredDate { get; set; }
        public DateTime DetectedDate { get; set; }
        public InsightSourceType SourceType { get; set; }
        public Guid? SourceId { get; set; }
        public string ExternalId { get; set; }
        public string ExternalStatus { get; set; }
        public string ExternalMetadata { get; set; }
        public int OccurrenceCount { get; set; }
        public Dictionary<string, string> AnalyticsProperties { get; set; }
		public Guid? CreatedUserId { get; set; }
	}
}
