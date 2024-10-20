using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.Models;
using System.Linq;
using PlatformPortalXL.Services.Sites;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Features.Scopes
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class ScopesController : ControllerBase
    {
        private readonly IUserAuthorizedSitesService _userAuthorizedSitesService;
        private readonly ISiteService _siteService;

        public ScopesController(IUserAuthorizedSitesService userAuthorizedSitesService, ISiteService siteService)
        {
            _userAuthorizedSitesService = userAuthorizedSitesService;
            _siteService = siteService;
        }

        /// <summary>
        /// Retrieves a list of sites based on the specified scope.
        /// </summary>
        /// <remarks>
        /// If the provided dtId corresponds to a site, the returned list will contain that single site.
        /// If the provided dtId corresponds to a campus, portfolio, or any other entity that includes
        /// sites, the returned list will include all the sites directly or indirectly associated with
        /// the specified entity. Non-site entities, such as campuses, will not be included. For example,
        /// if the input is a portfolio that contains campuses, which in turn contain sites, only the
        /// sites will be included in the result, excluding the campuses.
        /// </remarks>
        /// <param name="request">The request object specifying the scope (dtId).</param>
        /// <returns>A list of TwinDto objects representing the retrieved sites.</returns>
        [HttpPost("scopes/sites")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<TwinDto>>> GetSites([FromBody] GetScopeSitesRequest request)
        {
            var userSites = await _userAuthorizedSitesService.GetAuthorizedSites(this.GetCurrentUserId(), Permissions.ViewSites);

            if (userSites == null || !userSites.Any())
            {
                return null;
            }

            return await _siteService.GetUserSiteTwinsByScopeIdAsync(new ScopeIdRequest
            {
                 DtId = request.Scope.DtId,
                 UserSites = userSites
            });
        }
    }
}
