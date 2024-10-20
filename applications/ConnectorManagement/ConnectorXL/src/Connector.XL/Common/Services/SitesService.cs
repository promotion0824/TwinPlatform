namespace Connector.XL.Common.Services;

using Connector.XL.Common.Extensions;
using Connector.XL.Common.Models;
using Connector.XL.Infrastructure;

internal class SitesService : ISitesService
{
    private readonly IHttpClientFactory httpClientFactory;

    public SitesService(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<SiteDto> GetSiteAsync(Guid siteId)
    {
        var client = httpClientFactory.CreateClient(Constants.DirectoryCoreApiClientName);
        var url = $"/sites/{siteId:D}";
        return await client.GetJsonAsync<SiteDto>(url);
    }
}
