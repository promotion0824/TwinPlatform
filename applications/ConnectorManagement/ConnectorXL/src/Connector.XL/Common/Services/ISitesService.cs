namespace Connector.XL.Common.Services;

using Connector.XL.Common.Models;

internal interface ISitesService
{
    Task<SiteDto> GetSiteAsync(Guid siteId);
}
