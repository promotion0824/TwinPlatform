using PlatformPortalXL.Services.Sites;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Platform.Models;

namespace PlatformPortalXL.Auth.Services
{
    public interface IUserAuthorizedSitesService
    {
        Task<List<Site>> GetAuthorizedSites(Guid userId, string permissionId);
    }

    public class UserAuthorizedSitesService : IUserAuthorizedSitesService
    {
        private readonly IAuthFeatureFlagService _featureFlagService;
        private readonly IDirectoryApiService _directoryApiService;
        private readonly ISiteService _siteService;

        public UserAuthorizedSitesService(
            IAuthFeatureFlagService authFeatureFlagService,
            IDirectoryApiService directoryApiService,
            ISiteService siteService)
        {
            _featureFlagService = authFeatureFlagService;
            _directoryApiService = directoryApiService;
            _siteService = siteService;
        }

        public async Task<List<Site>> GetAuthorizedSites(Guid userId, string permissionId)
        {
            if (_featureFlagService.IsFineGrainedAuthEnabled)
            {
                // TODO #136199: The GetAuthedSitesForUser is only internal to SiteServiceWithAuthFiltering so we need to cast it to the correct type
                // This will be updated when the SiteService merged with SiteServiceWithAuthFiltering 
                return (await (_siteService as SiteServiceWithAuthFiltering).GetAuthedSitesForUser(userId, permissionId)).ToList();
            }

            return await _directoryApiService.GetUserSites(userId, permissionId);
        }
    }
}
