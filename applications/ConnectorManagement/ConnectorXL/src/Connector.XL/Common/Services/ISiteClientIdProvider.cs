namespace Connector.XL.Common.Services;

internal interface ISiteClientIdProvider
{
    Task<Guid> GetClientIdForSiteAsync(Guid siteId);
}
