using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Willow.IoTService.Monitoring.Dtos.LiveDataCore;

namespace Willow.IoTService.Monitoring.Services.Core;

public interface ILiveDataCoreApiService
{
    Task<ConnectorStatsDto?> GetConnectorStats(Guid? customerId, Guid connectorId, DateTime start, DateTime end);
    Task<UniqueTrendsResult?> GetUniqueTrends(Guid? customerId, IList<Guid>? connectorIds, DateTime start, DateTime end);
    Task<IReadOnlyDictionary<string, string>> GetMissingTrends(Guid? customerId, IList<Guid>? connectorIds, DateTime start, DateTime end);
}

public class LiveDataCoreApiService : ILiveDataCoreApiService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LiveDataCoreApiService> _logger;

    public LiveDataCoreApiService(HttpClient httpClient, ITokenService tokenService, ILogger<LiveDataCoreApiService> logger)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<ConnectorStatsDto?> GetConnectorStats(Guid? customerId, Guid connectorId, DateTime start, DateTime end)
    {
        var connectorIds = new[] { connectorId };
        await _tokenService.ConfigureHttpClientAuth(_httpClient);
        var urlBuilder = new StringBuilder($"api/livedata/stats/connectors?clientId={customerId}&start={start:O}&end={end:O}&binFullInterval=true");

        var request = new HttpRequestMessage
        {
            RequestUri = new Uri(_httpClient.BaseAddress + urlBuilder.ToString()),
            Method = HttpMethod.Get,
            Content = new StringContent(JsonSerializer.Serialize(new { connectorIds }))
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content.Headers.Clear();
        request.Content.Headers.Add("Content-Type", "application/json");
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var strResult = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ConnectorStatsResult>(strResult, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.Data?.FirstOrDefault();
    }

    public async Task<IReadOnlyDictionary<string, string>> GetMissingTrends(Guid? customerId, IList<Guid>? connectorIds, DateTime start, DateTime end)
    {
        await _tokenService.ConfigureHttpClientAuth(_httpClient);
        var urlBuilder = new StringBuilder($"api/livedata/stats/missingTrends?clientId={customerId}&start={start:O}&end={end:O}");

        var request = new HttpRequestMessage
        {
            RequestUri = new Uri(_httpClient.BaseAddress + urlBuilder.ToString()),
            Method = HttpMethod.Get,
            Content = new StringContent(JsonSerializer.Serialize(new { connectorIds }))
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content.Headers.Clear();
        request.Content.Headers.Add("Content-Type", "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        // Convert back to object - "data": [ { "connectorId": "", "details": [ { "trendId": "", "twinId": "", "name": "", "model": "", "isCapabilityOf": "", "isHostedBy": "" } ...] } ...],
        var strResult = await response.Content.ReadAsStringAsync();
        var missingTrends = JsonSerializer.Deserialize<MissingTrendsResult>(strResult, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var result = new Dictionary<string, string>();

        // Anything missing?
        var missingTrendList = missingTrends?.Data?.ToList();
        if (missingTrendList == null || missingTrendList.Count == 0)
        {
            return result;
        }

        var sb = new StringBuilder();
        foreach (var missingTrend in missingTrendList)
        {
            sb.Clear();
            sb.AppendLine(MissingTrendsDetailDto.GetProps());       // Header

            var connectorId = missingTrend.ConnectorId;

            var details = missingTrend.Details;
            if (details != null)
            {
                foreach (var row in details)
                {
                    sb.AppendLine(row.ToString());
                }
            }

            result.Add(connectorId.ToString(), sb.ToString());
        }

        return result;
    }

    /// <summary>
    /// Call the LiveData.Core /api/livedata/stats/uniqueTrends API to retrieve trending Capability statistics
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="connectorIds"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<UniqueTrendsResult?> GetUniqueTrends(
        Guid? customerId,        // Customer
        IList<Guid>? connectorIds,      // Connectors to query
        DateTime start,     // Beginning of time range
        DateTime end)       // Finish of time range
    {
        await _tokenService.ConfigureHttpClientAuth(_httpClient);
        var urlBuilder = new StringBuilder($"api/livedata/stats/uniqueTrends?clientId={customerId}&start={start:O}&end={end:O}");

        var request = new HttpRequestMessage
        {
            RequestUri = new Uri(_httpClient.BaseAddress + urlBuilder.ToString()),
            Method = HttpMethod.Get,
            Content = new StringContent(JsonSerializer.Serialize(new { connectorIds }))
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content.Headers.Clear();
        request.Content.Headers.Add("Content-Type", "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var strResult = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<UniqueTrendsResult>(strResult, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;
    }
}

