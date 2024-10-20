using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Willow.IoTService.Monitoring.Dtos.DirectoryCore;

namespace Willow.IoTService.Monitoring.Services.Core;

public interface IDirectoryApiService
{
    Task<IEnumerable<SiteDto>> GetSites();
    Task<IEnumerable<CustomerDto>> GetCustomers();
}

public class DirectoryApiService : IDirectoryApiService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenService _tokenService;

    public DirectoryApiService(
        HttpClient httpClient,
        ITokenService tokenService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
    }

    public async Task<IEnumerable<SiteDto>> GetSites()
    {
        await _tokenService.ConfigureHttpClientAuth(_httpClient);
        var response = await _httpClient.GetAsync($"sites");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsAsync<IEnumerable<SiteDto>>();
        return result;
    }

    public async Task<IEnumerable<CustomerDto>> GetCustomers()
    {
        await _tokenService.ConfigureHttpClientAuth(_httpClient);
        var response = await _httpClient.GetAsync($"customers");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsAsync<IEnumerable<CustomerDto>>();
        return result;
    }
}