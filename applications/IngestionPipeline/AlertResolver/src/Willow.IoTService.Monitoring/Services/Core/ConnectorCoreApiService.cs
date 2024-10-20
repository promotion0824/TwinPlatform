using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.IoTService.Monitoring.Dtos.ConnectorCore;

namespace Willow.IoTService.Monitoring.Services.Core;

public interface IConnectorCoreApiService
{
    Task<IEnumerable<ConnectorDto>> GetEnabledConnectorsAsync(Guid siteId);
    Task<IDictionary<string, string>> GetConnectorTypesAsync();
}

public class ConnectorCoreApiService : IConnectorCoreApiService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenService _tokenService;

    public ConnectorCoreApiService(
        HttpClient httpClient,
        ITokenService tokenService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
    }

    public async Task<IEnumerable<ConnectorDto>> GetEnabledConnectorsAsync(Guid siteId)
    {
        await _tokenService.ConfigureHttpClientAuth(_httpClient);
        var response = await _httpClient.GetAsync(new Uri(_httpClient.BaseAddress + $"sites/{siteId}/Connectors"));
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsAsync<IEnumerable<ConnectorDto>>();
        return result.Where(x => x.IsEnabled && x.IsLoggingEnabled).ToList();
    }

    public async Task<IDictionary<string, string>> GetConnectorTypesAsync()
    {
        await _tokenService.ConfigureHttpClientAuth(_httpClient);
        var response = await _httpClient.GetAsync(new Uri(_httpClient.BaseAddress + "ConnectorTypes"));
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsAsync<IEnumerable<ConnectorTypeDto>>();
        var keyValuePairs = result.ToDictionary(x => x.Id.ToString().ToUpperInvariant(), x => x.Name);
        return keyValuePairs;
    }
}