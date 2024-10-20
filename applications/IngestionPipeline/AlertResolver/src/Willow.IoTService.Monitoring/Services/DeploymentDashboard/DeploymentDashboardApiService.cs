using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Willow.IoTService.Monitoring.Dtos.DeploymentDashboard;
using Willow.IoTService.Monitoring.Services.Core;

namespace Willow.IoTService.Monitoring.Services.DeploymentDashboard;

public class DeploymentDashboardApiService : IDeploymentDashboardApiService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenService _tokenService;

    public DeploymentDashboardApiService(HttpClient httpClient,
                                         ITokenService tokenService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
    }

    public async Task<(string, string)> GetDeviceProperties(string connectorName)
    {
        _tokenService.ConfigureDashboardAzureAdHttpClientAuth(_httpClient);
        var urlBuilder = new StringBuilder($"api/v1/Modules/search?name={connectorName}");

        var response = await _httpClient.GetAsync(new Uri(_httpClient.BaseAddress + urlBuilder.ToString()));

        var result = await response.Content.ReadAsAsync<PagedResult>();
        response.EnsureSuccessStatusCode();
        var modules = (result.Items ?? Array.Empty<ModuleDto>()).ToList();
        var iotHubName= modules.FirstOrDefault()?.IoTHubName;
        var deviceName= modules.FirstOrDefault()?.DeviceName;

        return (deviceName ?? string.Empty, iotHubName ?? string.Empty);
    }
}


public interface IDeploymentDashboardApiService
{
    Task<(string, string)> GetDeviceProperties(string connectorName);
}
