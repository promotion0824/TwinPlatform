using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SiteCore.Services
{
    public interface ISitePreferencesScopePopulateService
    {
        /// <summary>
        /// Populate the scopeId of SitePreferences table from DigitalTwinCore
        /// only when all scopeId are empty
        /// </summary>
        Task PopulateScopeIdToSitePreferences();
    }

    public class SitePreferencesScopePopulateService : ISitePreferencesScopePopulateService
    {
        private readonly ISiteService _siteService;

        public SitePreferencesScopePopulateService(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task PopulateScopeIdToSitePreferences()
        {
            await _siteService.PopulateScopeIdToSitePreferences();
        }
    }
}
