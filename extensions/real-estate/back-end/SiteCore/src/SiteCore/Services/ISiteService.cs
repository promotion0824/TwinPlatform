using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using SiteCore.Domain;
using SiteCore.Requests;

namespace SiteCore.Services
{
    public interface ISiteService
    {
        Task<List<Site>> GetAllSites();
        Task<List<Site>> GetAllSitesByIdsAsync(List<Guid> siteIds);
        Task<List<Site>> GetSitesForPortfolio(Guid portfolioId);
        Task<List<Site>> GetSites(Guid customerId, Guid? portfolioId = null);
        Task<Site> GetSite(Guid siteId);
        Task SoftDeleteSite(Guid siteId);
        Task<Site> UpdateSiteLogo(Guid siteId, byte[] logoImageContent);
        Task<Site> CreateSite(Guid customerId, Guid portfolioId, CreateSiteRequest createSiteRequest);
        Task<Site> UpdateSite(Guid customerId, Guid portfolioId, Guid siteId, UpdateSiteRequest updateSiteRequest);
        Task<SitePreferences> GetSitePreferences(Guid siteId);
        Task CreateOrUpdateSitePreferences(Guid siteId, SitePreferencesRequest sitePreferencesRequest);
        Task<SitePreferences> GetSitePreferencesByScope(string scopeId);
        Task CreateOrUpdateSitePreferencesByScope(string scopeId, SitePreferencesRequest sitePreferencesRequest);
        Task PopulateScopeIdToSitePreferences();
        void RemoveSitesCache();
    }
}
