using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.Services.LiveDataApi
{

    public delegate Task<Dictionary<string, List<T>>> GetTimeSeriesData<T>(Guid customerId, IEnumerable<string> twinIds, DateTime start, DateTime end, TimeSpan? selectedInterval)
        where T : TimeSeriesData;
    public interface ILiveDataApiService
    {
        Task<List<ConnectorStats>> GetConnectorStats(DateTime? start, DateTime? end, List<Connector> connectors);

        Task<List<ConnectorStats>> GetConnectorStats(Guid customerId, DateTime? start, DateTime? end, Guid[] connectorIds);

        Task<List<PointTimeSeriesRawData>> GetLastTrendlogsAsync(Guid customerId, Guid siteId, IEnumerable<string> twinIds);

        Task<List<TimeSeriesAnalogData>> GetTimeSeriesAnalogData(Guid customerId, string twinId, DateTime start,
            DateTime end, TimeSpan? interval);

        Task<Dictionary<string, List<TimeSeriesAnalogData>>> GetTimeSeriesAnalogData(Guid customerId,
            IEnumerable<string> twinIds, DateTime start, DateTime end, TimeSpan? interval);
        Task<List<TimeSeriesBinaryData>> GetTimeSeriesBinaryData(Guid customerId, string twinId, DateTime start,
            DateTime end, TimeSpan? interval);

        Task<Dictionary<string, List<TimeSeriesBinaryData>>> GetTimeSeriesBinaryData(Guid customerId,
            IEnumerable<string> twinIds, DateTime start, DateTime end, TimeSpan? interval);
        Task<List<TimeSeriesSumData>> GetTimeSeriesSumData(Guid customerId, string twinId, DateTime start,
            DateTime end, TimeSpan? interval);
        Task<Dictionary<string, List<TimeSeriesSumData>>> GetTimeSeriesSumData(Guid customerId,
            IEnumerable<string> twinIds, DateTime start, DateTime end, TimeSpan? interval);
        Task<List<TimeSeriesMulitStateData>> GetTimeSeriesMultiStateData(Guid customerId, string twinId, DateTime start,
            DateTime end, TimeSpan? interval);
        Task<Dictionary<string, List<TimeSeriesMulitStateData>>> GetTimeSeriesMultiStateData(Guid customerId,
            IEnumerable<string> twinIds, DateTime start, DateTime end, TimeSpan? interval);
        Task<List<TimeSeriesAnalogData>> GetTimeSeriesAnalogByExternalId(Guid customerId, Guid connectorId, string externalId,
           DateTime start, DateTime end, TimeSpan? interval);
    }

    public class LiveDataApiService : ILiveDataApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LiveDataApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<ConnectorStats>> GetConnectorStats(DateTime? start, DateTime? end, List<Connector> connectors)
        {
            var connectorStats = new List<ConnectorStats>();

            if (connectors.Count > 0)
            {
                var connectorIds = connectors.Select(x => x.Id).ToArray();
                connectorStats = await GetConnectorStats(connectors.First().ClientId, start, end, connectorIds);
            }

            return connectorStats;
        }

        public async Task<List<ConnectorStats>> GetConnectorStats(Guid customerId, DateTime? start, DateTime? end, Guid[] connectorIds)
        {
            if (connectorIds == null || connectorIds.Length == 0)
            {
                return new List<ConnectorStats>();
            }

            using var client = _httpClientFactory.CreateClient(ApiServiceNames.LiveDataCore);

            var urlBuilder = new StringBuilder($"api/livedata/stats/connectors?clientId={customerId}");

            if (start.HasValue)
            {
                var startString = HttpUtility.UrlEncode(start.Value.ToString("O", CultureInfo.InvariantCulture));
                urlBuilder.Append($"&start={startString}");
            }

            if (end.HasValue)
            {
                var endString = HttpUtility.UrlEncode(end.Value.ToString("O", CultureInfo.InvariantCulture));
                urlBuilder.Append($"&end={endString}");
            }

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(client.BaseAddress + urlBuilder.ToString()),
                Method = HttpMethod.Get,
                Content = new StringContent(JsonSerializerHelper.Serialize(new { connectorIds }))
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content.Headers.Clear();
            request.Content.Headers.Add("Content-Type", "application/json");

            var response = await client.SendAsync(request);
            await response.EnsureSuccessStatusCode(ApiServiceNames.LiveDataCore);
            var strResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializerHelper.Deserialize<ConnectorsStats>(strResponse).Data;
        }

        public async Task<List<PointTimeSeriesRawData>> GetLastTrendlogsAsync(Guid customerId, Guid siteId, IEnumerable<string> twinIds)
        {
            var urlBuilder = new StringBuilder();
            urlBuilder.Append($"api/telemetry/sites/{siteId}/lastTrendlogs?clientId={customerId}");
            foreach (var twinId in twinIds)
            {
                urlBuilder.Append($"&twinId={twinId}");
            }

            using var client = _httpClientFactory.CreateClient(ApiServiceNames.LiveDataCore);
            var response = await client.GetAsync(urlBuilder.ToString());
            await response.EnsureSuccessStatusCode(ApiServiceNames.LiveDataCore);
            var strResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializerHelper.Deserialize<List<PointTimeSeriesRawData>>(strResponse);
        }

        public async Task<List<TimeSeriesAnalogData>> GetTimeSeriesAnalogData(Guid customerId, string twinId,
            DateTime start, DateTime end, TimeSpan? interval)
        {
            return await GetTimeSeriesData<List<TimeSeriesAnalogData>>(customerId, twinId, "analog", start, end, interval);
        }

        public async Task<Dictionary<string, List<TimeSeriesAnalogData>>> GetTimeSeriesAnalogData(Guid customerId,
            IEnumerable<string> twinIds, DateTime start, DateTime end, TimeSpan? interval)
        {
            return await GetTimeSeriesDataBulk<Dictionary<string, List<TimeSeriesAnalogData>>>(customerId, twinIds, "analog", start, end, interval);
        }

        public async Task<List<TimeSeriesBinaryData>> GetTimeSeriesBinaryData(Guid customerId, string twinId,
            DateTime start, DateTime end, TimeSpan? interval)
        {
            return await GetTimeSeriesData<List<TimeSeriesBinaryData>>(customerId, twinId, "binary", start, end, interval);
        }

        public async Task<Dictionary<string, List<TimeSeriesBinaryData>>> GetTimeSeriesBinaryData(Guid customerId,
            IEnumerable<string> twinIds, DateTime start, DateTime end, TimeSpan? interval)
        {
            return await GetTimeSeriesDataBulk<Dictionary<string, List<TimeSeriesBinaryData>>>(customerId, twinIds, "binary", start, end,interval);
        }

        public async Task<List<TimeSeriesSumData>> GetTimeSeriesSumData(Guid customerId, string twinId,
            DateTime start, DateTime end, TimeSpan? interval)
        {
            return await GetTimeSeriesData<List<TimeSeriesSumData>>(customerId, twinId, "sum", start, end, interval);
        }

        public async Task<Dictionary<string, List<TimeSeriesSumData>>> GetTimeSeriesSumData(Guid customerId,
            IEnumerable<string> twinIds, DateTime start, DateTime end, TimeSpan? interval)
        {
            return await GetTimeSeriesDataBulk<Dictionary<string, List<TimeSeriesSumData>>>(customerId, twinIds, "sum", start, end, interval);
        }

        public async Task<Dictionary<string, List<TimeSeriesMulitStateData>>> GetTimeSeriesMultiStateData(
            Guid customerId,
            IEnumerable<string> twinIds,
            DateTime start,
            DateTime end,
            TimeSpan? interval)
        {
            return await GetTimeSeriesDataBulk<Dictionary<string, List<TimeSeriesMulitStateData>>>(customerId, twinIds, "multistate", start, end, interval);
        }

        public async Task<List<TimeSeriesMulitStateData>> GetTimeSeriesMultiStateData(Guid customerId, string twinId,
            DateTime start, DateTime end, TimeSpan? interval)
        {
            return await GetTimeSeriesData<List<TimeSeriesMulitStateData>>(customerId, twinId, "multistate", start, end, interval);
        }

        public async Task<List<TimeSeriesAnalogData>> GetTimeSeriesAnalogByExternalId(Guid customerId, Guid connectorId, string externalId,
           DateTime startUtc, DateTime endUtc, TimeSpan? interval)
        {
            return await GetTimeSeriesDataByExternalId<List<TimeSeriesAnalogData>>(customerId, connectorId, externalId, startUtc, endUtc, interval);
        }

        private async Task<T> GetTimeSeriesDataByExternalId<T>(Guid customerId, Guid connectorId, string externalId, DateTime startUtc, DateTime endUtc, TimeSpan? interval)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.LiveDataCore);

            var startString = HttpUtility.UrlEncode(startUtc.ToString("O", CultureInfo.InvariantCulture));
            var endString = HttpUtility.UrlEncode(endUtc.ToString("O", CultureInfo.InvariantCulture));
            var urlBuilder = new StringBuilder($"api/telemetry/point/analog/{connectorId}/{externalId}?clientId={customerId}&startUtc={startString}&endUtc={endString}");
            if (interval != null)
            {
                urlBuilder.Append($"&interval={HttpUtility.UrlEncode(interval.ToString())}");
            }

            var response = await client.GetAsync(urlBuilder.ToString());
            await response.EnsureSuccessStatusCode(ApiServiceNames.LiveDataCore);
            return await response.Content.ReadAsAsync<T>();
        }

        private async Task<T> GetTimeSeriesData<T>(Guid customerId, string twinId, string type, DateTime start, DateTime end, TimeSpan? interval)
        {
            using var client = _httpClientFactory.CreateClient(ApiServiceNames.LiveDataCore);

            var startString = HttpUtility.UrlEncode(start.ToString("O", CultureInfo.InvariantCulture));
            var endString = HttpUtility.UrlEncode(end.ToString("O", CultureInfo.InvariantCulture));
            var urlBuilder = new StringBuilder($"api/telemetry/point/{type}/{twinId}?clientId={customerId}&start={startString}&end={endString}");
            if (interval != null)
            {
                urlBuilder.Append($"&interval={HttpUtility.UrlEncode(interval.ToString())}");
            }

            var response = await client.GetAsync(urlBuilder.ToString());
            await response.EnsureSuccessStatusCode(ApiServiceNames.LiveDataCore);
            return await response.Content.ReadAsAsync<T>();
        }

        private async Task<T> GetTimeSeriesDataBulk<T>(Guid customerId, IEnumerable<string> twinIds, string type, DateTime start, DateTime end, TimeSpan? interval)
        {
            var startString = HttpUtility.UrlEncode(start.ToString("O", CultureInfo.InvariantCulture));
            var endString = HttpUtility.UrlEncode(end.ToString("O", CultureInfo.InvariantCulture));
            var urlBuilder = new StringBuilder($"api/telemetry/point/{type}?clientId={customerId}&start={startString}&end={endString}");
            if (interval != null)
            {
                urlBuilder.Append($"&interval={HttpUtility.UrlEncode(interval.ToString())}");
            }
            foreach (var twinId in twinIds)
            {
                urlBuilder.Append($"&twinId={twinId}");
            }

            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.LiveDataCore))
            {
                var response = await client.GetAsync(urlBuilder.ToString());
                await response.EnsureSuccessStatusCode(ApiServiceNames.LiveDataCore);
                var responseStr = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseStr);
            }
        }

    }
}
